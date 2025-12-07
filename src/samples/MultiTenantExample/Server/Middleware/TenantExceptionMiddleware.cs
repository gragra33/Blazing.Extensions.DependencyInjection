namespace MultiTenantExample.Server.Middleware;

/// <summary>
/// Middleware to handle tenant-specific exceptions and provide standardized error responses.
/// </summary>
public sealed partial class TenantExceptionMiddleware
{
    private const string TenantIdItemKey = "TenantId";

    private readonly RequestDelegate _next;
    private readonly ILogger<TenantExceptionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantExceptionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public TenantExceptionMiddleware(RequestDelegate next, ILogger<TenantExceptionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (TenantNotFoundException ex)
        {
            await HandleTenantNotFoundExceptionAsync(context, ex).ConfigureAwait(false);
        }
        catch (TenantAccessDeniedException ex)
        {
            await HandleTenantAccessDeniedExceptionAsync(context, ex).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleGenericExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleTenantNotFoundExceptionAsync(
        HttpContext context,
        TenantNotFoundException exception)
    {
        var tenantId = GetTenantId(context);
        LogTenantNotFound(tenantId, exception);

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Tenant not found",
            message = exception.Message,
            tenantId
        };

        await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
    }

    private async Task HandleTenantAccessDeniedExceptionAsync(
        HttpContext context,
        TenantAccessDeniedException exception)
    {
        var tenantId = GetTenantId(context);
        LogTenantAccessDenied(tenantId, exception);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Access denied",
            message = exception.Message,
            tenantId
        };

        await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
    }

    private async Task HandleGenericExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var tenantId = GetTenantId(context);
        LogGenericError(tenantId, exception);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Internal server error",
            message = "An unexpected error occurred",
            tenantId
        };

        await context.Response.WriteAsJsonAsync(response).ConfigureAwait(false);
    }

    private static string GetTenantId(HttpContext context)
    {
        if (context.Items.TryGetValue(TenantIdItemKey, out var tenantIdObj) &&
            tenantIdObj is string tenantId)
        {
            return tenantId;
        }

        return "unknown";
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tenant not found: '{TenantId}'")]
    partial void LogTenantNotFound(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Tenant access denied: '{TenantId}'")]
    partial void LogTenantAccessDenied(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception for tenant: '{TenantId}'")]
    partial void LogGenericError(string tenantId, Exception exception);
}

/// <summary>
/// Exception thrown when a tenant is not found.
/// </summary>
public sealed class TenantNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantNotFoundException"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant ID that was not found.</param>
    public TenantNotFoundException(string tenantId)
        : base($"Tenant '{tenantId}' was not found")
    {
        TenantId = tenantId;
    }

    /// <summary>
    /// Gets the tenant ID that was not found.
    /// </summary>
    public string TenantId { get; }
}

/// <summary>
/// Exception thrown when access to a tenant is denied.
/// </summary>
public sealed class TenantAccessDeniedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TenantAccessDeniedException"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant ID for which access was denied.</param>
    /// <param name="reason">The reason for access denial.</param>
    public TenantAccessDeniedException(string tenantId, string reason)
        : base($"Access denied to tenant '{tenantId}': {reason}")
    {
        TenantId = tenantId;
        Reason = reason;
    }

    /// <summary>
    /// Gets the tenant ID for which access was denied.
    /// </summary>
    public string TenantId { get; }

    /// <summary>
    /// Gets the reason for access denial.
    /// </summary>
    public string Reason { get; }
}
