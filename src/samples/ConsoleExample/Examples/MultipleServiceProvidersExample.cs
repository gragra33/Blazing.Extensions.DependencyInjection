namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class MultipleServiceProvidersExample : IExample
{
    public string Name => "Multiple Service Providers";
    
    public void Run()
    {
        var primaryHost = new ApplicationHost();
        var secondaryHost = new ApplicationHost();
        
        ConfigurePrimaryHost(primaryHost);
        ConfigureSecondaryHost(secondaryHost);
        
        TestMultipleHosts(primaryHost, secondaryHost);
    }

    private static void ConfigurePrimaryHost(ApplicationHost host)
    {
        ConsoleHelper.LogTiming("Primary ServiceProvider setup starting");
        host.ConfigureServices(services =>
        {
            ServiceCollectionServiceExtensions.AddSingleton<IRepository, DatabaseRepository>(services);
            ServiceCollectionServiceExtensions.AddScoped<IUserService, UserService>(services);
            ServiceCollectionServiceExtensions.AddTransient<ILoggingService, ConsoleLoggingService>(services);
        });
        ConsoleHelper.LogTiming("Primary ServiceProvider setup completed");
    }

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