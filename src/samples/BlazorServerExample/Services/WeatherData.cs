namespace BlazorServerExample.Services;

/// <summary>
/// Weather data class containing weather information for a location.
/// </summary>
public class WeatherData
{
    /// <summary>
    /// Gets or sets the location name.
    /// </summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the temperature in Celsius.
    /// </summary>
    public int Temperature { get; set; }

    /// <summary>
    /// Gets or sets the weather condition.
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the humidity percentage.
    /// </summary>
    public int Humidity { get; set; }

    /// <summary>
    /// Gets or sets the wind speed in km/h.
    /// </summary>
    public double WindSpeed { get; set; }

    /// <summary>
    /// Gets or sets the last update timestamp.
    /// </summary>
    public DateTime LastUpdated { get; set; }
}
