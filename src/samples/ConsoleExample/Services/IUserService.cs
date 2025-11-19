namespace ConsoleExample.Services;

/// <summary>
/// Abstraction for user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Retrieves a user by their identifier.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <returns>The <see cref="User"/> instance.</returns>
    User GetUser(int userId);
}