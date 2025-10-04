# WPF Example - TabViewHandler Pattern with Blazing.Extensions.DependencyInjection

This example demonstrates the **TabViewHandler pattern** with **complete decoupling** and **automatic tab discovery**, featuring the **Blazing.ToggleSwitch.Wpf** control library.

## Features Demonstrated

### Core Architecture Patterns

-   **TabViewHandler Pattern**: Complete decoupling of MainViewModel from specific View types
-   **Automatic Tab Discovery**: Service-based tab management with zero configuration
-   **Dependency Injection**: Clean service registration and resolution using Blazing.Extensions.DependencyInjection
-   **MVVM Architecture**: Proper separation of concerns with automatic View/ViewModel pairing

### UI Components & Controls

-   **Blazing.ToggleSwitch.Wpf**: Custom WPF toggle switch control with rich styling options
-   **Responsive Layout**: Adaptive UI design that works across different window sizes
-   **Modern Styling**: Contemporary WPF styling and visual design patterns
-   **Data Visualization**: Charts and data display components (in Data tab)

### Service Integration

-   **HTTP Services**: Weather API integration demonstrating async service patterns
-   **Data Services**: Mock data services for demonstration purposes
-   **Dialog Services**: Application dialog and notification services
-   **Settings Management**: Persistent application settings with data binding

## Blazing.ToggleSwitch.Wpf Integration

This example showcases the **Blazing.ToggleSwitch.Wpf** control library, which provides a modern toggle switch control for WPF applications. The toggle switch is featured prominently in the Settings view to demonstrate:

-   Customizable appearance and styling options
-   Data binding to ViewModel properties with full two-way binding
-   Smooth animations and state transitions
-   Integration with WPF's data binding and command systems
-   Consistent alignment with other form controls using SharedSizeGroup

### ToggleSwitch Features Demonstrated:

```xml
<toggleSwitch:ToggleSwitch
    IsChecked="{Binding EnableNotifications}"
    CheckedText="On"
    UncheckedText="Off"
    HeaderContentPlacement="Left"
    Header="Enable Notifications:"
    SwitchWidth="50"
    SharedSizeGroupName="{StaticResource SettingsGroup}"
    Margin="10,5"/>
```

**For complete ToggleSwitch documentation and API reference, see: [Blazing.ToggleSwitch.Wpf README](../../libs/Blazing.ToggleSwitch.Wpf/README.md)**

## Key Architecture Principle

**MainViewModel has ZERO knowledge of specific View types!**

Tab discovery is handled automatically by `ITabViewHandler` service, creating ultimate loose coupling through service-based discovery.

## Architecture Overview

```
MainWindow (injected with MainViewModel)
├─ MainViewModel (depends only on ITabViewHandler)
├─ TabViewHandler (discovers all ITabView implementations)
├─ TabViewModel wrappers (automatic View/ViewModel pairing)
└─ Individual Views (HomeView, WeatherView, DataView, SettingsView)
    └─ Automatic ViewModel resolution via naming convention
```

## The TabViewHandler Pattern

### Core Components

1. **ITabView Interface** - Views implement this for automatic discovery
2. **TabMetadata** - Static metadata to avoid startup dependencies
3. **TabViewHandler** - Service that discovers all tab views automatically
4. **TabViewModel** - Wrapper that handles View/ViewModel pairing
5. **MainViewModel** - Completely decoupled, depends only on services

## The Problem Without TabViewHandler Pattern

### Traditional Approach (Tightly Coupled)

Without the TabViewHandler pattern, you would have to manually manage each tab:

**1. MainViewModel must know about ALL tab Views:**

```csharp
public class MainViewModel
{
    public HomeView HomeView { get; }
    public WeatherView WeatherView { get; }
    public DataView DataView { get; }
    public SettingsView SettingsView { get; }

    public MainViewModel(
        HomeView homeView,
        WeatherView weatherView,
        DataView dataView,
        SettingsView settingsView)
    {
        HomeView = homeView;
        WeatherView = weatherView;
        DataView = dataView;
        SettingsView = settingsView;
    }
}
```

