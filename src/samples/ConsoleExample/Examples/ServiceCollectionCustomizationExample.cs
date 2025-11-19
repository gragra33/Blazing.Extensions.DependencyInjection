namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating customization of the service collection and building a custom provider.
/// Shows how to obtain a configurable <see cref="IServiceCollection"/>, add additional services,
/// and build a custom <see cref="IServiceProvider"/> with specific <see cref="ServiceProviderOptions"/>.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class ServiceCollectionCustomizationExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Service Collection Customization";
    
    /// <summary>
    /// Executes the example. Retrieves a service collection configured by the host,
    /// customizes it, builds a provider with validation options, and resolves a scoped service.
    /// </summary>
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

    /// <summary>
    /// Configures the basic services used by this example and added to the returned
    /// <see cref="IServiceCollection"/> by the host.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    private static void ConfigureBasicServices(IServiceCollection services)
    {
        services.AddSingleton<IRepository, DatabaseRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ILoggingService, ConsoleLoggingService>();
    }
}