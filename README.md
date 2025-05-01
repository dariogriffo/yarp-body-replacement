# YARP Body Replacement Example

This solution demonstrates how to transform HTTP response bodies using YARP (Yet Another Reverse Proxy) in an ASP.NET Core application.

## Overview

This example showcases a common API gateway pattern: transforming upstream service responses before they reach the client. Specifically, it demonstrates how to:

1. Intercept responses from an upstream service
2. Modify the response body and status code based on the content
3. Return a transformed response to the client

## How It Works

The solution consists of:

1. **YARP Reverse Proxy**: Routes requests to the upstream service and applies transformations
2. **WireMock.Net Server**: Simulates an upstream service that returns validation responses
3. **Custom Response Transformer**: Transforms validation error responses

### Flow

1. Client sends a request to `/api/validate/`
2. YARP routes the request to the WireMock server
3. WireMock randomly returns either:
   - Success response: `{ "success": true }`
   - Error response: `{ "success": false, "errors": ["one", "two"] }`
4. The custom transformer intercepts error responses and:
   - Changes the status code from 200 to 422 (Unprocessable Entity)
   - Transforms the response body to contain only the errors array: `["one", "two"]`

## Technologies Used

- **.NET 9.0**: Latest .NET framework
- **YARP 2.1.0**: Microsoft's reverse proxy library
- **WireMock.Net 1.5.46**: HTTP mocking library for simulating upstream services

## Running the Solution

1. Clone the repository
2. Open the solution in Visual Studio or your preferred IDE
3. Run the application
4. Send requests to `http://localhost:5075/api/validate/` to see the transformation in action

## Use Cases

This pattern is useful for:

- Standardizing error responses across different upstream services
- Transforming legacy API responses to match modern API standards
- Implementing cross-cutting concerns like validation at the API gateway level
- Simplifying client-side error handling by providing consistent error formats
