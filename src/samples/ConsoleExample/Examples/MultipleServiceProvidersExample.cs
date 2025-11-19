namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating use of multiple ApplicationHost instances and service isolation.
/// Shows how separate hosts can have independent service registrations and lifetimes.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class MultipleServiceProvidersExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Multiple Service Providers";
    
    /// <summary>
    /// Executes the example. Creates two isolated hosts, configures each with different
    /// service implementations and demonstrates resolving services from each host.
    /// </summary>
    public void Run()
    {
        var primaryHost = new ApplicationHost();
        var secondaryHost = new ApplicationHost();
        
        ConfigurePrimaryHost(primaryHost);
        ConfigureSecondaryHost(secondaryHost);
        
        TestMultipleHosts(primaryHost, secondaryHost);
    }

    /// <summary>
    /// Configures the primary host with its set of services.
    /// </summary>
    /// <param name="host">The primary <see cref="ApplicationHost"/> to configure.</param>
    private static void ConfigurePrimaryHost(ApplicationHost host)
    {
        ConsoleHelper.LogTiming("Primary ServiceProvider setup starting");
        host.ConfigureServices(services =>
        {
            services.AddSingleton<IRepository, DatabaseRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<ILoggingService, ConsoleLoggingService>();
        });
        ConsoleHelper.LogTiming("Primary ServiceProvider setup completed");
    }

    /// <summary>
    /// Configures the secondary host with an alternate set of services.
    /// </summary>
    /// <param name="host">The secondary <see cref="ApplicationHost"/> to configure.</param>
    private static void ConfigureSecondaryHost(ApplicationHost host)
    {
        ConsoleHelper.LogTiming("Secondary ServiceProvider setup starting");
        host.ConfigureServices(services =>
        {
            services.AddSingleton<IRepository, CacheRepository>();
            services.AddScoped<IUserService, CachedUserService>();
            services.AddTransient<ILoggingService, FileLoggingService>();
        });
        ConsoleHelper.LogTiming("Secondary ServiceProvider setup completed");
    }

    /// <summary>
    /// Resolves user services from both hosts and writes results to the console to demonstrate
    /// that hosts are isolated and return different implementations.
    /// </summary>
    /// <param name="primaryHost">The primary <see cref="ApplicationHost"/> used to resolve services.</param>
    /// <param name="secondaryHost">The secondary <see cref="ApplicationHost"/> used to resolve services.</param>
    private static void TestMultipleHosts(ApplicationHost primaryHost, ApplicationHost secondaryHost)
    {
        var primaryUserService = primaryHost.GetRequiredService<IUserService>();
        var secondaryUserService = secondaryHost.GetRequiredService<IUserService>();
        
        Console.WriteLine("Primary Host Services:");
        var primaryUser = primaryUserService.GetUser(1);
        Console.WriteLine($"  User from primary: {primaryUser.Name} - {primaryUser.Email}");
        
        Console.WriteLine("Secondary Host Services:");
        var secondaryUser = secondaryUserService.GetUser(1);
        Console.WriteLine($"  User from secondary: {secondaryUser.Name} - {secondaryUser.Email}");
    }
}