namespace ConsoleExample.Services;

/// <summary>
/// Provides simple user-related operations backed by an <see cref="IRepository"/>.
/// </summary>
public class UserService(IRepository repository, ILoggingService logger) : IUserService
{
    /// <summary>
    /// Retrieves a user by id using the configured repository.
    /// </summary>
    /// <param name="id">The identifier of the user to retrieve.</param>
    /// <returns>The retrieved user.</returns>
    public User GetUser(int id)
    {
        logger.Log($"Getting user {id} from {repository.GetType().Name}");
        return repository.GetUser(id);
    }

    /// <summary>
    /// Creates and persists a new user using the configured repository.
    /// </summary>
    /// <param name="name">The name of the new user.</param>
    /// <param name="email">The email address of the new user.</param>
    public void CreateUser(string name, string email)
    {
        logger.Log($"Creating user {name}");
        var user = new User(0, name, email);
        repository.SaveUser(user);
    }
}