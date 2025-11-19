namespace WpfExample.Services;

/// <summary>
/// Service interface for retrieving weather information.
/// Demonstrates service abstraction for external data sources.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets weather data for the specified location asynchronously.
    /// </summary>
    Task<WeatherData> GetWeatherAsync(string location);
    
    /// <summary>
    /// Gets available locations for weather data.
    /// </summary>
    IEnumerable<string> GetAvailableLocations();
}