namespace ConsoleExample.Services;

/// <summary>
/// IUserService implementation that uses a repository and logging service and simulates a cached user store.
/// </summary>
public class CachedUserService(IRepository repository, ILoggingService logger) : IUserService
{
    /// <summary>
    /// Retrieves a user by id using the underlying repository.
    /// </summary>
    /// <param name="id">The user id.</param>
    /// <returns>The resolved <see cref="User"/> instance.</returns>
    public User GetUser(int id)
    {
        logger.Log($"Getting cached user {id} from {repository.GetType().Name}");
        return repository.GetUser(id);
    }

    /// <summary>
    /// Creates a new user and persists it using the repository.
    /// </summary>
    /// <param name="name">The user's name.</param>
    /// <param name="email">The user's email address.</param>
    public void CreateUser(string name, string email)
    {
        logger.Log($"Creating cached user {name}");
        var user = new User(0, name, email);
        repository.SaveUser(user);
    }
}