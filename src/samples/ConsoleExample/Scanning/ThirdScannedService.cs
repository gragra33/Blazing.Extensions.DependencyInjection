namespace ConsoleExample.Scanning;

[AutoRegister(ServiceLifetime.Transient, typeof(IScannedService))]
public class ThirdScannedService : IScannedService
{
    public string GetMessage() => "Third scanned service";
}