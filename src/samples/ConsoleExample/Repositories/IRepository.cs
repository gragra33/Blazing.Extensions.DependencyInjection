namespace ConsoleExample.Repositories;

/// <summary>
/// Defines a repository abstraction used by the console samples for user persistence.
/// </summary>
public interface IRepository
{
    /// <summary>
    /// Retrieves a user by identifier.
    /// </summary>
    /// <param name="id">The user's identifier.</param>
    /// <returns>The user with the specified id.</returns>
    User GetUser(int id);

    /// <summary>
    /// Saves a user to the repository.
    /// </summary>
    /// <param name="user">The user to save.</param>
    void SaveUser(User user);
}