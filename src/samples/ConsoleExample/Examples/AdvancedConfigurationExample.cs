namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class AdvancedConfigurationExample : IExample
{
    public string Name => "Advanced Configuration";
    
    public void Run()
    {
        var host = new ApplicationHost();
        host.ConfigureServices(
            ConfigureBasicServices,
            ValidateServices
        );
    }

    private static void ConfigureBasicServices(IServiceCollection services)
    {
        services.AddSingleton<IRepository, DatabaseRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ILoggingService, ConsoleLoggingService>();
    }

    private static void ValidateServices(IServiceProvider provider)
    {
        ConsoleHelper.LogTiming("Post-build validation starting");
        var repo = provider.GetRequiredService<IRepository>();
        var userSvc = provider.GetRequiredService<IUserService>();
        Console.WriteLine("Post-build validation: All required services are available");
        ConsoleHelper.LogTiming("Post-build validation completed");
    }
}