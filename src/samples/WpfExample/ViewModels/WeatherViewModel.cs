using System;
using CommunityToolkit.Mvvm.Input;
using WpfExample.Services;

namespace WpfExample.ViewModels;

/// <summary>
/// WeatherViewModel demonstrates dependency injection of IWeatherService.
/// Shows how each tab ViewModel can have its own specific service dependencies
/// without MainViewModel needing to know about them.
/// </summary>
public partial class WeatherViewModel : ViewModelBase
{
    private readonly IWeatherService _weatherService;
    private string _currentWeather = "Click button to get weather...";
    private string _weatherForecast = "Click button to get forecast...";
    private bool _useCelsius = true; // Default to Celsius

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherViewModel"/> class.
    /// </summary>
    /// <param name="weatherService">Service for retrieving weather information.</param>
    /// <exception cref="ArgumentNullException">Thrown when weatherService is null.</exception>
    public WeatherViewModel(IWeatherService weatherService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
    }

    /// <summary>
    /// Gets or sets whether to use Celsius (true) or Fahrenheit (false) for temperature display.
    /// </summary>
    public bool UseCelsius
    {
        get => _useCelsius;
        set => SetProperty(ref _useCelsius, value);
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
    /// Gets or sets the weather forecast information displayed to the user.
    /// </summary>
    public string WeatherForecast
    {
        get => _weatherForecast;
        set => SetProperty(ref _weatherForecast, value);
    }

    /// <summary>
    /// Gets the current weather information from the weather service.
    /// Demonstrates service injection and usage within tab ViewModels.
    /// </summary>
    [RelayCommand]
    private void GetWeather()
    {
        Console.WriteLine("WeatherViewModel: GetWeather command executed!");
        CurrentWeather = _weatherService.GetCurrentWeather();
    }

    /// <summary>
    /// Gets the weather forecast information from the weather service.
    /// Demonstrates service injection and usage within tab ViewModels.
    /// </summary>
    [RelayCommand]
    private void GetForecast()
    {
        Console.WriteLine("WeatherViewModel: GetForecast command executed!");
        WeatherForecast = _weatherService.GetForecast();
    }
}
