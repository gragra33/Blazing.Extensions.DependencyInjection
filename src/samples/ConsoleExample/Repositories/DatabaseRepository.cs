namespace ConsoleExample.Repositories;

/// <summary>
/// Simple repository implementation that simulates a database-backed store.
/// </summary>
public class DatabaseRepository : IRepository
{
    /// <inheritdoc/>
    public User GetUser(int id) => new(id, "John Doe (DB)", "john.db@example.com");

    /// <inheritdoc/>
    public void SaveUser(User user) => Console.WriteLine($"Saving user {user.Name} to database");
}