using System.Collections.Generic;
using System.Linq;

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

/// <summary>
/// Implementation of IDataService that manages data in memory.
/// Demonstrates service implementation with simple data operations.
/// In a real application, this might connect to a database or external API.
/// </summary>
public class DataService : IDataService
{
    /// <summary>
    /// Sample data collection initialized with default items for demonstration purposes.
    /// </summary>
    private readonly List<string> _data = new()
    {
        "Sample Item 1",
        "Sample Item 2",
        "Sample Item 3"
    };

    /// <inheritdoc/>
    public IEnumerable<string> GetAllData() => _data.ToList();

    /// <inheritdoc/>
    public void AddData(string item)
    {
        if (!string.IsNullOrWhiteSpace(item))
            _data.Add(item);
    }

    /// <inheritdoc/>
    public void ClearData() => _data.Clear();

    /// <inheritdoc/>
    public int GetDataCount() => _data.Count;
}
