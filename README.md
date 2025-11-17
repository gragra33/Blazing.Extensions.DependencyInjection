# Blazing.Extensions.DependencyInjection: A Simplified Universal Dependency Injection for .NET

[![NuGet Version](https://img.shields.io/nuget/v/Blazing.Extensions.DependencyInjection.svg)](https://www.nuget.org/packages/Blazing.Extensions.DependencyInjection)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Blazing.Extensions.DependencyInjection.svg)](https://www.nuget.org/packages/Blazing.Extensions.DependencyInjection)

## Overview

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
-   [Requirements](#requirements)
-   [Project Structure](#project-structure)
-   [Building](#building)
-   [Contributing](#contributing)
-   [License](#license)
-   [Acknowledgments](#acknowledgments)
-   [History](#history)

## Quick Start

Get up and running with dependency injection in minutes across any .NET application type. This library eliminates the complexity of setting up Microsoft's dependency injection container in WPF, WinForms, and Console applications.

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

Configure the library in your application startup. The `ConfigureServices` method will add the required services and enable dependency injection for any .NET object. The setup process is identical across all application types, providing a consistent experience whether you're building desktop or console applications.

> **🔑 Key Pattern**: For advanced scenarios, use `GetServiceCollection` + `BuildServiceProvider` to separate configuration from building, giving you full control over the service provider creation.

#### WPF Applications

WPF applications benefit from dependency injection through the Application.Current instance, providing global access to services throughout the application lifecycle. The integration seamlessly works with MVVM patterns and supports both manual service registration and attribute-based auto-discovery.

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

WinForms applications use a standard ServiceCollection and ServiceProvider pattern, typically configured in the Program.cs entry point. This approach provides excellent control over form instantiation and supports dependency injection into forms, user controls, and business logic components.

```csharp
// Program.cs
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // Configure services
            var services = new ServiceCollection();

            // Auto-discover and register services with AutoRegister attribute
            services.Register();

            // Or manually register services
            // services.AddSingleton<IDataService, DataService>();

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

// MainForm with AutoRegister attribute
[AutoRegister(ServiceLifetime.Transient)]
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        // Services are injected automatically via constructor or resolved via static methods
    }
}
```

#### Console Applications

Console applications can leverage a simple ApplicationHost pattern for lightweight dependency injection scenarios. This approach is perfect for command-line tools, background services, and simple applications that need service management without the overhead of full hosting models.

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

Once configured, the library provides intuitive methods for service registration and resolution throughout your application. The API follows Microsoft's standard dependency injection patterns while adding convenience methods for common scenarios.

#### Basic Service Configuration

Service configuration follows the familiar Microsoft.Extensions.DependencyInjection patterns with additional extension methods for convenience. The library supports all standard service lifetimes and provides fluent configuration methods for complex scenarios.

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

Service resolution is available anywhere in your application through extension methods on Application.Current or direct ServiceProvider access. The library provides both required and optional service resolution methods, with full support for generic and keyed service resolution.

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

For complex applications requiring fine-grained control over service provider creation, the library offers separation between service collection configuration and provider building. This pattern enables conditional service registration, environment-specific configurations, and advanced scenarios like service decoration or interception.

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

Assembly discovery can be explicitly controlled by adding specific assemblies to the scanning process before service configuration. This approach is particularly useful in modular applications where services are distributed across multiple assemblies or when you need to avoid scanning unnecessary assemblies for performance reasons.

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

The fluent assembly management API enables clean, chainable configuration of multiple assemblies in a single operation. This pattern provides excellent readability and maintainability when working with complex multi-assembly applications.

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

This library delivers enterprise-grade dependency injection capabilities with unique features like keyed services support and attribute-based auto-registration. The implementation focuses on performance, safety, and developer productivity while maintaining compatibility with Microsoft's standard dependency injection patterns.

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

Keyed services enable multiple implementations of the same interface to coexist within the same service container, identified by unique keys. This powerful feature is essential for implementing strategy patterns, multi-tenant architectures, and complex business scenarios requiring conditional service resolution.

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

Keyed service resolution provides type-safe access to specific service implementations through their registered keys. The resolution methods are available through extension methods on Application.Current, making keyed services accessible throughout your application with consistent API patterns.

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

ViewModels can leverage keyed services for implementing complex business logic that requires different service implementations based on runtime conditions. This pattern is particularly powerful in MVVM architectures where ViewModels need to adapt their behavior based on user preferences, application state, or business rules.

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

Keyed services excel in enterprise scenarios requiring multiple implementations of the same interface with different behaviors or configurations. Common use cases include multi-database architectures, payment gateway abstractions, notification providers, and environment-specific service implementations.

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

The **Blazing.Extensions.DependencyInjection** ecosystem provides a comprehensive set of tools for dependency injection across different .NET application types. Each library is designed with specific use cases in mind while maintaining consistency in API design and behavior patterns.

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

The core extension methods provide the foundation for dependency injection across all application types, offering both simple configuration methods and advanced control patterns. These methods are designed to work consistently whether you're building WPF, WinForms, or Console applications.

| Method                 | Description                                                      | Parameters                                                                        | Returns             |
| ---------------------- | ---------------------------------------------------------------- | --------------------------------------------------------------------------------- | ------------------- |
| `ConfigureServices<T>` | Configures and builds a service provider for any object instance | `instance`: Target object<br/>`configureServices`: Service configuration action   | `IServiceProvider`  |
| `GetServices`          | Gets the service provider associated with an object instance     | `instance`: Target object                                                         | `IServiceProvider?` |
| `SetServices`          | Sets or removes the service provider for an object instance      | `instance`: Target object<br/>`serviceProvider`: Provider to set (null to remove) | `void`              |
| `AddAssembly<T>`       | Adds an assembly to the instance for auto-discovery              | `instance`: Target object<br/>`assembly`: Assembly to add                         | `T` (for chaining)  |
| `AddAssemblies<T>`     | Adds multiple assemblies to the instance for auto-discovery      | `instance`: Target object<br/>`assemblies`: Assemblies to add                     | `T` (for chaining)  |

#### Service Resolution Methods

Service resolution methods provide convenient access to registered services with support for both traditional and keyed service patterns. These methods handle null safety and provide clear error messages when services are not found or improperly configured.

| Method                              | Description                                                       | Parameters                                              | Returns     |
| ----------------------------------- | ----------------------------------------------------------------- | ------------------------------------------------------- | ----------- |
| `GetRequiredService<TService>`      | Gets a required service from the object's service provider        | `instance`: Target object                               | `TService`  |
| `GetService<TService>`              | Gets an optional service from the object's service provider       | `instance`: Target object                               | `TService?` |
| `GetRequiredKeyedService<TService>` | Gets a required keyed service from the object's service provider  | `instance`: Target object<br/>`serviceKey`: Service key | `TService`  |
| `GetKeyedService<TService>`         | Gets an optional keyed service from the object's service provider | `instance`: Target object<br/>`serviceKey`: Service key | `TService?` |
| `ClearServices`                     | Removes the service provider from the object instance             | `instance`: Target object                               | `bool`      |

#### Advanced Configuration Methods

Advanced configuration methods enable fine-grained control over service provider creation and lifecycle management. These methods are essential for complex applications requiring custom validation, performance optimization, or integration with existing service provider patterns.

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

Manual service registration provides explicit control and clear dependency visibility, while auto-discovery reduces boilerplate code and maintenance overhead. Choose manual registration for critical services requiring specific configuration, and auto-discovery for conventional services following standard patterns.

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

The included sample applications provide practical, real-world examples of dependency injection implementation across different .NET application types. Each sample demonstrates best practices, common patterns, and advanced scenarios you'll encounter in production applications.

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

# .NET 10.0
dotnet run --project src/samples/WpfExample --framework net10.0-windows
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

#### WinFormsExample - AutoRegister Pattern Demo

A WinForms application demonstrating the AutoRegister attribute pattern:

-   **AutoRegister Attributes**: All services and views marked with `[AutoRegister]`
-   **Dynamic Tab Discovery**: Automatic ITabView implementation discovery
-   **Service-Based Architecture**: Clean separation with dependency injection
-   **Console Window Integration**: Shows both GUI and console output

**Location**: `src/samples/WinFormsExample/`

**Run the example**:

```bash
dotnet run --project src/samples/WinFormsExample
```

**Key Features**:

-   Complete AutoRegister attribute usage pattern
-   Automatic tab discovery and registration
-   Console window integration for debugging
-   Service-based architecture matching WPF example

#### ConsoleExample - Comprehensive API Demo

A console application demonstrating all library features:

-   **Basic Service Configuration**: Standard DI patterns
-   **Keyed Services**: Multiple implementations per interface
-   **AutoRegister Discovery**: Attribute-based service registration
-   **Performance Testing**: Service resolution benchmarks

**Location**: `src/samples/ConsoleExample/`

**Run the example**:

```bash
dotnet run --project src/samples/ConsoleExample
```

### Recommended Patterns

The library supports different patterns depending on your application type. Here are the recommended approaches for each:

#### WPF Application Pattern

WPF applications should configure dependency injection in the App.xaml.cs OnStartup method to ensure services are available before any windows are created. This pattern integrates seamlessly with WPF's application lifecycle and provides global service access through the Application.Current instance.

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

WinForms applications should configure services in the Program.cs Main method before creating any forms, then resolve the main form from the service provider to ensure proper dependency injection. This pattern provides excellent control over form instantiation and supports complex dependency scenarios.

```csharp
// Program.cs
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // Configure services
            var services = new ServiceCollection();

            // Auto-discover and register services with AutoRegister attribute
            services.Register();

            // Or manually register services
            // services.AddSingleton<IDataService, DataService>();

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

// MainForm with AutoRegister attribute
[AutoRegister(ServiceLifetime.Transient)]
public partial class MainForm : Form
{
    // Services are injected automatically via constructor or resolved via static methods
}
```

Alternative: Direct ServiceProvider pattern (matches WinFormsExample sample)

```csharp
internal static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            // Configure services directly
            var services = new ServiceCollection();

            // Manual registration
            services.AddSingleton<IDataService, DataService>();
            services.AddTransient<IDialogService, DialogService>();

            // Auto-discovery from multiple assemblies
            services.Register(typeof(Business.IBusinessService).Assembly,
                            typeof(Data.IRepository).Assembly);

            // Build and store service provider globally
            ServiceProvider = services.BuildServiceProvider();

            // Resolve and run main form
            var mainForm = ServiceProvider.GetRequiredService<MainForm>();
            Application.Run(mainForm);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error: {ex.Message}", "Application Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

// In your forms
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();

        // Access services via static ServiceProvider
        var dataService = Program.ServiceProvider.GetRequiredService<IDataService>();
    }
}
```

#### Console Application Pattern

Console applications can use either a simple ApplicationHost pattern for basic scenarios or integrate with Microsoft.Extensions.Hosting for more complex applications. The simple pattern is ideal for command-line tools and utilities, while the hosting pattern supports background services and complex application lifecycles.

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

## Requirements

-   .NET 8.0 or later
-   Microsoft.Extensions.DependencyInjection 9.0.0 or later

## Project Structure

The solution is organized into logical components with clear separation between core functionality, supporting libraries, sample applications, and tests. This structure facilitates easy navigation, maintenance, and extension of the codebase while supporting multiple development workflows.

```
Blazing.Extensions.DependencyInjection/           # Solution root
├── src/
│   ├── Blazing.Extensions.DependencyInjection/   # Main DI library
│   ├── libs/
│   │   ├── Blazing.ToggleSwitch.Wpf/             # WPF ToggleSwitch control library
│   │   └── Blazing.ToggleSwitch.WinForms/        # WinForms ToggleSwitch control library
│   └── samples/
│       ├── WpfExample/                           # WPF MVVM with TabViewHandler pattern
│       ├── WinFormsExample/                      # WinForms with AutoRegister attributes
│       └── ConsoleExample/                       # Console app demonstrating all features
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
```

## Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

### Development Setup

1. Clone the repository
2. Install .NET 8.0, 9.0, & 10.0 SDK
3. Run `dotnet restore`
4. Run `dotnet test`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

-   Built on Microsoft's excellent [Microsoft.Extensions.DependencyInjection](https://www.nuget.org/packages/Microsoft.Extensions.DependencyInjection/) framework
-   Toggle switch control inspired by the CodeProject article "[Flexible WPF ToggleSwitch Lookless Control](https://www.codeproject.com/articles/WPF-ToggleSwitch-Control)" by Graeme Grant
-   Modern .NET patterns and practices from the .NET community

## History

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
