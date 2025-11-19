namespace ConsoleExample.Helpers;

/// <summary>
/// Helper methods for console output used by the sample applications.
/// Provides consistent header/footer printing, simple timing logs and results formatting.
/// </summary>
public static class ConsoleHelper
{
    /// <summary>
    /// Separator line used by header and footer output.
    /// </summary>
    private static readonly string _sepLine = "==============================================";

    /// <summary>
    /// Writes the standardized examples header to the console.
    /// </summary>
    public static void WriteHeader()
    {
        Console.WriteLine(_sepLine);
        Console.WriteLine("Blazing.Extensions.DependencyInjection Examples");
        Console.WriteLine(_sepLine);
        Console.WriteLine();
        Console.WriteLine("This project demonstrates all features of");
        Console.WriteLine("Blazing.Extensions.DependencyInjection including");
        Console.WriteLine("multi-ServiceProvider support for advanced scenarios.");
        Console.WriteLine();
        Console.WriteLine("===============");
        Console.WriteLine("Blazing.Extensions.DependencyInjection Examples");
        Console.WriteLine("===============");
    }

    /// <summary>
    /// Writes the standardized footer to the console indicating overall success or failure.
    /// </summary>
    /// <param name="success">If <c>true</c> writes a success message; otherwise writes an error message.</param>
    public static void WriteFooter(bool success)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(_sepLine);
        if (success)
        {
            Console.WriteLine("Examples completed successfully!");
        }
        else
        {
            Console.WriteLine("Examples completed with errors!");
        }
        Console.WriteLine(_sepLine);
    }

    /// <summary>
    /// Logs a timing message to the console prepended with a timestamp and the label "TIMING".
    /// </summary>
    /// <param name="message">The message describing the timing event.</param>
    public static void LogTiming(string message)
    {
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - {message}");
    }

    /// <summary>
    /// Prints a concise results summary to the console. Skips the aggregated "All Examples" entry.
    /// </summary>
    /// <param name="results">A dictionary mapping example names to a boolean success indicator.</param>
    public static void PrintResults(Dictionary<string, bool> results)
    {
        Console.WriteLine();
        Console.WriteLine("---------------");
        foreach (var result in results)
        {
            if (result.Key == "All Examples") continue;
            
            var status = result.Value ? "Y" : "N";
            var dots = new string('.', Math.Max(1, 63 - result.Key.Length));
            Console.WriteLine($"{result.Key}{dots}{status}");
        }
    }
}