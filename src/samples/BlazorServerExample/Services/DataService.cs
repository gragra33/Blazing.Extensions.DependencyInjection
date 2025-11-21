using Blazing.Extensions.DependencyInjection;

namespace BlazorServerExample.Services;

/// <summary>
/// Implementation of IDataService that manages data in memory.
/// Demonstrates service implementation with simple data operations.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IDataService))]
public class DataService : IDataService
{
    /// <summary>
    /// Sample data collection initialized with default items for demonstration purposes.
    /// </summary>
    private readonly List<string> _data =
    [
        "Sample Item 1",
        "Sample Item 2",
        "Sample Item 3"
    ];

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
