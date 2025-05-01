using YarpBodyReplacement;
using YarpBodyReplacement.Transformers;
using YarpBodyReplacement.Upstream;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add WireMock server as a hosted service
builder.Services.AddSingleton<WireMockServerService>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<WireMockServerService>());

// Add YARP reverse proxy services
builder
    .Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilderContext =>
    {
        // Add our validation error transformer
        transformBuilderContext.WithBodyTransform();
    });

WebApplication app = builder.Build();


// Map the YARP reverse proxy routes
app.MapReverseProxy();

app.Run();
