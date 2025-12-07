namespace MultiTenantExample.Server.Middleware;

/// <summary>
/// Middleware to resolve the tenant from the incoming HTTP request.
/// </summary>
public sealed partial class TenantResolutionMiddleware
{
    private const string TenantIdHeader = "X-Tenant-Id";
    private const string TenantIdQueryParam = "tenantId";
    private const string TenantIdItemKey = "TenantId";

    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantResolutionMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Invokes the middleware to resolve the tenant from the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantId = ResolveTenantId(context);

        if (!string.IsNullOrEmpty(tenantId))
        {
            context.Items[TenantIdItemKey] = tenantId;
            LogTenantResolved(tenantId, context.Request.Path);
        }
        else
        {
            LogTenantNotResolved(context.Request.Path);
        }

        await _next(context).ConfigureAwait(false);
    }

    private static string? ResolveTenantId(HttpContext context)
    {
        // Try to get tenant ID from header
        if (context.Request.Headers.TryGetValue(TenantIdHeader, out var headerValue))
        {
            var tenantId = headerValue.ToString();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return tenantId;
            }
        }

        // Try to get tenant ID from query string
        if (context.Request.Query.TryGetValue(TenantIdQueryParam, out var queryValue))
        {
            var tenantId = queryValue.ToString();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                return tenantId;
            }
        }

        // Could also check subdomain, path segment, etc.
        return null;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Tenant '{TenantId}' resolved for request to '{Path}'")]
    partial void LogTenantResolved(string tenantId, string path);

    [LoggerMessage(Level = LogLevel.Warning, Message = "No tenant ID found in request to '{Path}'")]
    partial void LogTenantNotResolved(string path);
}
