namespace WpfExample.ViewModels;

/// <summary>
/// HomeViewModel for the welcome/home tab.
/// Demonstrates View-First pattern - this ViewModel is resolved by HomeView automatically via TabViewModel, not MainViewModel.
/// This achieves complete decoupling where the MainViewModel has no knowledge of specific child ViewModels.
/// </summary>
public partial class HomeViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;

    /// <summary>
    /// Gets or sets the welcome message displayed to the user.
    /// </summary>
    [ObservableProperty]
    private string _welcomeMessage = "Welcome to the Blazing.Extensions.DependencyInjection Demo!";

    /// <summary>
    /// Gets or sets the timestamp of the last refresh operation.
    /// </summary>
    [ObservableProperty]
    private string _lastRefreshed = "Not yet refreshed";

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeViewModel"/> class.
    /// </summary>
    /// <param name="dialogService">Service for displaying user dialogs.</param>
    /// <exception cref="ArgumentNullException">Thrown when dialogService is null.</exception>
    public HomeViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
    }

    /// <summary>
    /// Refreshes the welcome message and updates the last refreshed timestamp.
    /// Demonstrates independent tab state management.
    /// </summary>
    [RelayCommand]
    private void RefreshWelcome()
    {
        Console.WriteLine("HomeViewModel: RefreshWelcome command executed!");
        WelcomeMessage = $"Page refreshed! Each tab operates independently with its own ViewModel.";
        LastRefreshed = $"Last refreshed: {DateTime.Now:HH:mm:ss}";
    }

    /// <summary>
    /// Shows information about the View-First pattern implementation.
    /// Demonstrates how automatic ViewModel resolution works through TabViewModel.
    /// </summary>
    [RelayCommand]
    private void ShowInfo()
    {
        Console.WriteLine("HomeViewModel: ShowInfo command executed!");
        _dialogService.ShowMessage(
            "View-First Pattern",
            "This HomeView resolved its own HomeViewModel via:\n\n" +
            "Application.Current.GetRequiredService<HomeViewModel>()\n\n" +
            "MainViewModel has no knowledge of HomeViewModel - perfect decoupling!");
    }
}
