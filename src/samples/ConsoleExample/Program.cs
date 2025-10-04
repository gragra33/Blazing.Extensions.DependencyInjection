using Microsoft.Extensions.DependencyInjection;
using Blazing.Extensions.DependencyInjection;
using System.Diagnostics;

namespace ConsoleExample;

class Program
{
    private static readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    
    static void Main(string[] args)
    {
        WriteHeader();
        
        // Create main application host
        var host = new ApplicationHost();
        
        Console.WriteLine();
        Console.WriteLine("===============");
        Console.WriteLine("Blazing.Extensions.DependencyInjection Examples");
        Console.WriteLine("===============");
        Console.WriteLine();

        var results = new Dictionary<string, bool>();

        try
        {
            // Example 1: Basic Service Configuration
            Console.WriteLine("Setting up main service provider...");
            LogTiming("Main ServiceProvider setup starting");
            
            host.ConfigureServices(services =>
            {
                services.AddSingleton<IRepository, DatabaseRepository>();
                services.AddScoped<IUserService, UserService>();
                services.AddTransient<INotificationService, EmailNotificationService>();
                services.AddTransient<ILoggingService, ConsoleLoggingService>();
                
                // Register services using auto-discovery
                services.Register();
            });
            
            LogTiming("Main ServiceProvider setup completed");
            results["Basic Service Configuration"] = true;
            
            // Test basic service resolution
            Console.WriteLine("Testing basic service resolution...");
            var userService = host.GetRequiredService<IUserService>();
            var user = userService.GetUser(1);
            Console.WriteLine($"Retrieved user: {user.Name} ({user.Email})");
            results["Service Resolution"] = true;
            
            // Example 2: Keyed Services
            Console.WriteLine();
            Console.WriteLine("Testing keyed services...");
            
            var host2 = new ApplicationHost();
            host2.ConfigureServices(services =>
            {
                services.AddKeyedSingleton<INotificationService, EmailNotificationService>("email");
                services.AddKeyedSingleton<INotificationService, SmsNotificationService>("sms");
                services.AddKeyedSingleton<INotificationService, PushNotificationService>("push");
            });
            
            var emailService = host2.GetRequiredKeyedService<INotificationService>("email");
            var smsService = host2.GetRequiredKeyedService<INotificationService>("sms");
            var pushService = host2.GetRequiredKeyedService<INotificationService>("push");
            
            emailService.SendNotification("Test email notification");
            smsService.SendNotification("Test SMS notification");
            pushService.SendNotification("Test push notification");
            results["Keyed Services"] = true;
            
            // Example 3: Assembly Scanning
            Console.WriteLine();
            Console.WriteLine("Testing assembly scanning...");
            
            var host3 = new ApplicationHost();
            host3.AddAssembly(typeof(Program).Assembly)
                 .ConfigureServices(services =>
            {
                services.Register<IScannedService>(ServiceLifetime.Transient);
                // Register all services with AutoRegister attribute
                services.Register();
            });
            
            var scannedServices = host3.GetServices()!.GetServices<IScannedService>();
            Console.WriteLine($"Found {scannedServices.Count()} scanned services:");
            foreach (var service in scannedServices)
            {
                Console.WriteLine($"  - {service.GetType().Name}: {service.GetMessage()}");
            }
            results["Assembly Scanning"] = true;
            
            // Example 4: AutoRegister Attribute
            Console.WriteLine();
            Console.WriteLine("Testing AutoRegister attribute...");
            
            var autoRegisteredService = host3.GetRequiredService<AutoRegisteredService>();
            autoRegisteredService.DoWork();
            results["AutoRegister Attribute"] = true;
            
            // Example 4.1: Generic-Constrained Services with Multiple Interfaces
            Console.WriteLine();
            Console.WriteLine("Testing generic-constrained services with multiple interfaces...");
            
            var host4 = new ApplicationHost();
            host4.AddAssembly(typeof(Program).Assembly)
                 .ConfigureServices(services =>
            {
                // Register base interfaces and implementations
                services.AddSingleton<IDataProvider<string>, StringDataProvider>();
                services.AddSingleton<IDataProvider<int>, IntegerDataProvider>();
                services.AddSingleton<IDataProvider<decimal>, DecimalDataProvider>();
                
                // Auto-register services with AutoRegister attribute
                services.Register();
                
                // Register generic processors that work with specific data types
                services.AddScoped<IGenericProcessor<string>, StringProcessor>();
                services.AddScoped<IGenericProcessor<int>, IntegerProcessor>();
                services.AddScoped<IGenericProcessor<decimal>, DecimalProcessor>();
            });
            
            // Test retrieving all data providers using the base interface
            Console.WriteLine("Data Providers (via base interface):");
            var stringProvider = host4.GetRequiredService<IDataProvider<string>>();
            var intProvider = host4.GetRequiredService<IDataProvider<int>>();
            var decimalProvider = host4.GetRequiredService<IDataProvider<decimal>>();
            
            Console.WriteLine($"  - String Provider: {stringProvider.GetData()}");
            Console.WriteLine($"  - Integer Provider: {intProvider.GetData()}");
            Console.WriteLine($"  - Decimal Provider: {decimalProvider.GetData()}");
            
            // Test auto-registered services with multiple interfaces
            Console.WriteLine("Auto-Registered Services:");
            var dataService = host4.GetRequiredService<IDataService>();
            var validationService = host4.GetRequiredService<IValidationService>();
            var cacheService = host4.GetRequiredService<ICacheService>();
            
            Console.WriteLine($"  - Data Service: {dataService.ProcessData("test")}");
            Console.WriteLine($"  - Validation Service: {validationService.ValidateData("test")}");
            Console.WriteLine($"  - Cache Service: {cacheService.GetFromCache("test")}");
            
            // Test generic processors with constraints
            Console.WriteLine("Generic Processors:");
            var stringProcessor = host4.GetRequiredService<IGenericProcessor<string>>();
            var intProcessor = host4.GetRequiredService<IGenericProcessor<int>>();
            var decimalProcessor = host4.GetRequiredService<IGenericProcessor<decimal>>();
            
            Console.WriteLine($"  - String Processor: {stringProcessor.Process("Hello")}");
            Console.WriteLine($"  - Integer Processor: {intProcessor.Process(42)}");
            Console.WriteLine($"  - Decimal Processor: {decimalProcessor.Process(3.14m)}");
            
            results["Generic-Constrained Services"] = true;
            
            // Example 5: Multiple Service Collections
            Console.WriteLine();
            Console.WriteLine("Testing multiple service collections...");
            
            var primaryHost = new ApplicationHost();
            var secondaryHost = new ApplicationHost();
            
            // Configure primary host with database services
            LogTiming("Primary ServiceProvider setup starting");
            primaryHost.ConfigureServices(services =>
            {
                services.AddSingleton<IRepository, DatabaseRepository>();
                services.AddScoped<IUserService, UserService>();
                services.AddTransient<ILoggingService, ConsoleLoggingService>();
            });
            LogTiming("Primary ServiceProvider setup completed");
            
            // Configure secondary host with cache services
            LogTiming("Secondary ServiceProvider setup starting");
            secondaryHost.ConfigureServices(services =>
            {
                services.AddSingleton<IRepository, CacheRepository>();
                services.AddScoped<IUserService, CachedUserService>();
                services.AddTransient<ILoggingService, FileLoggingService>();
            });
            LogTiming("Secondary ServiceProvider setup completed");
            
            // Test both service providers work independently
            var primaryUserService = primaryHost.GetRequiredService<IUserService>();
            var secondaryUserService = secondaryHost.GetRequiredService<IUserService>();
            
            Console.WriteLine("Primary Host Services:");
            var primaryUser = primaryUserService.GetUser(1);
            Console.WriteLine($"  User from primary: {primaryUser.Name} - {primaryUser.Email}");
            
            Console.WriteLine("Secondary Host Services:");
            var secondaryUser = secondaryUserService.GetUser(1);
            Console.WriteLine($"  User from secondary: {secondaryUser.Name} - {secondaryUser.Email}");
            
            results["Multiple Service Providers"] = true;
            
            // Example 6: Advanced Configuration with Post-Build Action
            Console.WriteLine();
            Console.WriteLine("Testing advanced configuration with post-build validation...");
            
            var advancedHost = new ApplicationHost();
            advancedHost.ConfigureServices(
                services =>
                {
                    services.AddSingleton<IRepository, DatabaseRepository>();
                    services.AddScoped<IUserService, UserService>();
                    services.AddTransient<ILoggingService, ConsoleLoggingService>();
                },
                provider =>
                {
                    // Post-build validation
                    LogTiming("Post-build validation starting");
                    var repo = provider.GetRequiredService<IRepository>();
                    var userSvc = provider.GetRequiredService<IUserService>();
                    Console.WriteLine("Post-build validation: All required services are available");
                    LogTiming("Post-build validation completed");
                });
            
            results["Advanced Configuration"] = true;
            
            // Example 7: Service Collection Customization
            Console.WriteLine();
            Console.WriteLine("Testing service collection customization...");
            
            var customHost = new ApplicationHost();
            var serviceCollection = customHost.GetServiceCollection(services =>
            {
                services.AddSingleton<IRepository, DatabaseRepository>();
                services.AddScoped<IUserService, UserService>();
                services.AddTransient<ILoggingService, ConsoleLoggingService>();
            });
            
            // Add additional services to the collection
            serviceCollection.AddTransient<INotificationService, EmailNotificationService>();
            
            // Build with custom options
            var customProvider = customHost.BuildServiceProvider(serviceCollection, new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
            
            // Create a scope to resolve scoped services
            using var scope = customProvider.CreateScope();
            var customUserService = scope.ServiceProvider.GetRequiredService<IUserService>();
            var customUser = customUserService.GetUser(1);
            Console.WriteLine($"Custom configured user: {customUser.Name}");
            
            results["Service Collection Customization"] = true;
            
            // Example 8: Service Provider Replacement
            Console.WriteLine();
            Console.WriteLine("Testing service provider replacement...");
            
            var replaceableHost = new ApplicationHost();
            
            // Initial configuration
            replaceableHost.ConfigureServices(services =>
            {
                services.AddSingleton<IRepository, DatabaseRepository>();
            });
            
            var initialRepo = replaceableHost.GetRequiredService<IRepository>();
            Console.WriteLine($"Initial repository: {initialRepo.GetType().Name}");
            
            // Replace with different configuration
            replaceableHost.ConfigureServices(services =>
            {
                services.AddSingleton<IRepository, CacheRepository>();
            });
            
            var replacedRepo = replaceableHost.GetRequiredService<IRepository>();
            Console.WriteLine($"Replaced repository: {replacedRepo.GetType().Name}");
            
            results["Service Provider Replacement"] = true;
            
            // Example 9: Memory Management
            Console.WriteLine();
            Console.WriteLine("Testing memory management...");
            
            var memoryHost = new ApplicationHost();
            memoryHost.ConfigureServices(services =>
            {
                services.AddTransient<DisposableService>();
            });
            
            var disposableService = memoryHost.GetRequiredService<DisposableService>();
            Console.WriteLine($"Disposable service created: {!disposableService.IsDisposed}");
            
            // Clear services to test cleanup
            var cleared = memoryHost.ClearServices();
            Console.WriteLine($"Services cleared: {cleared}");
            Console.WriteLine($"Services now null: {memoryHost.GetServices() == null}");
            
            results["Memory Management"] = true;
            
            // All tests passed
            results["All Examples"] = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            results["All Examples"] = false;
        }

        // Print results summary
        Console.WriteLine();
        Console.WriteLine("---------------");
        PrintResults(results);
        
        WriteFooter(results["All Examples"]);
    }

    private static void WriteHeader()
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("Blazing.Extensions.DependencyInjection Examples");
        Console.WriteLine("==============================================");
        Console.WriteLine();
        Console.WriteLine("This project demonstrates all features of");
        Console.WriteLine("Blazing.Extensions.DependencyInjection including");
        Console.WriteLine("multi-ServiceProvider support for advanced scenarios.");
        Console.WriteLine();
    }

