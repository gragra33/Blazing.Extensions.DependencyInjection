using Blazing.Extensions.DependencyInjection;

namespace BlazorServerExample.Services;

/// <summary>
/// Implementation of IWeatherService with mock data.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IWeatherService))]
public class WeatherService : IWeatherService
{
    private readonly Random _random = new Random();
    private readonly string[] _conditions = ["Sunny", "Cloudy", "Rainy", "Snowy", "Partly Cloudy"];
    private readonly string[] _locations = ["Sydney", "New York", "London", "Tokyo", "Paris", "Berlin", "Toronto"];

    /// <inheritdoc/>
    public async Task<WeatherData> GetWeatherAsync(string location)
    {
        // Simulate API call delay
        await Task.Delay(500);

        var weatherData = new WeatherData
        {
            Location = location,
            Temperature = _random.Next(-10, 35),
            Condition = _conditions[_random.Next(_conditions.Length)],
            Humidity = _random.Next(30, 90),
            WindSpeed = _random.NextDouble() * 20,
            LastUpdated = DateTime.Now
        };

        return weatherData;
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetAvailableLocations()
    {
        return _locations;
    }
}
