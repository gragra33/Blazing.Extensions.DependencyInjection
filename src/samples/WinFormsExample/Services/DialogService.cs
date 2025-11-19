namespace WinFormsExample.Services;

/// <summary>
/// Implementation of IDialogService for WinForms.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IDialogService))]
public class DialogService : IDialogService
{
    public DialogService()
    {
        Console.WriteLine(@"DialogService: Constructor called - Service registered as singleton");
    }

    public void ShowMessage(string title, string message)
    {
        Console.WriteLine($@"DialogService: ShowMessage called - Title: '{title}'");
        Console.WriteLine($@"DialogService: Message content: '{message}'");
        MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
        Console.WriteLine(@"DialogService: Message dialog closed by user");
    }

    public bool ShowConfirmation(string title, string message)
    {
        Console.WriteLine($@"DialogService: ShowConfirmation called - Title: '{title}'");
        Console.WriteLine($@"DialogService: Confirmation message: '{message}'");
        var result = MessageBox.Show(message, title, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        var userChoice = result == DialogResult.Yes;
        Console.WriteLine($@"DialogService: User selected: {(userChoice ? "Yes" : "No")}");
        return userChoice;
    }
}