    private static void WriteFooter(bool success)
    {
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("==============================================");
        if (success)
        {
            Console.WriteLine("Examples completed successfully!");
        }
        else
        {
            Console.WriteLine("Examples completed with errors!");
        }
        Console.WriteLine("==============================================");
    }

    private static void LogTiming(string message)
    {
        Console.WriteLine($"[TIMING] {DateTime.Now:HH:mm:ss.fff} - {message}");
    }

    private static void PrintResults(Dictionary<string, bool> results)
    {
        foreach (var result in results)
        {
            if (result.Key == "All Examples") continue;
            
            var status = result.Value ? "Y" : "N";
            var dots = new string('.', Math.Max(1, 63 - result.Key.Length));
            Console.WriteLine($"{result.Key}{dots}{status}");
        }
    }
}

// Application host class
public class ApplicationHost
{
    public string Name { get; set; } = "Console Application";
}

// Core interfaces and services
public interface IRepository
{
    User GetUser(int id);
    void SaveUser(User user);
}

public interface IUserService
{
    User GetUser(int id);
    void CreateUser(string name, string email);
}

public interface INotificationService
{
    void SendNotification(string message);
}

public interface ILoggingService
{
    void Log(string message);
}

public interface IScannedService
{
    string GetMessage();
}

// Domain models
public record User(int Id, string Name, string Email);

