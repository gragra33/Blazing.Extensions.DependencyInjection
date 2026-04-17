namespace WinFormsExample.Services;

/// <summary>
/// Demonstrates caching decorator support for synchronous, <see cref="Task{TResult}"/>,
/// and <see cref="ValueTask{TResult}"/> return types.
/// </summary>
public interface IProductCatalogService
{
    /// <summary>Gets the product name for <paramref name="id"/> synchronously.</summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product display name.</returns>
    string GetName(int id);

    /// <summary>Gets the product name for <paramref name="id"/> asynchronously.</summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product display name.</returns>
    Task<string> GetNameAsync(int id);

    /// <summary>Gets the product stock count for <paramref name="id"/> asynchronously.</summary>
    /// <param name="id">The product identifier.</param>
    /// <returns>The product stock count.</returns>
    ValueTask<int> GetCountAsync(int id);
}
