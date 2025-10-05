namespace ConsoleExample.Helpers;

public static class ConsoleHelper
{
    private static readonly string _sepLine = "==============================================";

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

    public static void LogTiming(string message)
    {
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - {message}");
    }

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