// Repository implementations
public class DatabaseRepository : IRepository
{
    public User GetUser(int id) => new(id, "John Doe (DB)", "john.db@example.com");
    public void SaveUser(User user) => Console.WriteLine($"Saving user {user.Name} to database");
}

public class CacheRepository : IRepository
{
    public User GetUser(int id) => new(id, "Jane Smith (Cache)", "jane.cache@example.com");
    public void SaveUser(User user) => Console.WriteLine($"Saving user {user.Name} to cache");
}

// Service implementations
public class UserService(IRepository repository, ILoggingService logger) : IUserService
{
    public User GetUser(int id)
    {
        logger.Log($"Getting user {id} from {repository.GetType().Name}");
        return repository.GetUser(id);
    }

    public void CreateUser(string name, string email)
    {
        logger.Log($"Creating user {name}");
        var user = new User(0, name, email);
        repository.SaveUser(user);
    }
}

public class CachedUserService(IRepository repository, ILoggingService logger) : IUserService
{
    public User GetUser(int id)
    {
        logger.Log($"Getting cached user {id} from {repository.GetType().Name}");
        return repository.GetUser(id);
    }

    public void CreateUser(string name, string email)
    {
        logger.Log($"Creating cached user {name}");
        var user = new User(0, name, email);
        repository.SaveUser(user);
    }
}

