using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Shared.Interfaces;

/// <summary>
/// Defines the contract for tenant service operations.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets all active tenants in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A collection of all active tenants.</returns>
    Task<IEnumerable<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a tenant by its unique identifier.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The tenant if found; otherwise, null.</returns>
    Task<Tenant?> GetTenantByIdAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a tenant exists and is active.
    /// </summary>
    /// <param name="tenantId">The tenant identifier to validate.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the tenant exists and is active; otherwise, false.</returns>
    Task<bool> ValidateTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new tenant in the system.
    /// </summary>
    /// <param name="tenant">The tenant to create.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The created tenant with assigned ID.</returns>
    Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
