namespace ConsoleExample.Examples;

/// <summary>
/// Example demonstrating generic-constrained services and processors.
/// Shows how to configure generic data providers and processors and how to resolve them
/// from an <see cref="ApplicationHost"/> instance.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class GenericConstrainedServicesExample : IExample
{
    /// <summary>
    /// Gets the human-readable name of this example.
    /// </summary>
    public string Name => "Generic-Constrained Services";
    
    /// <summary>
    /// Executes the example. Creates and configures a host, then exercises the
    /// registered generic providers, auto-registered services, and processors.
    /// </summary>
    public void Run()
    {
        var host = CreateGenericConstrainedHost();
        
        TestDataProviders(host);
        TestAutoRegisteredServices(host);
        TestGenericProcessors(host);
    }
    
    /// <summary>
    /// Creates and configures an <see cref="ApplicationHost"/> that registers
    /// generic data providers and processors and performs assembly scanning.
    /// </summary>
    /// <returns>An initialized <see cref="ApplicationHost"/>.</returns>
    private ApplicationHost CreateGenericConstrainedHost()
    {
        var host = new ApplicationHost();
        host.AddAssembly(typeof(Program).Assembly)
            .ConfigureServices(services =>
            {
                ConfigureDataProviders(services);
                services.Register();
                ConfigureGenericProcessors(services);
            });
        return host;
    }

    /// <summary>
    /// Registers concrete implementations of <see cref="IDataProvider{T}"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    private static void ConfigureDataProviders(IServiceCollection services)
    {
        services.AddSingleton<IDataProvider<string>, StringDataProvider>();
        services.AddSingleton<IDataProvider<int>, IntegerDataProvider>();
        services.AddSingleton<IDataProvider<decimal>, DecimalDataProvider>();
    }

    /// <summary>
    /// Registers concrete implementations of <see cref="IGenericProcessor{T}"/>.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    private static void ConfigureGenericProcessors(IServiceCollection services)
    {
        services.AddScoped<IGenericProcessor<string>, StringProcessor>();
        services.AddScoped<IGenericProcessor<int>, IntegerProcessor>();
        services.AddScoped<IGenericProcessor<decimal>, DecimalProcessor>();
    }

    /// <summary>
    /// Resolves and displays values from registered <see cref="IDataProvider{T}"/> implementations.
    /// </summary>
    /// <param name="host">The application host used to resolve services.</param>
    private static void TestDataProviders(ApplicationHost host)
    {
        Console.WriteLine("Data Providers (via base interface):");
        var stringProvider = host.GetRequiredService<IDataProvider<string>>();
        var intProvider = host.GetRequiredService<IDataProvider<int>>();
        var decimalProvider = host.GetRequiredService<IDataProvider<decimal>>();
        
        Console.WriteLine($"  - String Provider: {stringProvider.GetData()}");
        Console.WriteLine($"  - Integer Provider: {intProvider.GetData()}");
        Console.WriteLine($"  - Decimal Provider: {decimalProvider.GetData()}");
    }

    /// <summary>
    /// Resolves and exercises auto-registered services discovered by assembly scanning.
    /// </summary>
    /// <param name="host">The application host used to resolve services.</param>
    private static void TestAutoRegisteredServices(ApplicationHost host)
    {
        Console.WriteLine("Auto-Registered Services:");
        var dataService = host.GetRequiredService<IDataService>();
        var validationService = host.GetRequiredService<IValidationService>();
        var cacheService = host.GetRequiredService<ICacheService>();
        
        Console.WriteLine($"  - Data Service: {dataService.ProcessData("test")}" );
        Console.WriteLine($"  - Validation Service: {validationService.ValidateData("test")}" );
        Console.WriteLine($"  - Cache Service: {cacheService.GetFromCache("test")}" );
    }

    /// <summary>
    /// Resolves and exercises registered generic processors for different type parameters.
    /// </summary>
    /// <param name="host">The application host used to resolve services.</param>
    private static void TestGenericProcessors(ApplicationHost host)
    {
        Console.WriteLine("Generic Processors:");
        var stringProcessor = host.GetRequiredService<IGenericProcessor<string>>();
        var intProcessor = host.GetRequiredService<IGenericProcessor<int>>();
        var decimalProcessor = host.GetRequiredService<IGenericProcessor<decimal>>();
        
        Console.WriteLine($"  - String Processor: {stringProcessor.Process("Hello")}" );
        Console.WriteLine($"  - Integer Processor: {intProcessor.Process(42)}" );
        Console.WriteLine($"  - Decimal Processor: {decimalProcessor.Process(3.14m)}" );
    }
}