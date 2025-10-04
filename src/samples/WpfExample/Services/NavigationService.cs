using System.Windows;

namespace WpfExample.Services;

/// <summary>
/// Service for navigation between views.
/// </summary>
public interface INavigationService
{
    void NavigateToSettings();
    void NavigateBack();
}

public class NavigationService : INavigationService
{
    public void NavigateToSettings()
    {
        // In a real app, this would navigate to a settings page/window
        MessageBox.Show(
            "In a full application, this would navigate to a Settings view.\n\n" +
            "This demonstrates how services can be injected into ViewModels for navigation.",
            "Navigation Service",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    public void NavigateBack()
    {
        // Navigate back logic
    }
}
