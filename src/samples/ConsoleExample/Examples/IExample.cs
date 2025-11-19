namespace ConsoleExample.Examples;

/// <summary>
/// Represents a runnable example used by the console samples.
/// Implementations are discovered and executed by <see cref="ExampleRunner"/>.
/// </summary>
public interface IExample
{
    /// <summary>
    /// Gets the human-readable name of the example.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes the example logic.
    /// </summary>
    void Run();
}