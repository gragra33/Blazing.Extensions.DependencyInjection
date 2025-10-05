namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class MemoryManagementExample : IExample
{
    public string Name => "Memory Management";
    
    public void Run()
    {
        var host = new ApplicationHost();
        host.ConfigureServices(services => ServiceCollectionServiceExtensions.AddTransient<DisposableService>(services));
        
        var disposableService = host.GetRequiredService<DisposableService>();
        Console.WriteLine($"Disposable service created: {!disposableService.IsDisposed}");
        
        var cleared = host.ClearServices();
        Console.WriteLine($"Services cleared: {cleared}");
        Console.WriteLine($"Services now null: {host.GetServices() == null}");
    }
}