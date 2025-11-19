namespace WinFormsExample.Views;

/// <summary>
/// HomeView demonstrates a welcome screen with basic information about the application.
/// Shows complete decoupling - resolved via dependency injection.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ITabView))]
public partial class HomeView : UserControl, ITabView
{
    private readonly IDialogService _dialogService;

    /// <summary>
    /// The header text displayed in the tab.
    /// </summary>
    public string TabHeader => "🏠 Home";

    /// <summary>
    /// The display order of the tab (lower numbers appear first).
    /// </summary>
    public int Order => 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeView"/> class.
    /// </summary>
    public HomeView(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        InitializeComponent();
        SetupEventHandlers();
        UpdateWelcomeMessage();
        Console.WriteLine(@"HomeView: Constructor called - View resolved via dependency injection");
    }

    private void SetupEventHandlers()
    {
        Console.WriteLine(@"HomeView: Setting up event handlers");
        if (refreshButton != null)
            refreshButton.Click += RefreshButton_Click;
            
        if (showInfoButton != null)
            showInfoButton.Click += ShowInfoButton_Click;
    }

    private void RefreshButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine(@"HomeView: RefreshButton_Click executed!");
        UpdateWelcomeMessage();
        _dialogService.ShowMessage("Welcome message refreshed!", "Refresh Complete");
    }

    private void ShowInfoButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine(@"HomeView: ShowInfoButton_Click executed!");
        _dialogService.ShowMessage("This HomeView was resolved via Dependency Injection!\n\n" +
            "The IDialogService was automatically injected into the constructor.",
            "Dependency Injection Info");
    }

    private void UpdateWelcomeMessage()
    {
        Console.WriteLine(@"HomeView: UpdateWelcomeMessage called");
        if (welcomeLabel != null)
        {
            welcomeLabel.Text = @"Welcome to the Blazing.Extensions.DependencyInjection Demo!";
        }
        
        if (lastRefreshedLabel != null)
        {
            lastRefreshedLabel.Text = $@"Last refreshed: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
    }
}