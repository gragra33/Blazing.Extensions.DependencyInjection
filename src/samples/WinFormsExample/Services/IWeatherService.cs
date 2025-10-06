using System.Threading.Tasks;

namespace WinFormsExample.Services;

/// <summary>
/// Interface for weather service operations.
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