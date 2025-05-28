# Troubleshooting SSL Connection Issues

If you're encountering SSL connection issues when trying to connect to the MCP server in Docker, here are some steps to help diagnose and fix the problem:

## Common Error Messages

- **"The SSL connection could not be established, see inner exception"**
- **"Could not establish trust relationship for the SSL/TLS secure channel"**
- **"The remote certificate is invalid according to the validation procedure"**

## Possible Causes and Solutions

### 1. Azure OpenAI Endpoint SSL Certificate Issues

The Azure OpenAI endpoint URL in your `.env` file may have an SSL certificate that isn't trusted by the Docker container.

Check your `.env` file:
```
AZURE_ENDPOINT=https://davinci-dev-openai-api.corp.hpicloud.net/salesly
```

Solutions:
- Try removing the `https://` prefix (the system will add it automatically)
- If this is a corporate internal endpoint, it may be using a self-signed certificate

### 2. Protocol Mismatch

If you're connecting to the Docker container using HTTPS but it's only configured for HTTP, you'll get SSL errors.

Check your client URL:
- Make sure you're using `http://` (not `https://`) when connecting to the MCP server

### 3. Certificate Validation Bypass (For Development Only)

We've added certificate validation bypass code in the latest updates. Rebuild your Docker container with:

```powershell
cd c:\Code\Personal\MCPSample\MyFirstServerMCP
docker-compose down
docker-compose up -d
```

The Docker container now includes the following environment variables to disable SSL certificate validation:
```
DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
DOTNET_SSL_ENABLE_SERVER_CERTIFICATE_VALIDATION=false
```

Additionally, we've modified the NlpService.cs to use a custom HttpClient that bypasses SSL certificate validation for the Azure OpenAI client.

### 4. Docker Network Issues

Sometimes Docker's networking can cause SSL issues:

```powershell
# Stop and remove the container
docker stop mcpserver
docker rm mcpserver

# Rebuild and run with host networking
docker run -d --network host -e AZURE_API_KEY="your-key" -e AZURE_ENDPOINT="your-endpoint" -e AZURE_DEPLOYMENT_NAME="GPT-4o" --name mcpserver myfirstservermcp
```

### 5. Direct Validation Test

You can test the Azure OpenAI endpoint directly:

```powershell
# Using Invoke-RestMethod
$endpoint = "https://davinci-dev-openai-api.corp.hpicloud.net/salesly"
$apiKey = "your-api-key"

# Skip certificate validation (for testing ONLY)
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

try {
    $headers = @{
        "api-key" = $apiKey
        "Content-Type" = "application/json"
    }
    
    # Just making a simple request to test connectivity
    $response = Invoke-RestMethod -Uri "$endpoint/info" -Headers $headers -Method Get -SkipCertificateCheck
    Write-Host "Connection successful!"
} catch {
    Write-Host "Error: $_"
}
```

## Updated Docker Setup

The latest updates include SSL certificate validation handling, which should resolve most issues. Make sure to rebuild your Docker container to get these changes.

If you continue to experience issues, please check the Docker logs for more specific error information:

```powershell
docker logs mcpserver
```
