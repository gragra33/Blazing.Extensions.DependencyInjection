namespace WpfExample.Services;

/// <summary>
/// Represents weather data for demonstration purposes.
/// </summary>
public class WeatherData
{
    /// <summary>
    /// Gets or sets the location for which the weather data applies.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temperature value in Celsius.
    /// </summary>
    public double Temperature { get; set; }

    /// <summary>
    /// Gets or sets the weather condition (e.g., Sunny, Cloudy).
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the humidity percentage.
    /// </summary>
    public int Humidity { get; set; }

    /// <summary>
    /// Gets or sets the wind speed in kilometers per hour.
    /// </summary>
    public double WindSpeed { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the weather data was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
