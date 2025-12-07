using Blazing.Extensions.DependencyInjection;
using MultiTenantExample.Shared.Interfaces;

namespace MultiTenantExample.Server.Initialization;

/// <summary>
/// Service to warm up caches for all tenants on application startup.
/// This demonstrates Async Initialization with priority ordering and dependencies.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IAsyncInitializable))]
public sealed partial class TenantCacheInitializer : IAsyncInitializable
{
    private readonly ILogger<TenantCacheInitializer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantCacheInitializer"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TenantCacheInitializer(ILogger<TenantCacheInitializer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Lower priority - runs last after database and validation.
    /// </remarks>
    public int InitializationPriority => 10;

    /// <inheritdoc/>
    /// <remarks>
    /// Depends on validation to ensure all tenants are valid before caching.
    /// </remarks>
    public IEnumerable<Type> DependsOn => new[]
    {
        typeof(TenantMigrationService),
        typeof(TenantValidationService)
    };

    /// <inheritdoc/>
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        LogCacheWarmupStarted();

        try
        {
            var tenantService = serviceProvider.GetRequiredService<ITenantService>();
            var tenants = await tenantService.GetAllTenantsAsync().ConfigureAwait(false);

            var tenantList = tenants.ToList();
            LogWarmingCaches(tenantList.Count);

            // Warm up caches in parallel for better performance
            var warmupTasks = tenantList.Select(tenant =>
                WarmupTenantCacheAsync(tenant.Id, serviceProvider));

            await Task.WhenAll(warmupTasks).ConfigureAwait(false);

            LogCacheWarmupCompleted(tenantList.Count);
        }
        catch (Exception ex)
        {
            LogCacheWarmupFailed(ex);
            throw;
        }
    }

    private async Task WarmupTenantCacheAsync(string tenantId, IServiceProvider serviceProvider)
    {
        LogWarmingTenantCache(tenantId);

        try
        {
            // In a real application, this would:
            // 1. Load frequently accessed data into cache
            // 2. Pre-compute expensive queries
            // 3. Initialize connection pools
            
            // Simulate cache warmup
            await Task.Delay(100).ConfigureAwait(false);

            LogTenantCacheWarmed(tenantId);
        }
        catch (Exception ex)
        {
            LogTenantCacheWarmupError(tenantId, ex);
            // Don't throw - cache warmup failures shouldn't prevent startup
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting tenant cache warmup")]
    partial void LogCacheWarmupStarted();

    [LoggerMessage(Level = LogLevel.Information, Message = "Warming up caches for {Count} tenants")]
    partial void LogWarmingCaches(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Warming up cache for tenant: '{TenantId}'")]
    partial void LogWarmingTenantCache(string tenantId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Cache warmed for tenant: '{TenantId}'")]
    partial void LogTenantCacheWarmed(string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Cache warmup error for tenant '{TenantId}'")]
    partial void LogTenantCacheWarmupError(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tenant cache warmup completed for {Count} tenants")]
    partial void LogCacheWarmupCompleted(int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Tenant cache warmup failed")]
    partial void LogCacheWarmupFailed(Exception exception);
}
