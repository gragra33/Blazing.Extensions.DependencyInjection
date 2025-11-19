namespace ConsoleExample.Services;

/// <summary>
/// Service interface for generic data processing operations.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Processes the provided string data and returns a result.
    /// </summary>
    /// <param name="data">The data to process.</param>
    /// <returns>A string representing the processed data.</returns>
    string ProcessData(string data);
}