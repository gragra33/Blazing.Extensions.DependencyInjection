# Blazing.Extensions.DependencyInjection: A Simplified Universal Dependency Injection for .NET

This project provides universal dependency injection capabilities for any .NET application, bringing **Microsoft's Dependency Injection** container to WPF, WinForms, Console apps, and more without framework-specific integration requirements. Built as extension methods for Microsoft.Extensions.DependencyInjection, it features full **keyed services support** for advanced dependency resolution scenarios and **attribute-based auto-registration** for declarative service configuration.

## Table of Contents

-   [Quick Start](#quick-start)
    -   [Installation](#installation)
        -   [Package Installation](#package-installation)
        -   [Project Reference](#project-reference)
    -   [Configuration](#configuration)
        -   [WPF Applications](#wpf-applications)
        -   [WinForms Applications](#winforms-applications)
        -   [Console Applications](#console-applications)
    -   [Usage](#usage)
        -   [Basic Service Configuration](#basic-service-configuration)
        -   [Resolving Services](#resolving-services)
        -   [Advanced Configuration Patterns](#advanced-configuration-patterns)
-   [Key Features and Critical Patterns](#key-features-and-critical-patterns)
-   [Keyed Services Support](#keyed-services-support)
    -   [Registering Keyed Services](#registering-keyed-services)
    -   [Resolving Keyed Services](#resolving-keyed-services)
    -   [Keyed Services in ViewModels](#keyed-services-in-viewmodels)
    -   [Real-World Keyed Services Examples](#real-world-keyed-services-examples)
-   [Give a ⭐](#give-a-)
-   [Documentation](#documentation)
    -   [Core Libraries](#core-libraries)
        -   [Blazing.Extensions.DependencyInjection](#blazingextensionsdependencyinjection)
        -   [Blazing.ToggleSwitch.Wpf](#blazingtoggleswitchwpf)
    -   [API Reference](#api-reference)
        -   [Core Extension Methods](#core-extension-methods)
        -   [Service Resolution Methods](#service-resolution-methods)
        -   [Advanced Configuration Methods](#advanced-configuration-methods)
        -   [Auto-Discovery Registration](#auto-discovery-registration)
    -   [Memory Management](#memory-management)
    -   [Thread Safety](#thread-safety)
    -   [Sample Applications](#sample-applications)
        -   [WpfExample - Complete Real-World Application](#wpfexample---complete-real-world-application)
    -   [Recommended Patterns](#recommended-patterns)
        -   [WPF Application Pattern](#wpf-application-pattern)
        -   [WinForms Application Pattern](#winforms-application-pattern)
        -   [Console Application Pattern](#console-application-pattern)
-   [History](#history)

## Quick Start

### Installation

Add the [Blazing.Extensions.DependencyInjection](https://www.nuget.org/packages/Blazing.Extensions.DependencyInjection) package to your project.

Install the package via project reference or .NET CLI.

#### Package Installation

```bash
dotnet add package Blazing.Extensions.DependencyInjection
```

#### Project Reference

```xml
<ItemGroup>
  <ProjectReference Include="..\src\Blazing.Extensions.DependencyInjection\Blazing.Extensions.DependencyInjection.csproj" />
</ItemGroup>
```

### Configuration

Configure the library in your application startup. The `ConfigureServices` method will add the required services and enable dependency injection for any .NET object.

> **🔑 Key Pattern**: For advanced scenarios, use `GetServiceCollection` + `BuildServiceProvider` to separate configuration from building, giving you full control over the service provider creation.

#### WPF Applications

```csharp
using Blazing.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// Services with auto-registration attributes
[AutoRegister(ServiceLifetime.Singleton)]
public class DataService : IDataService { }

[AutoRegister(ServiceLifetime.Transient)]
public class DialogService : IDialogService { }

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

            // Or use attribute-based auto-registration
            services.Register(); // Scans for [AutoRegister] attributes
        });
    }
}
```

#### WinForms Applications

```csharp
// Program.cs
public static class Program
{
    public static ApplicationHost Host { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Host = new ApplicationHost();
        Host.ConfigureServices(services =>
        {
            services.AddSingleton<IDataService, DataService>();
        });

        Application.Run(new MainForm());
    }
}

public class ApplicationHost { }
```

#### Console Applications

```csharp
class Program
{
    static void Main(string[] args)
    {
        var host = new ApplicationHost();

        host.ConfigureServices(services =>
        {
            services.AddSingleton<IService, ServiceImpl>();
        });

        var serviceProvider = host.GetServices();
        var service = serviceProvider!.GetRequiredService<IService>();

        // Use service...
    }
}

class ApplicationHost { }
```

### Usage

#### Basic Service Configuration

```csharp
using Blazing.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

// 1. Configure services during application startup
var provider = Application.Current.ConfigureServices(services =>
{
    services.AddSingleton<IMyService, MyService>();
    services.AddTransient<IOtherService, OtherService>();
});

// 2. Resolve services anywhere in your application
var myService = provider.GetRequiredService<IMyService>();
```

#### Resolving Services

```csharp
// In your windows/views
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Get provider from Application
        var services = Application.Current.GetServices();
        var dataService = services!.GetRequiredService<IDataService>();

        // Resolve keyed services (new in v1.0)
        var primaryDb = services!.GetRequiredKeyedService<IDbContext>("primary");
        var secondaryDb = services!.GetRequiredKeyedService<IDbContext>("secondary");
    }
}
```

#### Advanced Configuration Patterns

```csharp
// Step 1: Get service collection for manual control
var services = Application.Current.GetServiceCollection(services =>
{
    services.AddSingleton<IDataService, DataService>();
    services.AddTransient<IDialogService, DialogService>();

    // Keyed services support
    services.AddKeyedSingleton<IDbContext, PrimaryDbContext>("primary");
    services.AddKeyedSingleton<IDbContext, SecondaryDbContext>("secondary");
    services.AddKeyedTransient<IEmailService, SmtpEmailService>("smtp");
    services.AddKeyedTransient<IEmailService, SendGridEmailService>("sendgrid");

    // Interface-based auto-discovery: Register all implementations of ITabView
    services.Register<ITabView>(ServiceLifetime.Transient);

    // Attribute-based auto-discovery: Register all [AutoRegister] classes
    services.Register();
});

// Step 2: Build and assign services - CRITICAL STEP!
// This ensures the service provider is fully built before resolving any services
var serviceProvider = Application.Current.BuildServiceProvider(services);

// Step 3: Resolve services
var mainWindow = serviceProvider.GetRequiredService<MainWindow>();

// Step 4: Resolve keyed services anywhere in your application
var primaryDb = Application.Current.GetRequiredKeyedService<IDbContext>("primary");
var smtpEmail = Application.Current.GetRequiredKeyedService<IEmailService>("smtp");
```

### Assembly Management for Auto-Discovery

The library provides assembly management for controlling which assemblies are scanned during auto-discovery. This is especially useful when you have services distributed across multiple assemblies.

### Adding Assemblies for Discovery

```csharp
// Add assemblies before configuring services
Application.Current
    .AddAssembly(typeof(BusinessLayer.IBusinessService).Assembly)
    .AddAssembly(typeof(DataLayer.IRepository).Assembly)
    .ConfigureServices(services =>
    {
        // Auto-discovery will scan the added assemblies
        services.Register<IBusinessService>(ServiceLifetime.Scoped);
        services.Register<IRepository>(ServiceLifetime.Singleton);

        // Attribute-based discovery across all added assemblies
        services.Register();
    });
```

### Fluent Assembly Management

```csharp
// Fluent chaining for multiple assemblies
var host = new ApplicationHost();
host.AddAssemblies(
        typeof(Core.ICoreService).Assembly,
        typeof(Business.IBusinessService).Assembly,
        typeof(Data.IDataService).Assembly
    )
    .ConfigureServices(services =>
    {
        // All specified assemblies will be scanned
        services.Register();
    });
```

### Assembly Fallback Behavior

If no assemblies are explicitly added:

-   **Interface-based discovery** (`services.Register<T>()`) scans the calling assembly
-   **Attribute-based discovery** (`services.Register()`) scans the calling assembly
-   This ensures discovery works out-of-the-box for simple scenarios

## Key Features and Critical Patterns

> **🔥 CRITICAL PATTERN**: When using `GetServiceCollection` + `BuildServiceProvider`, you MUST call `BuildServiceProvider` to complete the setup:
>
> ```csharp
> // Step 1: Get service collection
> var services = this.GetServiceCollection(services => { /* configure */ });
>
> // Step 2: CRITICAL - Build and assign services
> var serviceProvider = this.BuildServiceProvider(services);
> ```
>
> This ensures the service provider is fully built before resolving any services.

## Keyed Services Support

Blazing.Extensions.DependencyInjection provides full support for .NET's keyed services feature, allowing you to register multiple implementations of the same interface and retrieve them by key.

#### Registering Keyed Services

```csharp
// Configure keyed services during application startup
Application.Current.ConfigureServices(services =>
{
    // Register multiple database contexts
    services.AddKeyedSingleton<IDbContext, PrimaryDbContext>("primary");
    services.AddKeyedSingleton<IDbContext, SecondaryDbContext>("secondary");
    services.AddKeyedSingleton<IDbContext, ReadOnlyDbContext>("readonly");

    // Register multiple email providers
    services.AddKeyedTransient<IEmailService, SmtpEmailService>("smtp");
    services.AddKeyedTransient<IEmailService, SendGridEmailService>("sendgrid");
    services.AddKeyedTransient<IEmailService, MailgunEmailService>("mailgun");

    // Register multiple cache providers
    services.AddKeyedScoped<ICacheService, MemoryCacheService>("memory");
    services.AddKeyedScoped<ICacheService, RedisCacheService>("redis");
    services.AddKeyedScoped<ICacheService, SqlCacheService>("sql");
});
```

#### Resolving Keyed Services

```csharp
public partial class DataManager : UserControl
{
    public DataManager()
    {
        InitializeComponent();

        // Resolve keyed services from Application.Current
        var primaryDb = Application.Current.GetRequiredKeyedService<IDbContext>("primary");
        var secondaryDb = Application.Current.GetRequiredKeyedService<IDbContext>("secondary");

        // Optional keyed service resolution
        var cacheService = Application.Current.GetKeyedService<ICacheService>("redis");
        if (cacheService != null)
        {
            // Use Redis cache
        }

        // Different email providers based on configuration
        var emailProvider = GetEmailProvider();
        var emailService = Application.Current.GetRequiredKeyedService<IEmailService>(emailProvider);
    }

    private string GetEmailProvider()
    {
        // Return "smtp", "sendgrid", or "mailgun" based on configuration
        return Configuration["EmailProvider"] ?? "smtp";
    }
}
```

#### Keyed Services in ViewModels

```csharp
public class OrderViewModel : ViewModelBase
{
    private readonly IDbContext _primaryDb;
    private readonly IDbContext _auditDb;
    private readonly IEmailService _emailService;

    public OrderViewModel()
    {
        // Resolve keyed services in ViewModels
        _primaryDb = Application.Current.GetRequiredKeyedService<IDbContext>("primary");
        _auditDb = Application.Current.GetRequiredKeyedService<IDbContext>("audit");
        _emailService = Application.Current.GetRequiredKeyedService<IEmailService>("sendgrid");
    }

    public async Task ProcessOrderAsync(Order order)
    {
        // Use primary database for order processing
        await _primaryDb.SaveAsync(order);

        // Use audit database for compliance logging
        await _auditDb.LogAsync(new AuditEntry { Action = "OrderProcessed", OrderId = order.Id });

        // Send confirmation email using SendGrid
        await _emailService.SendAsync(order.CustomerEmail, "Order Confirmed", GetEmailTemplate(order));
    }
}
```

#### Real-World Keyed Services Examples

```csharp
// Multi-tenant application with tenant-specific services
services.AddKeyedSingleton<IDbContext, TenantADbContext>("tenant-a");
services.AddKeyedSingleton<IDbContext, TenantBDbContext>("tenant-b");
services.AddKeyedSingleton<IDbContext, TenantCDbContext>("tenant-c");

// Environment-specific configurations
services.AddKeyedSingleton<IApiClient, ProductionApiClient>("production");
services.AddKeyedSingleton<IApiClient, StagingApiClient>("staging");
services.AddKeyedSingleton<IApiClient, DevelopmentApiClient>("development");

// Feature-specific services
services.AddKeyedTransient<IPaymentProcessor, StripePaymentProcessor>("stripe");
services.AddKeyedTransient<IPaymentProcessor, PayPalPaymentProcessor>("paypal");
services.AddKeyedTransient<IPaymentProcessor, SquarePaymentProcessor>("square");

// Usage in business logic
public class CheckoutService
{
    public async Task ProcessPaymentAsync(Order order, string paymentMethod)
    {
        var processor = Application.Current.GetRequiredKeyedService<IPaymentProcessor>(paymentMethod);
        var result = await processor.ProcessAsync(order.Total, order.PaymentDetails);

        if (result.IsSuccessful)
        {
            await CompleteOrderAsync(order);
        }
    }
}
```

## Give a ⭐

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

Also, if you find this library useful, and you're feeling really generous, then please consider [buying me a coffee ☕](https://bmc.link/gragra33).

## Documentation

The library supports the following application types:

-   WPF Applications
-   WinForms Applications
-   Console Applications
-   Any .NET Class Library

The library package includes:

-   `ConfigureServices`, `GetServices`, `SetServices` extension methods for quick and easy dependency injection setup.
-   `ConditionalWeakTable` based memory management for automatic cleanup.
-   Sample applications for getting started quickly with all application types.

### Core Libraries

#### Blazing.Extensions.DependencyInjection

A lightweight library that brings Microsoft's dependency injection to any .NET class without requiring framework-specific integration. Perfect for adding DI to WPF, WinForms, Console apps, and more!

**Features:**

-   ✅ **Universal Compatibility**: Works with any .NET class (WPF, WinForms, Console, etc.)
-   ✅ **Memory Efficient**: Uses `ConditionalWeakTable` for automatic garbage collection
-   ✅ **Type Safe**: Full IntelliSense and compile-time checking
-   ✅ **Microsoft DI**: Built on `Microsoft.Extensions.DependencyInjection`
-   ✅ **Easy to Use**: Simple extension methods for configuration and resolution
-   ✅ **Auto-Discovery**: Interface-based and attribute-based automatic service registration
-   ✅ **Keyed Services**: Full support for .NET's keyed services feature
-   ✅ **Attribute Registration**: Declarative `[AutoRegister]` attribute for service marking
-   ✅ **Well Tested**: Comprehensive test suite with unit and integration tests
-   ✅ **Sample Applications**: Includes working WPF example demonstrating best practices

#### Blazing.ToggleSwitch.Wpf

A modern, customizable toggle switch control for WPF applications. This is a separate component library that complements the dependency injection system.

For detailed documentation, usage examples, and API reference, see: **[Blazing.ToggleSwitch.Wpf README](src/libs/Blazing.ToggleSwitch.Wpf/README.md)**

### API Reference

The **Blazing.Extensions.DependencyInjection** library provides a set of extension methods that enable dependency injection for any .NET object. These methods are designed to work seamlessly with Microsoft's dependency injection container while providing automatic memory management through `ConditionalWeakTable`.

#### Core Extension Methods

| Method                 | Description                                                      | Parameters                                                                        | Returns             |
| ---------------------- | ---------------------------------------------------------------- | --------------------------------------------------------------------------------- | ------------------- |
| `ConfigureServices<T>` | Configures and builds a service provider for any object instance | `instance`: Target object<br/>`configureServices`: Service configuration action   | `IServiceProvider`  |
| `GetServices`          | Gets the service provider associated with an object instance     | `instance`: Target object                                                         | `IServiceProvider?` |
| `SetServices`          | Sets or removes the service provider for an object instance      | `instance`: Target object<br/>`serviceProvider`: Provider to set (null to remove) | `void`              |
| `AddAssembly<T>`       | Adds an assembly to the instance for auto-discovery              | `instance`: Target object<br/>`assembly`: Assembly to add                         | `T` (for chaining)  |
| `AddAssemblies<T>`     | Adds multiple assemblies to the instance for auto-discovery      | `instance`: Target object<br/>`assemblies`: Assemblies to add                     | `T` (for chaining)  |

#### Service Resolution Methods

| Method                              | Description                                                       | Parameters                                              | Returns     |
| ----------------------------------- | ----------------------------------------------------------------- | ------------------------------------------------------- | ----------- |
| `GetRequiredService<TService>`      | Gets a required service from the object's service provider        | `instance`: Target object                               | `TService`  |
| `GetService<TService>`              | Gets an optional service from the object's service provider       | `instance`: Target object                               | `TService?` |
| `GetRequiredKeyedService<TService>` | Gets a required keyed service from the object's service provider  | `instance`: Target object<br/>`serviceKey`: Service key | `TService`  |
| `GetKeyedService<TService>`         | Gets an optional keyed service from the object's service provider | `instance`: Target object<br/>`serviceKey`: Service key | `TService?` |
| `ClearServices`                     | Removes the service provider from the object instance             | `instance`: Target object                               | `bool`      |

#### Advanced Configuration Methods

| Method                                   | Description                                    | Use Case                                       |
| ---------------------------------------- | ---------------------------------------------- | ---------------------------------------------- |
| `ConfigureServices<T>` (with post-build) | Configures services with post-build action     | Execute code after service provider is built   |
| `ConfigureServicesAdvanced<T>`           | Configure with custom `ServiceProviderOptions` | Custom validation, scope validation settings   |
| `GetServiceCollection<T>`                | Get service collection for delayed building    | Build service provider manually later          |
| `BuildServiceProvider<T>`                | **Build and assign from existing collection**  | **Separate configuration and building phases** |

> **⚠️ CRITICAL**: When using `GetServiceCollection` followed by `BuildServiceProvider`, you MUST call `BuildServiceProvider` to complete the setup. This ensures the service provider is fully built and assigned before resolving any services.

**Example of the Critical Pattern:**

```csharp
// Step 1: Get service collection
var services = Application.Current.GetServiceCollection(services =>
{
    services.AddSingleton<IDataService, DataService>();
    services.Register(); // Auto-discovery
});

// Step 2: CRITICAL - Build and assign services
var serviceProvider = Application.Current.BuildServiceProvider(services);

// Step 3: Now you can safely resolve services
var dataService = serviceProvider.GetRequiredService<IDataService>();
```

#### Auto-Discovery Registration

The library includes two powerful auto-discovery features for automatic service registration:

1. **Interface-Based Discovery**: Automatically finds and registers all implementations of a specified interface type
2. **Attribute-Based Discovery**: Uses `AutoRegisterAttribute` to declaratively mark classes for automatic registration

| Method                                  | Description                                            | Use Case                                   |
| --------------------------------------- | ------------------------------------------------------ | ------------------------------------------ |
| `Register<TInterface>(ServiceLifetime)` | Auto-discovers and registers interface implementations | Automatically register all implementations |
| `Register()`                            | Auto-discovers classes with `AutoRegisterAttribute`    | Declarative attribute-based registration   |
| `Register(Assembly)`                    | Auto-discovers from specific assembly                  | Target specific assemblies for scanning    |
| `Register(Assembly[])`                  | Auto-discovers from multiple assemblies                | Multi-assembly attribute-based discovery   |

**Interface-Based Auto-Discovery Features:**

-   ✅ **Default Assembly Scan**: Automatically scans the calling assembly if no assemblies specified
-   ✅ **Multiple Assembly Support**: Can scan multiple assemblies for implementations
-   ✅ **Flexible Service Scope**: Support for Singleton, Scoped, and Transient lifetimes
-   ✅ **Convention-Based**: No configuration needed - finds all implementations automatically
-   ✅ **Type Safety**: Compile-time checking with generic constraints

**Attribute-Based Auto-Discovery Features:**

-   ✅ **Declarative Registration**: Use `[AutoRegister]` attribute to mark classes for registration
-   ✅ **Service Scope Control**: Specify Singleton, Scoped, or Transient lifetime per class
-   ✅ **Interface Targeting**: Register class as specific interface type
-   ✅ **Automatic Interface Detection**: Registers all implemented interfaces when no specific type provided
-   ✅ **Dual Registration**: When interface specified, registers both interface and concrete type
-   ✅ **Assembly Scanning**: Scan calling assembly or specify target assemblies

**Interface-Based Discovery Examples:**

```csharp
// Auto-discover all ITabView implementations from assemblies added via AddAssembly()
host.AddAssembly(typeof(MyClass).Assembly)
    .ConfigureServices(services => services.Register<ITabView>(ServiceLifetime.Transient));

// Auto-discover from current assembly (default if no AddAssembly calls)
services.Register<ITabView>(ServiceLifetime.Transient);

// Auto-discover all IRepository implementations as scoped services
services.Register<IRepository>(ServiceLifetime.Scoped);

// Auto-discover all IService implementations as singletons
services.Register<IService>(ServiceLifetime.Singleton);
```

**Attribute-Based Discovery Examples:**

```csharp
// Step 1: Mark classes with AutoRegisterAttribute
[AutoRegister(ServiceLifetime.Singleton)]
public class DatabaseService : IDatabaseService
{
    // Implementation
}

[AutoRegister(ServiceLifetime.Transient)]
public class EmailService : IEmailService
{
    // Implementation
}

[AutoRegister(ServiceLifetime.Scoped, typeof(IUserService))]
public class UserService : IUserService, IAdminService
{
    // This will be registered as IUserService (specified) and UserService (concrete)
    // IAdminService will NOT be registered automatically
}

[AutoRegister] // Default: Transient scope, all interfaces
public class LoggingService : ILoggingService, IDisposable
{
    // Registered as: ILoggingService and LoggingService
    // IDisposable is excluded automatically
}

// Step 2: Auto-discover during service configuration
this.ConfigureServices(services =>
{
    // Scan calling assembly for all [AutoRegister] classes
    services.Register();

    // Or scan specific assemblies
    services.Register(typeof(BusinessLayer).Assembly, typeof(DataLayer).Assembly);
});
```

**AutoRegisterAttribute API:**

```csharp
// Default constructor - Transient scope, all interfaces
[AutoRegister]

// Specify service scope only
[AutoRegister(ServiceLifetime.Singleton)]
[AutoRegister(ServiceLifetime.Scoped)]
[AutoRegister(ServiceLifetime.Transient)]

// Specify scope and target interface
[AutoRegister(ServiceLifetime.Singleton, typeof(IMyService))]
```

**Comparison: Manual vs Auto-Discovery**

```csharp
// ❌ Traditional Manual Registration
services.AddTransient<ITabView, HomeView>();
services.AddTransient<ITabView, WeatherView>();
services.AddTransient<ITabView, DataView>();
services.AddTransient<ITabView, SettingsView>();
services.AddSingleton<IDatabaseService, DatabaseService>();
services.AddScoped<IUserService, UserService>();
// ... must add each new implementation manually

// ✅ Interface-Based Auto-Discovery
services.Register<ITabView>(ServiceLifetime.Transient);
// Automatically finds and registers ALL ITabView implementations!

// ✅ Attribute-Based Auto-Discovery
services.Register();
// Automatically finds and registers ALL [AutoRegister] classes!
```

### Memory Management

The library uses `ConditionalWeakTable<object, IServiceProvider>` internally. This means:

-   ✅ Objects can be garbage collected normally
-   ✅ Service providers are automatically cleaned up
-   ✅ No memory leaks from long-lived service providers
-   ✅ No need to manually dispose or clean up

### Thread Safety

The `ConditionalWeakTable` is thread-safe for concurrent reads and writes. However, individual service providers follow Microsoft's DI guidelines:

-   **Singleton services**: Thread-safe (single instance)
-   **Transient services**: New instance per request (no shared state)
-   **Scoped services**: One instance per scope (not thread-safe across scopes)

### Sample Applications

#### WpfExample - Complete Real-World Application

A comprehensive WPF application demonstrating:

-   **TabViewHandler Pattern**: Complete decoupling with automatic tab discovery
-   **Blazing.ToggleSwitch.Wpf**: Modern toggle switch controls in the Settings tab
-   **Service-Based Architecture**: Clean separation of concerns with DI
-   **MVVM Best Practices**: Proper View/ViewModel separation with automatic pairing

**Location**: `src/samples/WpfExample/`

**Run the example**:

```bash
# .NET 8.0
dotnet run --project src/samples/WpfExample --framework net8.0-windows

# .NET 9.0
dotnet run --project src/samples/WpfExample --framework net9.0-windows
```

**Key Features**:

-   Automatic tab discovery without hardcoded references
-   Toggle switches with customizable styling and modern design
-   Weather service integration with HTTP calls and async patterns
-   Data visualization with charts and interactive components
-   Settings management with persistent preferences using toggle controls
-   Complete dependency injection throughout the application stack
-   Responsive design that adapts to different window sizes

See `src/samples/WpfExample/README.md` for detailed documentation of the TabViewHandler pattern and complete architecture guide.

### Recommended Patterns

The library supports different patterns depending on your application type. Here are the recommended approaches for each:

#### WPF Application Pattern

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Configure services ONCE on Application.Current
        this.ConfigureServices(services =>
        {
            services.AddSingleton<IDataService, DataService>();
            services.AddTransient<IDialogService, DialogService>();

            // Add keyed services for multi-database scenarios
            services.AddKeyedSingleton<IDbContext, PrimaryDbContext>("primary");
            services.AddKeyedSingleton<IDbContext, AnalyticsDbContext>("analytics");

            // Auto-discovery
            services.Register<ITabView>(ServiceLifetime.Transient);
            services.Register(); // Attribute-based discovery
        });
    }
}

// Alternative: Advanced pattern with assembly management
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Step 1: Add assemblies for discovery
        var services = this.AddAssembly(typeof(Business.IBusinessService).Assembly)
                          .AddAssembly(typeof(Data.IRepository).Assembly)
                          .GetServiceCollection(services =>
        {
            services.AddSingleton<IDataService, DataService>();
            services.Register<IBusinessService>(ServiceLifetime.Scoped);
            services.Register(); // Scans added assemblies
        });

        // Step 2: Build and assign services - CRITICAL!
        var serviceProvider = this.BuildServiceProvider(services);
    }
}

// In your windows/views
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Get provider from Application
        var services = Application.Current.GetServices();
        var dataService = services!.GetRequiredService<IDataService>();

        // Resolve keyed services
        var primaryDb = Application.Current.GetRequiredKeyedService<IDbContext>("primary");
        var analyticsDb = Application.Current.GetRequiredKeyedService<IDbContext>("analytics");
    }
}
```

#### WinForms Application Pattern

```csharp
// Program.cs
public static class Program
{
    public static ApplicationHost Host { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Host = new ApplicationHost();
        Host.ConfigureServices(services =>
        {
            services.AddSingleton<IDataService, DataService>();
            services.Register(); // Auto-discovery
        });

        Application.Run(new MainForm());
    }
}

// Advanced pattern with assembly management
public static class Program
{
    public static ApplicationHost Host { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        Host = new ApplicationHost();

        // Step 1: Add assemblies and configure
        var services = Host.AddAssembly(typeof(Business.IBusinessService).Assembly)
                          .GetServiceCollection(services =>
        {
            services.AddSingleton<IDataService, DataService>();
            services.Register<IBusinessService>(ServiceLifetime.Scoped);
            services.Register(); // Scans added assemblies
        });

        // Step 2: Build and assign services - CRITICAL!
        var serviceProvider = Host.BuildServiceProvider(services);

        Application.Run(new MainForm());
    }
}

public class ApplicationHost { }

// In your forms
public class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        var services = Program.Host.GetServices();
        var dataService = services!.GetRequiredService<IDataService>();
    }
}
```

#### Console Application Pattern

```csharp
class Program
{
    static void Main(string[] args)
    {
        var host = new ApplicationHost();

        // Simple pattern
        host.ConfigureServices(services =>
        {
            services.AddSingleton<IService, ServiceImpl>();
            services.Register(); // Auto-discovery
        });

        var serviceProvider = host.GetServices();
        var service = serviceProvider!.GetRequiredService<IService>();

        // Use service...
    }
}

// Advanced pattern with assembly management
class Program
{
    static void Main(string[] args)
    {
        var host = new ApplicationHost();

        // Step 1: Add assemblies and get service collection
        var services = host.AddAssembly(typeof(Business.IBusinessService).Assembly)
                          .GetServiceCollection(services =>
        {
            services.AddSingleton<IService, ServiceImpl>();
            services.Register<IBusinessService>(ServiceLifetime.Scoped);
            services.Register(); // Scans added assemblies
        });

        // Step 2: Build and assign services - CRITICAL!
        var serviceProvider = host.BuildServiceProvider(services);

        // Step 3: Use services
        var service = serviceProvider.GetRequiredService<IService>();
        var businessService = serviceProvider.GetRequiredService<IBusinessService>();
    }
}

class ApplicationHost { }
```

## History

### V1.0.0

-   Added universal dependency injection support for any .NET object
-   Implemented `ConditionalWeakTable` based memory management
-   Created comprehensive API with `ConfigureServices`, `GetServices`, and `SetServices` extension methods
-   **Added full keyed services support** with `GetRequiredKeyedService` and `GetKeyedService` methods
-   Added support for WPF, WinForms, and Console applications
-   Included advanced configuration options with `ServiceProviderOptions`
-   Built comprehensive test suite with 90 unit tests (100% passing)
-   Created WpfExample sample application demonstrating real-world usage
-   Added Blazing.ToggleSwitch.Wpf component library
-   Established project structure with centralized build and package management

**Requirements:**

-   .NET 8.0 or later
-   Microsoft.Extensions.DependencyInjection 9.0.0 or later

**Project Structure:**

```
Blazing.Extensions.DependencyInjection/           # Solution root
├── src/
│   ├── Blazing.Extensions.DependencyInjection/   # Main DI library
│   ├── libs/
│   │   └── Blazing.ToggleSwitch.Wpf/             # WPF ToggleSwitch control library
│   └── samples/
│       └── WpfExample/                          # Comprehensive WPF example
├── tests/
│   └── UnitTests/                              # Comprehensive unit tests (90 tests)
├── Directory.Build.props                        # Centralized build properties
├── Directory.Packages.props                     # Centralized package versions
└── README.md                                   # This file
```

**Building:**

```bash
# Build everything
dotnet build

# Run tests
dotnet test

# Run WPF example (.NET 8.0)
dotnet run --project src/samples/WpfExample --framework net8.0-windows

# Run WPF example (.NET 9.0)
dotnet run --project src/samples/WpfExample --framework net9.0-windows
```

**Contributing:**

Contributions are welcome! Please feel free to submit issues or pull requests.

**License:**

This project is licensed under MIT License. See the LICENSE file for details.

**Acknowledgments:**

-   Built on Microsoft's excellent [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/) framework
-   Toggle switch control inspired by the CodeProject article "[Flexible WPF ToggleSwitch Lookless Control](https://www.codeproject.com/articles/WPF-ToggleSwitch-Control)" by Graeme Grant
-   Modern .NET patterns and practices from the .NET community
