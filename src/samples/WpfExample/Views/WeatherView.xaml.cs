namespace WpfExample.Views;

/// <summary>
/// WeatherView demonstrates async service operations and data display.
/// Uses View-First pattern with injected services for weather data retrieval.
/// Shows complete decoupling - MainViewModel has no knowledge of this View.
/// </summary>
public partial class WeatherView : UserControl, ITabView
{
    private readonly IWeatherService _weatherService;
    private readonly IDialogService _dialogService;

    /// <inheritdoc/>
    public string TabHeader => "🌤️ Weather";
    
    /// <inheritdoc/>
    public int Order => 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherView"/> class.
    /// Resolves WeatherViewModel via dependency injection and sets it as DataContext.
    /// </summary>
    public WeatherView(IWeatherService weatherService, IDialogService dialogService)
    {
        _weatherService = weatherService ?? throw new ArgumentNullException(nameof(weatherService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        InitializeComponent();
        
        // VIEW-FIRST PATTERN: Resolve WeatherViewModel via DI
        DataContext = Application.Current.GetRequiredService<WeatherViewModel>();
        
        Console.WriteLine(@"WeatherView: Constructor called - WeatherViewModel resolved via DI");
    }
}
