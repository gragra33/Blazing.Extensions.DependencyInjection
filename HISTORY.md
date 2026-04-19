# Changes History

All notable changes to this project will be documented in this file.

### V3.0.1 - 19 April 2026

**Bug Fixes:**

- **Missing source generator in NuGet package** — The Roslyn source generator DLL was not included in the published NuGet package. Consumers who installed v3.0.0 from NuGet.org did not receive the compile-time auto-registration code generation. The generator is now correctly embedded under `analyzers/dotnet/cs/` via an explicit `<None Pack="true">` item.
- **snupkg symbol package rejected by NuGet.org** — The symbol package upload failed with `pdb(s) for a corresponding dll(s) not found` because the generator's separate `.pdb` file was included in the snupkg but NuGet.org only validates PDBs against `lib/` assemblies. Fixed by setting `<DebugType>embedded</DebugType>` on the source generator project, which bakes the PDB into the DLL and produces no standalone `.pdb` file.

### V3.0.0 - 17 April 2026

**Breaking Changes:**

- Removed `services.Register<TInterface>(lifetime)` — use `[AutoRegister(lifetime, typeof(TInterface))]` on the implementation class instead
- Removed `services.Register(Assembly[])` — cross-assembly discovery is now automatic via the source generator
- Removed `host.AddAssembly()` / `host.AddAssemblies()` — cross-assembly discovery is now automatic via the source generator; use `services.Register()` directly
- Removed `services.AddCachingDecorator<T>()` / `services.AddLoggingDecorator<T>()` — use `[CachingDecorator]` / `[LoggingDecorator]` attributes instead
- Removed single-argument open generic overloads from `GenericServiceExtensions`

**New Features:**

- **Source Generator / AOT Compatible** — `services.Register()` is now powered by a Roslyn `IIncrementalGenerator`; emitted at compile time; zero runtime reflection; fully compatible with `PublishAot=true`
- **Cross-Assembly Discovery** — `[AutoRegister]` services in all referenced assemblies are discovered automatically via `compilation.References`; no assembly lists needed
- **`[AutoRegister]` — `Key` property** — emits `AddKeyedSingleton/Scoped/Transient` for keyed service registration
- **`[AutoRegister]` — `LocalOnly` property** — when `true`, the service is excluded from cross-assembly discovery; only registered in the declaring assembly (canonical use case: Blazor WASM services that require `HttpClient` unavailable in a Blazor Server host)
- **`[CachingDecorator(seconds)]`** — attribute-driven, source-generated caching decorator; caches sync non-void, `Task<T>`, and `ValueTask<T>` method results; no DispatchProxy; AOT-safe
- **`[LoggingDecorator]`** — attribute-driven, source-generated logging decorator; uses `[LoggerMessage]` partial methods (three per interface method: Calling/Completed/Failed); high-performance structured logging; AOT-safe
- **`IDecoratorCache`** — pluggable cache abstraction injected into every `[CachingDecorator]`-generated class; four built-in adapters: `DefaultDecoratorCache` (ConcurrentDictionary), `MemoryCacheDecoratorCache` (IMemoryCache), `HybridCacheDecoratorCache` (HybridCache L1+L2), `DistributedCacheDecoratorCache` (IDistributedCache + serializer)
- **`IBlazingCacheInvalidator<T>`** — injectable per-service invalidator; `InvalidateAsync(methodName, args[])` removes individual cached entries without coupling the caller to the generated decorator
- **`IBlazingInvalidatable`** — implemented by every generated caching decorator; `InvalidateCacheAsync(key)` for single-entry eviction; `InvalidateAllCacheAsync()` for full flush
- **Cache key format** — `__{InterfaceSimpleName}__{MethodName}_{arg0}_{arg1}…` — deterministic, predictable, composable with `RemoveByPrefixAsync`
- **`SwitchableDecoratorCache` sample pattern** — runtime backend switching support pattern used by sample applications (Default / MemoryCache / HybridCache)

**New Samples:**

- **MultiTenantExample** — Advanced multi-tenant ASP.NET Core Web API with Blazor WebAssembly client; demonstrates `LocalOnly` for client-only services, keyed services per tenant, cross-assembly discovery, and enterprise-grade DI patterns
- **BlazorServerExample — Cache page** — Interactive `/cache` Blazor page: live hit/miss detection for sync / `Task<T>` / `ValueTask<T>` methods, per-key invalidation via `IBlazingCacheInvalidator<T>`, and runtime backend switching (Default / MemoryCache / HybridCache) via `SwitchableDecoratorCache`
- **WpfExample — Cache tab** — WPF MVVM "Cache" tab with identical demonstration in a desktop context; uses `CacheViewModel` / `CacheView` / `SwitchableDecoratorCache`
- **ConsoleExample — CachingDecoratorExample** — Automated test-style console output showing miss→hit for all three return-type variants, per-key invalidation, and runtime backend switching (Default / MemoryCache / HybridCache) via `DemoSwitchableDecoratorCache`

**Improvements:**

- **Bug fix** — `InitializeAllAsync()` now correctly initialises multiple instances of the same concrete type; tracking changed from `HashSet<Type>` to `HashSet<IAsyncInitializable>` (reference equality)
- **Test infrastructure** — test project restructured to industry-standard `Fixtures/UnitTests/IntegrationTests` folder layout; renamed to `Blazing.Extensions.DependencyInjection.Tests`; 172 tests across net8.0, net9.0, net10.0

### V2.2.0 - 8 December 2025

**Improvements:**

- **Enhanced Extension Methods** — Improved `AsyncInitializationExtensions`, `ServiceEnumerationExtensions`, and `ServiceValidationExtensions` with better error handling and performance optimizations

**New Sample:**

- **MultiTenantExample** — Advanced multi-tenant ASP.NET Core Web API with Blazor WebAssembly client demonstrating enterprise-grade patterns, tenant isolation, and comprehensive DI features integration

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

- **.NET 10.0 Support** - Added support for .NET 10.0 applications

### V1.0.0 (.Net 8.0+) - 5 October, 2025

- **Universal DI Support** - Added universal dependency injection support for any .NET object
- **Memory Management** - Implemented `ConditionalWeakTable` based memory management for automatic cleanup
- **Core API** - Created comprehensive API with `ConfigureServices`, `GetServices`, and `SetServices` extension methods
- **Keyed Services** - Added full keyed services support with `GetRequiredKeyedService` and `GetKeyedService` methods
- **Platform Support** - Supports all .Net Application types; specifically designed for a common usage pattern across WPF, WinForms, and Console applications
- **Advanced Configuration** - Included advanced configuration options with `ServiceProviderOptions`
- **Comprehensive Testing** - Built comprehensive test suite with 104 unit tests (100% passing)
- **Sample Applications** - Created three sample applications: WpfExample, WinFormsExample, and ConsoleExample
- **AutoRegister Attribute** - Added AutoRegister attribute for declarative service registration
- **Bonus Component Libraries** - Added Blazing.ToggleSwitch.Wpf and Blazing.ToggleSwitch.WinForms component libraries
- **Project Structure** - Established project structure with centralized build and package management
