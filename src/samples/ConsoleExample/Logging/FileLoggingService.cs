namespace ConsoleExample.Logging;

public class FileLoggingService : ILoggingService
{
    public void Log(string message) => Console.WriteLine($"[FILE LOG] {message}");
}