namespace ConsoleExample.Services;

public interface ICacheService
{
    string? GetFromCache(string key);
    void SetCache(string key, string value);
}