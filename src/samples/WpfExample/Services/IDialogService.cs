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