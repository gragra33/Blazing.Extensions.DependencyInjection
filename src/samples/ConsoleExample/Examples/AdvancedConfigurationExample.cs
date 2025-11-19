namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating advanced DI configuration and post-build validation.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class AdvancedConfigurationExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Advanced Configuration";
    
    /// <summary>
    /// Executes the example. Configures services and performs post-build validation.
    /// </summary>
    public void Run()
    {
        var host = new ApplicationHost();
        host.ConfigureServices(
            ConfigureBasicServices,
            ValidateServices
        );
    }

    /// <summary>
    /// Configures the basic service registrations used by the example.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    private static void ConfigureBasicServices(IServiceCollection services)
    {
        services.AddSingleton<IRepository, DatabaseRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ILoggingService, ConsoleLoggingService>();
    }

    /// <summary>
    /// Performs post-build validation of required services from the provided <see cref="IServiceProvider"/>.
    /// Logs timing information and confirms required services are available.
    /// </summary>
    /// <param name="provider">The service provider to resolve services from for validation.</param>
    private static void ValidateServices(IServiceProvider provider)
    {
        ConsoleHelper.LogTiming("Post-build validation starting");
        var repo = provider.GetRequiredService<IRepository>();
        var userSvc = provider.GetRequiredService<IUserService>();
        Console.WriteLine("Post-build validation: All required services are available");
        ConsoleHelper.LogTiming("Post-build validation completed");
    }
}