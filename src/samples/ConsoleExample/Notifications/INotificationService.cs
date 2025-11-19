namespace ConsoleExample.Notifications;

/// <summary>
/// Abstraction for sending notifications used by the console examples.
/// Implementations send a message through a specific channel such as email, SMS or push.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification message through the service-specific channel.
    /// </summary>
    /// <param name="message">The notification message to send.</param>
    void SendNotification(string message);
}