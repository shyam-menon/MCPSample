using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyFirstServerMCP;
using MyFirstServerMCP.Services;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Add HttpClient with SSL certificate validation handling for Azure OpenAI
// This is helpful when dealing with endpoints that might have certificate issues
builder.Services.AddHttpClient("OpenAI")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // For development environments only - allows self-signed certificates
        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
    });

// Register our services
builder.Services.AddSingleton<TodoService>();
builder.Services.AddSingleton<ItsmService>();
builder.Services.AddSingleton<McpService>();
builder.Services.AddSingleton<NlpService>();

// Add controllers for API endpoints
builder.Services.AddControllers();

// Add CORS support
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Enable static file serving for our test client
app.UseStaticFiles();

app.UseRouting();

// Enable CORS
app.UseCors();

app.MapControllers();

// Redirect root to our test client
app.MapGet("/", context => {
    context.Response.Redirect("/index.html");
    return Task.CompletedTask;
});

await app.RunAsync();
