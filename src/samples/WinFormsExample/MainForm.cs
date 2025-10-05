namespace WinFormsExample;

/// <summary>
/// Main form for WinForms tab-based example with dependency injection.
/// Demonstrates dynamic tab discovery with complete separation of concerns.
/// Tabs are discovered automatically via ITabView interface and dependency injection.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public partial class MainForm : Form
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly ITabViewHandler _tabViewHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainForm"/> class.
    /// </summary>
    public MainForm(IServiceProvider serviceProvider, IDialogService dialogService, INavigationService navigationService, ITabViewHandler tabViewHandler)
    {
        Console.WriteLine("MainForm: Constructor called - Injecting dependencies");
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _tabViewHandler = tabViewHandler ?? throw new ArgumentNullException(nameof(tabViewHandler));
        
        Console.WriteLine("MainForm: Dependencies injected successfully");
        Console.WriteLine("MainForm: Initializing components");
        InitializeComponent();
        
        Console.WriteLine("MainForm: Setting up dynamic tabs");
        SetupDynamicTabs();
        
        // Subscribe to navigation service
        Console.WriteLine("MainForm: Subscribing to navigation service events");
        _navigationService.NavigationChanged += OnNavigationChanged;
        Console.WriteLine("MainForm: Constructor completed successfully");
    }

    private void SetupDynamicTabs()
    {
        Console.WriteLine("MainForm: SetupDynamicTabs called");
        if (mainTabControl == null) 
        {
            Console.WriteLine("MainForm: Warning - mainTabControl is null");
            return;
        }

        try
        {
            // Get all tab metadata from the TabViewHandler (automatically discovers all ITabView implementations)
            Console.WriteLine("MainForm: Discovering tab views via TabViewHandler");
            var tabMetadata = _tabViewHandler.GetTabMetadata().ToList();
            
            Console.WriteLine($"MainForm: Discovered {tabMetadata.Count} tab views");
            foreach (var tab in tabMetadata)
            {
                Console.WriteLine($"  - {tab.TabHeader} (Order: {tab.Order}, Type: {tab.ViewType.Name})");
            }

            // Create tabs dynamically for each discovered view
            foreach (var tabInfo in tabMetadata)
            {
                AddDynamicTab(tabInfo);
            }

            // Select the first tab
            if (mainTabControl.TabCount > 0)
            {
                mainTabControl.SelectedIndex = 0;
                Console.WriteLine($"MainForm: Default tab selected (index 0)");
            }

            // Handle tab selection changes
            mainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
            
            Console.WriteLine($"MainForm: Dynamic tab setup complete. Total tabs: {mainTabControl.TabCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MainForm: Error setting up dynamic tabs: {ex.Message}");
            _dialogService.ShowMessage("Error", $"Failed to setup tabs: {ex.Message}");
        }
    }

    private void AddDynamicTab(TabInfo tabInfo)
    {
        if (mainTabControl == null) return;

        try
        {
            Console.WriteLine($"MainForm: Adding dynamic tab - {tabInfo.TabHeader}");
            // Resolve the view from DI container
            var view = _serviceProvider.GetRequiredService(tabInfo.ViewType) as UserControl;
            if (view == null)
            {
                Console.WriteLine($"MainForm: Failed to resolve view: {tabInfo.ViewType.Name}");
                return;
            }

            Console.WriteLine($"MainForm: View {tabInfo.ViewType.Name} resolved successfully");

            // Create tab page
            var tabPage = new TabPage(tabInfo.TabHeader)
            {
                Name = $"Tab{tabInfo.ViewType.Name}",
                UseVisualStyleBackColor = true
            };

            // Set up the view
            view.Dock = DockStyle.Fill;
            tabPage.Controls.Add(view);

            // Add to tab control
            mainTabControl.TabPages.Add(tabPage);

            Console.WriteLine($"MainForm: ? Added dynamic tab: {tabInfo.TabHeader} (Order: {tabInfo.Order}) with view: {tabInfo.ViewType.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MainForm: Error adding dynamic tab {tabInfo.TabHeader}: {ex.Message}");
            _dialogService.ShowMessage("Error", $"Failed to load {tabInfo.TabHeader} tab: {ex.Message}");
        }
    }

    private void MainTabControl_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (mainTabControl != null)
        {
            var selectedTab = mainTabControl.SelectedTab;
            Console.WriteLine($"MainForm: Tab selection changed to index {mainTabControl.SelectedIndex}" + 
                            (selectedTab != null ? $" ({selectedTab.Text})" : ""));
            _navigationService.NavigateTo(mainTabControl.SelectedIndex);
        }
    }

    private void OnNavigationChanged(object? sender, int tabIndex)
    {
        Console.WriteLine($"MainForm: Navigation changed event received - navigating to tab index {tabIndex}");
        if (mainTabControl != null && tabIndex >= 0 && tabIndex < mainTabControl.TabCount)
        {
            mainTabControl.SelectedIndex = tabIndex;
            Console.WriteLine($"MainForm: Navigation completed successfully");
        }
        else
        {
            Console.WriteLine($"MainForm: Invalid tab index {tabIndex} - navigation ignored");
        }
    }

    protected override void Dispose(bool disposing)
    {
        Console.WriteLine("MainForm: Dispose called");
        if (disposing)
        {
            if (_navigationService != null)
            {
                Console.WriteLine("MainForm: Unsubscribing from navigation service events");
                _navigationService.NavigationChanged -= OnNavigationChanged;
            }
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
        Console.WriteLine("MainForm: Dispose completed");
    }
}