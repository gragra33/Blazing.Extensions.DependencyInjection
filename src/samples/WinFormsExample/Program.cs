using System.Runtime.InteropServices;

namespace WinFormsExample;

/// <summary>
/// WinForms application demonstrating Blazing.Extensions.DependencyInjection with tab-based UI.
/// Each tab is a separate user control, all resolved automatically via DI.
/// </summary>
internal static class Program
{
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool AllocConsole();

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // Allocate a console window to show both GUI and console output
        AllocConsole();
        
        // Console window is now enabled
        Console.WriteLine("=== WinForms Application Starting ===");
        Console.WriteLine("Console window allocated successfully");

        ApplicationConfiguration.Initialize();
        Console.WriteLine("WinForms application configuration initialized");

        try
        {
            // STEP 1: Configure services
            Console.WriteLine("\n--- STEP 1: Configuring Services ---");
            var services = new ServiceCollection();

            // Auto-discover and register all classes with AutoRegister attribute from current assembly
            // This includes all ITabView implementations, MainForm, and all services decorated with [AutoRegister]
            Console.WriteLine("Auto-discovering and registering services with [AutoRegister] attribute...");
            services.Register();
            Console.WriteLine("Service auto-registration complete");

            // STEP 2: Build ServiceProvider
            Console.WriteLine("\n--- STEP 2: Building Service Provider ---");
            var serviceProvider = services.BuildServiceProvider();
            Console.WriteLine("Service provider built successfully");
            
            // STEP 3: Test that service provider is working
            Console.WriteLine("\n--- STEP 3: Testing Service Resolution ---");
            Console.WriteLine("Testing service provider functionality...");
            
            // Test some key service resolutions
            var dialogService = serviceProvider.GetRequiredService<IDialogService>();
            Console.WriteLine("? IDialogService resolved successfully");
            
            var dataService = serviceProvider.GetRequiredService<IDataService>();
            Console.WriteLine("? IDataService resolved successfully");
            
            var weatherService = serviceProvider.GetRequiredService<IWeatherService>();
            Console.WriteLine("? IWeatherService resolved successfully");
            
            var navigationService = serviceProvider.GetRequiredService<INavigationService>();
            Console.WriteLine("? INavigationService resolved successfully");
            
            var tabViewHandler = serviceProvider.GetRequiredService<ITabViewHandler>();
            Console.WriteLine("? ITabViewHandler resolved successfully");
            
            // STEP 4: Resolve MainForm from the service provider
            Console.WriteLine("\n--- STEP 4: Creating Main Application Form ---");
            var mainForm = serviceProvider.GetRequiredService<MainForm>();
            Console.WriteLine("MainForm resolved and created successfully");
            
            Console.WriteLine("\n--- STEP 5: Starting Application ---");
            Console.WriteLine("Running WinForms application...");
            Console.WriteLine("==========================================");
            Console.WriteLine();
            
            // Run the application
            Application.Run(mainForm);
            
            Console.WriteLine("\n==========================================");
            Console.WriteLine("=== WinForms Application Shutdown ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n!!! STARTUP ERROR !!!");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            
            MessageBox.Show($"Startup Error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", 
                "Application Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}