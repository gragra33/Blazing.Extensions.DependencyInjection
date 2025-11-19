namespace WpfExample.Views;

/// <summary>
/// SettingsView demonstrates View-First pattern where the View resolves its own ViewModel.
/// Implements ITabView for automatic discovery by TabViewHandler.
/// Shows complete decoupling - MainViewModel has no knowledge of this View.
/// </summary>
public partial class SettingsView : UserControl, ITabView
{
    /// <inheritdoc/>
    public string TabHeader => "⚙️ Settings";
    
    /// <inheritdoc/>
    public int Order => 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsView"/> class.
    /// Delays ViewModel resolution until Loaded event to ensure DI is configured.
    /// </summary>
    public SettingsView()
    {
        InitializeComponent();
        
        // Delay ViewModel resolution until Loaded event to ensure DI is configured
        Loaded += OnLoaded;
    }

    /// <summary>
    /// Handles the Loaded event to resolve the SettingsViewModel via dependency injection.
    /// Demonstrates the View-First pattern where each View resolves its own ViewModel.
    /// SettingsViewModel is injected with all required services automatically.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // VIEW-FIRST PATTERN: Each View resolves its own ViewModel via DI
        // SettingsViewModel is injected with all required services automatically!
        //if (DataContext == null)
        //{
        //    DataContext = Application.Current.GetRequiredService<SettingsViewModel>();
        //}
    }
}
