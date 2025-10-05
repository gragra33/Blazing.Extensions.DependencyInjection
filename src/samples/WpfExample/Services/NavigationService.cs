namespace WpfExample.Services;

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
