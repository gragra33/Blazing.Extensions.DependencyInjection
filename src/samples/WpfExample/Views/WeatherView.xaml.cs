namespace WpfExample.Views;

/// <summary>
/// WeatherView demonstrates View-First pattern where TabViewModel automatically sets the correct ViewModel as DataContext.
/// Implements ITabView for automatic discovery by TabViewHandler.
/// Shows complete decoupling - MainViewModel has no knowledge of this View.
/// </summary>
public partial class WeatherView : UserControl, ITabView
{
    /// <inheritdoc/>
    public string TabHeader => "🌤️ Weather";
    
    /// <inheritdoc/>
    public int Order => 2;

    /// <summary>
    /// Initializes a new instance of the <see cref="WeatherView"/> class.
    /// TabViewModel will automatically resolve and assign the correct WeatherViewModel as DataContext.
    /// </summary>
    public WeatherView()
    {
        InitializeComponent();
        Console.WriteLine("WeatherView: Constructor called - TabViewModel will set DataContext");
    }
}
