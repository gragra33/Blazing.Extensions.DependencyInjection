namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class ServiceProviderReplacementExample : IExample
{
    public string Name => "Service Provider Replacement";
    
    public void Run()
    {
        var host = new ApplicationHost();
        
        host.ConfigureServices(services => ServiceCollectionServiceExtensions.AddSingleton<IRepository, DatabaseRepository>(services));
        var initialRepo = host.GetRequiredService<IRepository>();
        Console.WriteLine($"Initial repository: {initialRepo.GetType().Name}");
        
        host.ConfigureServices(services => services.AddSingleton<IRepository, CacheRepository>());
        var replacedRepo = host.GetRequiredService<IRepository>();
        Console.WriteLine($"Replaced repository: {replacedRepo.GetType().Name}");
    }
}