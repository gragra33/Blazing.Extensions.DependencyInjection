# BlazorServerExample - Blazing.Extensions.DependencyInjection

This example demonstrates dependency injection in a **Blazor Server** application using **Blazing.Extensions.DependencyInjection**, with interactive server-side rendering and a tabbed page structure.

## Features Demonstrated

### Core Architecture Patterns

- **`[AutoRegister]` Attribute**: Declarative service registration — `services.Register()` discovers all decorated services automatically, eliminating boilerplate `AddSingleton`/`AddTransient` calls
- **Assembly Scanning**: `builder.Services.AddAssembly(typeof(Program).Assembly)` registers the assembly for scanning before `Register()` is called
- **Service Abstraction**: Interfaces for data, weather, and product catalog services injected directly into Razor components via `@inject`
- **Async Operations**: Async service calls with `await` patterns and Blazor's `OnInitializedAsync` lifecycle hooks

### Caching Decorator

- **`[CachingDecorator]` attribute**: Source-generated decorator wraps `IProductCatalogService` methods with `IDecoratorCache`, caching results for a configurable TTL
- **Runtime backend switching**: Toggle between Default, MemoryCache, and HybridCache without restarting the application
- **Cache hit/miss tracking**: Live counters showing backend calls vs cache hits
- **Per-key invalidation**: Invalidate individual cached entries via `IBlazingCacheInvalidator<T>`
- **`SwitchableDecoratorCache`**: Custom `IDecoratorCache` wrapper registered before `services.Register()` to override the source-generated default

### UI & Interactivity

- **Interactive Server Render Mode**: All pages use `@rendermode InteractiveServer` for real-time interactivity
- **Toggle Switch**: `<Toggle>` component for boolean settings with `OnText`/`OffText` customisation
- **Blazor Router**: Page-based navigation with `@page` directives

## Application Structure

```
BlazorServerExample/
├── Program.cs                           # DI setup with AddAssembly + Register()
├── Components/
│   ├── App.razor                        # Root component
│   ├── Routes.razor                     # Router configuration
│   ├── MainLayout.razor                 # Shared page layout
│   └── Pages/
│       ├── Home.razor                   # Welcome page (@page "/")
│       ├── Weather.razor                # Async weather data (@page "/weather")
│       ├── Data.razor                   # CRUD data management (@page "/data")
│       ├── Settings.razor               # Preferences with Toggle controls (@page "/settings")
│       └── Cache.razor                  # Caching decorator demo (@page "/cache", "/caching")
└── Services/
    ├── IWeatherService.cs               # Weather service interface
    ├── WeatherService.cs                # [AutoRegister(Transient)] implementation
    ├── WeatherData.cs                   # Weather data model
    ├── IDataService.cs                  # Data service interface
    ├── DataService.cs                   # [AutoRegister(Singleton)] implementation
    ├── IProductCatalogService.cs        # Product catalog interface
    ├── ProductCatalogService.cs         # [AutoRegister(Singleton)] + [CachingDecorator(seconds: 30)]
    └── SwitchableDecoratorCache.cs      # Runtime-switchable IDecoratorCache wrapper
```

## Key Implementation Details

### AutoRegister + Assembly Scanning in Program.cs

```csharp
// Register the assembly for scanning
builder.Services.AddAssembly(typeof(Program).Assembly);

// Register cache backends BEFORE Register() to prevent the source-generated
// TryAddSingleton<IDecoratorCache, DefaultDecoratorCache> from overriding SwitchableDecoratorCache
builder.Services.AddMemoryCache();
builder.Services.AddHybridCache();
builder.Services.AddSingleton<SwitchableDecoratorCache>();
builder.Services.AddSingleton<IDecoratorCache>(sp => sp.GetRequiredService<SwitchableDecoratorCache>());

// Auto-discover and register all [AutoRegister]-decorated services
builder.Services.Register();
```

### Service Registration with AutoRegister

```csharp
// Transient — new instance per request
[AutoRegister(ServiceLifetime.Transient, typeof(IWeatherService))]
public class WeatherService : IWeatherService { }

// Singleton with caching decorator
[AutoRegister(ServiceLifetime.Singleton)]
[CachingDecorator(seconds: 30)]
public class ProductCatalogService : IProductCatalogService { }
```

### Injecting Services into Razor Components

```razor
@inject IWeatherService WeatherService
@inject IDataService DataService
@inject IProductCatalogService ProductCatalog
@inject SwitchableDecoratorCache SwitchableCache
@inject IBlazingCacheInvalidator<IProductCatalogService> CacheInvalidator
```

### CachingDecorator — Cache Hit/Miss Pattern

```csharp
// The source generator wraps ProductCatalogService methods.
// On first call → backend is invoked, result is cached.
// On subsequent calls within TTL → cached result returned, backend skipped.
var name = ProductCatalog.GetName(productId);        // sync, cached
var nameAsync = await ProductCatalog.GetNameAsync(productId);  // Task<T>, cached
var count = await ProductCatalog.GetCountAsync(productId);     // ValueTask<T>, cached
```

## Pages Overview

| Page         | Route       | Feature Demonstrated                                        |
| ------------ | ----------- | ----------------------------------------------------------- |
| **Home**     | `/`         | Welcome, static overview                                    |
| **Weather**  | `/weather`  | Async service call, Toggle for °C/°F                        |
| **Data**     | `/data`     | CRUD operations via `IDataService`                          |
| **Settings** | `/settings` | Toggle switches, preference binding                         |
| **Caching**  | `/cache`    | CachingDecorator, runtime backend switch, hit/miss tracking |

## Running the Application

1. **Build the solution:**

    ```bash
    dotnet build
    ```

2. **Run the server:**

    ```bash
    dotnet run --project src/samples/BlazorServerExample
    ```

3. **Access the application:**
    - Open your browser to `https://localhost:5001` (or check the console for the actual URL)

4. **Navigate through the pages:**
    - **Home**: Overview and status information
    - **Weather**: Select a location and fetch simulated weather data
    - **Data**: Add and clear data items via `IDataService`
    - **Settings**: Toggle dark mode, notifications, and refresh interval
    - **Caching**: Switch cache backends, track hits/misses, and invalidate specific product entries

## Dependencies

- .NET 8.0, 9.0, or 10.0
- Blazing.Extensions.DependencyInjection
- Microsoft.Extensions.Caching.Memory
- Microsoft.Extensions.Caching.Hybrid

## Architecture Benefits

1. **Zero Boilerplate Registration**: `[AutoRegister]` eliminates manual `AddSingleton`/`AddTransient` calls
2. **Source-Generated Caching**: `[CachingDecorator]` wraps service methods at compile time — no runtime reflection
3. **Switchable Backends**: `SwitchableDecoratorCache` shows how to override the default cache at registration time
4. **Clean Components**: Razor pages depend only on interfaces, injected by the DI container
5. **Testability**: All services are interface-based and easily mockable

This example serves as a practical demonstration of how Blazing.Extensions.DependencyInjection simplifies service wiring in Blazor Server applications while enabling advanced patterns like source-generated caching decorators.
