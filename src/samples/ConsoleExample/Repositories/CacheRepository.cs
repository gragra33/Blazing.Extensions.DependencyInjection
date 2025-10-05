namespace ConsoleExample.Repositories;

public class CacheRepository : IRepository
{
    public User GetUser(int id) => new(id, "Jane Smith (Cache)", "jane.cache@example.com");
    public void SaveUser(User user) => Console.WriteLine($"Saving user {user.Name} to cache");
}