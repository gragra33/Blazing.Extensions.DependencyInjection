namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating basic service registration and resolution.
/// Shows how to configure common lifetimes and resolve a service from the host.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class BasicServiceConfigurationExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Basic Service Configuration";
    
    /// <summary>
    /// Executes the example: configures the host service collection, performs post-setup timing
    /// logs, and resolves a user service to demonstrate service resolution.
    /// </summary>
    public void Run()
    {
        Console.WriteLine("Setting up main service provider...");
        ConsoleHelper.LogTiming("Main ServiceProvider setup starting");
        
        var host = new ApplicationHost();
        host.ConfigureServices(services =>
        {
            services.AddSingleton<IRepository, DatabaseRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<INotificationService, EmailNotificationService>();
            services.AddTransient<ILoggingService, ConsoleLoggingService>();
            services.Register();
        });
        
        ConsoleHelper.LogTiming("Main ServiceProvider setup completed");
        
        Console.WriteLine("Testing basic service resolution...");
        var userService = host.GetRequiredService<IUserService>();
        var user = userService.GetUser(1);
        Console.WriteLine($"Retrieved user: {user.Name} ({user.Email})");
    }
}