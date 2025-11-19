namespace ConsoleExample;

/// <summary>
/// Executes all discovered <see cref="IExample"/> implementations registered in the application host.
/// Collects and prints results for each example.
/// </summary>
public class ExampleRunner
{
    private readonly Dictionary<string, bool> _results = new();
    private readonly ApplicationHost _host;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ExampleRunner"/> class and configures the application host.
    /// </summary>
    public ExampleRunner()
    {
        _host = new ApplicationHost();
        _host.AddAssembly(typeof(Program).Assembly)
             .ConfigureServices(services =>
        {
            // Register all services with AutoRegister attribute, including IExample implementations
            services.Register();
        });
    }
    
    /// <summary>
    /// Runs all registered <see cref="IExample"/> implementations and prints results to the console.
    /// </summary>
    public void RunAllExamples()
    {
        ConsoleHelper.WriteHeader();
        
        try
        {
            // Get all registered IExample services using GetServices<IExample>()
            var examples = _host.GetServices()!.GetServices<IExample>()
                               .OrderBy(example => example.Name) // Order alphabetically for consistent output
                               .ToList();
            
            Console.WriteLine($"Found {examples.Count} example services registered via AutoRegister attribute:");
            foreach (var example in examples)
            {
                Console.WriteLine($"  - {example.GetType().Name}: {example.Name}");
            }
            Console.WriteLine();
            
            // Run each example
            foreach (var example in examples)
            {
                RunExample(example);
            }
            
            _results["All Examples"] = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            _results["All Examples"] = false;
        }

        ConsoleHelper.PrintResults(_results);
        ConsoleHelper.WriteFooter(_results["All Examples"]);
    }

    /// <summary>
    /// Runs a single <see cref="IExample"/> and records its result.
    /// </summary>
    /// <param name="example">The example to run.</param>
    private void RunExample(IExample example)
    {
        Console.WriteLine();
        Console.WriteLine($"Testing {example.Name.ToLower()}...");
        example.Run();
        _results[example.Name] = true;
    }
}