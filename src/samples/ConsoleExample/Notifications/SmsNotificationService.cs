namespace ConsoleExample.Notifications;

public class SmsNotificationService : INotificationService
{
    public void SendNotification(string message) => 
        Console.WriteLine($"[SMS] Sending: {message}");
}