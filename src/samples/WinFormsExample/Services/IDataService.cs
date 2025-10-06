using System.Threading.Tasks;

namespace WinFormsExample.Services;

/// <summary>
/// Interface for handling data operations.
/// </summary>
public interface IDataService
{
    /// <summary>
    /// Gets sample data items asynchronously.
    /// </summary>
    Task<IEnumerable<DataItem>> GetDataAsync();
    
    /// <summary>
    /// Saves a data item asynchronously.
    /// </summary>
    Task SaveDataAsync(DataItem item);
    
    /// <summary>
    /// Deletes a data item asynchronously.
    /// </summary>
    Task DeleteDataAsync(int id);
}