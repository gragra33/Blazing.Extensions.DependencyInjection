namespace WinFormsExample.Services;

/// <summary>
/// Interface for navigation services within the application.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigates to the specified view by index.
    /// </summary>
    void NavigateTo(int tabIndex);
    
    /// <summary>
    /// Gets the current tab index.
    /// </summary>
    int CurrentTabIndex { get; }
    
    /// <summary>
    /// Event raised when navigation occurs.
    /// </summary>
    event EventHandler<int>? NavigationChanged;
}