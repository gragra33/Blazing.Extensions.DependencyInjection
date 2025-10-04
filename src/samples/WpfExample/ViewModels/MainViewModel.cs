using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfExample.Services;

namespace WpfExample.ViewModels;

/// <summary>
/// MainViewModel demonstrates complete decoupling from child ViewModels.
/// Uses TabViewHandler service to discover available tabs without knowing specific View types.
/// This is the ultimate loose coupling solution!
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    private readonly IDialogService _dialogService;
    private readonly ITabViewHandler _tabViewHandler;
    private IEnumerable<TabViewModel> _tabViewModels;

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private string _welcomeMessage = "Welcome! This app demonstrates View-First MVVM with complete decoupling.";

    /// <summary>
    /// Available tab ViewModels - discovered automatically via TabViewHandler.
    /// MainViewModel has NO knowledge of specific View types!
    /// These are safe to bind in XAML without converters.
    /// Lazy-loaded to avoid resolution during construction.
    /// </summary>
    public IEnumerable<TabViewModel> TabViewModels 
    { 
        get 
        {
            if (_tabViewModels == null)
            {
                Console.WriteLine("MainViewModel: Resolving TabViewModels...");
                _tabViewModels = _tabViewHandler.GetTabViewModels();
                Console.WriteLine("MainViewModel: TabViewModels resolved successfully");
            }
            return _tabViewModels;
        }
    }

    /// <summary>
    /// Available tab view types - discovered automatically via TabViewHandler.
    /// MainViewModel has NO knowledge of specific View types!
    /// </summary>
    public IEnumerable<Type> TabViewTypes => _tabViewHandler.GetAvailableTabViewTypes();

    /// <summary>
    /// Initializes a new instance of the <see cref="MainViewModel"/> class.
    /// MainViewModel only knows about services it directly needs.
    /// Tab discovery is handled by ITabViewHandler - perfect separation of concerns!
    /// </summary>
    /// <param name="dialogService">Service for displaying user dialogs.</param>
    /// <param name="tabViewHandler">Service for discovering and managing tab views automatically.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    public MainViewModel(IDialogService dialogService, ITabViewHandler tabViewHandler)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _tabViewHandler = tabViewHandler ?? throw new ArgumentNullException(nameof(tabViewHandler));
        
        UpdateStatus("Application started - tabs discovered automatically via TabViewHandler!");
    }

    /// <summary>
    /// Refreshes the welcome message with current timestamp to demonstrate independent tab state management.
    /// </summary>
    [RelayCommand]
    private void RefreshWelcome()
    {
        WelcomeMessage = $"Refreshed at {DateTime.Now:HH:mm:ss} - Each tab manages its own state independently!";
        UpdateStatus("Welcome message refreshed");
    }

    /// <summary>
    /// Shows information about the application and the TabViewHandler pattern implementation.
    /// Demonstrates how complete loose coupling is achieved.
    /// </summary>
    [RelayCommand]
    private void ShowAbout()
    {
        _dialogService.ShowMessage(
            "About This Application",
            "This app demonstrates TabViewHandler pattern:\n\n" +
            "✓ MainViewModel has ZERO knowledge of View types\n" +
            "✓ Tabs discovered automatically via ITabViewHandler\n" +
            "✓ Complete loose coupling and separation of concerns\n" +
            "✓ Add new tabs by implementing ITabView - no code changes needed!\n" +
            "✓ Powered by Blazing.Extensions.DependencyInjection");
        UpdateStatus("About dialog shown");
    }

    /// <summary>
    /// Updates the status message with current timestamp for user feedback.
    /// </summary>
    /// <param name="message">The status message to display.</param>
    private void UpdateStatus(string message)
    {
        StatusMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
    }
}
