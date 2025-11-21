namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates open generic type registration for repository patterns, validators, and generic handlers.
/// Shows single registration resolving to multiple closed-type variants automatically.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class OpenGenericsExample : IExample
{
    private readonly ApplicationHost _host;

    /// <inheritdoc/>
    public string Name => "Open Generic Type Registration";

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenGenericsExample"/> class.
    /// </summary>
    public OpenGenericsExample()
    {
        _host = new ApplicationHost();
        ConfigureServices();
    }

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateGenericRepository();
        DemonstrateGenericValidator();
        DemonstrateMultipleGenerics();
    }

    /// <summary>
    /// Configures open generic services for demonstration.
    /// </summary>
    private void ConfigureServices()
    {
        _host.ConfigureServices(services =>
        {
            // Register open generic repository
            services.AddGenericScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Register open generic validator
            services.AddGenericTransient(typeof(IGenericValidator<>), typeof(GenericValidator<>));

            // Register multiple open generics at once
            services.AddGenericServices(ServiceLifetime.Transient,
                (typeof(ICommandHandler<>), typeof(CommandHandler<>)),
                (typeof(IQueryHandler<>), typeof(QueryHandler<>)));
        });
    }

    /// <summary>
    /// Demonstrates generic repository pattern with automatic type resolution.
    /// </summary>
    private void DemonstrateGenericRepository()
    {
        Console.WriteLine("  Resolving generic repositories...");

        var customerRepo = _host.GetRequiredService<IGenericRepository<Customer>>();
        var productRepo = _host.GetRequiredService<IGenericRepository<Product>>();

        Console.WriteLine($"    + Customer repository: {customerRepo.GetTypeName()}");
        Console.WriteLine($"    + Product repository: {productRepo.GetTypeName()}");
        Console.WriteLine("    + Single registration, multiple closed types");
    }

    /// <summary>
    /// Demonstrates generic validator pattern.
    /// </summary>
    private void DemonstrateGenericValidator()
    {
        Console.WriteLine("  Resolving generic validators...");

        var customerValidator = _host.GetRequiredService<IGenericValidator<Customer>>();
        var productValidator = _host.GetRequiredService<IGenericValidator<Product>>();

        var customerValid = customerValidator.Validate(new Customer { Name = "Acme Corp" });
        var productValid = productValidator.Validate(new Product { Name = "Widget" });

        Console.WriteLine($"    + Customer validation: {customerValid}");
        Console.WriteLine($"    + Product validation: {productValid}");
    }

    /// <summary>
    /// Demonstrates batch registration of multiple open generics.
    /// </summary>
    private void DemonstrateMultipleGenerics()
    {
        Console.WriteLine("  Resolving multiple generic patterns...");

        var commandHandler = _host.GetRequiredService<ICommandHandler<CreateCustomerCommand>>();
        var queryHandler = _host.GetRequiredService<IQueryHandler<GetCustomerQuery>>();

        Console.WriteLine($"    + Command handler: {commandHandler.GetType().Name}");
        Console.WriteLine($"    + Query handler: {queryHandler.GetType().Name}");
        Console.WriteLine("    + Multiple patterns registered with single call");
    }
}

/// <summary>
/// Generic repository interface for entity operations.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public interface IGenericRepository<T> where T : class
{
    /// <summary>
    /// Gets the type name of the entity.
    /// </summary>
    /// <returns>The entity type name.</returns>
    string GetTypeName();
}

/// <summary>
/// Generic repository implementation.
/// </summary>
/// <typeparam name="T">The entity type.</typeparam>
public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    /// <inheritdoc/>
    public string GetTypeName() => typeof(T).Name;
}

/// <summary>
/// Generic validator interface.
/// </summary>
/// <typeparam name="T">The type to validate.</typeparam>
public interface IGenericValidator<T> where T : class
{
    /// <summary>
    /// Validates an instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <returns><c>true</c> if valid; otherwise, <c>false</c>.</returns>
    bool Validate(T instance);
}

/// <summary>
/// Generic validator implementation.
/// </summary>
/// <typeparam name="T">The type to validate.</typeparam>
public class GenericValidator<T> : IGenericValidator<T> where T : class
{
    /// <inheritdoc/>
    public bool Validate(T? instance) => instance != null;
}

/// <summary>
/// Generic command handler interface.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public interface ICommandHandler<in TCommand> where TCommand : class
{
    /// <summary>
    /// Handles the command.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    void Handle(TCommand command);
}

/// <summary>
/// Generic command handler implementation.
/// </summary>
/// <typeparam name="TCommand">The command type.</typeparam>
public class CommandHandler<TCommand> : ICommandHandler<TCommand> where TCommand : class
{
    /// <inheritdoc/>
    public void Handle(TCommand command) { }
}

/// <summary>
/// Generic query handler interface.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
public interface IQueryHandler<in TQuery> where TQuery : class
{
    /// <summary>
    /// Handles the query.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    void Handle(TQuery query);
}

/// <summary>
/// Generic query handler implementation.
/// </summary>
/// <typeparam name="TQuery">The query type.</typeparam>
public class QueryHandler<TQuery> : IQueryHandler<TQuery> where TQuery : class
{
    /// <inheritdoc/>
    public void Handle(TQuery query) { }
}

/// <summary>
/// Sample customer entity.
/// </summary>
public class Customer
{
    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Sample product entity.
/// </summary>
public class Product
{
    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Sample create customer command.
/// </summary>
public class CreateCustomerCommand
{
    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Sample get customer query.
/// </summary>
public class GetCustomerQuery
{
    /// <summary>
    /// Gets or sets the customer ID.
    /// </summary>
    public int Id { get; set; }
}
