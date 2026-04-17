namespace WpfExample.Services;

/// <summary>
/// Demonstrates caching decorator on three method return-type variants:
/// synchronous, <see cref="Task{T}"/>, and <see cref="ValueTask{T}"/>.
/// </summary>
public interface IProductCatalogService
{
    /// <summary>Gets the product name for <paramref name="id"/> — synchronous, cached.</summary>
    string GetName(int id);

    /// <summary>Gets the product name for <paramref name="id"/> — <see cref="Task{T}"/>, cached.</summary>
    Task<string> GetNameAsync(int id);

    /// <summary>Gets the stock count for <paramref name="id"/> — <see cref="ValueTask{T}"/>, cached.</summary>
    ValueTask<int> GetCountAsync(int id);
}
