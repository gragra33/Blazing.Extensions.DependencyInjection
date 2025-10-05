namespace WpfExample.Services;

/// <summary>
/// Service interface for managing application data operations.
/// Demonstrates service abstraction and dependency injection patterns.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Retrieves all data items in the collection.
    /// </summary>
    /// <returns>A collection of all data items.</returns>
    IEnumerable<string> GetAllData();
    
    /// <summary>
    /// Adds a new data item to the collection.
    /// </summary>
    /// <param name="item">The item to add. Null or whitespace items are ignored.</param>
    void AddData(string item);
    
    /// <summary>
    /// Removes all data items from the collection.
    /// </summary>
    void ClearData();
    
    /// <summary>
    /// Gets the total number of data items in the collection.
    /// </summary>
    /// <returns>The count of data items.</returns>
    int GetDataCount();
}