**2. Manual tab registration in XAML:**

```xaml
<TabControl>
    <TabItem Header="🏠 Home">
        <ContentPresenter Content="{Binding HomeView}" />
    </TabItem>
    <TabItem Header="🌤️ Weather">
        <ContentPresenter Content="{Binding WeatherView}" />
    </TabItem>
    <!-- Add each tab manually... -->
</TabControl>
```

### Problems with Traditional Approach:

-   Manual tab registration for each new tab
-   Tight coupling between MainViewModel and specific View types
-   MainViewModel violates Single Responsibility Principle
-   Hard to add new tabs without changing existing code
-   No automatic discovery mechanism

## The Solution: TabViewHandler Pattern with Blazing.Extensions.DependencyInjection

### Step 1: Views Implement ITabView Interface

```csharp
public partial class WeatherView : UserControl, ITabView
{
    public string TabHeader => "🌤️ Weather";
    public int Order => 2;

    public WeatherView()
    {
        InitializeComponent();
        // TabViewModel will automatically set the correct ViewModel as DataContext
    }
}
```

### Step 2: Static Metadata for Tab Discovery

```csharp
public static class TabMetadata
{
    private static readonly TabInfo[] StaticTabDefinitions =
    {
        new(typeof(HomeView), "🏠 Home", 1),
        new(typeof(WeatherView), "🌤️ Weather", 2),
        new(typeof(DataView), "📊 Data", 3),
        new(typeof(SettingsView), "⚙️ Settings", 4)
    };

    public static IEnumerable<TabInfo> GetAllTabs()
    {
        return StaticTabDefinitions.OrderBy(tab => tab.Order);
    }
}
```

### Step 3: TabViewHandler Service for Automatic Discovery

```csharp
public class TabViewHandler : ITabViewHandler
{
    public IEnumerable<TabViewModel> GetTabViewModels()
    {
        var tabViewModels = TabMetadata.GetAllTabs()
            .Select((tab, index) => new TabViewModel(tab.ViewType, tab.Header, tab.Order, isFirstTab: index == 0))
            .OrderBy(vm => vm.Order)
            .ToList();

        return tabViewModels;
    }
}
```

### Step 4: Completely Decoupled MainViewModel

```csharp
public partial class MainViewModel : ViewModelBase
{
    private readonly ITabViewHandler _tabViewHandler;
    private IEnumerable<TabViewModel> _tabViewModels;

    // MainViewModel has ZERO knowledge of specific View types!
    public MainViewModel(IDialogService dialogService, ITabViewHandler tabViewHandler)
    {
        _dialogService = dialogService;
        _tabViewHandler = tabViewHandler;
    }

    // Tabs discovered automatically - no View type knowledge needed!
    public IEnumerable<TabViewModel> TabViewModels
    {
        get
        {
            if (_tabViewModels == null)
            {
                _tabViewModels = _tabViewHandler.GetTabViewModels();
            }
            return _tabViewModels;
        }
    }
}
```

### Step 5: TabViewModel Wrapper for Automatic View/ViewModel Pairing

```csharp
public class TabViewModel
{
    public TabViewModel(Type viewType, string header, int order, bool isFirstTab = false)
    {
        ViewType = viewType;
        Header = header;
        Order = order;

        // Resolve view immediately and set correct ViewModel as DataContext
        ResolveView();
    }

    public void ResolveView()
    {
        var serviceProvider = Application.Current.GetServices();
        _viewInstance = serviceProvider.GetRequiredService(ViewType);

        // Automatic ViewModel pairing using naming convention
        if (_viewInstance is FrameworkElement view)
        {
            var viewModelTypeName = ViewType.Name.Replace("View", "ViewModel");
            var viewModelType = ViewType.Assembly.GetTypes()
                .FirstOrDefault(t => t.Name == viewModelTypeName);

            if (viewModelType != null)
            {
                var viewModel = serviceProvider.GetRequiredService(viewModelType);
                view.DataContext = viewModel;
            }
        }
    }
}
```

