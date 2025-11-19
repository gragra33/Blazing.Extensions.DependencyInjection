namespace ConsoleExample.Processing;

/// <summary>
/// Processor for string values implementing <see cref="IGenericProcessor{T}"/>.
/// </summary>
public class StringProcessor : IGenericProcessor<string>
{
    /// <summary>
    /// Processes the specified string and returns a descriptive result.
    /// </summary>
    /// <param name="data">The string to process.</param>
    /// <returns>A formatted string describing the processed input.</returns>
    public string Process(string data) => $"Processed string: '{data}' (length: {data.Length})";
}