# Console Example

This console application demonstrates all the features of **Blazing.Extensions.DependencyInjection**, including advanced scenarios with multiple ServiceProviders and generic-constrained services.

## Features Demonstrated

### 1. Basic Service Configuration

-   Simple service registration and configuration
-   Singleton, Scoped, and Transient lifetimes
-   Basic dependency injection

### 2. Keyed Services

-   Multiple implementations of the same interface
-   Service resolution by key
-   Different notification services (Email, SMS, Push)

### 3. Assembly Scanning

-   Automatic discovery of service implementations
-   Registration of all classes implementing a specific interface
-   Dynamic service discovery at runtime

### 4. AutoRegister Attribute

-   Declarative service registration using attributes
-   Automatic service lifetime configuration
-   Reduced boilerplate registration code

### 5. Generic-Constrained Services with Multiple Interfaces

-   **Generic data providers** with type constraints (`IDataProvider<T>`)
-   **Generic processors** with type constraints (`IGenericProcessor<T>`)
-   **Multiple implementations** for different types (String, Integer, Decimal)
-   **Base interface usage** for service retrieval using `GetServices<T>()`
-   **Multi-interface services** using AutoRegister attribute
-   Service that implements multiple interfaces (`IDataService`, `IValidationService`, `ICacheService`)

### 6. Multiple Service Providers (Advanced)

-   Independent service providers for different contexts
-   Isolation between different application areas
-   Primary and secondary service configurations

### 7. Advanced Configuration

-   Post-build validation and setup
-   Custom service provider configuration
-   Service warming and validation

### 8. Service Collection Customization

-   Manual service collection modification
-   Custom ServiceProviderOptions with scope validation
-   Fine-grained control over service building
-   Proper scope management for scoped services

### 9. Service Provider Replacement

-   Dynamic service provider replacement
-   Configuration changes at runtime
-   Service provider swapping

### 10. Memory Management

-   Proper cleanup and disposal
-   ConditionalWeakTable usage for leak prevention
-   Service provider clearing

## Running the Example

```bash
cd samples/ConsoleExample
dotnet run
```

The application targets .NET 9.0 and will demonstrate each feature with detailed output showing the internal workings and timing information.

## Expected Output

The console output includes:

-   Header with project information
-   Step-by-step feature demonstrations with 11 examples
-   Generic service registration and resolution examples
-   AutoRegister attribute demonstrations
-   Timing information for performance insights
-   Final summary with Y/N status for each feature
-   Success/failure footer

## New Features Highlighted

This example now showcases:

-   **Generic-constrained services** that work with specific types while sharing common interfaces
-   **AutoRegister attribute** for automatic service discovery and registration
-   **Multi-interface services** registered through a single AutoRegister attribute
-   **Service resolution patterns** using `GetRequiredService<T>()` with base interfaces
-   **Proper scope management** when using `ValidateScopes = true`

This example serves as both a demonstration and a comprehensive test of all Blazing.Extensions.DependencyInjection capabilities, including the latest generic service features.
