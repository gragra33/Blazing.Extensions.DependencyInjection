namespace ConsoleExample.Scanning;

/// <summary>
/// A simple implementation of <see cref="IScannedService"/> that is discovered via
/// assembly scanning and auto-registered for use in the console examples.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IScannedService))]
public class SecondScannedService : IScannedService
{
    /// <summary>
    /// Gets a short message identifying this scanned service used in examples.
    /// </summary>
    public string GetMessage() => "Second scanned service";
}