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
