namespace ConsoleExample.Logging;

/// <summary>
/// Logs messages to a file for demonstration purposes.
/// </summary>
public class FileLoggingService : ILoggingService
{
    /// <summary>
    /// Logs the specified message to a file. In samples this writes to console for simplicity.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message) => Console.WriteLine($"[FILE LOG] {message}");
}