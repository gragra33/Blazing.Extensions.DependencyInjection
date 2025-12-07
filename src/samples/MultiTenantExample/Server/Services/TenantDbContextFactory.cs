using MultiTenantExample.Shared.Interfaces;

namespace MultiTenantExample.Server.Services;

/// <summary>
/// Factory for creating tenant-specific database contexts.
/// This demonstrates Service Factories feature of Blazing.Extensions.DependencyInjection.
/// </summary>
public sealed partial class TenantDbContextFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TenantDbContextFactory> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContextFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="logger">The logger instance.</param>
    public TenantDbContextFactory(
        IServiceProvider serviceProvider,
        ILogger<TenantDbContextFactory> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a database context for the specified tenant.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <returns>A tenant-specific database context.</returns>
    public ITenantDbContext CreateContext(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));
        }

        LogCreatingContext(tenantId);

        // In a real application, this would create an Entity Framework DbContext
        // with the tenant's connection string
        var logger = _serviceProvider.GetRequiredService<ILogger<TenantDbContext>>();
        return new TenantDbContext(tenantId, logger);
    }

    /// <summary>
    /// Creates a database context for the specified tenant asynchronously.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A tenant-specific database context.</returns>
    public Task<ITenantDbContext> CreateContextAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var context = CreateContext(tenantId);
        return Task.FromResult(context);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Creating database context for tenant: '{TenantId}'")]
    partial void LogCreatingContext(string tenantId);
}
