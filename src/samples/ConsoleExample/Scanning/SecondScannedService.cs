namespace ConsoleExample.Scanning;

[AutoRegister(ServiceLifetime.Transient, typeof(IScannedService))]
public class SecondScannedService : IScannedService
{
    public string GetMessage() => "Second scanned service";
}