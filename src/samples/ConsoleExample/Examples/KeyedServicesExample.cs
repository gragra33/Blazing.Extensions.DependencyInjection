namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating registration and resolution of keyed services.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class KeyedServicesExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Keyed Services";
    
    /// <summary>
    /// Executes the example. Configures keyed notification services on an
    /// <see cref="ApplicationHost"/>, resolves them by key and sends test notifications.
    /// </summary>
    public void Run()
    {
        var host = new ApplicationHost();
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<INotificationService, EmailNotificationService>("email");
            services.AddKeyedSingleton<INotificationService, SmsNotificationService>("sms");
            services.AddKeyedSingleton<INotificationService, PushNotificationService>("push");
        });
        
        var emailService = host.GetRequiredKeyedService<INotificationService>("email");
        var smsService = host.GetRequiredKeyedService<INotificationService>("sms");
        var pushService = host.GetRequiredKeyedService<INotificationService>("push");
        
        emailService.SendNotification("Test email notification");
        smsService.SendNotification("Test SMS notification");
        pushService.SendNotification("Test push notification");
    }
}