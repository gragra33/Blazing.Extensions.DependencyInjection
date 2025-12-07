using Microsoft.OpenApi.Models;

namespace MultiTenantExample.Server.Extensions;

/// <summary>
/// Extension methods for configuring API documentation services.
/// </summary>
public static class SwaggerServiceExtensions
{
    /// <summary>
    /// Adds Swagger/OpenAPI documentation with multi-tenant support.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "MultiTenant Example API",
                Version = "v1",
                Description = "Multi-tenant API demonstrating Blazing.Extensions.DependencyInjection features"
            });

            // Add support for X-Tenant-Id header
            c.AddSecurityDefinition("TenantId", new OpenApiSecurityScheme
            {
                Description = "Tenant identifier for multi-tenant operations. Use one of: tenant-a, tenant-b, tenant-c",
                Name = "X-Tenant-Id",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "TenantIdScheme"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "TenantId"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger UI middleware.
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }
}
