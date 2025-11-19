namespace ConsoleExample.Examples;

/// <summary>
/// Example showing how the <c>AutoRegister</c> attribute can be used to auto-register services.
/// Discovers scanned services and demonstrates resolving an auto-registered service via an
/// assembly-scanning host.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class AutoRegisterAttributeExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "AutoRegister Attribute";
    
    /// <summary>
    /// Executes the example which builds a small host, resolves an auto-registered service and invokes it.
    /// </summary>
    public void Run()
    {
        var host = CreateAssemblyScanningHost();
        var autoRegisteredService = host.GetRequiredService<AutoRegisteredService>();
        autoRegisteredService.DoWork();
    }
    
    /// <summary>
    /// Creates and configures an <see cref="ApplicationHost"/> instance that is used for assembly scanning.
    /// </summary>
    /// <returns>A configured <see cref="ApplicationHost"/>.</returns>
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