namespace ConsoleExample.Repositories;

public class DatabaseRepository : IRepository
{
    public User GetUser(int id) => new(id, "John Doe (DB)", "john.db@example.com");
    public void SaveUser(User user) => Console.WriteLine($"Saving user {user.Name} to database");
}