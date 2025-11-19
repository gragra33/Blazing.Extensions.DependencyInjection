namespace ConsoleExample;

/// <summary>
/// Entry point for the ConsoleExample application.
/// </summary>
static class Program
{
    /// <summary>
    /// Application entry point. Runs all registered examples.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    static void Main(string[] args)
    {
        var runner = new ExampleRunner();
        runner.RunAllExamples();
    }
}