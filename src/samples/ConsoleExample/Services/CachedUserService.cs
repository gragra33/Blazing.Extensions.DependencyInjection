namespace ConsoleExample.Services;

public class CachedUserService(IRepository repository, ILoggingService logger) : IUserService
{
    public User GetUser(int id)
    {
        logger.Log($"Getting cached user {id} from {repository.GetType().Name}");
        return repository.GetUser(id);
    }

    public void CreateUser(string name, string email)
    {
        logger.Log($"Creating cached user {name}");
        var user = new User(0, name, email);
        repository.SaveUser(user);
    }
}