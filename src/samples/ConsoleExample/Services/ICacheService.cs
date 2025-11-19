namespace ConsoleExample.Services;

/// <summary>
/// Abstraction for a simple cache service.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from the cache by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value or null if not found.</returns>
    string? GetFromCache(string key);

    /// <summary>
    /// Sets a value in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to store.</param>
    void SetCache(string key, string value);
}