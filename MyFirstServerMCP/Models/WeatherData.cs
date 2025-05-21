namespace MyFirstServerMCP.Models;

public class WeatherData
{
    public string? Location { get; set; }
    public double Temperature { get; set; }
    public string? Description { get; set; }
    public double FeelsLike { get; set; }
    public double Humidity { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
