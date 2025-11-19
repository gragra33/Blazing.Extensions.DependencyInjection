namespace ConsoleExample.Processing;

/// <summary>
/// Processor for integer values implementing <see cref="IGenericProcessor{T}"/>.
/// </summary>
public class IntegerProcessor : IGenericProcessor<int>
{
    /// <summary>
    /// Processes the specified integer and returns a descriptive string.
    /// </summary>
    /// <param name="data">The integer to process.</param>
    /// <returns>A formatted string describing the processed integer.</returns>
    public string Process(int data) => $"Processed integer: {data} (squared: {data * data})";
}