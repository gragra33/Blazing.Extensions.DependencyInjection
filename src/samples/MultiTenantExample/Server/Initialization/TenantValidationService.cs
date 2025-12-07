using Blazing.Extensions.DependencyInjection;
using MultiTenantExample.Shared.Interfaces;

namespace MultiTenantExample.Server.Initialization;

/// <summary>
/// Service to validate tenant configurations on application startup.
/// This demonstrates Async Initialization with dependency ordering.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IAsyncInitializable))]
public sealed partial class TenantValidationService : IAsyncInitializable
{
    private readonly ILogger<TenantValidationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantValidationService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TenantValidationService(ILogger<TenantValidationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Medium priority - runs after database migrations but before cache warmup.
    /// </remarks>
    public int InitializationPriority => 50;

    /// <inheritdoc/>
    /// <remarks>
    /// Depends on TenantMigrationService to ensure database is ready.
    /// </remarks>
    public IEnumerable<Type> DependsOn => new[] { typeof(TenantMigrationService) };

    /// <inheritdoc/>
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        LogValidationStarted();

        try
        {
            var tenantService = serviceProvider.GetRequiredService<ITenantService>();
            var tenants = await tenantService.GetAllTenantsAsync().ConfigureAwait(false);

            var tenantList = tenants.ToList();
            LogValidatingTenants(tenantList.Count);

            var validationErrors = new List<string>();

            foreach (var tenant in tenantList)
            {
                var errors = await ValidateTenantAsync(tenant.Id, serviceProvider).ConfigureAwait(false);
                validationErrors.AddRange(errors);
            }

            if (validationErrors.Count > 0)
            {
                LogValidationWarnings(validationErrors.Count);
                foreach (var error in validationErrors)
                {
                    LogValidationError(error);
                }
            }
            else
            {
                LogValidationCompleted(tenantList.Count);
            }
        }
        catch (Exception ex)
        {
            LogValidationFailed(ex);
            throw;
        }
    }

    private async Task<List<string>> ValidateTenantAsync(string tenantId, IServiceProvider serviceProvider)
    {
        LogValidatingTenant(tenantId);

        var errors = new List<string>();

        try
        {
            var tenantService = serviceProvider.GetRequiredService<ITenantService>();
            var tenant = await tenantService.GetTenantByIdAsync(tenantId).ConfigureAwait(false);

            if (tenant == null)
            {
                errors.Add($"Tenant '{tenantId}' not found");
                return errors;
            }

            // Validate tenant configuration
            if (string.IsNullOrWhiteSpace(tenant.Name))
            {
                errors.Add($"Tenant '{tenantId}' has no name");
            }

            if (string.IsNullOrWhiteSpace(tenant.ConnectionString))
            {
                errors.Add($"Tenant '{tenantId}' has no connection string");
            }

            if (!tenant.IsActive)
            {
                errors.Add($"Tenant '{tenantId}' is inactive");
            }

            if (errors.Count == 0)
            {
                LogTenantValid(tenantId);
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error validating tenant '{tenantId}': {ex.Message}");
            LogTenantValidationError(tenantId, ex);
        }

        return errors;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Starting tenant configuration validation")]
    partial void LogValidationStarted();

    [LoggerMessage(Level = LogLevel.Information, Message = "Validating {Count} tenant configurations")]
    partial void LogValidatingTenants(int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Validating tenant: '{TenantId}'")]
    partial void LogValidatingTenant(string tenantId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Tenant configuration is valid: '{TenantId}'")]
    partial void LogTenantValid(string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{ValidationError}")]
    partial void LogValidationError(string validationError);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tenant validation found {Count} warnings")]
    partial void LogValidationWarnings(int count);

    [LoggerMessage(Level = LogLevel.Information, Message = "Tenant validation completed for {Count} tenants")]
    partial void LogValidationCompleted(int count);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error validating tenant '{TenantId}'")]
    partial void LogTenantValidationError(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Tenant validation failed")]
    partial void LogValidationFailed(Exception exception);
}
