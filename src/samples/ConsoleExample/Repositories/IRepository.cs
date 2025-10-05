namespace ConsoleExample.Repositories;

public interface IRepository
{
    User GetUser(int id);
    void SaveUser(User user);
}