using System.Text.Json;
using System.Text.Json.Serialization;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace YarpBodyReplacement.Upstream;

public class WireMockServerService : IHostedService, IDisposable
{
    private WireMockServer? _server;
    private readonly ILogger<WireMockServerService> _logger;
    private readonly IConfiguration _configuration;
    private readonly Random _random = new();

    public WireMockServerService(
        ILogger<WireMockServerService> logger,
        IConfiguration configuration
    )
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting WireMock server");

        // Get port from configuration or use default
        int port = _configuration.GetValue<int>("WireMock:Port", 9000);

        // Start the server
        _server = WireMockServer.Start(port);

        // Configure the /validate endpoint with random success value
        _server
            .Given(Request.Create().WithPath("/validate").UsingGet())
            .RespondWith(
                Response
                    .Create()
                    .WithStatusCode(200)
                    .WithHeader("Content-Type", "application/json")
                    .WithBody(_ =>
                    {
                        // Randomly determine if success is true or false
                        bool success = _random.Next(2) == 1;

                        _logger.LogInformation(
                            "Generating random response with success = {Success}",
                            success
                        );

                        // Create the typed response object
                        ValidationResponse responseObject = new()
                        {
                            Success = success,
                            // Only set errors when success is false
                            Errors = success ? null : ["one", "two"],
                        };

                        // Serialize to JSON
                        return JsonSerializer.Serialize(
                            responseObject,
                            new JsonSerializerOptions
                            {
                                WriteIndented = true,
                                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                            }
                        );
                    })
            );

        _logger.LogInformation("WireMock server started on port {Port}", port);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping WireMock server");
        _server?.Stop();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _server?.Dispose();
    }

    public string BaseUrl => _server?.Urls.FirstOrDefault() ?? string.Empty;
}
