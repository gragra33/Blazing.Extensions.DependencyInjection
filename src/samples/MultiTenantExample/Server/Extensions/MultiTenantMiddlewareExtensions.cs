using MultiTenantExample.Server.Middleware;

namespace MultiTenantExample.Server.Extensions;

/// <summary>
/// Extension methods for configuring multi-tenant middleware pipeline.
/// </summary>
public static class MultiTenantMiddlewareExtensions
{
    /// <summary>
    /// Adds multi-tenant middleware to the application pipeline.
    /// Middleware order: TenantResolution -> TenantException -> TenantScope
    /// </summary>
    public static IApplicationBuilder UseMultiTenantMiddleware(this IApplicationBuilder app)
    {
        // 6. Tenant Resolution Middleware - Extract tenant from request
        app.UseMiddleware<TenantResolutionMiddleware>();

        // 7. Tenant Exception Middleware - Handle tenant-specific errors
        app.UseMiddleware<TenantExceptionMiddleware>();

        // 8. Tenant Scope Middleware - Create tenant-scoped services
        // This demonstrates Service Scoping feature
        app.UseMiddleware<TenantScopeMiddleware>();

        return app;
    }
}
