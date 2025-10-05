namespace ConsoleExample.Services;

public class UserService(IRepository repository, ILoggingService logger) : IUserService
{
    public User GetUser(int id)
    {
        logger.Log($"Getting user {id} from {repository.GetType().Name}");
        return repository.GetUser(id);
    }

    public void CreateUser(string name, string email)
    {
        logger.Log($"Creating user {name}");
        var user = new User(0, name, email);
        repository.SaveUser(user);
    }
}