using Blazing.Extensions.DependencyInjection;
using MultiTenantExample.Shared.Interfaces;

namespace MultiTenantExample.Server.Initialization;

/// <summary>
/// Service to initialize and migrate tenant databases on application startup.
/// This demonstrates Async Initialization feature of Blazing.Extensions.DependencyInjection.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IAsyncInitializable))]
public sealed partial class TenantMigrationService : IAsyncInitializable
{
    private readonly ILogger<TenantMigrationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantMigrationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TenantMigrationService(ILogger<TenantMigrationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// High priority to ensure database migrations run first.
    /// </remarks>
    public int InitializationPriority => 100;

    /// <inheritdoc/>
    public IEnumerable<Type>? DependsOn => null;

    /// <inheritdoc/>
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        LogMigrationStarted();

        try
        {
            var tenantService = serviceProvider.GetRequiredService<ITenantService>();
            var tenants = await tenantService.GetAllTenantsAsync().ConfigureAwait(false);

            var tenantList = tenants.ToList();
            LogMigratingTenants(tenantList.Count);

            foreach (var tenant in tenantList)
            {
                await MigrateTenantDatabaseAsync(tenant.Id, serviceProvider).ConfigureAwait(false);
            }

            LogMigrationCompleted(tenantList.Count);
        }
        catch (Exception ex)
        {
            LogMigrationFailed(ex);
            throw;
        }
    }

    private async Task MigrateTenantDatabaseAsync(string tenantId, IServiceProvider serviceProvider)
    {
        LogMigratingTenant(tenantId);

        // In a real application, this would use Entity Framework migrations
        // For demo purposes, we just simulate the migration
        await Task.Delay(50).ConfigureAwait(false);

        LogTenantMigrated(tenantId);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting tenant database migrations")]
    partial void LogMigrationStarted();

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrating {Count} tenant databases")]
    partial void LogMigratingTenants(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Migrating database for tenant: '{TenantId}'")]
    partial void LogMigratingTenant(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Database migrated for tenant: '{TenantId}'")]
    partial void LogTenantMigrated(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tenant database migrations completed for {Count} tenants")]
    partial void LogMigrationCompleted(int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Tenant database migration failed")]
    partial void LogMigrationFailed(Exception exception);
}
