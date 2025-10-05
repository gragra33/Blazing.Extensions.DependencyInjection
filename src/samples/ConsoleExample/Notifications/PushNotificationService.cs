namespace ConsoleExample.Notifications;

public class PushNotificationService : INotificationService
{
    public void SendNotification(string message) => 
        Console.WriteLine($"[PUSH] Sending: {message}");
}