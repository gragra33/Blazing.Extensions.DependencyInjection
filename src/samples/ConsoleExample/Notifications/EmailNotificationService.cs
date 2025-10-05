namespace ConsoleExample.Notifications;

public class EmailNotificationService : INotificationService
{
    public void SendNotification(string message) => 
        Console.WriteLine($"[EMAIL] Sending: {message}");
}