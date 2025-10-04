namespace WpfExample.Services;

/// <summary>
/// Service interface for retrieving weather information.
/// Demonstrates service abstraction for external data sources.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets the current weather conditions.
    /// </summary>
    /// <returns>A string describing current weather conditions and temperature in Celsius.</returns>
    string GetCurrentWeather();
    
    /// <summary>
    /// Gets a weather forecast for upcoming days.
    /// </summary>
    /// <returns>A string containing forecast information in Celsius.</returns>
    string GetForecast();
}