namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class AutoRegisterAttributeExample : IExample
{
    public string Name => "AutoRegister Attribute";
    
    public void Run()
    {
        var host = CreateAssemblyScanningHost();
        var autoRegisteredService = host.GetRequiredService<AutoRegisteredService>();
        autoRegisteredService.DoWork();
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