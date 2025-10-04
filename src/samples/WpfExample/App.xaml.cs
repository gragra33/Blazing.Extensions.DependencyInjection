using System;
using System.Diagnostics;
using System.Windows;
using Blazing.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using WpfExample.Services;
using WpfExample.ViewModels;
using WpfExample.Views;

namespace WpfExample;

/// <summary>
/// WPF application demonstrating Blazing.Extensions.DependencyInjection with tab-based MVVM pattern.
/// Each tab has its own view and view model, all resolved automatically via DI.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Console window is now enabled via OutputType=Exe in csproj
        Console.WriteLine("=== WPF Application Starting ===");
        
        base.OnStartup(e);

        try
        {
            // STEP 1: Configure services but don't build yet
            var services = this.GetServiceCollection(services =>
            {
                // Register ViewModels - each will be injected with required services automatically
                // NOTE: MainViewModel is simple and doesn't know about child ViewModels (decoupled design)
                services.AddTransient<MainViewModel>();
                services.AddTransient<HomeViewModel>();
                services.AddTransient<WeatherViewModel>();
                services.AddTransient<DataViewModel>();
                services.AddTransient<SettingsViewModel>();

                // Register Views - each View resolves its own ViewModel independently (View-First pattern)
                services.AddTransient<HomeView>();
                services.AddTransient<WeatherView>();
                services.AddTransient<DataView>();
                services.AddTransient<SettingsView>();

                // Auto-discover and register all ITabView implementations from current assembly
                services.Register<ITabView>(ServiceLifetime.Transient);

                // Register Services
                services.AddSingleton<IDataService, DataService>();
                services.AddSingleton<IDialogService, DialogService>();
                services.AddSingleton<INavigationService, NavigationService>();
                services.AddTransient<IWeatherService, WeatherService>();
                
                // Register TabViewHandler service for automatic tab discovery and loose coupling
                services.AddSingleton<ITabViewHandler, TabViewHandler>();
                
                // Register MainWindow
                services.AddTransient<MainWindow>();
            });

            // STEP 2: BuildServiceProvider and assign services - this is the critical step!
            // This ensures the service provider is fully built before resolving any services
            var serviceProvider = this.BuildServiceProvider(services);

            // STEP 3: Test that service provider is working
            Debug.WriteLine("Service provider built successfully");
            
            // STEP 4: Resolve MainWindow directly from the service provider to avoid extension method issues
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            
            Debug.WriteLine("MainWindow created and shown successfully");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}

