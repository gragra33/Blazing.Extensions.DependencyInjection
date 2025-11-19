namespace ConsoleExample.Notifications;

/// <summary>
/// Service for sending push notifications.
/// </summary>
public class PushNotificationService : INotificationService
{
    /// <summary>
    /// Sends a push notification with the supplied message.
    /// </summary>
    /// <param name="message">The notification message.</param>
    public void SendNotification(string message) => 
        Console.WriteLine($"[PUSH] Sending: {message}");
}