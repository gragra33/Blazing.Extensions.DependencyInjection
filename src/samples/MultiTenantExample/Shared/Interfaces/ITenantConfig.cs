namespace MultiTenantExample.Shared.Interfaces;

/// <summary>
/// Defines the contract for tenant-specific configuration.
/// </summary>
public interface ITenantConfig
{
    /// <summary>
    /// Gets the tenant ID this configuration is for.
    /// </summary>
    string TenantId { get; }

    /// <summary>
    /// Gets the maximum number of orders allowed for this tenant.
    /// </summary>
    int MaxOrders { get; }

    /// <summary>
    /// Gets the maximum number of products allowed for this tenant.
    /// </summary>
    int MaxProducts { get; }

    /// <summary>
    /// Gets a value indicating whether premium features are enabled for this tenant.
    /// </summary>
    bool IsPremium { get; }

    /// <summary>
    /// Gets the API rate limit per minute for this tenant.
    /// </summary>
    int RateLimitPerMinute { get; }

    /// <summary>
    /// Gets custom configuration settings for this tenant.
    /// </summary>
    Dictionary<string, string> CustomSettings { get; }
}