### Step 6: Simple XAML with Automatic Tab Binding

```xaml
<TabControl ItemsSource="{Binding TabViewModels}" SelectedIndex="0">
    <TabControl.ItemTemplate>
        <DataTemplate>
            <TextBlock Text="{Binding Header}" />
        </DataTemplate>
    </TabControl.ItemTemplate>
    <TabControl.ContentTemplate>
        <DataTemplate>
            <ContentPresenter Content="{Binding ViewInstance}" />
        </DataTemplate>
    </TabControl.ContentTemplate>
</TabControl>
```

### Step 7: Clean Service Registration with Blazing.Extensions.DependencyInjection

You can register tab views using either **manual registration** or **auto-discovery** methods:

#### Option A: Auto-Discovery Registration (Recommended)

```csharp
// App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    // STEP 1: Configure services using Blazing.Extensions.DependencyInjection
    var services = this.GetServiceCollection(services =>
    {
        // Register ViewModels - automatic dependency injection
        services.AddTransient<MainViewModel>();
        services.AddTransient<HomeViewModel>();
        services.AddTransient<WeatherViewModel>();
        services.AddTransient<DataViewModel>();
        services.AddTransient<SettingsViewModel>();

        // Register Views for DI resolution
        services.AddTransient<HomeView>();
        services.AddTransient<WeatherView>();
        services.AddTransient<DataView>();
        services.AddTransient<SettingsView>();

        // AUTO-DISCOVERY: Automatically find and register all ITabView implementations
        // Scans current assembly by default
        services.Register<ITabView>(ServiceScope.Transient);

        // Or scan specific assemblies:
        // services.Register<ITabView>(ServiceScope.Transient, typeof(MyView).Assembly, typeof(OtherAssembly).Assembly);

        // Register TabViewHandler for automatic tab discovery
        services.AddSingleton<ITabViewHandler, TabViewHandler>();

        // Register application services
        services.AddSingleton<IDataService, DataService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddTransient<IWeatherService, WeatherService>();

        services.AddTransient<MainWindow>();
    });

    // STEP 2: Build and assign services - critical for proper DI setup
    var serviceProvider = this.BuildServiceProvider(services);

    // STEP 3: One line resolves entire application!
    var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
    mainWindow.Show();
}
```

#### Option B: Manual Registration (Traditional)

```csharp
// App.xaml.cs - Manual registration approach
var services = this.GetServiceCollection(services =>
{
    // ... ViewModels and Views registration same as above ...

    // MANUAL: Register each ITabView implementation individually
    services.AddTransient<ITabView, HomeView>();
    services.AddTransient<ITabView, WeatherView>();
    services.AddTransient<ITabView, DataView>();
    services.AddTransient<ITabView, SettingsView>();

    // ... rest of service registration same as above ...
});
```

**Key Benefits of Auto-Discovery:**

-   ✅ **Zero Configuration**: No need to manually register new tab views
-   ✅ **Default Assembly Scan**: Automatically scans current assembly if no assemblies specified
-   ✅ **Multiple Assembly Support**: Can scan multiple assemblies for implementations
-   ✅ **Consistent Lifetime**: All discovered services get the same specified scope
-   ✅ **Automatic Updates**: New `ITabView` implementations are discovered automatically

## Benefits of TabViewHandler Pattern

