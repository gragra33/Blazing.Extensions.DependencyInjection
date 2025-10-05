namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class KeyedServicesExample : IExample
{
    public string Name => "Keyed Services";
    
    public void Run()
    {
        var host = new ApplicationHost();
        host.ConfigureServices(services =>
        {
            ServiceCollectionServiceExtensions.AddKeyedSingleton<INotificationService, EmailNotificationService>(services, "email");
            ServiceCollectionServiceExtensions.AddKeyedSingleton<INotificationService, SmsNotificationService>(services, "sms");
            ServiceCollectionServiceExtensions.AddKeyedSingleton<INotificationService, PushNotificationService>(services, "push");
        });
        
        var emailService = host.GetRequiredKeyedService<INotificationService>("email");
        var smsService = host.GetRequiredKeyedService<INotificationService>("sms");
        var pushService = host.GetRequiredKeyedService<INotificationService>("push");
        
        emailService.SendNotification("Test email notification");
        smsService.SendNotification("Test SMS notification");
        pushService.SendNotification("Test push notification");
    }
}