namespace ConsoleExample.Scanning;

/// <summary>
/// Represents a service discovered by assembly scanning in the samples.
/// Implementations return a short message used by the examples to demonstrate discovery.
/// </summary>
public interface IScannedService
{
    /// <summary>
    /// Gets a short message describing the scanned service.
    /// </summary>
    /// <returns>A string message provided by the scanned service.</returns>
    string GetMessage();
}