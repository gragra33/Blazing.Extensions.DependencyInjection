namespace WpfExample;

/// <summary>
/// Main window for WPF MVVM tab-based example with dependency injection.
/// Demonstrates TabViewHandler pattern with complete decoupling between MainViewModel and tab ViewModels.
/// All ViewModels are injected via constructor by Blazing.Extensions.DependencyInjection.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="mainViewModel">The main view model injected via dependency injection.</param>
    public MainWindow(MainViewModel mainViewModel)
    {
        InitializeComponent();
        DataContext = mainViewModel;
        
        // Ensure the first tab is selected when the window loads
        Loaded += MainWindow_Loaded;
    }

    /// <summary>
    /// Handles the Loaded event to ensure the first tab is selected by default.
    /// Provides better user experience by showing content immediately.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Find the TabControl and select the first tab
        var tabControl = FindName("MainTabControl") as TabControl;
        if (tabControl?.Items.Count > 0)
        {
            tabControl.SelectedIndex = 0;
        }
    }
}