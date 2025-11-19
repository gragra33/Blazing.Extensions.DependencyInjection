namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating assembly scanning and dynamic service registration.
/// Discovers all <see cref="IScannedService"/> implementations and prints their messages.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class AssemblyScanningExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Assembly Scanning";
    
    /// <summary>
    /// Executes the example. This creates an <see cref="ApplicationHost"/> configured
    /// for assembly scanning, resolves all <see cref="IScannedService"/> implementations
    /// and writes their messages to the console.
    /// </summary>
    public void Run()
    {
        var host = CreateAssemblyScanningHost();
        
        var scannedServices = host.GetServices()!.GetServices<IScannedService>().ToArray();
        Console.WriteLine($"Found {scannedServices.Length} scanned services:");
        foreach (var service in scannedServices)
        {
            Console.WriteLine($"  - {service.GetType().Name}: {service.GetMessage()}");
        }
    }
    
    /// <summary>
    /// Creates and configures an <see cref="ApplicationHost"/> instance used by this example.
    /// The host is configured to scan the current assembly and register discovered services.
    /// </summary>
    /// <returns>An initialized <see cref="ApplicationHost"/>.</returns>
    private ApplicationHost CreateAssemblyScanningHost()
    {
        var host = new ApplicationHost();
        host.AddAssembly(typeof(Program).Assembly)
            .ConfigureServices(services =>
            {
                services.Register<IScannedService>(ServiceLifetime.Transient);
                services.Register();
            });
        return host;
    }
}