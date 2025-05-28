FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY MyFirstServerMCP.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV AZURE_DEPLOYMENT_NAME="GPT-4o"
# SSL Certificate handling - development only
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV DOTNET_SSL_ENABLE_SERVER_CERTIFICATE_VALIDATION=false

# Expose port 5001
EXPOSE 5001

ENTRYPOINT ["dotnet", "MyFirstServerMCP.dll"]
