namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class ServiceCollectionCustomizationExample : IExample
{
    public string Name => "Service Collection Customization";
    
    public void Run()
    {
        var host = new ApplicationHost();
        var serviceCollection = host.GetServiceCollection(ConfigureBasicServices);
        
        serviceCollection.AddTransient<INotificationService, EmailNotificationService>();
        
        var customProvider = host.BuildServiceProvider(serviceCollection, new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true
        });
        
        using var scope = customProvider.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var user = userService.GetUser(1);
        Console.WriteLine($"Custom configured user: {user.Name}");
    }

    private static void ConfigureBasicServices(IServiceCollection services)
    {
        services.AddSingleton<IRepository, DatabaseRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ILoggingService, ConsoleLoggingService>();
    }
}