using Blazing.Extensions.DependencyInjection.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests.GeneratorTests;

/// <summary>
/// Tests for <see cref="ServiceDecoratorGenerator"/> that verify the emitted
/// concrete decorator classes for <c>[CachingDecorator]</c> and <c>[LoggingDecorator]</c>.
/// </summary>
public sealed class DecoratorGeneratorTests
{
    /// <summary>
    /// Verifies that a class with <c>[CachingDecorator]</c> causes a sealed caching decorator
    /// class to be emitted in the <c>Blazing.Extensions.DependencyInjection.Generated</c> namespace.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_EmitsSealedDecoratorClass()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IProductService))]
                [CachingDecorator(seconds: 120)]
                public class ProductService : IProductService
                {
                    public string GetName(int id) => "Product";
                }
                public interface IProductService
                {
                    string GetName(int id);
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("Blazing.Extensions.DependencyInjection.Generated");
        generated.ShouldContain("sealed class Blazing_CachingDecorator_ProductService");
        generated.ShouldContain("global::MyApp.IProductService");
        generated.ShouldContain("_cacheDurationSeconds = 120");
        generated.ShouldContain("IBlazingInvalidatable");
        generated.ShouldContain("IDecoratorCache");
        generated.ShouldContain("_cacheKeyPrefix");
    }

    /// <summary>
    /// Verifies that a caching decorator correctly delegates void/async methods without caching.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_VoidMethodDelegatesDirectly()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IVoidService))]
                [CachingDecorator]
                public class VoidService : IVoidService
                {
                    public void DoWork() { }
                }
                public interface IVoidService
                {
                    void DoWork();
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        // Void method should delegate, not cache
        generated.ShouldContain("Blazing_CachingDecorator_VoidService");
        generated.ShouldContain("_inner.DoWork(");
    }

