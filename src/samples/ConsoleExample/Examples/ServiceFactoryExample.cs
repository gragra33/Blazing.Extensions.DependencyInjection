namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates service factory delegates for conditional service creation based on runtime conditions.
/// Shows configuration-based, environment-based, and keyed factory patterns.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class ServiceFactoryExample : IExample
{
    private readonly ApplicationHost _host;

    /// <inheritdoc/>
    public string Name => "Service Factory Delegates";

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceFactoryExample"/> class.
    /// </summary>
    public ServiceFactoryExample()
    {
        _host = new ApplicationHost();
        ConfigureServices();
    }

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateConditionalFactory();
        DemonstrateKeyedFactory();
        DemonstrateLifetimeFactories();
    }

    /// <summary>
    /// Configures factory-based services.
    /// </summary>
    private void ConfigureServices()
    {
        _host.ConfigureServices(services =>
        {
            // Conditional factory based on environment
            services.RegisterConditionalFactory<ICacheFactory>(provider =>
            {
                var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
                return isDevelopment
                    ? new MemoryCacheFactory()
                    : (ICacheFactory)new RedisCacheFactory();
            });

            // Keyed factories
            services.RegisterKeyedFactory<IMessageSender>("email", (provider, key) => new EmailSender());
            services.RegisterKeyedFactory<IMessageSender>("sms", (provider, key) => new SmsSender());

            // Lifetime-specific factories
            services.RegisterTransientFactory<ITransientFactory>(provider => new TransientFactory());
            services.RegisterScopedFactory<IScopedFactory>(provider => new ScopedFactory());
        });
    }

    /// <summary>
    /// Demonstrates conditional service creation based on configuration.
    /// </summary>
    private void DemonstrateConditionalFactory()
    {
        Console.WriteLine("  Conditional factory based on environment...");

        var cacheFactory = _host.GetRequiredService<ICacheFactory>();
        Console.WriteLine($"    + Cache factory type: {cacheFactory.GetType().Name}");
        Console.WriteLine($"    + Factory selected based on environment");
    }

    /// <summary>
    /// Demonstrates keyed factory pattern for multiple implementations.
    /// </summary>
    private void DemonstrateKeyedFactory()
    {
        Console.WriteLine("  Keyed factories for message senders...");

        var emailSender = _host.GetRequiredKeyedService<IMessageSender>("email");
        var smsSender = _host.GetRequiredKeyedService<IMessageSender>("sms");

        emailSender.Send("Test message");
        smsSender.Send("Test message");

        Console.WriteLine($"    + Email sender: {emailSender.GetType().Name}");
        Console.WriteLine($"    + SMS sender: {smsSender.GetType().Name}");
    }

    /// <summary>
    /// Demonstrates factories with different service lifetimes.
    /// </summary>
    private void DemonstrateLifetimeFactories()
    {
        Console.WriteLine("  Factories with different lifetimes...");

        var transient1 = _host.GetRequiredService<ITransientFactory>();
        var transient2 = _host.GetRequiredService<ITransientFactory>();

        Console.WriteLine($"    + Transient factory creates new instances: {transient1.GetId() != transient2.GetId()}");
    }
}

/// <summary>Cache factory interface.</summary>
public interface ICacheFactory
{
    /// <summary>Creates a cache instance.</summary>
    object CreateCache();
}

/// <summary>Memory cache factory.</summary>
public class MemoryCacheFactory : ICacheFactory
{
    /// <inheritdoc/>
    public object CreateCache() => new { Type = "Memory" };
}

/// <summary>Redis cache factory.</summary>
public class RedisCacheFactory : ICacheFactory
{
    /// <inheritdoc/>
    public object CreateCache() => new { Type = "Redis" };
}

/// <summary>Message sender interface.</summary>
public interface IMessageSender
{
    /// <summary>Sends a message.</summary>
    void Send(string message);
}

/// <summary>Email sender implementation.</summary>
public class EmailSender : IMessageSender
{
    /// <inheritdoc/>
    public void Send(string message) { }
}

/// <summary>SMS sender implementation.</summary>
public class SmsSender : IMessageSender
{
    /// <inheritdoc/>
    public void Send(string message) { }
}

/// <summary>Transient factory interface.</summary>
public interface ITransientFactory
{
    /// <summary>Gets the factory ID.</summary>
    string GetId();
}

/// <summary>Transient factory implementation.</summary>
public class TransientFactory : ITransientFactory
{
    private readonly string _id = Guid.NewGuid().ToString("N")[..8];
    /// <inheritdoc/>
    public string GetId() => _id;
}

/// <summary>Scoped factory interface.</summary>
public interface IScopedFactory
{
    /// <summary>Gets the factory ID.</summary>
    string GetId();
}

/// <summary>Scoped factory implementation.</summary>
public class ScopedFactory : IScopedFactory
{
    private readonly string _id = Guid.NewGuid().ToString("N")[..8];
    /// <inheritdoc/>
    public string GetId() => _id;
}
