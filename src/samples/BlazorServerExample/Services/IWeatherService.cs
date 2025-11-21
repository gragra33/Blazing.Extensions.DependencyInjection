namespace BlazorServerExample.Services;

/// <summary>
/// Weather service interface for retrieving weather information.
/// </summary>
public interface IWeatherService
{
    /// <summary>
    /// Gets weather data for a specified location asynchronously.
    /// </summary>
    /// <param name="location">The location to get weather for.</param>
    /// <returns>Weather data for the location.</returns>
    Task<WeatherData> GetWeatherAsync(string location);

    /// <summary>
    /// Gets the list of available weather locations.
    /// </summary>
    /// <returns>Collection of available locations.</returns>
    IEnumerable<string> GetAvailableLocations();
}
