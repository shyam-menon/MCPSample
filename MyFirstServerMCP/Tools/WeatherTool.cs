using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using MyFirstServerMCP.Models;

namespace MyFirstServerMCP.Tools;

[McpServerToolType]
public static class WeatherTool
{
    private static readonly HttpClient _httpClient = new HttpClient();
    
    [McpServerTool, Description("Get current weather for a location")]
    public static async Task<WeatherData> GetWeather(string location)
    {
        try
        {
            // This is a mock example - in a real scenario, you would call an actual weather API
            // For example: OpenWeatherMap, WeatherAPI, etc.
            // You would need to register and get an API key for most services
            
            // For demonstration, we'll return mock data
            await Task.Delay(500); // Simulate API call delay
            
            var weatherData = new WeatherData
            {
                Location = location,
                Temperature = GetRandomTemperature(),
                Description = GetRandomWeatherDescription(),
                FeelsLike = GetRandomTemperature() - 2,
                Humidity = Random.Shared.NextDouble() * 100,
            };
            
            return weatherData;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get weather data: {ex.Message}");
        }
    }
    
    // Helper methods to generate random weather data for demonstration
    private static double GetRandomTemperature()
    {
        return Math.Round(10 + Random.Shared.NextDouble() * 25, 1); // 10-35°C
    }
    
    private static string GetRandomWeatherDescription()
    {
        string[] descriptions = { "Sunny", "Partly cloudy", "Cloudy", "Rainy", "Thunderstorm", "Clear", "Foggy" };
        return descriptions[Random.Shared.Next(descriptions.Length)];
    }
    
    [McpServerTool, Description("Get weather forecast for the next few days")]
    public static async Task<List<WeatherData>> GetWeatherForecast(string location, int days = 3)
    {
        try
        {
            // Limit the number of days to a reasonable range
            days = Math.Min(Math.Max(days, 1), 7);
            
            // Simulate API call delay
            await Task.Delay(500);
            
            // Generate mock forecast data
            var forecast = new List<WeatherData>();
            for (int i = 0; i < days; i++)
            {
                forecast.Add(new WeatherData
                {
                    Location = location,
                    Temperature = GetRandomTemperature(),
                    Description = GetRandomWeatherDescription(),
                    FeelsLike = GetRandomTemperature() - 2,
                    Humidity = Random.Shared.NextDouble() * 100,
                    Timestamp = DateTime.Now.AddDays(i)
                });
            }
            
            return forecast;
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to get weather forecast: {ex.Message}");
        }
    }
}
