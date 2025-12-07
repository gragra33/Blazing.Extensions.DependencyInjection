using Microsoft.AspNetCore.Mvc;
using MultiTenantExample.Server.Services;
using MultiTenantExample.Shared.DTOs;
using MultiTenantExample.Shared.Interfaces;
using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Server.Controllers;

/// <summary>
/// API controller for tenant management operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed partial class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<TenantsController> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantsController"/> class.
    /// </summary>
    /// <param name="tenantService">The tenant service.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="serviceProvider">The service provider for resolving keyed services.</param>
    public TenantsController(
        ITenantService tenantService,
        ILogger<TenantsController> logger,
        IServiceProvider serviceProvider)
    {
        _tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Gets all tenants.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A collection of all tenants.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Tenant>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Tenant>>>> GetAllTenants(
        CancellationToken cancellationToken)
    {
        LogGettingAllTenants();

        try
        {
            var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken).ConfigureAwait(false);
            return Ok(ApiResponse<IEnumerable<Tenant>>.SuccessResponse(tenants));
        }
        catch (Exception ex)
        {
            LogGetAllTenantsFailed(ex);
            return StatusCode(500, ApiResponse<IEnumerable<Tenant>>.FailureResponse("Failed to retrieve tenants"));
        }
    }

    /// <summary>
    /// Gets a tenant by ID.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The tenant if found; otherwise, not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Tenant>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Tenant>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Tenant>>> GetTenant(
        string id,
        CancellationToken cancellationToken)
    {
        LogGettingTenant(id);

        try
        {
            var tenant = await _tenantService.GetTenantByIdAsync(id, cancellationToken).ConfigureAwait(false);

            if (tenant == null)
            {
                LogTenantNotFound(id);
                return NotFound(ApiResponse<Tenant>.FailureResponse($"Tenant '{id}' not found"));
            }

            return Ok(ApiResponse<Tenant>.SuccessResponse(tenant, tenant.Id));
        }
        catch (Exception ex)
        {
            LogGetTenantFailed(id, ex);
            return StatusCode(500, ApiResponse<Tenant>.FailureResponse("Failed to retrieve tenant"));
        }
    }

    /// <summary>
    /// Validates whether a tenant exists and is active.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>True if the tenant is valid; otherwise, false.</returns>
    [HttpGet("{id}/validate")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateTenant(
        string id,
        CancellationToken cancellationToken)
    {
        LogValidatingTenant(id);

        try
        {
            var isValid = await _tenantService.ValidateTenantAsync(id, cancellationToken).ConfigureAwait(false);
            return Ok(ApiResponse<bool>.SuccessResponse(isValid, id));
        }
        catch (Exception ex)
        {
            LogValidateTenantFailed(id, ex);
            return StatusCode(500, ApiResponse<bool>.FailureResponse("Failed to validate tenant"));
        }
    }

    /// <summary>
    /// Gets the configuration for a specific tenant.
    /// This demonstrates lazy initialization - the configuration is only loaded on first access.
    /// </summary>
    /// <param name="id">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The tenant configuration including custom settings.</returns>
    [HttpGet("{id}/configuration")]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, object>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Dictionary<string, object>>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Dictionary<string, object>>>> GetTenantConfiguration(
        string id,
        CancellationToken cancellationToken)
    {
        LogGettingTenantConfiguration(id);

        try
        {
            // First validate the tenant exists
            var isValid = await _tenantService.ValidateTenantAsync(id, cancellationToken).ConfigureAwait(false);
            if (!isValid)
            {
                LogTenantNotFound(id);
                return NotFound(ApiResponse<Dictionary<string, object>>.FailureResponse($"Tenant '{id}' not found"));
            }

            // Get the lazy configuration service - this demonstrates lazy initialization
            var lazyConfig = _serviceProvider.GetRequiredKeyedService<Lazy<TenantConfigurationService>>(id);
            
            // Access the Value property triggers lazy initialization on first call
            var config = lazyConfig.Value;
            
            var response = new Dictionary<string, object>
            {
                ["tenantId"] = config.TenantId,
                ["maxOrders"] = config.MaxOrders,
                ["maxProducts"] = config.MaxProducts,
                ["isPremium"] = config.IsPremium,
                ["rateLimitPerMinute"] = config.RateLimitPerMinute,
                ["customSettings"] = config.CustomSettings,
                ["isLazyLoaded"] = true,
                ["wasValueCreated"] = lazyConfig.IsValueCreated
            };

            return Ok(ApiResponse<Dictionary<string, object>>.SuccessResponse(response, id));
        }
        catch (Exception ex)
        {
            LogGetTenantConfigurationFailed(id, ex);
            return StatusCode(500, ApiResponse<Dictionary<string, object>>.FailureResponse("Failed to retrieve tenant configuration"));
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting all tenants")]
    partial void LogGettingAllTenants();

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get all tenants")]
    partial void LogGetAllTenantsFailed(Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting tenant: '{TenantId}'")]
    partial void LogGettingTenant(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting tenant configuration: '{TenantId}'")]
    partial void LogGettingTenantConfiguration(string tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tenant not found: '{TenantId}'")]
    partial void LogTenantNotFound(string tenantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get tenant: '{TenantId}'")]
    partial void LogGetTenantFailed(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get tenant configuration: '{TenantId}'")]
    partial void LogGetTenantConfigurationFailed(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Validating tenant: '{TenantId}'")]
    partial void LogValidatingTenant(string tenantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to validate tenant: '{TenantId}'")]
    partial void LogValidateTenantFailed(string tenantId, Exception exception);
}
