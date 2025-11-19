namespace ConsoleExample.Logging;

/// <summary>
/// Logs messages to the console for demonstration purposes.
/// </summary>
public class ConsoleLoggingService : ILoggingService
{
    /// <summary>
    /// Logs the specified message to the console output.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message) => Console.WriteLine($"[CONSOLE LOG] {message}");
}