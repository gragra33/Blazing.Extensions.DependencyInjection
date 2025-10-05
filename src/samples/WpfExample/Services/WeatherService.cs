namespace WpfExample.Services;

/// <summary>
/// Implementation of IWeatherService that generates simulated weather data.
/// Demonstrates service implementation with random data generation.
/// In a real application, this would connect to a weather API.
/// </summary>
public class WeatherService : IWeatherService
{
    /// <summary>
    /// Random number generator for simulating weather variability.
    /// </summary>
    private readonly Random _random = new();
    
    /// <summary>
    /// Available weather conditions for simulation.
    /// </summary>
    private readonly string[] _conditions = { "Sunny", "Cloudy", "Rainy", "Snowy", "Windy", "Partly Cloudy" };

    /// <inheritdoc/>
    public string GetCurrentWeather()
    {
        var condition = _conditions[_random.Next(_conditions.Length)];
        var temp = _random.Next(0, 35); // Celsius range (0-35°C)
        return $"{condition}, {temp}°C";
    }

    /// <inheritdoc/>
    public string GetForecast()
    {
        return $"Next 5 days: {string.Join(", ", _conditions.OrderBy(x => _random.Next()))}";
    }
}
