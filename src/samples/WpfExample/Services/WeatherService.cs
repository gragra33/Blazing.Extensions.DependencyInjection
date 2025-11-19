namespace WpfExample.Services;

/// <summary>
/// Implementation of IWeatherService with mock data.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IWeatherService))]
public class WeatherService : IWeatherService
{
    private readonly Random _random = new Random();
    private readonly string[] _conditions = ["Sunny", "Cloudy", "Rainy", "Snowy", "Partly Cloudy"];
    private readonly string[] _locations = ["Sydney", "New York", "London", "Tokyo", "Paris", "Berlin", "Toronto"];

    public WeatherService()
    {
        Console.WriteLine(@"WeatherService: Constructor called - Service registered as transient");
    }

    public async Task<WeatherData> GetWeatherAsync(string location)
    {
        Console.WriteLine($@"WeatherService: GetWeatherAsync called for location: {location}");
        
        // Simulate API call delay
        Console.WriteLine(@"WeatherService: Simulating API call delay (500ms)...");
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

        Console.WriteLine($@"WeatherService: Generated weather data for {location}:");
        Console.WriteLine($@"  - Temperature: {weatherData.Temperature}°C");
        Console.WriteLine($@"  - Condition: {weatherData.Condition}");
        Console.WriteLine($@"  - Humidity: {weatherData.Humidity}%");
        Console.WriteLine($@"  - Wind Speed: {weatherData.WindSpeed:F1} km/h");

        return weatherData;
    }

    public IEnumerable<string> GetAvailableLocations()
    {
        Console.WriteLine(@"WeatherService: GetAvailableLocations called");
        Console.WriteLine($@"WeatherService: Returning {_locations.Length} available locations: {string.Join(", ", _locations)}");
        return _locations;
    }
}
