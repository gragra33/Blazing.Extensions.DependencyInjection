# Blazing.Extensions.DependencyInjection: Universal Dependency Injection for .NET

[![NuGet Version](https://img.shields.io/nuget/v/Blazing.Extensions.DependencyInjection.svg)](https://www.nuget.org/packages/Blazing.Extensions.DependencyInjection)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Blazing.Extensions.DependencyInjection.svg)](https://www.nuget.org/packages/Blazing.Extensions.DependencyInjection)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-512BD4)](https://dotnet.microsoft.com/download)

## Overview

A comprehensive dependency injection library that **brings and enhances Microsoft's DI container** for any .NET application. WPF, WinForms, Blazor, MAUI, Console apps, and more, without framework-specific integration. Built as powerful extension methods for `Microsoft.Extensions.DependencyInjection`, it delivers enterprise-grade enhancements including **keyed services**, **attribute-based auto-registration**, **lazy initialization**, **service scoping**, **open generics**, and **service validation** that extend beyond the standard Microsoft DI features.

**V2.1.0 New Capabilities:**
- **Service Scoping** - Comprehensive scope management with async disposal
- **Lazy Initialization** - Deferred service creation for performance
- **Open Generic Types** - Single registration for all closed-type variants
- **Service Factories** - Flexible factory delegates with conditional logic
- **Service Enumeration** - Filter, map, and iterate service implementations
- **Service Decoration** - Proxy and decorator patterns for cross-cutting concerns
- **Service Validation** - Detect circular dependencies, lifetime violations, and duplicates
- **Async Initialization** - Declarative startup patterns with dependency ordering

## Table of Contents

- [Quick Start](#quick-start)
  - [Installation](#installation)
  - [Basic Configuration](#basic-configuration)
  - [WPF Applications](#wpf-applications)
  - [WinForms Applications](#winforms-applications)
  - [Blazor Server Applications](#blazor-server-applications)
  - [Console Applications](#console-applications)
- [Give a ⭐](#give-a-)
- [Core Features](#core-features)
  - [Service Configuration](#service-configuration)
  - [Service Resolution](#service-resolution)
  - [Keyed Services](#keyed-services)
  - [Auto-Discovery](#auto-discovery)
- [Advanced Features (V2.1.0)](#advanced-features-v210)
  - [Service Scoping & Lifecycle Management](#service-scoping--lifecycle-management)
  - [Lazy Service Initialization](#lazy-service-initialization)
  - [Open Generic Type Registration](#open-generic-type-registration)
  - [Service Factory Delegates](#service-factory-delegates)
  - [Service Enumeration & Filtering](#service-enumeration--filtering)
  - [Service Decoration Patterns](#service-decoration-patterns)
  - [Service Validation & Diagnostics](#service-validation--diagnostics)
  - [Async Service Initialization](#async-service-initialization)
- [API Reference](#api-reference)
- [Sample Applications](#sample-applications)
- [Requirements](#requirements)
- [Project Structure](#project-structure)
- [Building](#building)
- [Contributing](#contributing)
- [License](#license)
- [Acknowledgments](#acknowledgments)
- [History](#history)

## Quick Start

### Installation

Install via NuGet Package Manager or .NET CLI:

```bash
dotnet add package Blazing.Extensions.DependencyInjection
```

### Basic Configuration

The setup is consistent across all application types:

```csharp
using Blazing.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// Configure services on any object
var provider = instance.ConfigureServices(services =>
{
    services.AddSingleton<IMyService, MyService>();
    services.AddTransient<IOtherService, OtherService>();
    
    // Auto-discover services with [AutoRegister] attribute
    services.Register();
});

// Resolve services anywhere
var myService = provider.GetRequiredService<IMyService>();
```

### WPF Applications

Configure in `App.xaml.cs` during startup:

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services ONCE on Application.Current
        this.ConfigureServices(services =>
        {
            // Manual registration
            services.AddSingleton<IDataService, DataService>();
            services.AddTransient<IDialogService, DialogService>();

            // Keyed services for multi-database scenarios
            services.AddKeyedSingleton<IDbContext, PrimaryDbContext>("primary");
            services.AddKeyedSingleton<IDbContext, AnalyticsDbContext>("analytics");

            // Auto-discovery
            services.Register<ITabView>(ServiceLifetime.Transient);
            services.Register(); // Scans for [AutoRegister] attributes
        });

        // Resolve and show main window
        var mainWindow = this.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}

// Use services in your views
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Get services from Application
        var dataService = Application.Current.GetRequiredService<IDataService>();
        
        // Resolve keyed services
        var primaryDb = Application.Current.GetRequiredKeyedService<IDbContext>("primary");
    }
}
```

### WinForms Applications

Configure in `Program.cs`:

```csharp
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            var services = new ServiceCollection();

            // Auto-discover and register services
            services.Register();

            // Build ServiceProvider
            var serviceProvider = services.BuildServiceProvider();

            // Resolve MainForm from the service provider
            var mainForm = serviceProvider.GetRequiredService<MainForm>();

            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error: {ex.Message}", "Application Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

// Mark forms with AutoRegister attribute
[AutoRegister(ServiceLifetime.Transient)]
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        // Services injected automatically via constructor
    }
}
```

### Blazor Server Applications

Configure in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Razor Components
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure DI services
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddScoped<IUserService, UserService>();

// Auto-discovery
builder.Services.Register();

// Keyed services
builder.Services.AddKeyedScoped<IDbContext, PrimaryDbContext>("primary");
builder.Services.AddKeyedScoped<IDbContext, ReadOnlyDbContext>("readonly");

var app = builder.Build();

// ... app configuration

app.Run();
```

Use services in Blazor components:

```razor
@inject IDataService DataService
@inject IServiceProvider ServiceProvider

<h3>My Component</h3>

@code {
    protected override async Task OnInitializedAsync()
    {
        // Resolve keyed services
        var primaryDb = ServiceProvider.GetRequiredKeyedService<IDbContext>("primary");
        
        // Use scoped services
        await ServiceProvider.GetScopedServiceAsync<IDataProcessor>(async processor =>
        {
            await processor.ProcessAsync();
        });
    }
}
```

### Console Applications

Simple pattern for command-line tools:

```csharp
class Program
{
    static async Task Main(string[] args)
    {
        var host = new ApplicationHost();

        host.ConfigureServices(services =>
        {
            services.AddSingleton<IService, ServiceImpl>();
            services.AddTransient<IProcessor, Processor>();
            
            // Auto-discovery
            services.Register();
        });

        var serviceProvider = host.GetServices();
        var service = serviceProvider!.GetRequiredService<IService>();

        await service.RunAsync();
    }
}

class ApplicationHost { }
```

## Give a ⭐

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

Also, if you find this library useful and you're feeling really generous, please consider [buying me a coffee ☕](https://bmc.link/gragra33).

## Core Features

### Service Configuration

Standard Microsoft DI patterns with convenient extension methods:

```csharp
// Basic service registration
Application.Current.ConfigureServices(services =>
{
    // Standard lifetimes
    services.AddSingleton<IMyService, MyService>();
    services.AddScoped<IScopedService, ScopedService>();
    services.AddTransient<ITransientService, TransientService>();
    
    // Keyed services (multiple implementations)
    services.AddKeyedSingleton<IRepository, SqlRepository>("sql");
    services.AddKeyedSingleton<IRepository, NoSqlRepository>("nosql");
    
    // Factory delegates
    services.AddSingleton<IService>(provider => 
        new Service(provider.GetRequiredService<IDependency>()));
});
```

**Advanced Configuration Pattern:**

```csharp
// Separate configuration from building for complex scenarios
var services = host.GetServiceCollection(s =>
{
    s.AddSingleton<IDataService, DataService>();
    s.Register(); // Auto-discovery
});

// CRITICAL: Build and assign provider
var provider = host.BuildServiceProvider(services);
```

### Service Resolution

Resolve services throughout your application with type-safe methods:

| Method | Description | Returns |
|--------|-------------|---------|
| `GetRequiredService<T>()` | Resolves required service (throws if not found) | `T` |
| `GetService<T>()` | Resolves optional service (returns null if not found) | `T?` |
| `GetRequiredKeyedService<T>(key)` | Resolves required keyed service | `T` |
| `GetKeyedService<T>(key)` | Resolves optional keyed service | `T?` |
| `GetRequiredServices<T>()` | Resolves all registered implementations | `IEnumerable<T>` |

**Examples:**
```csharp
// Required service (throws if not found)
var service = instance.GetRequiredService<IMyService>();

// Optional service (returns null if not found)
var optional = instance.GetService<IOptionalService>();
if (optional != null)
{
    optional.DoWork();
}

// Keyed services
var sqlRepo = instance.GetRequiredKeyedService<IRepository>("sql");
var nosqlRepo = instance.GetKeyedService<IRepository>("nosql");

// All implementations
var allHandlers = instance.GetRequiredServices<IHandler>();
foreach (var handler in allHandlers)
{
    handler.Process(data);
}
```

### Keyed Services

Register and resolve multiple implementations of the same interface:

**Registration:**
```csharp
services.AddKeyedSingleton<IEmailService, SmtpEmailService>("smtp");
services.AddKeyedSingleton<IEmailService, SendGridEmailService>("sendgrid");
services.AddKeyedSingleton<IEmailService, MailgunEmailService>("mailgun");

services.AddKeyedTransient<IPaymentProcessor, StripeProcessor>("stripe");
services.AddKeyedTransient<IPaymentProcessor, PayPalProcessor>("paypal");
services.AddKeyedTransient<IPaymentProcessor, SquareProcessor>("square");

// Multi-database scenarios
services.AddKeyedScoped<IDbContext, PrimaryDbContext>("primary");
services.AddKeyedScoped<IDbContext, AnalyticsDbContext>("analytics");
services.AddKeyedScoped<IDbContext, AuditDbContext>("audit");
```

**Resolution:**
```csharp
public class CheckoutService
{
    public async Task ProcessPaymentAsync(Order order, string method)
    {
        // Dynamically resolve based on runtime condition
        var processor = Application.Current
            .GetRequiredKeyedService<IPaymentProcessor>(method);
        
        var result = await processor.ProcessAsync(order);
        return result;
    }
}

public class DataService
{
    public async Task SaveOrderAsync(Order order)
    {
        // Use different databases for different purposes
        var primaryDb = instance.GetRequiredKeyedService<IDbContext>("primary");
        await primaryDb.Orders.AddAsync(order);
        await primaryDb.SaveChangesAsync();
        
        // Log to audit database
        var auditDb = instance.GetRequiredKeyedService<IDbContext>("audit");
        await auditDb.AuditLog.AddAsync(new AuditEntry { Action = "OrderCreated" });
        await auditDb.SaveChangesAsync();
    }
}
```

### Auto-Discovery

Two powerful mechanisms for automatic service registration:

#### Interface-Based Discovery

Automatically register all implementations of a specified interface:

```csharp
// Register all ITabView implementations as transient
services.Register<ITabView>(ServiceLifetime.Transient);

// Register all IRepository implementations as scoped
services.Register<IRepository>(ServiceLifetime.Scoped);

// Register all IValidator implementations as singletons
services.Register<IValidator>(ServiceLifetime.Singleton);
```

#### Attribute-Based Discovery

Mark classes with `[AutoRegister]` for declarative registration:

```csharp
// Mark services with AutoRegister attribute
[AutoRegister(ServiceLifetime.Singleton)]
public class DataService : IDataService 
{
    public async Task<Data> GetDataAsync() => await LoadDataAsync();
}

[AutoRegister(ServiceLifetime.Transient)]
public class EmailService : IEmailService 
{
    public async Task SendAsync(string to, string subject, string body) 
        => await SendEmailAsync(to, subject, body);
}

[AutoRegister(ServiceLifetime.Scoped, typeof(IUserService))]
public class UserService : IUserService, IAdminService
{
    // Registered as IUserService (specified) and UserService (concrete)
    // IAdminService NOT registered automatically
}

[AutoRegister] // Default: Transient, all interfaces
public class LoggingService : ILoggingService, IDisposable
{
    // Registered as ILoggingService and LoggingService
    // IDisposable excluded automatically
}

// Scan and register all marked classes
services.Register();

// Or scan specific assemblies
services.Register(
    typeof(BusinessLayer).Assembly, 
    typeof(DataLayer).Assembly);
```

#### Assembly Management

Control which assemblies are scanned:

```csharp
// Add assemblies before configuration
Application.Current
    .AddAssembly(typeof(Business.IBusinessService).Assembly)
    .AddAssembly(typeof(Data.IRepository).Assembly)
    .ConfigureServices(services =>
    {
        services.Register(); // Scans added assemblies
    });

// Fluent chaining for multiple assemblies
host.AddAssemblies(
        typeof(Core.ICoreService).Assembly,
        typeof(Business.IBusinessService).Assembly,
        typeof(Data.IDataService).Assembly
    )
    .ConfigureServices(services =>
    {
        services.Register();
    });
```

**Fallback Behavior:**
- If no assemblies specified, scans the calling assembly
- Works out-of-the-box for single-assembly projects

## Advanced Features (V2.1.0)

Version 2.1.0 introduces eight powerful feature groups that extend Microsoft's Dependency Injection with enterprise-grade capabilities. These features work seamlessly across all application types (WPF, WinForms, Blazor, Console) and provide advanced patterns for complex scenarios.

### Service Scoping & Lifecycle Management

Comprehensive service scoping with support for both synchronous and asynchronous disposal patterns. Essential for request-level processing, background services, and proper resource management.

**Key Methods:**

| Method | Description | Use Case |
|--------|-------------|----------|
| `CreateScope()` | Creates a new synchronous service scope | Scoped operations with using statement |
| `CreateAsyncScope()` | Creates an async-capable service scope | Async operations requiring disposal |
| `GetScopedService<T>(action)` | Executes action with scoped service | Simple scoped operations |
| `GetScopedServiceAsync<T>(func)` | Async scoped service execution | Async operations in a scope |
| `GetScopedKeyedService<T>(key, action)` | Scoped keyed service execution | Keyed services with automatic disposal |
| `GetScopedOrRootService<T>()` | Gets service from scope or root | Flexible resolution with fallback |

**Examples:**

```csharp
// Synchronous scope with automatic disposal
using var scope = instance.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
await dbContext.SaveChangesAsync();

// Async scope for background operations
await using var asyncScope = instance.CreateAsyncScope();
var service = asyncScope.ServiceProvider.GetRequiredService<IAsyncService>();
await service.ProcessAsync();

// Convenience method - scope created and disposed automatically
instance.GetScopedService<IDbContext>(db => db.SaveChanges());

// Async convenience method
await instance.GetScopedServiceAsync<IDataProcessor>(async processor =>
{
    await processor.ProcessDataAsync();
    await processor.CommitAsync();
});

// Keyed scoped services
instance.GetScopedKeyedService<IRepository>("sql", repo => repo.SaveData());

// Fallback resolution - uses scope if available, otherwise root
var service = instance.GetScopedOrRootService<ILogger>();
```

### Lazy Service Initialization

Implements lazy initialization pattern for expensive-to-create services. Services are only instantiated when first accessed, improving application startup time and memory efficiency.

**Key Methods:**

| Method | Description | Lifetime |
|--------|-------------|----------|
| `AddLazySingleton<T, TImpl>()` | Register lazy singleton service | Singleton |
| `AddLazyScoped<T, TImpl>()` | Register lazy scoped service | Scoped |
| `AddLazyTransient<T, TImpl>()` | Register lazy transient service | Transient |
| `AddLazyKeyedSingleton<T, TImpl>(key)` | Register lazy keyed singleton | Singleton |
| `GetLazyService<T>()` | Resolve lazy-wrapped service | N/A |
| `GetLazyKeyedService<T>(key)` | Resolve lazy keyed service | N/A |

**Examples:**

```csharp
// Register lazy services by lifetime
services.AddLazySingleton<IExpensiveService, ExpensiveService>();
services.AddLazyScoped<IDataService, DataService>();
services.AddLazyTransient<ITemporaryService, TemporaryService>();

// With factory functions for complex initialization
services.AddLazySingleton<IService>(provider => 
    new Service(
        provider.GetRequiredService<IDependency1>(),
        provider.GetRequiredService<IDependency2>()
    ));

// Keyed lazy services
services.AddLazyKeyedSingleton<IRepository, SqlRepository>("sql");
services.AddLazyKeyedSingleton<IRepository, NoSqlRepository>("nosql");

// Resolve and use - service created on first access
var lazyService = instance.GetLazyService<IExpensiveService>();
Console.WriteLine($"Is created: {lazyService.IsValueCreated}"); // False

var service = lazyService.Value; // Service created NOW
Console.WriteLine($"Is created: {lazyService.IsValueCreated}"); // True

// Keyed lazy services
var lazySqlRepo = instance.GetLazyKeyedService<IRepository>("sql");
var sqlRepo = lazySqlRepo.Value; // Created on first access
```

### Open Generic Type Registration

Enables registration of open generic types, allowing single registration for all closed-type variants. Perfect for repository patterns, validators, command handlers, and other generic service patterns.

**Key Methods:**

| Method | Description | Use Case |
|--------|-------------|----------|
| `AddGenericSingleton(interfaceType, implType)` | Register open generic singleton | Generic repositories |
| `AddGenericScoped(interfaceType, implType)` | Register open generic scoped | Generic validators |
| `AddGenericTransient(interfaceType, implType)` | Register open generic transient | Generic handlers |
| `AddGenericServices(lifetime, pairs)` | Register multiple open generics | Batch registration |

**Examples:**

```csharp
// Register open generic types
services.AddGenericSingleton(typeof(IRepository<>), typeof(Repository<>));
services.AddGenericScoped(typeof(IValidator<>), typeof(Validator<>));
services.AddGenericTransient(typeof(ICommandHandler<>), typeof(CommandHandler<>));

// Multiple registrations at once
services.AddGenericServices(ServiceLifetime.Scoped,
    (typeof(IRepository<>), typeof(Repository<>)),
    (typeof(IValidator<>), typeof(Validator<>)),
    (typeof(IHandler<>), typeof(Handler<>)));

// Usage - automatic closed type resolution
var userRepo = provider.GetRequiredService<IRepository<User>>();
var productRepo = provider.GetRequiredService<IRepository<Product>>();
var orderRepo = provider.GetRequiredService<IRepository<Order>>();

// Validators work the same way
var userValidator = provider.GetRequiredService<IValidator<User>>();
var productValidator = provider.GetRequiredService<IValidator<Product>>();

// Real-world example: Generic repository pattern
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task SaveAsync(T entity);
}

public class Repository<T> : IRepository<T> where T : class
{
    private readonly DbContext _context;
    
    public Repository(DbContext context) => _context = context;
    
    public async Task<T?> GetByIdAsync(int id) => 
        await _context.Set<T>().FindAsync(id);
}
```

### Service Factory Delegates

Provides convenient methods for registering services with factory delegates, enabling conditional service creation and complex initialization logic based on runtime conditions.

**Key Methods:**

| Method | Description |
|--------|-------------|
| `RegisterFactory<T>(factory)` | Register singleton factory |
| `RegisterTransientFactory<T>(factory)` | Register transient factory |
| `RegisterScopedFactory<T>(factory)` | Register scoped factory |
| `RegisterConditionalFactory<T>(factory)` | Conditional singleton factory |
| `RegisterKeyedFactory<T>(key, factory)` | Keyed factory registration |

**Examples:**

```csharp
// Simple factory registration with dependencies
services.RegisterFactory<IService>(provider => 
    new Service(
        provider.GetRequiredService<IDependency>(),
        provider.GetRequiredService<IConfiguration>()
    ));

// Lifetime-specific factories
services.RegisterTransientFactory<IService>(provider => new Service());
services.RegisterScopedFactory<IService>(provider => new Service());

// Conditional factory based on configuration
services.RegisterConditionalFactory<ICache>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var cacheType = config["Cache:Type"];
    
    return cacheType switch
    {
        "Redis" => new RedisCache(config),
        "Sql" => new SqlCache(config),
        _ => new MemoryCache()
    };
});

// Environment-based conditional creation
services.RegisterConditionalFactory<IEmailService>(provider =>
{
    var env = provider.GetRequiredService<IHostEnvironment>();
    return env.IsDevelopment() 
        ? new MockEmailService() 
        : new SmtpEmailService(provider.GetRequiredService<IConfiguration>());
});

// Keyed factory services
services.RegisterKeyedFactory<IRepository>("sql", (provider, key) => 
    new SqlRepository(provider.GetRequiredService<IConfiguration>()));
```

### Service Enumeration & Filtering

Enables enumeration and filtering of registered service implementations. Essential for plugin architectures, middleware chains, composite patterns, and handler chains.

**Key Methods:**

| Method | Description |
|--------|-------------|
| `GetRequiredServices<T>()` | Get all implementations |
| `GetServices<T>(predicate)` | Filter by condition |
| `GetFirstService<T>(predicate)` | Get first matching |
| `ForEachService<T>(action)` | Execute for each |
| `MapServices<T, TResult>(selector)` | Transform services |
| `GetServiceCount<T>()` | Count implementations |

**Examples:**

```csharp
// Get all implementations
var allHandlers = instance.GetRequiredServices<IHandler>();
var allValidators = instance.GetServices<IValidator>();

// Filter with predicate
var enabledHandlers = instance.GetServices<IHandler>(h => h.IsEnabled);
var priorityHandlers = instance.GetServices<IHandler>(h => h.Priority > 5);

// Get first matching (Strategy pattern)
var handler = instance.GetFirstService<IHandler>(h => h.CanHandle(request));
var validator = instance.GetFirstServiceOrDefault<IValidator>(v => v.CanValidate(model));

// Execute for each service
instance.ForEachService<IValidator>(v => v.Validate(model));

// Async execution
await instance.ForEachServiceAsync<IInitializer>(async i => 
    await i.InitializeAsync());

// Map/transform services
var results = instance.MapServices<IProcessor, ProcessResult>(p => p.Process(data));

// Async mapping
var asyncResults = await instance.MapServicesAsync<IAsyncProcessor, int>(
    async p => await p.ProcessAsync(data));

// Count implementations
int handlerCount = instance.GetServiceCount<IHandler>();

// Real-world example: Validation pipeline
public async Task<ValidationResult> ValidateAsync<T>(T model)
{
    var validators = instance.GetRequiredServices<IValidator<T>>();
    var results = new List<ValidationError>();
    
    await instance.ForEachServiceAsync<IValidator<T>>(async validator =>
    {
        var result = await validator.ValidateAsync(model);
        if (!result.IsValid)
            results.AddRange(result.Errors);
    });
    
    return new ValidationResult { IsValid = results.Count == 0, Errors = results };
}
```

### Service Decoration Patterns

Provides patterns for wrapping services with decorators or proxies for cross-cutting concerns like caching, logging, validation, and performance monitoring.

**Examples:**

```csharp
// Register base service and decorate with caching
services.AddSingleton<IRepository>(provider => 
{
    var inner = ActivatorUtilities.CreateInstance<SqlRepository>(provider);
    var cache = provider.GetRequiredService<ICache>();
    return new CachedRepository(inner, cache);
});

// Multiple decorators (logging + caching)
services.AddSingleton<IRepository>(provider => 
    new CachedRepository(
        new LoggingRepository(
            new SqlRepository(provider.GetRequiredService<IConnectionString>()),
            provider.GetRequiredService<ILogger>()),
        provider.GetRequiredService<ICache>()));

// Decorator implementation example
public class CachedRepository : IRepository
{
    private readonly IRepository _inner;
    private readonly ICache _cache;

    public CachedRepository(IRepository inner, ICache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<T> GetAsync<T>(int id)
    {
        var cacheKey = $"{typeof(T).Name}:{id}";
        
        if (_cache.TryGet(cacheKey, out T? cached))
            return cached!;

        var result = await _inner.GetAsync<T>(id);
        _cache.Set(cacheKey, result);
        return result;
    }
}
```

### Service Validation & Diagnostics

Comprehensive validation and diagnostics tools for detecting circular dependencies, lifetime compatibility violations, duplicate registrations, and other configuration issues.

**Key Methods:**

| Method | Description | Use Case |
|--------|-------------|----------|
| `ValidateDuplicateRegistrations()` | Find duplicate service registrations | Detect configuration errors |
| `ValidateCircularDependencies()` | Detect circular dependency cycles | Prevent runtime failures |
| `ValidateLifetimeCompatibility()` | Check lifetime violations | Ensure proper scoping |
| `GetServiceDependencyGraph()` | Build complete dependency graph | Visualization and analysis |
| `GetDiagnostics()` | Get comprehensive diagnostics | Health checks |
| `ThrowIfInvalid()` | Validate and throw on errors | Startup validation |

**Examples:**

```csharp
// Validate for duplicate registrations
var duplicates = services.ValidateDuplicateRegistrations();
foreach (var dup in duplicates)
{
    Console.WriteLine($"Service {dup.ServiceType.Name} has {dup.Count} registrations");
}

// Detect circular dependencies
var cycles = services.ValidateCircularDependencies();
if (cycles.Any())
{
    foreach (var cycle in cycles)
        Console.WriteLine($"Cycle detected: {cycle.Description}");
}

// Validate lifetime compatibility
var violations = services.ValidateLifetimeCompatibility();
foreach (var violation in violations)
{
    Console.WriteLine($"Lifetime violation: {violation.ErrorMessage}");
}

// Get comprehensive diagnostics
var diagnostics = services.GetDiagnostics();
Console.WriteLine($"Total services: {diagnostics.TotalServices}");
Console.WriteLine($"Singletons: {diagnostics.SingletonCount}");
Console.WriteLine($"Scoped: {diagnostics.ScopedCount}");
Console.WriteLine($"Transient: {diagnostics.TransientCount}");
Console.WriteLine($"Warnings: {diagnostics.Warnings.Count}");

// Startup validation (throws on errors)
try
{
    services.ThrowIfInvalid();
    Console.WriteLine("✓ Service collection is valid");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"✗ Validation failed: {ex.Message}");
}

// Get dependency graph for visualization
var graph = services.GetServiceDependencyGraph();
Console.WriteLine($"Total services: {graph.Services.Count}");
Console.WriteLine($"Total dependencies: {graph.Dependencies.Count}");
```

### Async Service Initialization

Declarative startup patterns with dependency ordering for services that require async initialization during application startup.

**IAsyncInitializable Interface:**

```csharp
public interface IAsyncInitializable
{
    Task InitializeAsync(IServiceProvider serviceProvider);
    int InitializationPriority => 0; // Higher values initialize first
    IEnumerable<Type>? DependsOn => null; // Initialization dependencies
}
```

**Key Methods:**

| Method | Description | Use Case |
|--------|-------------|----------|
| `InitializeAsync<T>()` | Initialize single service | Specific service initialization |
| `InitializeAllAsync()` | Initialize all async services | Application startup |
| `InitializeAllAsync<T>()` | Initialize services by interface | Grouped initialization |
| `GetInitializationOrder()` | Get initialization order info | Diagnostics |
| `AddStartupAction(action, priority)` | Register startup action | Custom initialization |

**Examples:**

```csharp
// Implement IAsyncInitializable
public class DatabaseService : IAsyncInitializable
{
    private readonly IConfiguration _config;
    
    public int InitializationPriority => 100; // High priority
    public IEnumerable<Type>? DependsOn => null;
    
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Migrate database on startup
        var db = serviceProvider.GetRequiredService<DbContext>();
        await db.Database.MigrateAsync();
    }
}

public class CacheService : IAsyncInitializable
{
    public int InitializationPriority => 50; // Lower priority
    public IEnumerable<Type> DependsOn => new[] { typeof(DatabaseService) };
    
    public async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Warm up cache after database is ready
        await WarmUpCacheAsync();
    }
}

// Register services
services.AddSingleton<IAsyncInitializable, DatabaseService>();
services.AddSingleton<IAsyncInitializable, CacheService>();

// Initialize all services during startup
var provider = services.BuildServiceProvider();
await provider.InitializeAllAsync();

// Or initialize specific service
await provider.InitializeAsync<DatabaseService>();

// Add custom startup actions
services.AddStartupAction(async provider =>
{
    var logger = provider.GetRequiredService<ILogger>();
    logger.LogInformation("Application started");
}, priority: 0);

// Get initialization order for diagnostics
var order = provider.GetInitializationOrder();
foreach (var step in order.Steps)
{
    Console.WriteLine($"Step {step.Order}: {step.ServiceType.Name} (Priority: {step.Priority})");
}

```

## API Reference

Complete API documentation with all extension methods organized by feature area.

### Core Methods

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `ConfigureServices<T>` | `Action<IServiceCollection>` | `IServiceProvider` | Configure and build service provider |
| `GetServices` | - | `IServiceProvider?` | Get associated service provider |
| `SetServices` | `IServiceProvider?` | `void` | Set or remove service provider |
| `GetRequiredService<T>` | - | `T` | Resolve required service |
| `GetService<T>` | - | `T?` | Resolve optional service |
| `GetRequiredKeyedService<T>` | `object? key` | `T` | Resolve required keyed service |
| `GetKeyedService<T>` | `object? key` | `T?` | Resolve optional keyed service |

### Advanced Configuration

| Method | Description |
|--------|-------------|
| `ConfigureServicesAdvanced<T>` | Configure with custom `ServiceProviderOptions` |
| `GetServiceCollection<T>` | Get service collection for delayed building |
| `BuildServiceProvider<T>` | Build and assign from existing collection |

### Auto-Discovery

| Method | Parameters | Description |
|--------|------------|-------------|
| `Register<TInterface>` | `ServiceLifetime` | Register all interface implementations |
| `Register()` | - | Register all `[AutoRegister]` classes |
| `Register` | `Assembly[]` | Register from specific assemblies |

### Assembly Management

| Method | Parameters | Returns | Description |
|--------|------------|---------|-------------|
| `AddAssembly<T>` | `Assembly` | `T` | Add assembly for discovery |
| `AddAssemblies<T>` | `Assembly[]` | `T` | Add multiple assemblies |

### Service Scoping (V2.1.0)

| Method | Description |
|--------|-------------|
| `CreateScope()` | Create synchronous service scope |
| `CreateAsyncScope()` | Create async service scope |
| `GetScopedService<T>(action)` | Execute action with scoped service |
| `GetScopedServiceAsync<T>(func)` | Async scoped service execution |
| `GetScopedKeyedService<T>(key, action)` | Scoped keyed service execution |

### Lazy Services (V2.1.0)

| Method | Description |
|--------|-------------|
| `AddLazySingleton<T, TImpl>()` | Register lazy singleton |
| `AddLazyScoped<T, TImpl>()` | Register lazy scoped |
| `AddLazyTransient<T, TImpl>()` | Register lazy transient |
| `GetLazyService<T>()` | Resolve lazy service |
| `GetLazyKeyedService<T>(key)` | Resolve lazy keyed service |

### Open Generics (V2.1.0)

| Method | Description |
|--------|-------------|
| `AddGenericSingleton(interfaceType, implType)` | Register open generic singleton |
| `AddGenericScoped(interfaceType, implType)` | Register open generic scoped |
| `AddGenericTransient(interfaceType, implType)` | Register open generic transient |
| `AddGenericServices(lifetime, pairs)` | Register multiple open generics |

### Service Factories (V2.1.0)

| Method | Description |
|--------|-------------|
| `RegisterFactory<T>(factory)` | Register singleton factory |
| `RegisterTransientFactory<T>(factory)` | Register transient factory |
| `RegisterScopedFactory<T>(factory)` | Register scoped factory |
| `RegisterConditionalFactory<T>(factory)` | Conditional singleton factory |
| `RegisterKeyedFactory<T>(key, factory)` | Keyed factory registration |

### Service Enumeration (V2.1.0)

| Method | Description |
|--------|-------------|
| `GetRequiredServices<T>()` | Get all implementations |
| `GetServices<T>(predicate)` | Filter by condition |
| `GetFirstService<T>(predicate)` | Get first matching |
| `ForEachService<T>(action)` | Execute for each |
| `MapServices<T, TResult>(selector)` | Transform services |
| `GetServiceCount<T>()` | Count implementations |

### Service Validation (V2.1.0)

| Method | Description |
|--------|-------------|
| `ValidateDuplicateRegistrations()` | Find duplicate registrations |
| `ValidateCircularDependencies()` | Detect circular dependencies |
| `ValidateLifetimeCompatibility()` | Check lifetime violations |
| `GetServiceDependencyGraph()` | Build dependency graph |
| `GetDiagnostics()` | Get comprehensive diagnostics |
| `ThrowIfInvalid()` | Validate and throw on errors |

### Async Initialization (V2.1.0)

| Method | Description |
|--------|-------------|
| `InitializeAsync<T>()` | Initialize single service |
| `InitializeAllAsync()` | Initialize all async services |
| `InitializeAllAsync<T>()` | Initialize services by interface |
| `GetInitializationOrder()` | Get initialization order |
| `AddStartupAction(action, priority)` | Register startup action |

## Sample Applications

The solution includes comprehensive sample applications demonstrating real-world usage patterns across different application types.

### MultiTenantExample - Advanced Multi-Tenant ASP.NET Core Web API (NEW!)

**Location:** `src/samples/MultiTenantExample/`

A comprehensive multi-tenant application demonstrating:
- **Multi-Tenant Architecture** - Client/Server application with Blazor WebAssembly frontend
- **Tenant Isolation** - Per-tenant data isolation using keyed services and middleware
- **AutoRegister Attribute** - Automatic service discovery in both Server and Client projects
- **Keyed Services** - Tenant-specific configurations and database contexts
- **Service Scoping** - Request-level scope management with async disposal
- **Lazy Initialization** - Deferred tenant configuration loading for performance
- **Service Factories** - Dynamic tenant database context creation
- **Service Validation** - Startup validation with comprehensive diagnostics
- **Async Initialization** - Priority-based initialization (migrations → validation → cache warmup)
- **Swagger Integration** - Interactive API testing with tenant authentication
- **Production-Ready Patterns** - Middleware pipeline, error handling, XML documentation, source-generated logging

**Run the example:**
```bash
dotnet run --project src/samples/MultiTenantExample/Server
```

### WpfExample - Complete MVVM Application

**Location:** `src/samples/WpfExample/`

A comprehensive WPF application demonstrating:
- **TabViewHandler Pattern** - Complete decoupling with automatic tab discovery via `ITabView` interface
- **Blazing.ToggleSwitch.Wpf** - Modern toggle switch controls integrated into settings
- **AutoRegister Attribute** - Declarative service registration for ViewModels, Views, and Services
- **Interface-Based Discovery** - Automatic registration of all `ITabView` implementations
- **MVVM Best Practices** - Proper View/ViewModel separation with dependency injection
- **Service-Based Architecture** - Clean separation with `IDataService`, `IDialogService`, `IWeatherService`, and `INavigationService`

**Run the example:**
```bash
dotnet run --project src/samples/WpfExample --framework net10.0-windows
```

### WinFormsExample - AutoRegister Pattern

**Location:** `src/samples/WinFormsExample/`

A WinForms application demonstrating:
- **AutoRegister Attributes** - All services, views, and main form marked with `[AutoRegister]`
- **TabViewHandler Pattern** - Dynamic tab discovery using `ITabView` interface implementations
- **Console Window Integration** - Dual output showing both GUI and detailed console logging
- **Service Architecture** - Complete DI with `IDataService`, `IDialogService`, `IWeatherService`, and `INavigationService`
- **Interface-Based Discovery** - Automatic tab view registration and resolution

**Run the example:**
```bash
dotnet run --project src/samples/WinFormsExample
```

### BlazorServerExample - Blazor Integration

**Location:** `src/samples/BlazorServerExample/`

A Blazor Server application demonstrating:
- **AutoRegister Discovery** - Services marked with `[AutoRegister]` attribute for automatic registration
- **Assembly Scanning** - Using `AddAssembly()` to specify assemblies for service discovery
- **Blazor Components** - Multiple interactive server components (Home, Weather, Data, Settings)
- **Service Integration** - `IDataService` and `IWeatherService` injected into Razor components
- **Singleton Services** - Demonstrates singleton lifetime with `DataService` for shared state
- **Minimal Configuration** - Simple, clean DI setup in `Program.cs`

**Run the example:**
```bash
dotnet run --project src/samples/BlazorServerExample
```

### ConsoleExample - Comprehensive API Demo

**Location:** `src/samples/ConsoleExample/`

A console application demonstrating:
- **All V2.1.0 Features** - Complete demonstration of all 8 new advanced features
- **18 Example Scenarios** - Each implementing `IExample` interface with `[AutoRegister]` attribute
- **Service Scoping** - Synchronous and async scope management patterns
- **Lazy Initialization** - Deferred service creation with performance comparisons
- **Open Generic Types** - Repository, validator, and handler patterns
- **Service Factories** - Conditional and keyed factory registrations
- **Service Enumeration** - Plugin architecture and handler chain patterns
- **Service Decoration** - Caching and logging decorator implementations
- **Service Validation** - Circular dependency and lifetime violation detection
- **Async Initialization** - Priority-based startup initialization
- **Keyed Services** - Multiple implementations with runtime selection
- **Performance Testing** - Timing and benchmarking of service resolution

**Run the example:**
```bash
dotnet run --project src/samples/ConsoleExample
```

## Requirements

- **.NET 8.0 or later** (.NET 8, 9, 10 supported)
- **Microsoft.Extensions.DependencyInjection 9.0.0 or later**

## Project Structure

The solution is organized into logical components with clear separation between core functionality, supporting libraries, sample applications, and tests. This structure facilitates easy navigation, maintenance, and extension of the codebase while supporting multiple development workflows.

```
Blazing.Extensions.DependencyInjection/           # Solution root
├── src/
│   ├── Blazing.Extensions.DependencyInjection/   # Main DI library
│   ├── libs/
│   │   ├── Blazing.ToggleSwitch.Blazor/          # Blazor ToggleSwitch control library
│   │   ├── Blazing.ToggleSwitch.WinForms/        # WinForms ToggleSwitch control library
│   │   └── Blazing.ToggleSwitch.Wpf/             # WPF ToggleSwitch control library
│   └── samples/
│       ├── BlazorServerExample/                  # Blazor Server app with AutoRegister attributes
│       ├── ConsoleExample/                       # Console app demonstrating all features
│       ├── WinFormsExample/                      # WinForms with AutoRegister attributes
│       ├── WpfExample/                           # WPF MVVM with TabViewHandler pattern
│       └── MultiTenantExample/                  # Multi-tenant Blazor WebAssembly and ASP.NET Core API
├── tests/
│   └── UnitTests/                                # Comprehensive unit tests (104 tests)
├── Directory.Build.props                         # Centralized build properties
├── Directory.Packages.props                      # Centralized package versions
└── README.md                                     # This file
```

## Building

The solution supports multiple .NET versions and provides comprehensive build automation through PowerShell scripts and standard dotnet CLI commands. The build process includes compilation, testing, and packaging operations with support for both development and release scenarios.

```bash
# Build everything
dotnet build

# Run tests
dotnet test

# Run WPF example (.NET 8.0)
dotnet run --project src/samples/WpfExample --framework net8.0-windows

# Run WPF example (.NET 9.0)
dotnet run --project src/samples/WpfExample --framework net9.0-windows

# Run WPF example (.NET 10.0)
dotnet run --project src/samples/WpfExample --framework net10.0-windows

# Run WinForms example
dotnet run --project src/samples/WinFormsExample

# Run Console example
dotnet run --project src/samples/ConsoleExample

# Run MultiTenant example (Server project)
dotnet run --project src/samples/MultiTenantExample/Server

# Run MultiTenant example (Client project - Blazor WASM)
dotnet run --project src/samples/MultiTenantExample/Client
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 8.0, 9.0, & 10.0 SDK
3. Run `dotnet restore`
4. Run `dotnet build`
5. Run `dotnet test` to ensure all tests pass

### Pull Request Process

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built on Microsoft's excellent [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/) framework
- Toggle switch controls inspired by the CodeProject article "[Flexible WPF ToggleSwitch Control](https://www.codeproject.com/articles/WPF-ToggleSwitch-Control)" by Graeme Grant
- Modern .NET patterns and practices from the .NET community

## History

### V2.2.0 - 8 December 2025

**Improvements:**
- **Enhanced Extension Methods** - Improved AsyncInitializationExtensions, ServiceEnumerationExtensions, and ServiceValidationExtensions with better error handling and performance optimizations

**New Sample:**
- **MultiTenantExample** - Advanced multi-tenant ASP.NET Core Web API with Blazor WebAssembly client demonstrating enterprise-grade patterns, tenant isolation, and comprehensive DI features integration

### V2.1.0 - 21 November 2025

**New Features:**
- **Service Scoping** - Complete `IServiceScope` and `AsyncServiceScope` support with convenience methods for automatic disposal
- **Lazy Initialization** - `Lazy<T>` factory pattern support for expensive-to-create services across all lifetimes
- **Open Generic Types** - Open generic type registration for repository patterns, validators, and generic handlers
- **Service Factories** - Enhanced factory delegate registration with conditional logic and `IServiceProvider` parameter access
- **Service Enumeration** - Comprehensive service enumeration with filtering, mapping, and LINQ support
- **Service Decoration** - Service decoration/proxy pattern support for cross-cutting concerns like caching and logging
- **Service Validation** - Detect circular dependencies, lifetime violations, duplicate registrations, and build dependency graphs
- **Async Initialization** - Declarative `IAsyncInitializable` interface with priority-based initialization ordering

**New Sample:**
- **BlazorServerExample** - Complete Blazor Server application demonstrating AutoRegister discovery and service integration

**Sample Updates:**
- **ConsoleExample** - Expanded from 10 to 18 comprehensive examples demonstrating all V2.1.0 features with detailed implementations for service scoping, lazy initialization, open generics, factories, enumeration, decoration, validation, and async initialization patterns

**Improvements:**
- **Expanded Test Coverage** - 77 comprehensive tests covering all new features with 100% pass rate
- **Enhanced Documentation** - Reorganized README with feature-focused sections and comprehensive examples
- **Bug Fixes** - Resolved extension method ambiguity issues with `IServiceProvider` and `IServiceScope`

### V2.0.0 - 17 November 2025

 -   **.NET 10.0 Support** - Added support for .NET 10.0 applications

### V1.0.0 (.Net 8.0+)

-   **Universal DI Support** - Added universal dependency injection support for any .NET object
-   **Memory Management** - Implemented `ConditionalWeakTable` based memory management for automatic cleanup
-   **Core API** - Created comprehensive API with `ConfigureServices`, `GetServices`, and `SetServices` extension methods
-   **Keyed Services** - Added full keyed services support with `GetRequiredKeyedService` and `GetKeyedService` methods
-   **Platform Support** - Supports all .Net Application types; specifically designed for a common usage pattern across WPF, WinForms, and Console applications
-   **Advanced Configuration** - Included advanced configuration options with `ServiceProviderOptions`
-   **Comprehensive Testing** - Built comprehensive test suite with 104 unit tests (100% passing)
-   **Sample Applications** - Created three sample applications: WpfExample, WinFormsExample, and ConsoleExample
-   **AutoRegister Attribute** - Added AutoRegister attribute for declarative service registration
-   **Bonus Component Libraries** - Added Blazing.ToggleSwitch.Wpf and Blazing.ToggleSwitch.WinForms component libraries
-   **Project Structure** - Established project structure with centralized build and package management
