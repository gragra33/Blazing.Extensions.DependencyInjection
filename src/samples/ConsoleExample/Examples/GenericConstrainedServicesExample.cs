namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class GenericConstrainedServicesExample : IExample
{
    public string Name => "Generic-Constrained Services";
    
    public void Run()
    {
        var host = CreateGenericConstrainedHost();
        
        TestDataProviders(host);
        TestAutoRegisteredServices(host);
        TestGenericProcessors(host);
    }
    
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

    private static void ConfigureDataProviders(IServiceCollection services)
    {
        services.AddSingleton<IDataProvider<string>, StringDataProvider>();
        services.AddSingleton<IDataProvider<int>, IntegerDataProvider>();
        services.AddSingleton<IDataProvider<decimal>, DecimalDataProvider>();
    }

    private static void ConfigureGenericProcessors(IServiceCollection services)
    {
        services.AddScoped<IGenericProcessor<string>, StringProcessor>();
        services.AddScoped<IGenericProcessor<int>, IntegerProcessor>();
        services.AddScoped<IGenericProcessor<decimal>, DecimalProcessor>();
    }

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

    private static void TestAutoRegisteredServices(ApplicationHost host)
    {
        Console.WriteLine("Auto-Registered Services:");
        var dataService = host.GetRequiredService<IDataService>();
        var validationService = host.GetRequiredService<IValidationService>();
        var cacheService = host.GetRequiredService<ICacheService>();
        
        Console.WriteLine($"  - Data Service: {dataService.ProcessData("test")}");
        Console.WriteLine($"  - Validation Service: {validationService.ValidateData("test")}");
        Console.WriteLine($"  - Cache Service: {cacheService.GetFromCache("test")}");
    }

    private static void TestGenericProcessors(ApplicationHost host)
    {
        Console.WriteLine("Generic Processors:");
        var stringProcessor = host.GetRequiredService<IGenericProcessor<string>>();
        var intProcessor = host.GetRequiredService<IGenericProcessor<int>>();
        var decimalProcessor = host.GetRequiredService<IGenericProcessor<decimal>>();
        
        Console.WriteLine($"  - String Processor: {stringProcessor.Process("Hello")}");
        Console.WriteLine($"  - Integer Processor: {intProcessor.Process(42)}");
        Console.WriteLine($"  - Decimal Processor: {decimalProcessor.Process(3.14m)}");
    }
}