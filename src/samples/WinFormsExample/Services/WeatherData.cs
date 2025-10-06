namespace WinFormsExample.Services;

/// <summary>
/// Represents weather data for demonstration purposes.
/// </summary>
public class WeatherData
{
    public string Location { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public string Condition { get; set; } = string.Empty;
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public DateTime LastUpdated { get; set; }
}