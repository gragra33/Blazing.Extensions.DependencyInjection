namespace ConsoleExample.Data;

/// <summary>
/// Generic data provider interface.
/// </summary>
/// <typeparam name="T">The type of data provided.</typeparam>
public interface IDataProvider<T>
{
    /// <summary>
    /// Gets the data.
    /// </summary>
    T GetData();

    /// <summary>
    /// Gets the provider type description.
    /// </summary>
    /// <returns>A string describing the provider type.</returns>
    string GetProviderType();
}