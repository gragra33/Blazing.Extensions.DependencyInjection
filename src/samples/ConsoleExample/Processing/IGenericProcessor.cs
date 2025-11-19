namespace ConsoleExample.Processing;

/// <summary>
/// Generic processor interface for processing values of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The non-nullable type this processor handles.</typeparam>
public interface IGenericProcessor<T> where T : notnull
{
    /// <summary>
    /// Processes the provided data value and returns a result string.
    /// </summary>
    /// <param name="data">The data to process.</param>
    /// <returns>A string describing the processed result.</returns>
    string Process(T data);
}