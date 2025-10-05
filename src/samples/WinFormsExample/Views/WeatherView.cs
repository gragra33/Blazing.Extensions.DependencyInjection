using System;
using System.Linq;
using System.Threading.Tasks;
using Blazing.ToggleSwitch.WinForms;

namespace WinFormsExample.Views;

/// <summary>
/// WeatherView demonstrates async service operations and data display.
/// Features the Blazing.ToggleSwitch.WinForms control for temperature unit selection.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ITabView))]
public partial class WeatherView : UserControl, ITabView
{
    private readonly IWeatherService _weatherService;
    private readonly IDialogService _dialogService;
    private WeatherData? _currentWeatherData;
    private bool _useFahrenheit = false;

    /// <summary>
    /// The header text displayed in the tab.
    /// </summary>
    public string TabHeader => "🌤️ Weather";

    /// <summary>
    /// The display order of the tab (lower numbers appear first).
    /// </summary>
    public int Order => 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherView"/> class.
    /// </summary>
    public WeatherView(IWeatherService weatherService, IDialogService dialogService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        InitializeComponent();
        LoadLocations();
        SetupEventHandlers();
        Console.WriteLine("WeatherView: Constructor called - Weather and dialog services injected");
    }

    private void LoadLocations()
    {
        Console.WriteLine("WeatherView: LoadLocations called");
        if (locationComboBox == null) return;

        var locations = _weatherService.GetAvailableLocations().ToList();
        locationComboBox.Items.AddRange(locations.ToArray());
        Console.WriteLine($"WeatherView: Loaded {locations.Count} available locations");
        
        if (locations.Count > 0)
        {
            locationComboBox.SelectedIndex = 0;
            Console.WriteLine($"WeatherView: Default location set to {locations[0]}");
        }
    }

    private void SetupEventHandlers()
    {
        Console.WriteLine("WeatherView: Setting up event handlers");
        if (fetchButton != null)
            fetchButton.Click += FetchButton_Click;

        if (temperatureUnitToggle != null)
            temperatureUnitToggle.CheckedChanged += TemperatureUnitToggle_CheckedChanged;
    }

    private async void FetchButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("WeatherView: FetchButton_Click executed!");
        if (locationComboBox?.SelectedItem == null)
        {
            Console.WriteLine("WeatherView: No location selected");
            _dialogService.ShowMessage("Error", "Please select a location first.");
            return;
        }

        var location = locationComboBox.SelectedItem.ToString();
        if (string.IsNullOrEmpty(location)) return;

        Console.WriteLine($"WeatherView: Fetching weather for {location}");
        try
        {
            // Disable button during fetch
            if (fetchButton != null)
            {
                fetchButton.Enabled = false;
                fetchButton.Text = "Loading...";
                Console.WriteLine("WeatherView: Fetch button disabled during operation");
            }

            _currentWeatherData = await _weatherService.GetWeatherAsync(location);
            DisplayWeatherData(_currentWeatherData);
            Console.WriteLine("WeatherView: Weather data fetched and displayed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WeatherView: Error fetching weather - {ex.Message}");
            _dialogService.ShowMessage("Error", $"Failed to fetch weather data: {ex.Message}");
        }
        finally
        {
            // Re-enable button
            if (fetchButton != null)
            {
                fetchButton.Enabled = true;
                fetchButton.Text = "Get Weather";
                Console.WriteLine("WeatherView: Fetch button re-enabled");
            }
        }
    }

    private void TemperatureUnitToggle_CheckedChanged(object? sender, EventArgs e)
    {
        Console.WriteLine("WeatherView: TemperatureUnitToggle_CheckedChanged executed!");
        var toggle = sender as ToggleSwitch;
        _useFahrenheit = toggle?.Checked == true;
        Console.WriteLine($"WeatherView: Temperature unit changed to {(_useFahrenheit ? "Fahrenheit" : "Celsius")}");
        
        // Refresh the temperature display if we have data
        if (_currentWeatherData != null)
        {
            Console.WriteLine("WeatherView: Refreshing display with new temperature unit");
            DisplayWeatherData(_currentWeatherData);
        }
    }

    private void DisplayWeatherData(WeatherData weather)
    {
        Console.WriteLine("WeatherView: DisplayWeatherData called");
        // Convert temperature if needed
        var temperature = _useFahrenheit ? (weather.Temperature * 9.0 / 5.0) + 32 : weather.Temperature;
        var unit = _useFahrenheit ? "°F" : "°C";

        if (temperatureLabel != null)
            temperatureLabel.Text = $"Temperature: {temperature:F1}{unit}";
        
        if (conditionLabel != null)
            conditionLabel.Text = $"Condition: {weather.Condition}";
        
        if (humidityLabel != null)
            humidityLabel.Text = $"Humidity: {weather.Humidity}%";
        
        if (windSpeedLabel != null)
            windSpeedLabel.Text = $"Wind Speed: {weather.WindSpeed:F1} km/h";
        
        if (lastUpdatedLabel != null)
            lastUpdatedLabel.Text = $"Last Updated: {weather.LastUpdated:yyyy-MM-dd HH:mm:ss}";

        if (weatherInfoPanel != null)
        {
            weatherInfoPanel.Visible = true;
            Console.WriteLine("WeatherView: Weather information panel made visible");
        }
    }
}