namespace ConsoleExample.Scanning;

[AutoRegister(ServiceLifetime.Transient, typeof(IScannedService))]
public class FirstScannedService : IScannedService
{
    public string GetMessage() => "First scanned service";
}