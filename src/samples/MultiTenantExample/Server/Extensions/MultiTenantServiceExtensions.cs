using Blazing.Extensions.DependencyInjection;
using MultiTenantExample.Server.Services;

namespace MultiTenantExample.Server.Extensions;

/// <summary>
/// Extension methods for configuring multi-tenant services.
/// </summary>
public static class MultiTenantServiceExtensions
{
    /// <summary>
    /// Adds multi-tenant services using Blazing.Extensions.DependencyInjection features.
    /// Demonstrates AutoRegister, Service Factories, and Lazy Services.
    /// </summary>
    public static IServiceCollection AddMultiTenantServices(this IServiceCollection services)
    {
        // 1. Add assembly for AutoRegister scanning
        services.AddAssembly(typeof(Program).Assembly);

        // 2. AutoRegister - Automatic service discovery and registration
        // This scans for all [AutoRegister] attributes and registers services automatically
        // Examples: TenantService, TenantMigrationService, TenantValidationService, TenantCacheInitializer
        services.Register();

        // 3. Service Factories - Conditional service creation
        // Register TenantDbContextFactory for creating tenant-specific database contexts
        services.AddSingleton<TenantDbContextFactory>();

        // 4. Lazy Services - Deferred initialization for performance
        // Register tenant configurations as lazy singletons (loaded on-demand)
        services.AddLazyKeyedSingleton<TenantConfigurationService>("tenant-a",
            (provider, key) => new TenantConfigurationService(
                "tenant-a",
                provider.GetRequiredService<ILogger<TenantConfigurationService>>()));

        services.AddLazyKeyedSingleton<TenantConfigurationService>("tenant-b",
            (provider, key) => new TenantConfigurationService(
                "tenant-b",
                provider.GetRequiredService<ILogger<TenantConfigurationService>>()));

        services.AddLazyKeyedSingleton<TenantConfigurationService>("tenant-c",
            (provider, key) => new TenantConfigurationService(
                "tenant-c",
                provider.GetRequiredService<ILogger<TenantConfigurationService>>()));

        return services;
    }
}
