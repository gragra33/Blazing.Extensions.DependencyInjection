using MultiTenantExample.Shared.Interfaces;

namespace MultiTenantExample.Server.Services;

/// <summary>
/// Tenant-specific configuration service.
/// This demonstrates Lazy Initialization feature of Blazing.Extensions.DependencyInjection.
/// Configuration is loaded on-demand when first accessed.
/// </summary>
public sealed partial class TenantConfigurationService : ITenantConfig
{
    private readonly string _tenantId;
    private readonly ILogger<TenantConfigurationService> _logger;
    private readonly Lazy<Dictionary<string, string>> _lazySettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantConfigurationService"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="logger">The logger instance.</param>
    public TenantConfigurationService(string tenantId, ILogger<TenantConfigurationService> logger)
    {
        _tenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Settings are loaded lazily on first access
        _lazySettings = new Lazy<Dictionary<string, string>>(LoadSettings);

        LogConfigServiceCreated(tenantId);
    }

    /// <inheritdoc/>
    public string TenantId => _tenantId;

    /// <inheritdoc/>
    public int MaxOrders => GetIntSetting("MaxOrders", 1000);

    /// <inheritdoc/>
    public int MaxProducts => GetIntSetting("MaxProducts", 500);

    /// <inheritdoc/>
    public bool IsPremium => GetBoolSetting("IsPremium", false);

    /// <inheritdoc/>
    public int RateLimitPerMinute => GetIntSetting("RateLimitPerMinute", 60);

    /// <inheritdoc/>
    public Dictionary<string, string> CustomSettings => _lazySettings.Value;

    private Dictionary<string, string> LoadSettings()
    {
        LogLoadingSettings(_tenantId);

        // In a real application, this would load from database or configuration file
        // Simulating expensive loading operation
        Thread.Sleep(100);

        var settings = _tenantId.ToLowerInvariant() switch
        {
            "tenant-a" => new Dictionary<string, string>
            {
                ["MaxOrders"] = "5000",
                ["MaxProducts"] = "2000",
                ["IsPremium"] = "true",
                ["RateLimitPerMinute"] = "300",
                ["CustomFeature1"] = "enabled",
                ["CustomFeature2"] = "enabled"
            },
            "tenant-b" => new Dictionary<string, string>
            {
                ["MaxOrders"] = "1000",
                ["MaxProducts"] = "500",
                ["IsPremium"] = "false",
                ["RateLimitPerMinute"] = "60",
                ["CustomFeature1"] = "enabled"
            },
            "tenant-c" => new Dictionary<string, string>
            {
                ["MaxOrders"] = "2000",
                ["MaxProducts"] = "1000",
                ["IsPremium"] = "true",
                ["RateLimitPerMinute"] = "150",
                ["CustomFeature1"] = "enabled",
                ["CustomFeature2"] = "enabled",
                ["CustomFeature3"] = "enabled"
            },
            _ => new Dictionary<string, string>()
        };

        LogSettingsLoaded(_tenantId, settings.Count);

        return settings;
    }

    private int GetIntSetting(string key, int defaultValue)
    {
        if (_lazySettings.Value.TryGetValue(key, out var value) &&
            int.TryParse(value, out var intValue))
        {
            return intValue;
        }

        return defaultValue;
    }

    private bool GetBoolSetting(string key, bool defaultValue)
    {
        if (_lazySettings.Value.TryGetValue(key, out var value) &&
            bool.TryParse(value, out var boolValue))
        {
            return boolValue;
        }

        return defaultValue;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "TenantConfigurationService created for: '{TenantId}'")]
    partial void LogConfigServiceCreated(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Loading configuration settings for tenant: '{TenantId}'")]
    partial void LogLoadingSettings(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Configuration loaded for tenant: '{TenantId}' with {Count} settings")]
    partial void LogSettingsLoaded(string tenantId, int count);
}
