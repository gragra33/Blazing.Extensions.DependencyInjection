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
        Console.WriteLine(@"=== WPF Application Starting ===");
        
        base.OnStartup(e);

        try
        {
            // STEP 1: Configure services but don't build yet
            var services = this.GetServiceCollection(services =>
            {
                // Register cache backends — must be done BEFORE Register() so the generated
                // TryAddSingleton<IDecoratorCache, DefaultDecoratorCache> does not override SwitchableDecoratorCache.
                services.AddMemoryCache();
                services.AddHybridCache();
                services.AddSingleton<SwitchableDecoratorCache>();
                services.AddSingleton<IDecoratorCache>(sp => sp.GetRequiredService<SwitchableDecoratorCache>());

                // Auto-discover and register all classes decorated with AutoRegisterAttribute
                // This includes all ITabView implementations - each is tagged with [AutoRegister(Transient, typeof(ITabView))]
                services.Register();
            });

            // STEP 2: BuildServiceProvider and assign services - this is the critical step!
            // This ensures the service provider is fully built before resolving any services
            var serviceProvider = this.BuildServiceProvider(services);

            // STEP 3: Test that service provider is working
            Console.WriteLine(@"Service provider built successfully");
            
            // STEP 4: Resolve MainWindow directly from the service provider to avoid extension method issues
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            
            Console.WriteLine(@"MainWindow created and shown successfully");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Startup Error: {ex.Message}\n\nStack Trace: {ex.StackTrace}", "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }
}

