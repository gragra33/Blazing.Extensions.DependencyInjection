namespace ConsoleExample.Services;

[AutoRegister(ServiceLifetime.Singleton)]
public class MultiInterfaceService : IDataService, IValidationService, ICacheService
{
    private readonly Dictionary<string, string> _cache = new();
    
    public string ProcessData(string? data)
    {
        return $"Data processed: {data?.ToUpperInvariant() ?? "NULL"}";
    }
    
    public bool ValidateData(string data)
    {
        var isValid = !string.IsNullOrWhiteSpace(data) && data.Length >= 3;
        Console.WriteLine($"Validation result for '{data}': {isValid}");
        return isValid;
    }
    
    public string GetFromCache(string key)
    {
        _cache.TryGetValue(key, out var value);
        var result = value ?? $"Cache miss for key: {key}";
        Console.WriteLine($"Cache lookup for '{key}': {result}");
        return result;
    }
    
    public void SetCache(string key, string value)
    {
        _cache[key] = value;
        Console.WriteLine($"Cached '{key}' = '{value}'");
    }
}