using System.Text;
using System.Text.Json;
using Yarp.ReverseProxy.Transforms;
using Yarp.ReverseProxy.Transforms.Builder;
using YarpBodyReplacement.Upstream;

namespace YarpBodyReplacement.Transformers;

internal static class ValidationErrorTransformer
{
    internal static TransformBuilderContext WithBodyTransform(
        this TransformBuilderContext builder
    ) => builder.AddResponseTransform(Transform);

    private static async ValueTask Transform(ResponseTransformContext context)
    {
        HttpContext httpContext = context.HttpContext;
        HttpResponse response = httpContext.Response;

        // Only process responses with 200 status code
        if (response.StatusCode != 200)
        {
            return;
        }

        try
        {
            // Get the response content as a string

            if (context.ProxyResponse is not null)
            {
                string responseBody = await context.ProxyResponse.Content.ReadAsStringAsync();

                // Try to deserialize the response directly into our expected format
                try
                {
                    ValidationResponse? validationResponse =
                        JsonSerializer.Deserialize<ValidationResponse>(
                            responseBody,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                        );

                    // Check if this is a validation error response
                    if (validationResponse is { Success: false, Errors.Length: > 0 })
                    {
                        // Change status code to 422 Unprocessable Entity
                        response.StatusCode = 422;

                        // Replace the response body with only the array of errors
                        context.SuppressResponseBody = true;

                        // Serialize just the errors array
                        string errorsJson = JsonSerializer.Serialize(
                            validationResponse.Errors,
                            new JsonSerializerOptions { WriteIndented = true }
                        );

                        // Set content type and write the new response
                        response.ContentType = "application/json";
                        response.ContentLength = Encoding.UTF8.GetByteCount(errorsJson);
                        await response.WriteAsync(errorsJson);
                    }
                }
                catch (JsonException)
                {
                    //Do something like logging
                }
            }
            else
            {
                //Do something like logging
            }
        }
        catch (Exception)
        {
            // In case of errors, return a 500 Internal Server Error
            response.StatusCode = 500;
        }
    }
}
