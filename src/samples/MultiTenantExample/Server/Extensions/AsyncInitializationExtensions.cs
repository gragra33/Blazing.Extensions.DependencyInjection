using Blazing.Extensions.DependencyInjection;

namespace MultiTenantExample.Server.Extensions;

/// <summary>
/// Extension methods for async initialization of services.
/// </summary>
public static class AsyncInitializationExtensions
{
    /// <summary>
    /// Initializes all async services in priority order and displays startup information.
    /// Priority 100: TenantMigrationService (database migrations)
    /// Priority 50: TenantValidationService (configuration validation)
    /// Priority 10: TenantCacheInitializer (cache warmup)
    /// </summary>
    public static async Task InitializeServicesAsync(this IServiceProvider services)
    {
        Console.WriteLine("\n--- Starting Async Initialization ---");

        // Display initialization order
        var initOrder = services.GetInitializationOrder();
        if (initOrder.Steps.Any())
        {
            Console.WriteLine("\nPlanned Initialization Order:");
            foreach (var step in initOrder.Steps.OrderByDescending(s => s.Priority))
            {
                Console.WriteLine($"  {step.Order}. {step.ServiceType.Name} (Priority: {step.Priority})");
            }
            Console.WriteLine();
        }

        // Execute initialization
        await services.InitializeAllAsync();
        
        Console.WriteLine("--- Async Initialization Complete ---\n");
    }

    /// <summary>
    /// Prints application startup information to the console.
    /// </summary>
    public static void PrintStartupInformation(this WebApplication app)
    {
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("MultiTenantExample Server Started");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine("\nDemonstrates Blazing.Extensions.DependencyInjection features:");
        Console.WriteLine("  + AutoRegister - Automatic service discovery");
        Console.WriteLine("  + Keyed Services - Per-tenant database contexts");
        Console.WriteLine("  + Lazy Initialization - Deferred tenant configuration loading");
        Console.WriteLine("  + Service Factories - Tenant-specific service creation");
        Console.WriteLine("  + Service Scoping - Per-request tenant isolation");
        Console.WriteLine("  + Service Validation - Startup configuration checks");
        Console.WriteLine("  + Async Initialization - Priority-based startup sequence");
        Console.WriteLine("\nAvailable Tenants:");
        Console.WriteLine("  - tenant-a (Contoso Corporation)");
        Console.WriteLine("  - tenant-b (Fabrikam Industries)");
        Console.WriteLine("  - tenant-c (Adventure Works)");
        Console.WriteLine("\nAPI Testing Options:");
        Console.WriteLine("  1. Swagger UI: Navigate to /swagger");
        Console.WriteLine("     - Click 'Authorize' button");
        Console.WriteLine("     - Enter tenant ID (tenant-a, tenant-b, or tenant-c)");
        Console.WriteLine("     - Test endpoints with automatic tenant header");
        Console.WriteLine("\n  2. Manual Requests:");
        Console.WriteLine("     - Add 'X-Tenant-Id: tenant-a' header");
        Console.WriteLine("     - Or use '?tenantId=tenant-a' query parameter");
        Console.WriteLine("\n  3. Blazor Client: Navigate to /");
        Console.WriteLine("     - Select tenant from dropdown");
        Console.WriteLine("     - Browse products and orders per tenant");
        Console.WriteLine("=".PadRight(70, '='));
        Console.WriteLine();
    }
}
