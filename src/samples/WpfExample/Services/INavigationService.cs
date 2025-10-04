namespace WpfExample.Services;

/// <summary>
/// Service for navigation between views.
/// </summary>
public interface INavigationService
{
    void NavigateToSettings();
    void NavigateBack();
}