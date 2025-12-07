using Microsoft.AspNetCore.Components.Web;
using MultiTenantExample.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient with base address
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

// ========================================================================
// BLAZING.EXTENSIONS.DEPENDENCYINJECTION - Client-side Configuration
// ========================================================================

// 1. Add assembly for AutoRegister scanning
builder.Services.AddAssembly(typeof(Program).Assembly);

// 2. AutoRegister - Automatic service discovery and registration
// This scans for all [AutoRegister] attributes in the client assembly
// Examples: TenantApiService, TenantStateService
builder.Services.Register();

// 3. Service Validation - Detect configuration issues at startup
var diagnostics = builder.Services.GetDiagnostics();
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine("MultiTenantExample Client - Service Diagnostics");
Console.WriteLine("=".PadRight(70, '='));
Console.WriteLine($"Total Services: {diagnostics.TotalServices}");
Console.WriteLine($"Singletons: {diagnostics.SingletonCount}");
Console.WriteLine($"Scoped: {diagnostics.ScopedCount}");
Console.WriteLine($"Transient: {diagnostics.TransientCount}");

if (diagnostics.Warnings.Count > 0)
{
    Console.WriteLine($"Warnings: {diagnostics.Warnings.Count}");
    foreach (var warning in diagnostics.Warnings)
    {
        Console.WriteLine($"  - {warning}");
    }
}

Console.WriteLine("=".PadRight(70, '='));

await builder.Build().RunAsync();
