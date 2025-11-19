namespace ConsoleExample.Repositories;

/// <summary>
/// Repository implementation using an in-memory cache for demonstration.
/// </summary>
public class CacheRepository : IRepository
{
    /// <inheritdoc/>
    public User GetUser(int id) => new(id, "Jane Smith (Cache)", "jane.cache@example.com");

    /// <inheritdoc/>
    public void SaveUser(User user) => Console.WriteLine($"Saving user {user.Name} to cache");
}