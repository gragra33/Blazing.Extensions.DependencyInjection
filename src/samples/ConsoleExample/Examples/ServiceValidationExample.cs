namespace ConsoleExample.Examples;

/// <summary>
/// Demonstrates service validation and diagnostics for detecting circular dependencies,
/// lifetime violations, and duplicate registrations.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public class ServiceValidationExample : IExample
{
    /// <inheritdoc/>
    public string Name => "Service Validation & Diagnostics";

    /// <inheritdoc/>
    public void Run()
    {
        DemonstrateValidDiagnostics();
        DemonstrateLifetimeViolationDetection();
        DemonstrateDuplicateDetection();
    }

    /// <summary>
    /// Demonstrates diagnostics on a valid service collection.
    /// </summary>
    private void DemonstrateValidDiagnostics()
    {
        Console.WriteLine("  Analyzing valid service collection...");

        var services = new ServiceCollection();
        services.AddSingleton<IValidationTestService, ValidationTestService>();
        services.AddScoped<IScopedTestService, ScopedTestService>();
        services.AddTransient<ITransientTestService, TransientTestService>();

        var diagnostics = services.GetDiagnostics();

        Console.WriteLine($"    + Total services: {diagnostics.TotalServices}");
        Console.WriteLine($"    + Singletons: {diagnostics.SingletonCount}");
        Console.WriteLine($"    + Scoped: {diagnostics.ScopedCount}");
        Console.WriteLine($"    + Transient: {diagnostics.TransientCount}");
        Console.WriteLine($"    + Is valid: {diagnostics.IsValid}");
        Console.WriteLine($"    + Warnings: {diagnostics.Warnings.Count}");
    }

    /// <summary>
    /// Demonstrates detection of lifetime compatibility violations.
    /// </summary>
    private void DemonstrateLifetimeViolationDetection()
    {
        Console.WriteLine("  Detecting lifetime violations...");

        var services = new ServiceCollection();
        services.AddSingleton<IViolatingService, ViolatingService>();
        services.AddScoped<IScopedDependency, ScopedDependency>();

        var violations = services.ValidateLifetimeCompatibility();

        IEnumerable<ServiceLifetimeValidationError> serviceLifetimeValidationErrors = violations.ToList();
        if (serviceLifetimeValidationErrors.Any())
        {
            Console.WriteLine($"    + Found {serviceLifetimeValidationErrors.Count()} lifetime violations");
            foreach (var violation in serviceLifetimeValidationErrors.Take(2))
            {
                Console.WriteLine($"      -> {violation.ServiceType.Name}: {violation.ErrorMessage[..60]}...");
            }
        }
        else
        {
            Console.WriteLine("    + No lifetime violations detected");
        }
    }

    /// <summary>
    /// Demonstrates detection of duplicate registrations.
    /// </summary>
    private void DemonstrateDuplicateDetection()
    {
        Console.WriteLine("  Detecting duplicate registrations...");

        var services = new ServiceCollection();
        services.AddTransient<IDuplicateService, DuplicateServiceA>();
        services.AddTransient<IDuplicateService, DuplicateServiceB>();
        services.AddTransient<IDuplicateService, DuplicateServiceC>();

        var duplicates = services.ValidateDuplicateRegistrations();

        IEnumerable<ServiceDuplicateInfo> serviceDuplicateInfos = duplicates.ToList();
        Console.WriteLine($"    + Found {serviceDuplicateInfos.Count()} duplicate service types");
        foreach (var dup in serviceDuplicateInfos)
        {
            Console.WriteLine($"      -> {dup.Registrations.First().ServiceType.Name}: {dup.Count} registrations");
        }
    }
}

/// <summary>Validation test service interface.</summary>
public interface IValidationTestService;

/// <summary>Validation test service.</summary>
public class ValidationTestService : IValidationTestService;

/// <summary>Scoped test service interface.</summary>
public interface IScopedTestService;

/// <summary>Scoped test service.</summary>
public class ScopedTestService : IScopedTestService;

/// <summary>Transient test service interface.</summary>
public interface ITransientTestService;

/// <summary>Transient test service.</summary>
public class TransientTestService : ITransientTestService;

/// <summary>Violating service interface.</summary>
public interface IViolatingService;

/// <summary>Service with lifetime violation (singleton depending on scoped).</summary>
public class ViolatingService : IViolatingService
{
    /// <summary>Violates lifetime rules by injecting scoped into singleton.</summary>
    public ViolatingService(IScopedDependency dependency) { }
}

/// <summary>Scoped dependency interface.</summary>
public interface IScopedDependency;

/// <summary>Scoped dependency.</summary>
public class ScopedDependency : IScopedDependency;

/// <summary>Duplicate service interface.</summary>
public interface IDuplicateService;

/// <summary>Duplicate service A.</summary>
public class DuplicateServiceA : IDuplicateService;

/// <summary>Duplicate service B.</summary>
public class DuplicateServiceB : IDuplicateService;

/// <summary>Duplicate service C.</summary>
public class DuplicateServiceC : IDuplicateService;
