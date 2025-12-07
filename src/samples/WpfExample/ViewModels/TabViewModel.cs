namespace WpfExample.ViewModels;

/// <summary>
/// Wrapper for tab information that can be safely bound in XAML without converters.
/// Contains all necessary data and provides immediate View resolution with automatic ViewModel DataContext assignment.
/// This class eliminates the need for complex XAML converters and handles the View-ViewModel pairing automatically.
/// </summary>
public class TabViewModel
{
    /// <summary>
    /// Gets the type of the view that this tab represents.
    /// </summary>
    public Type ViewType { get; }
    
    /// <summary>
    /// Gets the display text for the tab header.
    /// </summary>
    public string Header { get; }
    
    /// <summary>
    /// Gets the display order for the tab (used for sorting).
    /// </summary>
    public int Order { get; }
    
    private object _viewInstance = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="TabViewModel"/> class.
    /// </summary>
    /// <param name="viewType">The type of the view to resolve.</param>
    /// <param name="header">The tab header text.</param>
    /// <param name="order">The display order of the tab.</param>
    public TabViewModel(Type viewType, string header, int order)
    {
        ViewType = viewType;
        Header = header;
        Order = order;
        
        // Resolve view immediately - no lazy loading needed
        ResolveView();
    }

    /// <summary>
    /// Gets the actual View instance - resolved immediately during construction.
    /// The view is paired with its corresponding ViewModel using naming conventions.
    /// </summary>
    public object ViewInstance => _viewInstance;

    /// <summary>
    /// Resolves the actual View instance from DI immediately and automatically sets the correct ViewModel as DataContext.
    /// Called during construction to eliminate loading delays and ensure proper data binding.
    /// Uses naming conventions to match Views with ViewModels (e.g., HomeView → HomeViewModel).
    /// </summary>
    public void ResolveView()
    {
        if (_viewInstance != null) return; // Already resolved

        try
        {
            Console.WriteLine($@"TabViewModel: Attempting to resolve view: {ViewType.Name}");
            
            // Get the service provider from Application.Current
            var serviceProvider = Application.Current.GetServices();
            if (serviceProvider != null)
            {
                // Use standard Microsoft DI GetRequiredService with the Type parameter
                _viewInstance = serviceProvider.GetRequiredService(ViewType);
                Console.WriteLine($@"TabViewModel: Successfully resolved view: {ViewType.Name}");

                // Automatically set the DataContext to the corresponding ViewModel
                if (_viewInstance is not FrameworkElement view)
                {
                    return;
                }

                // Determine the ViewModel type based on the View type
                var viewModelTypeName = ViewType.Name.Replace("View", "ViewModel");
                var viewModelType = ViewType.Assembly.GetTypes()
                    .FirstOrDefault(t => t.Name == viewModelTypeName);
                        
                if (viewModelType != null)
                {
                    try
                    {
                        var viewModel = serviceProvider.GetRequiredService(viewModelType);
                        view.DataContext = viewModel;
                        Console.WriteLine($@"TabViewModel: Set {viewModelTypeName} as DataContext for {ViewType.Name}");
                    }
                    catch (Exception vmEx)
                    {
                        Console.WriteLine($@"TabViewModel: Failed to resolve {viewModelTypeName}: {vmEx.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($@"TabViewModel: Could not find ViewModel type {viewModelTypeName}");
                }
            }
            else
            {
                throw new InvalidOperationException("Service provider is not configured on Application.Current");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"TabViewModel: Failed to resolve view {ViewType.Name}: {ex.Message}");
            Console.WriteLine($@"TabViewModel: Stack trace: {ex.StackTrace}");

            _viewInstance = new TextBlock
            {
                Text = $@"Error loading {ViewType.Name}: {ex.Message}",
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.Red,
                Margin = new Thickness(20)
            };
        }
    }
}