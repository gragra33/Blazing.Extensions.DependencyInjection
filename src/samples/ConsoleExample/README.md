# Console Example

This console application demonstrates all the features of **Blazing.Extensions.DependencyInjection**, including advanced scenarios with multiple ServiceProviders and generic-constrained services.

## Features Demonstrated

### 1. Basic Service Configuration

- Simple service registration and configuration
- Singleton, Scoped, and Transient lifetimes
- Basic dependency injection

### 2. Keyed Services

- Multiple implementations of the same interface
- Service resolution by key
- Different notification services (Email, SMS, Push)

### 3. Assembly Scanning

- Automatic discovery of service implementations
- Registration of all classes implementing a specific interface
- Dynamic service discovery at runtime

### 4. AutoRegister Attribute

- Declarative service registration using attributes
- Automatic service lifetime configuration
- Reduced boilerplate registration code

### 5. Generic-Constrained Services with Multiple Interfaces

- **Generic data providers** with type constraints (`IDataProvider<T>`)
- **Generic processors** with type constraints (`IGenericProcessor<T>`)
- **Multiple implementations** for different types (String, Integer, Decimal)
- **Base interface usage** for service retrieval using `GetServices<T>()`
- **Multi-interface services** using AutoRegister attribute
- Service that implements multiple interfaces (`IDataService`, `IValidationService`, `ICacheService`)

### 6. Multiple Service Providers (Advanced)

- Independent service providers for different contexts
- Isolation between different application areas
- Primary and secondary service configurations

### 7. Advanced Configuration

- Post-build validation and setup
- Custom service provider configuration
- Service warming and validation

### 8. Service Collection Customization

- Manual service collection modification
- Custom ServiceProviderOptions with scope validation
- Fine-grained control over service building
- Proper scope management for scoped services

### 9. Service Provider Replacement

- Dynamic service provider replacement
- Configuration changes at runtime
- Service provider swapping

### 10. Memory Management

- Proper cleanup and disposal
- ConditionalWeakTable usage for leak prevention
- Service provider clearing

### 11. Async Service Initialization

- Priority-based async initialization ordering
- `IAsyncInitializable` interface for startup patterns
- Dependency management between initializers
- `InitializeAllAsync()` for coordinated startup

### 12. Lazy Service Initialization

- Deferred service creation until first access
- Performance benefits for expensive-to-create services
- `AddLazyKeyedSingleton<T>()` and lazy resolution patterns
- Usage across different service lifetimes

### 13. Open Generic Type Registration

- Single registration resolving to multiple closed-type variants
- Repository patterns with open generics (`IRepository<T>`)
- Validator and generic handler patterns
- Automatic resolution of `IRepository<Customer>`, `IRepository<Order>`, etc.

### 14. Service Decoration Patterns

- Decorator pattern for cross-cutting concerns (caching, logging, validation)
- `[LoggingDecorator]` and `[CachingDecorator]` attributes
- Source-generated decorator wrapping
- Wrapping services with additional behaviour without modifying originals

### 15. Service Enumeration & Filtering

- Plugin architecture patterns with `GetServices<T>()`
- Filtering and mapping over multiple service implementations
- Handler chains and ordered execution
- Iterating over all registered implementations of an interface

### 16. Service Factory Delegates

- Factory delegates for conditional service creation
- Configuration-based and environment-based service selection
- Keyed factory patterns
- `AddSingleton<T>((provider, key) => ...)` usage

### 17. Service Scoping & Lifecycle Management

- Creating and managing service scopes
- `CreateScope()` and `CreateAsyncScope()` patterns
- Synchronous and asynchronous disposal
- Preventing memory leaks with proper scope management

### 18. Service Validation & Diagnostics

- `ThrowIfInvalid()` for startup validation
- `GetDiagnostics()` for service collection inspection
- Circular dependency detection
- Lifetime violation warnings

### 19. Caching Decorator

- `[CachingDecorator(seconds: N)]` on service implementations
- Source-generated cache wrapping for `T`, `Task<T>`, and `ValueTask<T>` return types
- Default, MemoryCache, and HybridCache backends
- `IBlazingCacheInvalidator<T>` for per-key cache invalidation

## Running the Example

```bash
cd samples/ConsoleExample
dotnet run
```

The application multi-targets .NET 8.0, 9.0, and 10.0. Specify a framework when needed:

```bash
dotnet run --framework net10.0
```

## Expected Output

The console output includes:

- Header with project information
- Step-by-step feature demonstrations with 19 examples (discovered and ordered automatically via `[AutoRegister]`)
- Generic service registration and resolution examples
- AutoRegister attribute demonstrations
- Timing information for performance insights
- Final summary with Y/N status for each feature
- Success/failure footer

## New Features Highlighted

This example now showcases:

- **Generic-constrained services** that work with specific types while sharing common interfaces
- **AutoRegister attribute** for automatic service discovery and registration
- **Multi-interface services** registered through a single AutoRegister attribute
- **Service resolution patterns** using `GetRequiredService<T>()` with base interfaces
- **Proper scope management** when using `ValidateScopes = true`

This example serves as both a demonstration and a comprehensive test of all Blazing.Extensions.DependencyInjection capabilities, including the latest generic service features.
