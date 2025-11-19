namespace WpfExample.ViewModels;

/// <summary>
/// SettingsViewModel demonstrates complex dependency injection with multiple services.
/// Shows how tab ViewModels can depend on multiple services (IDataService, IDialogService)
/// and manage application settings independently of other tabs.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    private readonly IDialogService _dialogService;
    private string _appVersion = "1.0.0";
    private int _totalDataItems;
    private string _userName = "User";

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// Demonstrates multiple service injection and automatic loading of settings.
    /// </summary>
    /// <param name="dataService">Service for accessing application data.</param>
    /// <param name="dialogService">Service for displaying user dialogs.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public SettingsViewModel(IDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        LoadSettings();
    }

    /// <summary>
    /// Gets or sets the application version information.
    /// </summary>
    public string AppVersion
    {
        get => _appVersion;
        set => SetProperty(ref _appVersion, value);
    }

    /// <summary>
    /// Gets or sets the total number of data items from the data service.
    /// Demonstrates cross-service data access.
    /// </summary>
    public int TotalDataItems
    {
        get => _totalDataItems;
        set => SetProperty(ref _totalDataItems, value);
    }

    /// <summary>
    /// Gets or sets the user name for personalization.
    /// </summary>
    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    /// <summary>
    /// Gets a personalized display message for the current user.
    /// Demonstrates computed properties in ViewModels.
    /// </summary>
    public string UserNameDisplay => $"Hello, {UserName}!";

    /// <summary>
    /// Saves the current settings and shows confirmation to the user.
    /// Demonstrates command execution with dialog service integration.
    /// </summary>
    [RelayCommand]
    private void SaveSettings()
    {
        Console.WriteLine($@"SettingsViewModel: SaveSettings command executed for user: {UserName}");
        _dialogService.ShowMessage("Settings", $"Settings saved for {UserName}!");
    }

    /// <summary>
    /// Shows detailed service information to demonstrate dependency injection status.
    /// Displays data from multiple injected services.
    /// </summary>
    [RelayCommand]
    private void ShowServiceInfo()
    {
        Console.WriteLine(@"SettingsViewModel: ShowServiceInfo command executed");
        var info = $"Data Service Items: {TotalDataItems}\nApp Version: {AppVersion}\nDI Status: ✅ Working";
        _dialogService.ShowMessage("Service Information", info);
    }

    /// <summary>
    /// Loads settings data from the data service during initialization.
    /// Demonstrates service usage in ViewModel initialization.
    /// </summary>
    private void LoadSettings()
    {
        TotalDataItems = _dataService.GetDataCount();
    }
}
