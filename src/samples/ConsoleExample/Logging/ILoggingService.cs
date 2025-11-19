namespace ConsoleExample.Logging;

/// <summary>
/// Defines a simple logging abstraction used by the console examples.
/// Implementations should provide a lightweight mechanism to write log messages.
/// </summary>
public interface ILoggingService
{
    /// <summary>
    /// Logs the specified message to the configured sink.
    /// </summary>
    /// <param name="message">The log message to write.</param>
    void Log(string message);
}