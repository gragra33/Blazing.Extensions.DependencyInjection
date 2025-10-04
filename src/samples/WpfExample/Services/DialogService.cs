using System.Windows;

namespace WpfExample.Services;

/// <summary>
/// Service interface for displaying user dialogs and messages.
/// Demonstrates service abstraction for UI operations, allowing for easier testing and UI framework independence.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an informational message dialog to the user.
    /// </summary>
    /// <param name="title">The title of the dialog window.</param>
    /// <param name="message">The message content to display.</param>
    void ShowMessage(string title, string message);
    
    /// <summary>
    /// Shows a confirmation dialog with Yes/No options.
    /// </summary>
    /// <param name="title">The title of the dialog window.</param>
    /// <param name="message">The confirmation message to display.</param>
    /// <returns>True if the user clicked Yes, false if No.</returns>
    bool ShowConfirmation(string title, string message);
}

/// <summary>
/// WPF implementation of IDialogService using standard MessageBox dialogs.
/// Demonstrates service implementation for UI operations.
/// In testing scenarios, this could be replaced with a mock implementation.
/// </summary>
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
