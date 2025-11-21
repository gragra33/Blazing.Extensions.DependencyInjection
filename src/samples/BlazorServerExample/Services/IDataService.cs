namespace BlazorServerExample.Services;

/// <summary>
/// Data service interface for managing application data.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Gets all data items.
    /// </summary>
    /// <returns>Collection of data items.</returns>
    IEnumerable<string> GetAllData();

    /// <summary>
    /// Adds a new data item.
    /// </summary>
    /// <param name="item">The item to add.</param>
    void AddData(string item);

    /// <summary>
    /// Clears all data items.
    /// </summary>
    void ClearData();

    /// <summary>
    /// Gets the count of data items.
    /// </summary>
    /// <returns>Number of items.</returns>
    int GetDataCount();
}
