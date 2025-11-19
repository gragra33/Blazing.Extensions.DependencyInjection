namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating replacing service registrations in the host.
/// Shows how calling <see cref="ApplicationHost.ConfigureServices(System.Action{IServiceCollection})"/>
/// multiple times can replace previously registered services.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class ServiceProviderReplacementExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Service Provider Replacement";
    
    /// <summary>
    /// Executes the example. Configures an initial repository implementation, resolves it,
    /// then reconfigures the host with a different repository implementation and resolves it again.
    /// </summary>
    public void Run()
    {
        var host = new ApplicationHost();
        
        host.ConfigureServices(services => services.AddSingleton<IRepository, DatabaseRepository>());
        var initialRepo = host.GetRequiredService<IRepository>();
        Console.WriteLine($"Initial repository: {initialRepo.GetType().Name}");
        
        host.ConfigureServices(services => services.AddSingleton<IRepository, CacheRepository>());
        var replacedRepo = host.GetRequiredService<IRepository>();
        Console.WriteLine($"Replaced repository: {replacedRepo.GetType().Name}");
    }
}