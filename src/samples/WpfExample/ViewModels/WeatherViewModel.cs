namespace WpfExample.ViewModels;

/// <summary>
/// WeatherViewModel demonstrates dependency injection of IWeatherService with async operations.
/// Shows how each tab ViewModel can have its own specific service dependencies
/// without MainViewModel needing to know about them.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public partial class WeatherViewModel : ViewModelBase
{
    private readonly IWeatherService _weatherService;
    private WeatherData? _currentWeatherData;
    private ObservableCollection<string> _availableLocations = [];
    private string _selectedLocation = string.Empty;
    private string _currentWeather = "Select a location and click Get Weather...";
    private bool _isLoading;
    private bool _useFahrenheit;

    /// <summary>
    /// Gets or sets the list of available weather locations.
    /// </summary>
    public ObservableCollection<string> AvailableLocations
    {
        get => _availableLocations;
        set => SetProperty(ref _availableLocations, value);
    }

    /// <summary>
    /// Gets or sets the currently selected weather location.
    /// </summary>
    public string SelectedLocation
    {
        get => _selectedLocation;
        set => SetProperty(ref _selectedLocation, value);
    }

    /// <summary>
    /// Gets or sets the current weather information displayed to the user.
    /// </summary>
    public string CurrentWeather
    {
        get => _currentWeather;
        set => SetProperty(ref _currentWeather, value);
    }

    /// <summary>
    /// Gets or sets whether weather data is currently being loaded.
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    /// <summary>
    /// Gets or sets whether to display temperature in Fahrenheit (true) or Celsius (false).
    /// </summary>
    public bool UseFahrenheit
    {
        get => _useFahrenheit;
        set
        {
            if (SetProperty(ref _useFahrenheit, value))
            {
                // Refresh display when temperature unit changes
                if (_currentWeatherData != null)
                {
                    DisplayWeatherData();
                }
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherViewModel"/> class.
    /// </summary>
    /// <param name="weatherService">Service for retrieving weather information.</param>
    /// <exception cref="ArgumentNullException">Thrown when weatherService is null.</exception>
    public WeatherViewModel(IWeatherService weatherService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        LoadAvailableLocations();
    }

    /// <summary>
    /// Loads available weather locations from the service.
    /// </summary>
    private void LoadAvailableLocations()
    {
        Console.WriteLine("WeatherViewModel: LoadAvailableLocations called");
        var locations = _weatherService.GetAvailableLocations().ToList();

        foreach (var location in locations)
        {
            AvailableLocations.Add(location);
        }

        if (locations.Count > 0)
        {
            SelectedLocation = locations[0];
            Console.WriteLine($"WeatherViewModel: Default location set to {locations[0]}");
        }
    }

    /// <summary>
    /// Gets weather data for the selected location asynchronously.
    /// Demonstrates async service operations and data display with temperature unit conversion.
    /// </summary>
    [RelayCommand]
    private async Task GetWeather()
    {
        Console.WriteLine("WeatherViewModel: GetWeather command executed!");

        if (string.IsNullOrEmpty(SelectedLocation))
        {
            Console.WriteLine("WeatherViewModel: No location selected");
            CurrentWeather = "Please select a location first.";
            return;
        }

        IsLoading = true;
        try
        {
            Console.WriteLine($"WeatherViewModel: Fetching weather for {SelectedLocation}");
            _currentWeatherData = await _weatherService.GetWeatherAsync(SelectedLocation);
            DisplayWeatherData();
            Console.WriteLine("WeatherViewModel: Weather data fetched and displayed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeatherViewModel: Error fetching weather - {ex.Message}");
            CurrentWeather = $"Error fetching weather: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Displays weather data with temperature unit conversion.
    /// </summary>
    private void DisplayWeatherData()
    {
        if (_currentWeatherData == null) return;

        Console.WriteLine("WeatherViewModel: DisplayWeatherData called");

        // Convert temperature if needed
        var temperature = UseFahrenheit
            ? (_currentWeatherData.Temperature * 9.0 / 5.0) + 32
            : _currentWeatherData.Temperature;
        var unit = UseFahrenheit ? "°F" : "°C";

        CurrentWeather = $"Location: {_currentWeatherData.Location}\n" +
                        $"Temperature: {temperature:F1}{unit}\n" +
                        $"Condition: {_currentWeatherData.Condition}\n" +
                        $"Humidity: {_currentWeatherData.Humidity}%\n" +
                        $"Wind Speed: {_currentWeatherData.WindSpeed:F1} km/h\n" +
                        $"Last Updated: {_currentWeatherData.LastUpdated:yyyy-MM-dd HH:mm:ss}";
    }
}
