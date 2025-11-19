namespace ConsoleExample.Processing;

/// <summary>
/// Processor for decimal values implementing <see cref="IGenericProcessor{T}"/>.
/// </summary>
public class DecimalProcessor : IGenericProcessor<decimal>
{
    /// <summary>
    /// Processes the specified decimal and returns a descriptive result.
    /// </summary>
    /// <param name="data">The decimal value to process.</param>
    /// <returns>A formatted string describing the processed decimal.</returns>
    public string Process(decimal data) => $"Processed decimal: {data:F4} (rounded: {Math.Round(data, 2)})";
}