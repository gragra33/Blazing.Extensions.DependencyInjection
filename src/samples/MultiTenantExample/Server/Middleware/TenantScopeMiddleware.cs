using MultiTenantExample.Shared.Interfaces;

namespace MultiTenantExample.Server.Middleware;

/// <summary>
/// Middleware to manage tenant-scoped services for the current request.
/// This demonstrates the Service Scoping feature of Blazing.Extensions.DependencyInjection.
/// </summary>
public sealed partial class TenantScopeMiddleware
{
    private const string TenantIdItemKey = "TenantId";

    private readonly RequestDelegate _next;
    private readonly ILogger<TenantScopeMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantScopeMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public TenantScopeMiddleware(RequestDelegate next, ILogger<TenantScopeMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to create a tenant-specific service scope.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="tenantService">The tenant service.</param>
    public async Task InvokeAsync(
        HttpContext context,
        ITenantService tenantService)
    {
        if (!context.Items.TryGetValue(TenantIdItemKey, out var tenantIdObj) ||
            tenantIdObj is not string tenantId ||
            string.IsNullOrWhiteSpace(tenantId))
        {
            // No tenant ID - continue without tenant scope
            await _next(context).ConfigureAwait(false);
            return;
        }

        // Validate tenant exists and is active
        var isValid = await tenantService.ValidateTenantAsync(tenantId).ConfigureAwait(false);
        if (!isValid)
        {
            LogInvalidTenant(tenantId);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(
                new { error = $"Tenant '{tenantId}' is not valid or inactive" }).ConfigureAwait(false);
            return;
        }

        // Create tenant-scoped services using Blazing.Extensions.DependencyInjection
        // This demonstrates the GetScopedKeyedService feature
        try
        {
            LogCreatingTenantScope(tenantId);

            // Use async scope for proper disposal
            await using var scope = context.RequestServices.CreateAsyncScope();
            
            // Store scope in context for controllers to use
            context.Items["TenantScope"] = scope;

            await _next(context).ConfigureAwait(false);

            LogTenantScopeCompleted(tenantId);
        }
        catch (Exception ex)
        {
            LogTenantScopeError(tenantId, ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invalid or inactive tenant: '{TenantId}'")]
    partial void LogInvalidTenant(string tenantId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Creating tenant scope for '{TenantId}'")]
    partial void LogCreatingTenantScope(string tenantId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Tenant scope completed for '{TenantId}'")]
    partial void LogTenantScopeCompleted(string tenantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Error in tenant scope for '{TenantId}'")]
    partial void LogTenantScopeError(string tenantId, Exception exception);
}
