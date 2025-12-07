using MultiTenantExample.Server.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ========================================================================
// SERVICE CONFIGURATION
// ========================================================================

// ASP.NET Core services
builder.Services.AddControllers();
builder.Services.AddSwaggerDocumentation();

// CORS for Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Blazing.Extensions.DependencyInjection features
builder.Services.AddMultiTenantServices();
builder.Services.ValidateAndPrintDiagnostics();

var app = builder.Build();

// ========================================================================
// MIDDLEWARE PIPELINE
// ========================================================================

// Development tools
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
    app.UseWebAssemblyDebugging();
}

// Static files and routing
app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseCors();
app.UseRouting();

// Multi-tenant middleware
app.UseMultiTenantMiddleware();

// Authorization and endpoints
app.UseAuthorization();
app.MapControllers();
app.MapFallbackToFile("index.html");

// ========================================================================
// ASYNC INITIALIZATION & STARTUP
// ========================================================================

await app.Services.InitializeServicesAsync();
app.PrintStartupInformation();

app.Run();
