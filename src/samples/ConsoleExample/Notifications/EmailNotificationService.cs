namespace ConsoleExample.Notifications;

/// <summary>
/// Service for sending email notifications.
/// </summary>
public class EmailNotificationService : INotificationService
{
    /// <summary>
    /// Sends an email notification with the supplied message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    public void SendNotification(string message) => 
        Console.WriteLine($"[EMAIL] Sending: {message}");
}