namespace ConsoleExample;

/// <summary>
/// Entry point for the ConsoleExample application.
/// </summary>
static class Program
{
    /// <summary>
    /// Application entry point. Runs all registered examples.
    /// </summary>
    static void Main()
    {
        var runner = new ExampleRunner();
        runner.RunAllExamples();
    }
}