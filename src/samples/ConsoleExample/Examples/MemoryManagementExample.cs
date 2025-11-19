namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating memory management and service clearing behavior.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class MemoryManagementExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Memory Management";
    
    /// <summary>
    /// Executes the example which configures a disposable service, resolves it,
    /// displays its disposal state, clears the host services and reports the result.
    /// </summary>
    public void Run()
    {
        var host = new ApplicationHost();
        host.ConfigureServices(services => services.AddTransient<DisposableService>());
        
        var disposableService = host.GetRequiredService<DisposableService>();
        Console.WriteLine($"Disposable service created: {!disposableService.IsDisposed}");
        
        var cleared = host.ClearServices();
        Console.WriteLine($"Services cleared: {cleared}");
        Console.WriteLine($"Services now null: {host.GetServices() == null}");
    }
}