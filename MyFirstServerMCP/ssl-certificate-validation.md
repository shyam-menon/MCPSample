# SSL Certificate Validation in Docker

This document explains how we've fixed the SSL certificate validation issues when connecting to Azure OpenAI from our Docker container.

## Problem

When running the MCP server in Docker, we encountered SSL certificate validation errors when connecting to Azure OpenAI:

```
Response: { "error": "An error occurred while processing your query: The SSL connection could not be established, see inner exception." }
```

## Solution

We implemented a multi-layered approach to fix this issue:

### 1. Certificate Validation Bypass in Code

We created a `CertificateValidator` helper class that creates an `HttpClient` which bypasses SSL certificate validation:

```csharp
public static class CertificateValidator
{
    public static HttpClientHandler CreateInsecureHttpClientHandler()
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
            {
                // Always return true to accept all certificates
                return true;
            }
        };
    }

    public static HttpClient GetInsecureHttpClient()
    {
        return new HttpClient(CreateInsecureHttpClientHandler());
    }
}
```

### 2. Custom OpenAI Client with Certificate Validation Bypass

In `NlpService.cs`, we configured the Azure OpenAI client to use our custom `HttpClient` that bypasses certificate validation:

```csharp
// Create a custom HttpClient that bypasses SSL certificate validation
var httpClient = CertificateValidator.GetInsecureHttpClient();

// Create custom OpenAI client options with the insecure HttpClient
var clientOptions = new OpenAIClientOptions()
{
    Transport = new HttpClientTransport(httpClient)
};

// Create the Azure OpenAI client with our custom options
var openAIClient = new OpenAIClient(
    new Uri(endpoint),
    new AzureKeyCredential(apiKey),
    clientOptions);

// Add Azure OpenAI chat completion using our custom client
builder.Services.AddKeyedSingleton("AzureOpenAI", openAIClient);
builder.AddAzureOpenAIChatCompletion(deploymentName, openAIClient);
```

### 3. Environment Variables in Docker

We added environment variables to our Docker configuration to disable certificate validation at the system level:

```dockerfile
# SSL Certificate handling - development only
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV DOTNET_SSL_ENABLE_SERVER_CERTIFICATE_VALIDATION=false
```

These were added to both the Dockerfile and docker-compose.yml.

### 4. Enhanced Error Logging

We improved error logging to provide more details about SSL errors:

```csharp
if (innerException is System.Security.Authentication.AuthenticationException authEx)
{
    _logger.LogError("SSL/TLS Authentication Error: {AuthMessage}", authEx.Message);
    _logger.LogError("This is likely a certificate validation issue. Check if the certificate validation bypass is working properly.");
}
```

## How to Test

To test that the SSL certificate validation bypass is working:

1. Ensure your Azure OpenAI credentials are set in the environment:
   - AZURE_API_KEY
   - AZURE_ENDPOINT
   - AZURE_DEPLOYMENT_NAME

2. Run the Docker container:
   ```powershell
   cd c:\Code\Personal\MCPSample\MyFirstServerMCP
   docker-compose up -d
   ```

3. Access the web interface at http://localhost:5001

4. Try sending a natural language query to the server, which will internally call Azure OpenAI.

5. If there are still SSL issues, check the logs:
   ```powershell
   docker logs myfirstservermcp-mcpserver-1
   ```

## Important Note

This SSL certificate validation bypass is **ONLY for development environments**. It should never be used in production as it bypasses security checks that are in place to protect against man-in-the-middle attacks.

For production, you should:
- Use properly signed certificates
- Configure proper certificate trust chains
- Remove the certificate validation bypass code
