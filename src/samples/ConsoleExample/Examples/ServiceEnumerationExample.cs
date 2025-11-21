namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates service enumeration and filtering for plugin architectures and handler chains.
/// Shows filtering, mapping, and iterating over multiple service implementations.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class ServiceEnumerationExample : IExample
{
    private readonly ApplicationHost _host;

    /// <inheritdoc/>
    public string Name => "Service Enumeration & Filtering";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceEnumerationExample"/> class.
    /// </summary>
    public ServiceEnumerationExample()
    {
        _host = new ApplicationHost();
        ConfigureServices();
    }

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateGetAllServices();
        DemonstrateFilterServices();
        DemonstrateForEachService();
        DemonstrateMapServices();
    }

    /// <summary>
    /// Configures multiple implementations for enumeration.
    /// </summary>
    private void ConfigureServices()
    {
        _host.ConfigureServices(services =>
        {
            services.AddTransient<IPluginHandler, PluginHandlerA>();
            services.AddTransient<IPluginHandler, PluginHandlerB>();
            services.AddTransient<IPluginHandler, PluginHandlerC>();

            services.AddTransient<IDataValidator, EmailValidator>();
            services.AddTransient<IDataValidator, PhoneValidator>();
            services.AddTransient<IDataValidator, AddressValidator>();
        });
    }

    /// <summary>
    /// Demonstrates getting all registered implementations.
    /// </summary>
    private void DemonstrateGetAllServices()
    {
        Console.WriteLine("  Getting all plugin handlers...");

        var handlers = _host.GetRequiredServices<IPluginHandler>();
        var count = _host.GetServiceCount<IPluginHandler>();

        Console.WriteLine($"    + Found {count} plugin handlers");
        foreach (var handler in handlers)
        {
            Console.WriteLine($"      - {handler.GetType().Name}");
        }
    }

    /// <summary>
    /// Demonstrates filtering services with predicates.
    /// </summary>
    private void DemonstrateFilterServices()
    {
        Console.WriteLine("  Filtering enabled handlers...");

        var enabledHandlers = _host.GetServices<IPluginHandler>(h => h.IsEnabled);

        Console.WriteLine($"    + Enabled handlers: {enabledHandlers.Count()}");
        foreach (var handler in enabledHandlers)
        {
            Console.WriteLine($"      - {handler.GetType().Name}");
        }
    }

    /// <summary>
    /// Demonstrates executing actions on all services.
    /// </summary>
    private void DemonstrateForEachService()
    {
        Console.WriteLine("  Executing validation chain...");

        var validationResults = new List<string>();
        _host.ForEachService<IDataValidator>(validator =>
        {
            var isValid = validator.Validate("test-data");
            validationResults.Add($"{validator.GetType().Name}: {isValid}");
        });

        Console.WriteLine($"    + Executed {validationResults.Count} validators");
        foreach (var result in validationResults)
        {
            Console.WriteLine($"      - {result}");
        }
    }

    /// <summary>
    /// Demonstrates mapping services to results.
    /// </summary>
    private void DemonstrateMapServices()
    {
        Console.WriteLine("  Mapping services to results...");

        var results = _host.MapServices<IPluginHandler, string>(h => h.GetStatus());

        Console.WriteLine($"    + Mapped {results.Count()} handlers to status strings");
        foreach (var result in results)
        {
            Console.WriteLine($"      - {result}");
        }
    }
}

/// <summary>Plugin handler interface.</summary>
public interface IPluginHandler
{
    /// <summary>Gets whether the handler is enabled.</summary>
    bool IsEnabled { get; }

    /// <summary>Gets the handler status.</summary>
    string GetStatus();
}

/// <summary>Plugin handler A.</summary>
public class PluginHandlerA : IPluginHandler
{
    /// <inheritdoc/>
    public bool IsEnabled => true;
    /// <inheritdoc/>
    public string GetStatus() => "PluginA: Ready";
}

/// <summary>Plugin handler B.</summary>
public class PluginHandlerB : IPluginHandler
{
    /// <inheritdoc/>
    public bool IsEnabled => false;
    /// <inheritdoc/>
    public string GetStatus() => "PluginB: Disabled";
}

/// <summary>Plugin handler C.</summary>
public class PluginHandlerC : IPluginHandler
{
    /// <inheritdoc/>
    public bool IsEnabled => true;
    /// <inheritdoc/>
    public string GetStatus() => "PluginC: Ready";
}

/// <summary>Data validator interface.</summary>
public interface IDataValidator
{
    /// <summary>Validates data.</summary>
    bool Validate(string data);
}

/// <summary>Email validator.</summary>
public class EmailValidator : IDataValidator
{
    /// <inheritdoc/>
    public bool Validate(string data) => data.Contains("@");
}

/// <summary>Phone validator.</summary>
public class PhoneValidator : IDataValidator
{
    /// <inheritdoc/>
    public bool Validate(string data) => data.Length >= 10;
}

/// <summary>Address validator.</summary>
public class AddressValidator : IDataValidator
{
    /// <inheritdoc/>
    public bool Validate(string data) => !string.IsNullOrWhiteSpace(data);
}
