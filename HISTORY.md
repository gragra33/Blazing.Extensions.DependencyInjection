# Changes History

All notable changes to this project will be documented in this file.

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

### V1.0.0 (.Net 8.0+) - 5 October, 2025

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
