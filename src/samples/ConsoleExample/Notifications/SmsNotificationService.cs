namespace ConsoleExample.Notifications;

/// <summary>
/// Service for sending SMS notifications.
/// </summary>
public class SmsNotificationService : INotificationService
{
    /// <summary>
    /// Sends an SMS notification with the supplied message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    public void SendNotification(string message) => 
        Console.WriteLine($"[SMS] Sending: {message}");
}