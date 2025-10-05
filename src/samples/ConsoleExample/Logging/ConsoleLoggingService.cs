namespace ConsoleExample.Logging;

public class ConsoleLoggingService : ILoggingService
{
    public void Log(string message) => Console.WriteLine($"[CONSOLE LOG] {message}");
}