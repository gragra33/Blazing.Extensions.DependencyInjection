using Blazing.Extensions.DependencyInjection;
using MultiTenantExample.Shared.Interfaces;
using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Server.Services;

/// <summary>
/// Service for managing tenants in the multi-tenant system.
/// Registered with AutoRegister attribute for automatic discovery.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(ITenantService))]
public sealed partial class TenantService : ITenantService
{
    private readonly ILogger<TenantService> _logger;
    private readonly Dictionary<string, Tenant> _tenants;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TenantService(ILogger<TenantService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize with sample tenants for demonstration
        _tenants = new Dictionary<string, Tenant>(StringComparer.OrdinalIgnoreCase)
        {
            ["tenant-a"] = new Tenant
            {
                Id = "tenant-a",
                Name = "Contoso Corporation",
                ConnectionString = "Data Source=contoso.db",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30),
                Settings = new Dictionary<string, string>
                {
                    ["MaxUsers"] = "100",
                    ["StorageQuotaGB"] = "50"
                }
            },
            ["tenant-b"] = new Tenant
            {
                Id = "tenant-b",
                Name = "Fabrikam Industries",
                ConnectionString = "Data Source=fabrikam.db",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                Settings = new Dictionary<string, string>
                {
                    ["MaxUsers"] = "50",
                    ["StorageQuotaGB"] = "25"
                }
            },
            ["tenant-c"] = new Tenant
            {
                Id = "tenant-c",
                Name = "Adventure Works",
                ConnectionString = "Data Source=adventureworks.db",
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                Settings = new Dictionary<string, string>
                {
                    ["MaxUsers"] = "200",
                    ["StorageQuotaGB"] = "100"
                }
            }
        };
    }

    /// <inheritdoc/>
    public Task<IEnumerable<Tenant>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        LogGettingAllTenants(_tenants.Count);

        var activeTenants = _tenants.Values.Where(t => t.IsActive).ToList();
        return Task.FromResult<IEnumerable<Tenant>>(activeTenants);
    }

    /// <inheritdoc/>
    public Task<Tenant?> GetTenantByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));
        }

        _tenants.TryGetValue(tenantId, out var tenant);

        if (tenant != null)
        {
            LogTenantFound(tenantId, tenant.Name);
        }
        else
        {
            LogTenantNotFound(tenantId);
        }

        return Task.FromResult(tenant);
    }

    /// <inheritdoc/>
    public async Task<bool> ValidateTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            return false;
        }

        var tenant = await GetTenantByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        var isValid = tenant is { IsActive: true };

        if (isValid)
        {
            LogTenantValid(tenantId);
        }
        else
        {
            LogTenantInvalid(tenantId);
        }

        return isValid;
    }

    /// <inheritdoc/>
    public Task<Tenant> CreateTenantAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null)
        {
            throw new ArgumentNullException(nameof(tenant));
        }

        if (_tenants.ContainsKey(tenant.Id))
        {
            throw new InvalidOperationException($"Tenant with ID '{tenant.Id}' already exists");
        }

        tenant.CreatedAt = DateTime.UtcNow;
        _tenants[tenant.Id] = tenant;

        LogTenantCreated(tenant.Id, tenant.Name);

        return Task.FromResult(tenant);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting all tenants. Total count: {Count}")]
    partial void LogGettingAllTenants(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Tenant found: '{TenantId}' - {TenantName}")]
    partial void LogTenantFound(string tenantId, string tenantName);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tenant not found: '{TenantId}'")]
    partial void LogTenantNotFound(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tenant is valid: '{TenantId}'")]
    partial void LogTenantValid(string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tenant is invalid or inactive: '{TenantId}'")]
    partial void LogTenantInvalid(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tenant created: '{TenantId}' - {TenantName}")]
    partial void LogTenantCreated(string tenantId, string tenantName);
}
