namespace WpfExample.Services;

/// <summary>
/// WPF implementation of IDialogService using standard MessageBox dialogs.
/// Demonstrates service implementation for UI operations.
/// In testing scenarios, this could be replaced with a mock implementation.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IDialogService))]
public class DialogService : IDialogService
{
    /// <inheritdoc/>
    public void ShowMessage(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <inheritdoc/>
    public bool ShowConfirmation(string title, string message)
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }
}
