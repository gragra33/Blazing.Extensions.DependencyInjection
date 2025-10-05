namespace WinFormsExample.Services;

/// <summary>
/// Service interface for displaying user dialogs and messages in WinForms.
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
/// Implementation of IDialogService for WinForms.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IDialogService))]
public class DialogService : IDialogService
{
    public DialogService()
    {
        Console.WriteLine("DialogService: Constructor called - Service registered as singleton");
    }

    public void ShowMessage(string title, string message)
    {
        Console.WriteLine($"DialogService: ShowMessage called - Title: '{title}'");
        Console.WriteLine($"DialogService: Message content: '{message}'");
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        Console.WriteLine("DialogService: Message dialog closed by user");
    }

    public bool ShowConfirmation(string title, string message)
    {
        Console.WriteLine($"DialogService: ShowConfirmation called - Title: '{title}'");
        Console.WriteLine($"DialogService: Confirmation message: '{message}'");
        var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        var userChoice = result == DialogResult.Yes;
        Console.WriteLine($"DialogService: User selected: {(userChoice ? "Yes" : "No")}");
        return userChoice;
    }
}