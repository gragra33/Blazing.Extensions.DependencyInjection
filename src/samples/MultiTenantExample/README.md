# MultiTenantExample - Advanced Multi-Tenant ASP.NET Core Web API

A comprehensive example demonstrating enterprise-grade multi-tenant architecture using **Blazing.Extensions.DependencyInjection** with a Blazor WebAssembly client.

## Table of Contents

- [Overview](#overview)
  - [Technology Stack](#technology-stack)
- [Architecture](#architecture)
- [Features Demonstrated](#features-demonstrated)
  - [1. AutoRegister - Automatic Service Discovery](#1-autoregister---automatic-service-discovery)
  - [2. Keyed Services - Per-Tenant Isolation](#2-keyed-services---per-tenant-isolation)
  - [3. Service Scoping - Lifecycle Management](#3-service-scoping---lifecycle-management)
  - [4. Lazy Initialization - Performance Optimization](#4-lazy-initialization---performance-optimization)
  - [5. Service Factories - Conditional Service Creation](#5-service-factories---conditional-service-creation)
  - [6. Service Validation - Configuration Safety](#6-service-validation---configuration-safety)
  - [7. Async Initialization - Priority-Based Startup](#7-async-initialization---priority-based-startup)
- [Feature Usage Matrix](#feature-usage-matrix)
- [Feature Location Guide](#feature-location-guide)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Running the Application](#running-the-application)
  - [Available Tenants](#available-tenants)
- [Using Swagger UI](#using-swagger-ui)
  - [Access Swagger UI](#1-access-swagger-ui)
  - [Authorize with Tenant ID](#2-authorize-with-tenant-id)
  - [Test API Endpoints](#3-test-api-endpoints)
  - [Switch Tenants](#4-switch-tenants)
  - [Verify Tenant Isolation](#5-verify-tenant-isolation)
  - [Alternative: Using Query Parameters](#alternative-using-query-parameters)
  - [Swagger Troubleshooting](#swagger-troubleshooting)
- [API Usage](#api-usage)
  - [Using Headers (Recommended)](#using-headers-recommended)
  - [Using Query Parameters](#using-query-parameters)
  - [Available Endpoints](#available-endpoints)
- [Key Implementation Details](#key-implementation-details)
  - [Tenant Resolution Flow](#tenant-resolution-flow)
  - [Database Context Management](#database-context-management)
  - [Middleware Order Importance](#middleware-order-importance)
- [Code Quality Features](#code-quality-features)
  - [XML Documentation](#xml-documentation)
  - [Source-Generated Logging](#source-generated-logging)
  - [Cancellation Token Support](#cancellation-token-support)
  - [Nullable Reference Types](#nullable-reference-types)
- [Testing the Features](#testing-the-features)
  - [Test AutoRegister](#test-autoregister)
  - [Test Async Initialization](#test-async-initialization)
  - [Test Lazy Initialization](#test-lazy-initialization)
  - [Test Tenant Isolation](#test-tenant-isolation)
  - [Testing Multi-Tenant Features via Swagger](#testing-multi-tenant-features-via-swagger)
- [Learning Resources](#learning-resources)
- [Production Considerations](#production-considerations)
  - [Security](#security)
  - [Performance](#performance)
  - [Reliability](#reliability)
  - [Scalability](#scalability)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)

## Overview

This sample application showcases real-world multi-tenant patterns combining multiple advanced features of Blazing.Extensions.DependencyInjection in a cohesive, production-ready architecture.

### Technology Stack

- **Server**: ASP.NET Core Web API (.NET 8.0, 9.0, 10.0)
- **Client**: Blazor WebAssembly (.NET 8.0, 9.0, 10.0)
- **Shared**: .NET Class Library (.NET 8.0, 9.0, 10.0)
- **DI Library**: Blazing.Extensions.DependencyInjection

## Architecture

```
MultiTenantExample/
├── Server/                              # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── TenantsController.cs         # Tenant management API
│   │   ├── OrdersController.cs          # Multi-tenant orders API
│   │   └── ProductsController.cs        # Multi-tenant products API
│   ├── Initialization/
│   │   ├── TenantMigrationService.cs
│   │   ├── TenantValidationService.cs
│   │   └── TenantCacheInitializer.cs
│   ├── Middleware/
│   │   ├── TenantResolutionMiddleware.cs
│   │   ├── TenantScopeMiddleware.cs
│   │   └── TenantExceptionMiddleware.cs
│   ├── Services/
│   │   ├── TenantService.cs
│   │   ├── TenantDbContext.cs
│   │   ├── TenantDbContextFactory.cs
│   │   └── TenantConfigurationService.cs
│   └── Program.cs
├── Client/                              # Blazor WebAssembly
│   ├── Pages/
│   │   ├── About.razor
│   │   ├── Home.razor
│   │   ├── Orders.razor
│   │   └── Products.razor
│   ├── Services/
│   │   ├── TenantApiService.cs
│   │   └── TenantStateService.cs
│   └── Program.cs
└── Shared/                              # Shared models and interfaces
    ├── Models/
    │   ├── Tenant.cs
    │   ├── Order.cs
    │   └── Product.cs
    ├── Interfaces/
    │   ├── ITenantService.cs
    │   ├── ITenantDbContext.cs
    │   └── ITenantConfig.cs
    └── DTOs/
        ├── ApiResponse.cs
        ├── OrderDTOs.cs
        └── ProductDTOs.cs
```

## Features Demonstrated

### 1. AutoRegister - Automatic Service Discovery

**What It Is:** Attribute-based service registration that automatically scans assemblies for services marked with `[AutoRegister]`.

**Implementation:**

```csharp
// Mark services with AutoRegister attribute
[AutoRegister(ServiceLifetime.Singleton, typeof(ITenantService))]
public sealed class TenantService : ITenantService { }

[AutoRegister(ServiceLifetime.Singleton, typeof(IAsyncInitializable))]
public sealed class TenantMigrationService : IAsyncInitializable { }

// In Program.cs
builder.Services.AddAssembly(typeof(Program).Assembly);
builder.Services.Register();  // Discovers and registers all marked services
```

**Services Using AutoRegister:**
- `TenantService` - Tenant management
- `TenantMigrationService` - Database migrations
- `TenantValidationService` - Configuration validation
- `TenantCacheInitializer` - Cache warmup
- `TenantApiService` (Client) - API calls
- `TenantStateService` (Client) - Tenant state management

**Files:**
- `Server/Services/TenantService.cs`
- `Server/Initialization/TenantMigrationService.cs`
- `Server/Initialization/TenantValidationService.cs`
- `Server/Initialization/TenantCacheInitializer.cs`
- `Client/Services/TenantApiService.cs`
- `Client/Services/TenantStateService.cs`

### 2. Keyed Services - Per-Tenant Isolation

**What It Is:** Multiple implementations of the same interface registered with different keys for runtime selection.

**Implementation:**

```csharp
// Register tenant-specific configurations with keys
builder.Services.AddLazyKeyedSingleton<TenantConfigurationService>("tenant-a",
    (provider, key) => new TenantConfigurationService("tenant-a", logger));
    
builder.Services.AddLazyKeyedSingleton<TenantConfigurationService>("tenant-b",
    (provider, key) => new TenantConfigurationService("tenant-b", logger));

// Resolve specific tenant configuration
var config = serviceProvider.GetRequiredKeyedService<ITenantConfig>("tenant-a");
```

**Use Cases:**
- Per-tenant database contexts
- Tenant-specific configuration settings
- Isolated tenant data access

**Location:** `Server/Program.cs` lines 40-55

### 3. Service Scoping - Lifecycle Management

**What It Is:** Creating isolated service scopes for request-level or operation-level service resolution.

**Implementation:**

```csharp
// TenantScopeMiddleware.cs
public async Task InvokeAsync(HttpContext context, ...)
{
    // Create async scope for tenant-specific services
    await using var scope = context.RequestServices.CreateAsyncScope();
    context.Items["TenantScope"] = scope;
    await _next(context);
}

// In controller
await using var dbContext = await _dbContextFactory
    .CreateContextAsync(tenantId, cancellationToken);
```

**Benefits:**
- Proper resource disposal
- Request-level isolation
- Prevents memory leaks

**Location:** `Server/Middleware/TenantScopeMiddleware.cs`

### 4. Lazy Initialization - Performance Optimization

**What It Is:** Deferred service creation until first access, improving startup time and memory efficiency.

**Implementation:**

```csharp
// TenantConfigurationService.cs - Settings loaded lazily
private readonly Lazy<Dictionary<string, string>> _lazySettings;

public TenantConfigurationService(string tenantId, ILogger logger)
{
    _lazySettings = new Lazy<Dictionary<string, string>>(LoadSettings);
}

public Dictionary<string, string> CustomSettings => _lazySettings.Value;
```

**Benefits:**
- Faster application startup
- Lower memory footprint
- Configuration loaded only when needed

**Location:** `Server/Services/TenantConfigurationService.cs`

### 5. Service Factories - Conditional Service Creation

**What It Is:** Factory pattern for creating services based on runtime conditions or parameters.

**Implementation:**

```csharp
// TenantDbContextFactory.cs
public sealed class TenantDbContextFactory
{
    public ITenantDbContext CreateContext(String tenantId)
    {
        // Create tenant-specific database context
        return new TenantDbContext(tenantId, logger);
    }
}

// Usage in controllers
await using var dbContext = await _dbContextFactory
    .CreateContextAsync(tenantId, cancellationToken);
```

**Use Cases:**
- Tenant-specific database connections
- Environment-based service selection
- Dynamic service configuration

**Location:** `Server/Services/TenantDbContextFactory.cs`

### 6. Service Validation - Configuration Safety

**What It Is:** Startup validation to detect configuration issues, circular dependencies, and lifetime violations.

**Implementation:**

```csharp
// Program.cs
var diagnostics = builder.Services.GetDiagnostics();
Console.WriteLine($"Total Services: {diagnostics.TotalServices}");
Console.WriteLine($"Singletons: {diagnostics.SingletonCount}");

try
{
    builder.Services.ThrowIfInvalid();
    Console.WriteLine("✅ Service collection validation passed");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"❌ Validation failed: {ex.Message}");
    throw;
}
```

**Checks:**
- Circular dependencies
- Lifetime compatibility violations
- Duplicate registrations
- Service dependency graph

**Location:** `Server/Program.cs` lines 58-77

### 7. Async Initialization - Priority-Based Startup

**What It Is:** Declarative async initialization with dependency ordering and priority-based execution.

**Implementation:**

```csharp
// TenantMigrationService.cs
[AutoRegister(ServiceLifetime.Singleton, typeof(IAsyncInitializable))]
public sealed class TenantMigrationService : IAsyncInitializable
{
    public int InitializationPriority => 100;  // Highest priority
    public IEnumerable<Type>? DependsOn => null;
    
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Migrate all tenant databases
    }
}

// TenantValidationService.cs
public int InitializationPriority => 50;  // Medium priority
public IEnumerable<Type> DependsOn => new[] { typeof(TenantMigrationService) };

// TenantCacheInitializer.cs
public int InitializationPriority => 10;  // Lower priority
public IEnumerable<Type> DependsOn => new[]
{
    typeof(TenantMigrationService),
    typeof(TenantValidationService)
};

// In Program.cs
await app.Services.InitializeAllAsync();
```

**Execution Order:**
1. **Priority 100**: `TenantMigrationService` - Database migrations
2. **Priority 50**: `TenantValidationService` - Configuration validation (after migrations)
3. **Priority 10**: `TenantCacheInitializer` - Cache warmup (after validation)

**Location:** All classes in `Server/Initialization/`, initialized in `Server/Program.cs` lines 111-124

## Feature Usage Matrix

| Feature | Server | Client | Shared |
|---------|--------|--------|--------|
| AutoRegister | ✅ | ✅ | ❌ |
| Keyed Services | ✅ | ❌ | ❌ |
| Service Scoping | ✅ | ❌ | ❌ |
| Lazy Initialization | ✅ | ❌ | ❌ |
| Service Factories | ✅ | ❌ | ❌ |
| Service Validation | ✅ | ✅ | ❌ |
| Async Initialization | ✅ | ❌ | ❌ |

## Feature Location Guide

| Feature | Primary File | Line Reference |
|---------|-------------|----------------|
| AutoRegister Setup | `Server/Program.cs` | Lines 24-27 |
| Keyed Services | `Server/Program.cs` | Lines 40-55 |
| Service Validation | `Server/Program.cs` | Lines 58-77 |
| Async Initialization | `Server/Program.cs` | Lines 111-124 |
| Service Scoping | `Server/Middleware/TenantScopeMiddleware.cs` | Lines 60-75 |
| Lazy Initialization | `Server/Services/TenantConfigurationService.cs` | Lines 23-26 |
| Service Factories | `Server/Services/TenantDbContextFactory.cs` | Lines 33-45 |

## Getting Started

### Prerequisites

- .NET 8.0, 9.0, or 10.0 SDK
- Visual Studio 2022 or VS Code
- Basic understanding of ASP.NET Core and Blazor

### Running the Application

1. **Clone the repository:**
   ```bash
   cd src/samples/MultiTenantExample
   ```

2. **Build the solution:**
   ```bash
   dotnet build
   ```

3. **Run the server:**
   ```bash
   dotnet run --project Server
   ```

4. **Access the application:**
   - Open your browser to: `https://localhost:5001`
   - Or check the console output for the actual URL

### Available Tenants

The application comes pre-configured with three sample tenants:

| Tenant ID | Name | Description |
|-----------|------|-------------|
| `tenant-a` | Contoso Corporation | Enterprise customer with premium features |
| `tenant-b` | Fabrikam Industries | Standard customer with basic features |
| `tenant-c` | Adventure Works | Premium customer with extended features |

## Using Swagger UI

### 1. Access Swagger UI
Navigate to: `https://localhost:52409/swagger` (port may vary based on your configuration)

### 2. Authorize with Tenant ID

1. Click the **🔓 Authorize** button at the top right of the Swagger UI
2. In the "TenantId (apiKey)" dialog, enter one of the tenant IDs:
   - `tenant-a`
   - `tenant-b`
   - `tenant-c`
3. Click **Authorize**
4. Click **Close**

The `X-Tenant-Id` header will now be automatically added to all API requests!

### 3. Test API Endpoints

#### Get All Tenants (No Tenant ID Required)
```
GET /api/Tenants
```
This endpoint lists all available tenants.

#### Get Orders for Current Tenant
```
GET /api/Orders
```
Returns orders specific to the tenant you authorized with.

#### Get Products for Current Tenant
```
GET /api/Products
```
Returns products specific to the tenant you authorized with.

#### Create an Order
```
POST /api/Orders
```
Example request body:
```json
{
  "customerName": "John Doe",
  "customerEmail": "john.doe@example.com",
  "items": [
    {
      "productId": 1,
      "quantity": 2
    }
  ]
}
```

### 4. Switch Tenants

To test with a different tenant:
1. Click the **🔓 Authorize** button again
2. Click **Logout** to clear the current tenant
3. Enter a different tenant ID
4. Click **Authorize**

### 5. Verify Tenant Isolation

Try this experiment to see multi-tenancy in action:

1. **Authorize as tenant-a**
   - GET `/api/Products` - Note the products (Enterprise License, Professional Support, etc.)
   - GET `/api/Orders` - Note the orders

2. **Logout and authorize as tenant-b**
   - GET `/api/Products` - See different products (Cloud Storage, Backup Service, etc.)
   - GET `/api/Orders` - See different orders

3. **Logout and authorize as tenant-c**
   - GET `/api/Products` - See different products again (Mountain Bike, Road Bike, etc.)
   - GET `/api/Orders` - See different orders

Each tenant has completely isolated data!

### Alternative: Using Query Parameters

If you prefer not to use the Authorize button, you can append the tenant ID as a query parameter:

```
GET /api/Orders?tenantId=tenant-a
GET /api/Products?tenantId=tenant-b
```

However, using the **Authorize** button is recommended as it:
- Automatically adds the header to all requests
- Is more RESTful (headers vs query params)
- Provides better security practices

### Swagger Troubleshooting

#### "Tenant ID is required" Error
**Problem:** You see `400 Bad Request` with message "Tenant ID is required"

**Solution:** 
1. Click the **Authorize** button in Swagger UI
2. Enter a valid tenant ID
3. Make sure you clicked **Authorize** (not just typed and closed)

#### "Tenant 'xxx' is not valid or inactive" Error
**Problem:** You see `403 Forbidden` with this message

**Solution:**
- Make sure you're using one of the valid tenant IDs: `tenant-a`, `tenant-b`, or `tenant-c`
- Check for typos (IDs are case-sensitive)

#### No Authorize Button Visible
**Problem:** The Authorize button doesn't appear

**Solution:**
- Refresh the Swagger UI page
- Make sure you're accessing `/swagger` not `/swagger/v1/swagger.json`
- Check that the server started successfully

## API Usage

### Using Headers (Recommended)

```bash
curl -H "X-Tenant-Id: tenant-a" https://localhost:5001/api/orders
```

### Using Query Parameters

```bash
curl https://localhost:5001/api/orders?tenantId=tenant-a
```

### Available Endpoints

**Tenants:**
- `GET /api/tenants` - Get all tenants
- `GET /api/tenants/{id}` - Get specific tenant
- `GET /api/tenants/{id}/validate` - Validate tenant
- `GET /api/tenants/{id}/configuration` - Get tenant configuration (demonstrates lazy initialization)

**Orders:**
- `GET /api/orders` - Get tenant orders
- `GET /api/orders/{id}` - Get specific order
- `POST /api/orders` - Create order
- `PUT /api/orders/{id}/status` - Update order status

**Products:**
- `GET /api/products` - Get tenant products
- `GET /api/products/{id}` - Get specific product
- `POST /api/products` - Create product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product (soft delete)

**Understanding API Responses:**

All API responses follow this format:

```json
{
  "success": true,
  "data": { /* your data here */ },
  "errorMessage": null,
  "tenantId": "tenant-a"
}
```

The `tenantId` field in the response confirms which tenant the data belongs to.

## Key Implementation Details

### Tenant Resolution Flow

1. **TenantResolutionMiddleware** extracts tenant ID from:
   - `X-Tenant-Id` header (preferred)
   - `tenantId` query parameter
   - Could be extended for subdomain, path segment, etc.

2. **TenantScopeMiddleware** validates tenant and creates scope:
   - Validates tenant exists and is active
   - Creates async service scope for the request
   - Stores scope in `HttpContext.Items`

3. **TenantExceptionMiddleware** handles tenant-specific errors:
   - `TenantNotFoundException` → 404
   - `TenantAccessDeniedException` → 403
   - Generic exceptions → 500

### Database Context Management

The sample uses **in-memory data stores** for demonstration. In a real application:

```csharp
// Replace TenantDbContext with Entity Framework DbContext
public class TenantDbContext : DbContext, ITenantDbContext
{
    private readonly string _tenantId;
    
    public TenantDbContext(string tenantId, DbContextOptions options)
        : base(options)
    {
        _tenantId = tenantId;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Get tenant-specific connection string
        var connectionString = GetConnectionString(_tenantId);
        optionsBuilder.UseSqlServer(connectionString);
    }
}
```

### Middleware Order Importance

```csharp
app.UseMiddleware<TenantResolutionMiddleware>();    // First: Extract tenant ID
app.UseMiddleware<TenantExceptionMiddleware>();     // Second: Error handling
app.UseMiddleware<TenantScopeMiddleware>();         // Third: Create scope
app.UseAuthorization();                              // Fourth: Authorization
app.MapControllers();                                // Fifth: Route to controllers
```

## Code Quality Features

### XML Documentation

All public members have comprehensive XML documentation:

```csharp
/// <summary>
/// Service for managing tenants in the multi-tenant system.
/// Registered with AutoRegister attribute for automatic discovery.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(ITenantService))]
public sealed partial class TenantService : ITenantService
{
    /// <summary>
    /// Gets all active tenants in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A collection of all active tenants.</returns>
    public Task<IEnumerable<Tenant>> GetAllTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

### Source-Generated Logging

All services use source-generated logging for performance:

```csharp
public sealed partial class TenantService : ITenantService
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Getting all tenants. Total count: {Count}")]
    partial void LogGettingAllTenants(int count);
    
    [LoggerMessage(Level = LogLevel.Warning, Message = "Tenant not found: '{TenantId}'")]
    partial void LogTenantNotFound(string tenantId);
}
```

### Cancellation Token Support

All async methods accept `CancellationToken`:

```csharp
public async Task<IEnumerable<Order>> GetOrdersAsync(
    CancellationToken cancellationToken = default)
{
    await using var dbContext = await _dbContextFactory
        .CreateContextAsync(tenantId, cancellationToken);
    
    return dbContext.Orders.ToList();
}
```

### Nullable Reference Types

All projects have nullable reference types enabled:

```xml
<PropertyGroup>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

## Testing the Features

### Test AutoRegister

Check the console output when the application starts:

```
✅ Service Collection Diagnostics:
  Total Services: 236
  Singletons: 171
  Scoped: 4
  Transient: 61
✅ Service collection validation passed
```

### Test Async Initialization

Check the startup sequence:

```
--- Starting Async Initialization ---

Planned Initialization Order:
  1. TenantMigrationService (Priority: 100)
  2. TenantValidationService (Priority: 50)
  3. TenantCacheInitializer (Priority: 10)

info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Starting tenant database migrations
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Migrating 3 tenant databases
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Migrating database for tenant: 'tenant-a'
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Database migrated for tenant: 'tenant-a'
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Migrating database for tenant: 'tenant-b'
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Database migrated for tenant: 'tenant-b'
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Migrating database for tenant: 'tenant-c'
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Database migrated for tenant: 'tenant-c'
info: MultiTenantExample.Server.Initialization.TenantMigrationService[0]
      Tenant database migrations completed for 3 tenants
info: MultiTenantExample.Server.Initialization.TenantValidationService[0]
      Starting tenant configuration validation
info: MultiTenantExample.Server.Initialization.TenantValidationService[0]
      Validating 3 tenant configurations
info: MultiTenantExample.Server.Initialization.TenantValidationService[0]
      Tenant validation completed for 3 tenants
info: MultiTenantExample.Server.Initialization.TenantCacheInitializer[0]
      Starting tenant cache warmup
info: MultiTenantExample.Server.Initialization.TenantCacheInitializer[0]
      Warming up caches for 3 tenants
info: MultiTenantExample.Server.Initialization.TenantCacheInitializer[0]
      Tenant cache warmup completed for 3 tenants
--- Async Initialization Complete ---

The initialization runs in priority order:
1. **Priority 100** - TenantMigrationService (database migrations first)
2. **Priority 50** - TenantValidationService (validation after migrations)
3. **Priority 10** - TenantCacheInitializer (cache warmup last)
```

### Test Lazy Initialization

The lazy initialization feature defers loading tenant configurations until they're first accessed. To see this in action:

**Using Swagger UI:**

1. **Access Swagger** at `https://localhost:52409/swagger`
2. **Call** `GET /api/tenants/tenant-a/configuration` 
3. **Watch the console logs** - you'll see:

```
info: MultiTenantExample.Server.Controllers.TenantsController[0]
      Getting tenant configuration: 'tenant-a'
info: MultiTenantExample.Server.Services.TenantConfigurationService[0]
      Loading configuration settings for tenant: 'tenant-a'
info: MultiTenantExample.Server.Services.TenantConfigurationService[0]
      Configuration loaded for tenant: 'tenant-a' with 6 settings
```

4. **Call the same endpoint again** - notice the "Loading configuration settings" log **does not appear** because the configuration is cached in the `Lazy<T>` wrapper.

**Using curl:**

```bash
# First call - triggers lazy initialization
curl https://localhost:52409/api/tenants/tenant-a/configuration

# Second call - uses cached configuration
curl https://localhost:52409/api/tenants/tenant-a/configuration
```

**What This Demonstrates:**

- **Deferred Loading**: Configuration is not loaded during app startup
- **On-Demand Creation**: Settings load only when `lazyConfig.Value` is accessed
- **Singleton Caching**: Once loaded, the same instance is reused
- **Performance**: Reduces startup time and memory footprint

**Key Code:**

```csharp
// Registration in Program.cs - creates Lazy wrapper
builder.Services.AddLazyKeyedSingleton<TenantConfigurationService>("tenant-a", ...);

// First access in controller - triggers initialization
var lazyConfig = serviceProvider.GetRequiredKeyedService<Lazy<TenantConfigurationService>>("tenant-a");
var config = lazyConfig.Value; // Settings loaded here on first call
```

### Test Tenant Isolation

Make requests for different tenants and verify data isolation:

```bash
# Get orders for tenant-a
curl -H "X-Tenant-Id: tenant-a" https://localhost:5001/api/orders

# Get orders for tenant-b (different data)
curl -H "X-Tenant-Id: tenant-b" https://localhost:5001/api/orders
```

### Testing Multi-Tenant Features via Swagger

#### Feature: Data Isolation
1. Create an order as `tenant-a`
2. Switch to `tenant-b` and list orders
3. The order from `tenant-a` should NOT appear

#### Feature: Tenant Configuration
Different tenants have different configurations:
- `tenant-a`: Premium features, 5000 max orders
- `tenant-b`: Standard features, 1000 max orders  
- `tenant-c`: Premium features, 2000 max orders

#### Feature: Tenant Validation
Try accessing with an invalid tenant:
1. Authorize with `invalid-tenant`
2. Make any API call
3. You should get a 403 Forbidden error

This demonstrates the tenant validation middleware in action!

## Learning Resources

### Blazing.Extensions.DependencyInjection Documentation

- [GitHub Repository](https://github.com/gragra33/Blazing.Extensions.DependencyInjection)
- [NuGet Package](https://www.nuget.org/packages/Blazing.Extensions.DependencyInjection)
- [Main README](../../../README.md)

### Related Samples

- **ConsoleExample** - All features demonstrated in isolation
- **BlazorServerExample** - Blazor Server integration
- **WpfExample** - WPF MVVM with AutoRegister
- **WinFormsExample** - WinForms with AutoRegister

### Key Demonstration Points

This sample demonstrates:
- ✅ Real-world multi-tenant architecture patterns
- ✅ Enterprise-grade dependency injection setup
- ✅ Production-ready code quality standards
- ✅ Comprehensive error handling and validation
- ✅ Performance optimization techniques (lazy loading, scoping)
- ✅ Clean architecture principles (separation of concerns)

### Blazing.Extensions.DependencyInjection Features in Action

When using the API through Swagger or the Blazor client, you're experiencing these features:

1. **AutoRegister** - Services automatically discovered and registered
2. **Keyed Services** - Tenant-specific configurations loaded by key
3. **Service Scoping** - Each request gets an isolated service scope
4. **Lazy Initialization** - Tenant configs loaded on-demand
5. **Service Factories** - Database contexts created per tenant
6. **Service Validation** - Startup validation prevents misconfigurations
7. **Async Initialization** - Database migrations ran on startup

All working together to provide a seamless multi-tenant experience!

## Production Considerations

### Security

1. **Authentication & Authorization**: Add JWT/OAuth for API security
2. **Tenant Validation**: Implement robust tenant validation
3. **Rate Limiting**: Add per-tenant rate limiting
4. **Input Validation**: Validate all API inputs

### Performance

1. **Caching**: Implement distributed caching (Redis)
2. **Connection Pooling**: Use proper connection pooling
3. **Async All The Way**: Ensure all I/O is async
4. **Query Optimization**: Add indexes and optimize queries

### Reliability

1. **Error Handling**: Implement comprehensive error handling
2. **Logging**: Add structured logging with Serilog
3. **Health Checks**: Implement health check endpoints
4. **Monitoring**: Add Application Insights or similar

### Scalability

1. **Database Per Tenant**: Consider separate databases for isolation
2. **Horizontal Scaling**: Design for multiple server instances
3. **Message Queues**: Use queues for background processing
4. **CDN**: Use CDN for static assets

## Troubleshooting

### "No tenant ID found in request"

**Solution:** Ensure you're sending the `X-Tenant-Id` header or `tenantId` query parameter.

```bash
curl -H "X-Tenant-Id: tenant-a" https://localhost:5001/api/orders
```

### "Tenant 'xxx' is not valid or inactive"

**Solution:** Use one of the pre-configured tenant IDs: `tenant-a`, `tenant-b`, or `tenant-c`.

### "Service collection validation failed"

**Solution:** Check the console output for specific validation errors and fix the service registrations.

## Contributing

This sample is part of the Blazing.Extensions.DependencyInjection project. Contributions are welcome!

## License

This sample is licensed under the MIT License - see the [LICENSE](../../../LICENSE) file for details.

## Acknowledgments

Built with [Blazing.Extensions.DependencyInjection](https://github.com/gragra33/Blazing.Extensions.DependencyInjection) by Graeme Grant.
