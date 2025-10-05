namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class AssemblyScanningExample : IExample
{
    public string Name => "Assembly Scanning";
    
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