| Aspect                     | Traditional                  | TabViewHandler Pattern            |
| -------------------------- | ---------------------------- | --------------------------------- |
| Coupling                   | Tight - knows specific Views | Zero - service-based discovery    |
| MainViewModel Dependencies | All tab Views                | Only ITabViewHandler service      |
| Adding New Tab             | Change MainViewModel + XAML  | Implement ITabView interface only |
| Tab Discovery              | Manual registration          | Automatic via metadata            |
| View/ViewModel Pairing     | Manual DataContext setting   | Automatic via naming convention   |
| Startup Performance        | All Views created upfront    | Views created when accessed       |
| Testing                    | Hard - many dependencies     | Easy - mock ITabViewHandler       |
| Code Maintenance           | High - scattered changes     | Low - localized changes           |

## Key Advantages

1. **Ultimate Loose Coupling** - MainViewModel has zero knowledge of specific View types
2. **Automatic Discovery** - New tabs added by implementing ITabView interface
3. **Service-Based Architecture** - All tab operations go through ITabViewHandler service
4. **Naming Convention Pairing** - Views automatically paired with ViewModels (HomeView → HomeViewModel)
5. **Static Metadata** - No service resolution during app startup
6. **Immediate Loading** - No lazy loading delays, tabs show content instantly
7. **Complete Separation of Concerns** - Each component has a single responsibility
8. **SOLID Principles** - Open/Closed principle for adding new tabs

## Adding a New Tab

With **auto-discovery**, adding a new tab is incredibly simple:

### Using Auto-Discovery (Recommended)

1. **Create the View** implementing ITabView:

```csharp
public partial class ProfileView : UserControl, ITabView
{
    public string TabHeader => "👤 Profile";
    public int Order => 5;
}
```

2. **Create the ViewModel**:

```csharp
public partial class ProfileViewModel : ViewModelBase
{
    // Automatic DI injection of required services
}
```

3. **Register in DI** (App.xaml.cs):

```csharp
services.AddTransient<ProfileView>();
services.AddTransient<ProfileViewModel>();
```

**That's it!** The `services.Register<ITabView>(ServiceScope.Transient)` call automatically discovers and registers your new `ProfileView` as an `ITabView`. No additional registration needed!

### Using Manual Registration (Alternative)

If you prefer manual registration, you would also need to add:

```csharp
services.AddTransient<ITabView, ProfileView>();
```

**Benefits of Auto-Discovery:**

-   ✅ No need to remember to register each new `ITabView` implementation
-   ✅ Consistent with the "convention over configuration" principle
-   ✅ Automatically maintains registration as you add/remove tabs
-   ✅ Works across multiple assemblies when specified

## Running the Example

This project targets both .NET 8.0 and .NET 9.0 to demonstrate cross-framework compatibility. Since it targets multiple frameworks, you need to specify which one to use when running:

### Command Line Options

#### Option 1: From Project Directory

```bash
cd src/samples/WpfExample

# Run with .NET 8.0
dotnet run --framework net8.0-windows

# Run with .NET 9.0
dotnet run --framework net9.0-windows
```

#### Option 2: From Solution Root

```bash
# Run with .NET 8.0
dotnet run --project src/samples/WpfExample --framework net8.0-windows

# Run with .NET 9.0
dotnet run --project src/samples/WpfExample --framework net9.0-windows
```

#### Option 3: Build and Run

```bash
# Build first
dotnet build src/samples/WpfExample

# Then run the executable directly
src/samples/WpfExample/bin/Debug/net8.0-windows/WpfExample.exe
```

### Visual Studio

If using Visual Studio, make sure to:

1. Set WpfExample as the startup project
2. Select your preferred target framework (net8.0-windows or net9.0-windows)
3. Press F5 or click "Start Debugging"

### Requirements

-   Windows 10 or later (required for WPF applications)
-   .NET 8.0 or .NET 9.0 runtime
-   The `-windows` suffix is required for WPF applications as they are Windows-specific

### What You'll See

Upon running, the application opens with:

-   **Home Tab**: Welcome page with application overview
-   **Weather Tab**: Live weather data integration (requires internet connection)
-   **Data Tab**: Charts and data visualization examples
-   **Settings Tab**: Toggle switches and configuration options