// Notification services
public class EmailNotificationService : INotificationService
{
    public void SendNotification(string message) => 
        Console.WriteLine($"[EMAIL] Sending: {message}");
}

public class SmsNotificationService : INotificationService
{
    public void SendNotification(string message) => 
        Console.WriteLine($"[SMS] Sending: {message}");
}

public class PushNotificationService : INotificationService
{
    public void SendNotification(string message) => 
        Console.WriteLine($"[PUSH] Sending: {message}");
}

// Logging services
public class ConsoleLoggingService : ILoggingService
{
    public void Log(string message) => Console.WriteLine($"[CONSOLE LOG] {message}");
}

public class FileLoggingService : ILoggingService
{
    public void Log(string message) => Console.WriteLine($"[FILE LOG] {message}");
}

// Services for assembly scanning demonstration
public class FirstScannedService : IScannedService
{
    public string GetMessage() => "First scanned service";
}

public class SecondScannedService : IScannedService
{
    public string GetMessage() => "Second scanned service";
}

public class ThirdScannedService : IScannedService
{
    public string GetMessage() => "Third scanned service";
}

// AutoRegister demonstration
[AutoRegister(ServiceLifetime.Singleton)]
public class AutoRegisteredService(ILoggingService? logger = null)
{
    public void DoWork()
    {
        var message = "AutoRegistered service is working!";
        logger?.Log(message);
        Console.WriteLine(message);
    }
}

// Disposable service for memory management testing
public class DisposableService : IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        IsDisposed = true;
        Console.WriteLine("DisposableService disposed");
    }
}

// Generic-constrained interfaces and implementations for Example 4.1
public interface IDataProvider<T>
{
    T GetData();
    string GetProviderType();
}

public interface IGenericProcessor<T> where T : notnull
{
    string Process(T data);
}

// Base interfaces for AutoRegister demonstration
public interface IDataService
{
    string ProcessData(string data);
}

public interface IValidationService
{
    bool ValidateData(string data);
}

public interface ICacheService
{
    string? GetFromCache(string key);
    void SetCache(string key, string value);
}

// Data provider implementations
public class StringDataProvider : IDataProvider<string>
{
    public string GetData() => "Sample string data";
    public string GetProviderType() => "String Provider";
}

public class IntegerDataProvider : IDataProvider<int>
{
    public int GetData() => 42;
    public string GetProviderType() => "Integer Provider";
}

public class DecimalDataProvider : IDataProvider<decimal>
{
    public decimal GetData() => 3.14159m;
    public string GetProviderType() => "Decimal Provider";
}

// Generic processor implementations with constraints
public class StringProcessor : IGenericProcessor<string>
{
    public string Process(string data) => $"Processed string: '{data}' (length: {data.Length})";
}

public class IntegerProcessor : IGenericProcessor<int>
{
    public string Process(int data) => $"Processed integer: {data} (squared: {data * data})";
}

public class DecimalProcessor : IGenericProcessor<decimal>
{
    public string Process(decimal data) => $"Processed decimal: {data:F4} (rounded: {Math.Round(data, 2)})";
}

// Multi-interface service using AutoRegister attribute
[AutoRegister(ServiceLifetime.Singleton)]
public class MultiInterfaceService : IDataService, IValidationService, ICacheService
{
    private readonly Dictionary<string, string> _cache = new();
    
    public string ProcessData(string data)
    {
        return $"Data processed: {data?.ToUpperInvariant() ?? "NULL"}";
    }
    
    public bool ValidateData(string data)
    {
        var isValid = !string.IsNullOrWhiteSpace(data) && data.Length >= 3;
        Console.WriteLine($"Validation result for '{data}': {isValid}");
        return isValid;
    }
    
    public string? GetFromCache(string key)
    {
        _cache.TryGetValue(key, out var value);
        var result = value ?? $"Cache miss for key: {key}";
        Console.WriteLine($"Cache lookup for '{key}': {result}");
        return result;
    }
    
    public void SetCache(string key, string value)
    {
        _cache[key] = value;
        Console.WriteLine($"Cached '{key}' = '{value}'");
    }
}