    /// <summary>
    /// Verifies that a class with <c>[LoggingDecorator]</c> emits a partial sealed class
    /// with <c>[LoggerMessage]</c> partial declarations.
    /// </summary>
    [Fact]
    public async Task LoggingDecorator_EmitsPartialClassWithLoggerMessageDeclarations()
    {
        var source = """
            using System.Threading.Tasks;
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IOrderService))]
                [LoggingDecorator]
                public class OrderService : IOrderService
                {
                    public Task<string> CreateOrderAsync(int quantity) => Task.FromResult("order");
                }
                public interface IOrderService
                {
                    Task<string> CreateOrderAsync(int quantity);
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("sealed partial class Blazing_LoggingDecorator_OrderService");
        generated.ShouldContain("LogCallingCreateOrderAsync");
        generated.ShouldContain("LogCompletedCreateOrderAsync");
        generated.ShouldContain("LogFailedCreateOrderAsync");
        generated.ShouldContain("ILogger<Blazing_LoggingDecorator_OrderService>");
    }

    /// <summary>
    /// Verifies that the logging decorator method body wraps calls with try/catch and a Stopwatch.
    /// </summary>
    [Fact]
    public async Task LoggingDecorator_MethodBody_WrapsWithTryCatch()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IService))]
                [LoggingDecorator]
                public class Service : IService
                {
                    public string Get() => "value";
                }
                public interface IService
                {
                    string Get();
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("Stopwatch");
        generated.ShouldContain("catch (global::System.Exception");
        generated.ShouldContain("ElapsedMilliseconds");
        generated.ShouldContain("_inner.Get(");
    }

    /// <summary>
    /// Verifies that the caching cache-key includes the interface name and method arguments.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_CacheKey_IncludesInterfaceNameAndMethodArguments()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IRepo))]
                [CachingDecorator]
                public class Repo : IRepo
                {
                    public string GetById(int id) => "item";
                }
                public interface IRepo
                {
                    string GetById(int id);
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        // Cache key format: __IRepo__GetById_{id}
        generated.ShouldContain("__IRepo__GetById_");
        generated.ShouldContain("{id}");
        generated.ShouldContain("GetOrCreate<");
        generated.ShouldNotContain("DateTime.UtcNow");
        generated.ShouldNotContain("lock (");
    }

    /// <summary>
    /// Verifies that a <c>Task&lt;T&gt;</c> method emits the async caching branch
    /// using <c>GetOrCreateAsync</c> and <c>IBlazingInvalidatable</c>.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_TaskOfT_EmitsAsyncCacheBranch()
    {
        var source = """
            using System.Threading.Tasks;
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IProductService))]
                [CachingDecorator(seconds: 60)]
                public class ProductService : IProductService
                {
                    public async System.Threading.Tasks.Task<string> GetNameAsync(int id)
                        => await System.Threading.Tasks.Task.FromResult("product");
                }
                public interface IProductService
                {
                    System.Threading.Tasks.Task<string> GetNameAsync(int id);
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("IBlazingInvalidatable");
        generated.ShouldContain("IDecoratorCache");
        generated.ShouldContain("GetOrCreateAsync<");
        generated.ShouldContain("async ");
        generated.ShouldContain("GetNameAsync");
        generated.ShouldNotContain("_asyncLock");
        generated.ShouldNotContain("System.IDisposable");
        generated.ShouldNotContain("DateTime.UtcNow");
    }

    /// <summary>
    /// Verifies that a <c>ValueTask&lt;T&gt;</c> method emits the async caching branch.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_ValueTaskOfT_EmitsAsyncCacheBranch()
    {
        var source = """
            using System.Threading.Tasks;
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(ICountService))]
                [CachingDecorator]
                public class CountService : ICountService
                {
                    public System.Threading.Tasks.ValueTask<int> GetCountAsync()
                        => new System.Threading.Tasks.ValueTask<int>(42);
                }
                public interface ICountService
                {
                    System.Threading.Tasks.ValueTask<int> GetCountAsync();
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("GetOrCreateAsync<");
        generated.ShouldContain("async ");
        generated.ShouldContain("GetCountAsync");
        generated.ShouldNotContain("_asyncLock");
    }

    /// <summary>
    /// Verifies that a non-generic <c>Task</c> method still delegates directly without caching.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_NonGenericTask_DelegatesDirectly()
    {
        var source = """
            using System.Threading.Tasks;
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IWorkerService))]
                [CachingDecorator]
                public class WorkerService : IWorkerService
                {
                    public System.Threading.Tasks.Task DoWorkAsync()
                        => System.Threading.Tasks.Task.CompletedTask;
                }
                public interface IWorkerService
                {
                    System.Threading.Tasks.Task DoWorkAsync();
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        // Non-generic Task should delegate, not use GetOrCreateAsync
        generated.ShouldContain("await _inner.DoWorkAsync()");
        generated.ShouldNotContain("GetOrCreateAsync<");
        generated.ShouldNotContain("_asyncLock");
    }

    /// <summary>
    /// Regression: synchronous <c>T</c> method uses <c>GetOrCreate</c>, not the async branch.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_SyncMethod_UsesGetOrCreate()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IProductService))]
                [CachingDecorator]
                public class ProductService : IProductService
                {
                    public string GetName(int id) => "Product";
                }
                public interface IProductService
                {
                    string GetName(int id);
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("GetOrCreate<");
        generated.ShouldNotContain("GetOrCreateAsync<");
        generated.ShouldNotContain("_asyncLock");
        generated.ShouldNotContain("lock (");
    }

    // --- Infrastructure ---

    /// <summary>
    /// Verifies that a <c>[CachingDecorator]</c> class emits an <c>IBlazingInvalidatable</c>
    /// implementation with <c>InvalidateCacheAsync</c> and <c>InvalidateAllCacheAsync</c>.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_EmitsIBlazingInvalidatableImplementation()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(ICatalogService))]
                [CachingDecorator]
                public class CatalogService : ICatalogService
                {
                    public string GetItem(int id) => "item";
                }
                public interface ICatalogService
                {
                    string GetItem(int id);
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("InvalidateCacheAsync");
        generated.ShouldContain("InvalidateAllCacheAsync");
        generated.ShouldContain("_cacheKeyPrefix");
        generated.ShouldContain("RemoveAsync");
        generated.ShouldContain("RemoveByPrefixAsync");
    }

    /// <summary>
    /// Verifies that a <c>[CachingDecorator]</c> class causes a <c>Blazing_CacheInvalidator_*</c>
    /// class to be emitted implementing <c>IBlazingCacheInvalidator&lt;T&gt;</c>.
    /// </summary>
    [Fact]
    public async Task CachingDecorator_EmitsCacheInvalidatorClass()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IOrderService))]
                [CachingDecorator]
                public class OrderService : IOrderService
                {
                    public string GetOrder(int id) => "order";
                }
                public interface IOrderService
                {
                    string GetOrder(int id);
                }
            }
            """;

        var generated = await RunDecoratorGeneratorAsync(source);

        generated.ShouldContain("Blazing_CacheInvalidator_OrderService");
        generated.ShouldContain("IBlazingCacheInvalidator<global::MyApp.IOrderService>");
        generated.ShouldContain("InvalidateAsync");
        generated.ShouldContain("InvalidateAllAsync");
    }

    // --- Infrastructure ---

    private static async Task<string> RunDecoratorGeneratorAsync(string source)
    {
        var references = BuildReferences();
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new ServiceDecoratorGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        var generatorErrors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        generatorErrors.ShouldBeEmpty(
            $"Generator produced errors: {string.Join(", ", generatorErrors.Select(d => d.GetMessage(System.Globalization.CultureInfo.InvariantCulture)))}");

        var runResult = driver.GetRunResult();
        var generatedSource = runResult.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .FirstOrDefault(t => t.Contains("Blazing.Extensions.DependencyInjection.Generated", StringComparison.Ordinal));

        generatedSource.ShouldNotBeNull("Expected Decorators.g.cs to be emitted but no output found.");
        return generatedSource!;
    }

    private static MetadataReference[] BuildReferences()
    {
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var netstandardPath = Path.Combine(runtimeDir, "netstandard.dll");

        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Threading.Tasks.Task).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Diagnostics.Stopwatch).Assembly.Location),
        };

        refs.Add(MetadataReference.CreateFromFile(
            typeof(Microsoft.Extensions.DependencyInjection.ServiceLifetime).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(
            typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(
            typeof(Microsoft.Extensions.Logging.ILogger).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(
            typeof(Microsoft.Extensions.Caching.Memory.IMemoryCache).Assembly.Location));

        refs.Add(MetadataReference.CreateFromFile(
            typeof(Blazing.Extensions.DependencyInjection.AutoRegisterAttribute).Assembly.Location));

        foreach (var dll in new[] { "System.Runtime.dll", "System.Collections.dll", "System.Threading.Tasks.dll" })
        {
            var path = Path.Combine(runtimeDir, dll);
            if (File.Exists(path))
                refs.Add(MetadataReference.CreateFromFile(path));
        }

        if (File.Exists(netstandardPath))
            refs.Add(MetadataReference.CreateFromFile(netstandardPath));

        return [.. refs];
    }
}
