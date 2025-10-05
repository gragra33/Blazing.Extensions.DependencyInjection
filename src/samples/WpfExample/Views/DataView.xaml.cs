namespace WpfExample.Views;

/// <summary>
/// DataView demonstrates View-First pattern where the View resolves its own ViewModel.
/// Implements ITabView for automatic discovery by TabViewHandler.
/// Shows complete decoupling - MainViewModel has no knowledge of this View.
/// </summary>
public partial class DataView : UserControl, ITabView
{
    /// <inheritdoc/>
    public string TabHeader => "📊 Data";
    
    /// <inheritdoc/>
    public int Order => 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataView"/> class.
    /// Delays ViewModel resolution until Loaded event to ensure DI is configured.
    /// </summary>
    public DataView()
    {
        InitializeComponent();
        
        // Delay ViewModel resolution until Loaded event to ensure DI is configured
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Handles the Loaded event to resolve the DataViewModel via dependency injection.
    /// Demonstrates the View-First pattern where each View resolves its own ViewModel.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // VIEW-FIRST PATTERN: Each View resolves its own ViewModel via DI
        // DataViewModel is injected with IDataService automatically!
        if (DataContext == null)
        {
            DataContext = Application.Current.GetRequiredService<DataViewModel>();
        }
    }
}
