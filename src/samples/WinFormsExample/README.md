# WinForms Example - Blazing.Extensions.DependencyInjection

This example demonstrates dependency injection in a WinForms application with a tabbed interface, featuring the custom **Blazing.ToggleSwitch.WinForms** control library.

## Features Demonstrated

### Core Architecture Patterns

- **Dependency Injection**: Clean service registration and resolution using Blazing.Extensions.DependencyInjection
- **Separation of Concerns**: Each tab is a separate UserControl with its own responsibilities
- **Service Abstraction**: Interfaces for dialog, data, weather, and navigation services
- **Async Operations**: Demonstrates async/await patterns with service calls

### UI Components & Controls

- **Blazing.ToggleSwitch.WinForms**: Custom WinForms toggle switch control with smooth animations
- **Tabbed Interface**: Clean navigation between different functional areas
- **Responsive Layout**: Proper control sizing and positioning
- **Modern Styling**: Contemporary WinForms design patterns

### Service Integration

- **Dialog Services**: Application dialogs and notifications using MessageBox abstraction
- **Data Services**: Mock data operations demonstrating CRUD functionality
- **Weather Services**: Simulated API calls with async data retrieval
- **Navigation Services**: Tab-based navigation coordination

## Blazing.ToggleSwitch.WinForms Integration

This example showcases the **Blazing.ToggleSwitch.WinForms** control library, which provides a modern toggle switch control for WinForms applications. The toggle switch is featured prominently in the Settings view to demonstrate:

- Customizable appearance with different colors and sizes
- Smooth animations between states
- Event handling with CheckedChanged events
- Text customization for checked/unchecked states
- Integration with standard WinForms layout and data binding

### ToggleSwitch Features Demonstrated:

```csharp
var toggleSwitch = new ToggleSwitch
{
    CheckedText = "ON",
    UncheckedText = "OFF", 
    CheckedBackColor = Color.FromArgb(0, 120, 215),
    UncheckedBackColor = Color.Gray,
    SwitchWidth = 60,
    SwitchHeight = 30
};

toggleSwitch.CheckedChanged += (sender, e) => 
{
    // Handle state change
};
```

## Application Structure

### Views (UserControls)
- **HomeView**: Welcome screen with application information
- **WeatherView**: Demonstrates async service calls and data display
- **DataView**: Shows CRUD operations with list management
- **SettingsView**: Features multiple ToggleSwitch controls for configuration

### Services
- **IDialogService**: Abstraction for user dialogs and messages
- **IDataService**: Data operations with async methods
- **IWeatherService**: Weather data retrieval simulation
- **INavigationService**: Tab navigation coordination

### Key Components
- **MainForm**: Main application window with TabControl
- **Program**: Application entry point with DI container setup
- **ToggleSwitch**: Custom control for boolean settings

## Running the Application

1. Build the solution to restore dependencies
2. Run the WinFormsExample project
3. Navigate through the tabs to see different features:
   - **Home**: Overview and basic dialog example
   - **Weather**: Async data loading demonstration
   - **Data**: List management with CRUD operations
   - **Settings**: ToggleSwitch controls and form handling

## Dependencies

- .NET 8.0+ or .NET 9.0+ (Windows)
- Blazing.Extensions.DependencyInjection
- Blazing.ToggleSwitch.WinForms (custom control library)

## Architecture Benefits

1. **Loose Coupling**: Services are injected rather than directly instantiated
2. **Testability**: Service interfaces allow for easy unit testing
3. **Maintainability**: Clear separation between UI and business logic
4. **Extensibility**: New services and views can be added easily
5. **Modern UI**: Custom controls provide contemporary user experience

This example serves as a practical demonstration of how dependency injection can improve WinForms application architecture while maintaining the familiar WinForms development experience.