## Project Structure

```
WpfExample/
├── App.xaml.cs                 # Service registration with BuildServiceProvider
├── MainWindow.xaml             # Simple TabControl with ItemsSource binding
├── Views/
│   ├── ITabView.cs             # Interface for automatic tab discovery
│   ├── HomeView.xaml/cs        # Implements ITabView, automatic ViewModel pairing
│   ├── WeatherView.xaml/cs     # Implements ITabView, automatic ViewModel pairing
│   ├── DataView.xaml/cs        # Implements ITabView, automatic ViewModel pairing
│   └── SettingsView.xaml/cs    # Implements ITabView, features ToggleSwitch controls
├── ViewModels/
│   ├── MainViewModel.cs        # Depends only on ITabViewHandler service
│   ├── TabViewModel.cs         # Wrapper for View/ViewModel pairing
│   ├── HomeViewModel.cs        # Independent with service dependencies
│   ├── WeatherViewModel.cs     # Independent with service dependencies
│   ├── DataViewModel.cs        # Independent with service dependencies
│   └── SettingsViewModel.cs    # Independent with service dependencies
├── Services/
│   ├── TabMetadata.cs          # Static metadata for tab discovery
│   ├── TabViewHandler.cs       # Service for automatic tab management
│   ├── IWeatherService.cs      # Business services
│   ├── IDataService.cs         # Business services
│   └── IDialogService.cs       # Business services
├── Converters/                 # Custom value converters for data binding
└── WpfExample.csproj           # References Blazing.ToggleSwitch.Wpf library
```

## Dependencies

This example demonstrates integration of multiple components:

### Core Dependencies

-   **Blazing.Extensions.DependencyInjection**: Main DI library providing universal dependency injection
-   **Blazing.ToggleSwitch.Wpf**: Custom toggle switch control library with modern styling
-   **.NET 8.0/9.0**: Multi-target framework support for cross-version compatibility

### Referenced Libraries

-   **Microsoft.Extensions.DependencyInjection**: Foundation for the DI system
-   **System.Net.Http**: HTTP client for weather service integration
-   **WPF Framework**: Windows Presentation Foundation for UI components

### Project References

```xml
<ProjectReference Include="..\..\Blazing.Extensions.DependencyInjection\Blazing.Extensions.DependencyInjection.csproj" />
<ProjectReference Include="..\..\libs\Blazing.ToggleSwitch.Wpf\Blazing.ToggleSwitch.Wpf.csproj" />
```

## Blazing.Extensions.DependencyInjection Integration

This example showcases the power of **Blazing.Extensions.DependencyInjection** for WPF applications:

### Key Features Used:

1. **GetServiceCollection()** - Configure services before building
2. **BuildServiceProvider()** - Proper service provider setup
3. **GetRequiredService<T>()** - Clean service resolution
4. **Application.Current.GetServices()** - Access service provider from anywhere
5. **Automatic Dependency Injection** - No manual wiring required

### TabControl Integration Benefits:

-   **Eliminates Complex Converters** - TabViewModel provides direct binding properties
-   **Automatic View Resolution** - Views resolved through DI, not XAML instantiation
-   **Service-Based Discovery** - Tabs discovered through service interface, not hardcoded
-   **Naming Convention Pairing** - Automatic View-ViewModel association
-   **Zero Configuration XAML** - Simple ItemsSource binding handles everything

## Pattern Summary

**TabViewHandler Pattern = Service-Based Tab Discovery + Automatic View/ViewModel Pairing**

-   MainViewModel depends only on ITabViewHandler service (zero View knowledge)
-   Views implement ITabView interface for automatic discovery
-   TabMetadata provides static definitions (no startup dependencies)
-   TabViewModel handles automatic View/ViewModel pairing via naming conventions
-   Adding new tabs requires only implementing ITabView interface
-   Powered by Blazing.Extensions.DependencyInjection for clean DI integration
