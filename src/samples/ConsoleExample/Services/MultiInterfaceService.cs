namespace ConsoleExample.Services;

/// <summary>
/// Provides multiple service functionalities: data processing, validation, and caching.
/// Implements <see cref="IDataService"/>, <see cref="IValidationService"/>, and <see cref="ICacheService"/>.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
public class MultiInterfaceService : IDataService, IValidationService, ICacheService
{
    private readonly Dictionary<string, string> _cache = new();
    
    /// <summary>
    /// Processes the provided data string and returns a normalized result.
    /// </summary>
    /// <param name="data">The input data to process. May be null.</param>
    /// <returns>A processed string representation of the input data.</returns>
    public string ProcessData(string? data)
    {
        return $"Data processed: {data?.ToUpperInvariant() ?? "NULL"}";
    }
    
    /// <summary>
    /// Validates the provided data according to simple rules used by the samples.
    /// </summary>
    /// <param name="data">The data string to validate.</param>
    /// <returns><c>true</c> if the data is considered valid; otherwise <c>false</c>.</returns>
    public bool ValidateData(string data)
    {
        var isValid = !string.IsNullOrWhiteSpace(data) && data.Length >= 3;
        Console.WriteLine($"Validation result for '{data}': {isValid}");
        return isValid;
    }
    
    /// <summary>
    /// Attempts to retrieve a value from the in-memory cache by key.
    /// </summary>
    /// <param name="key">The cache key to look up.</param>
    /// <returns>The cached value if present; otherwise a message indicating a cache miss.</returns>
    public string GetFromCache(string key)
    {
        _cache.TryGetValue(key, out var value);
        var result = value ?? $"Cache miss for key: {key}";
        Console.WriteLine($"Cache lookup for '{key}': {result}");
        return result;
    }
    
    /// <summary>
    /// Stores a value in the in-memory cache under the specified key.
    /// </summary>
    /// <param name="key">The cache key to set.</param>
    /// <param name="value">The value to store.</param>
    public void SetCache(string key, string value)
    {
        _cache[key] = value;
        Console.WriteLine($"Cached '{key}' = '{value}'");
    }
}