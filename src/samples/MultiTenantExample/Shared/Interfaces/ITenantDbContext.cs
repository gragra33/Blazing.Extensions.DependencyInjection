using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Shared.Interfaces;

/// <summary>
/// Defines the contract for tenant-specific database context operations.
/// </summary>
public interface ITenantDbContext : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Gets the tenant ID this context is associated with.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Gets the collection of orders for this tenant.
    /// </summary>
    IQueryable<Order> Orders { get; }

    /// <summary>
    /// Gets the collection of products for this tenant.
    /// </summary>
    IQueryable<Product> Products { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an entity to the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to add.</typeparam>
    /// <param name="entity">The entity to add.</param>
    void Add<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Updates an entity in the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to update.</typeparam>
    /// <param name="entity">The entity to update.</param>
    void Update<TEntity>(TEntity entity) where TEntity : class;

    /// <summary>
    /// Removes an entity from the context.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity to remove.</typeparam>
    /// <param name="entity">The entity to remove.</param>
    void Remove<TEntity>(TEntity entity) where TEntity : class;
}
