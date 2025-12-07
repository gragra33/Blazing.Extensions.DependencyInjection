namespace MultiTenantExample.Shared.Models;

/// <summary>
/// Represents a tenant in the multi-tenant system.
/// </summary>
public sealed class Tenant
{
    /// <summary>
    /// Gets or sets the unique identifier for the tenant.
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the tenant.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the database connection string for this tenant.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the tenant is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when the tenant was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets additional configuration settings for the tenant.
    /// </summary>
    public Dictionary<string, string> Settings { get; set; } = new();
}
