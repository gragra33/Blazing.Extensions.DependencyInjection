using Blazing.Extensions.DependencyInjection.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Shouldly;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests.GeneratorTests;

/// <summary>
/// Tests for <see cref="AutoRegisterGenerator"/> using Roslyn source-generator testing infrastructure.
/// Each test compiles a small snippet and verifies that the emitted <c>Register.g.cs</c> content
/// is correct and complete.
/// </summary>
public sealed class AutoRegisterGeneratorTests
{
    /// <summary>
    /// Verifies that a class decorated with <c>[AutoRegister(Singleton)]</c> that implements one interface
    /// emits registrations for both the interface and the concrete type.
    /// </summary>
    [Fact]
    public async Task SingletonWithInterface_EmitsInterfaceAndConcreteRegistrations()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IDataService))]
                public class DataService : IDataService { }
                public interface IDataService { }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldContain("AddSingleton<global::MyApp.IDataService, global::MyApp.DataService>");
        generated.ShouldContain("AddSingleton<global::MyApp.DataService, global::MyApp.DataService>");
    }

    /// <summary>
    /// Verifies that a class decorated with <c>[AutoRegister(Transient)]</c> (self-registration)
    /// emits only a concrete-type registration.
    /// </summary>
    [Fact]
    public async Task TransientSelfRegistration_EmitsSelfOnly()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Transient)]
                public class SimpleService { }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldContain("AddTransient<global::MyApp.SimpleService, global::MyApp.SimpleService>");
    }

    /// <summary>
    /// Verifies that keyed services emit <c>AddKeyedSingleton</c> with the correct key string.
    /// </summary>
    [Fact]
    public async Task KeyedSingleton_EmitsAddKeyedSingleton()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IDbContext), Key = "primary")]
                public class PrimaryDbContext : IDbContext { }
                public interface IDbContext { }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldContain("AddKeyedSingleton<global::MyApp.IDbContext, global::MyApp.PrimaryDbContext>(\"primary\")");
    }

    /// <summary>
    /// Verifies that <c>IDisposable</c> is excluded from the emitted interface registrations.
    /// </summary>
    [Fact]
    public async Task IDisposableInterface_IsExcludedFromRegistrations()
    {
        var source = """
            using System;
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton)]
                public class DisposableService : IMyService, IDisposable
                {
                    public void Dispose() { }
                }
                public interface IMyService { }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldNotContain("IDisposable");
        generated.ShouldContain("IMyService");
    }

    /// <summary>
    /// Verifies that the generated <c>Register()</c> method is inside the expected namespace
    /// and class, and is an extension method on <c>IServiceCollection</c>.
    /// </summary>
    [Fact]
    public async Task Generated_RegisterMethod_HasCorrectSignature()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Transient)]
                public class MyService { }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldContain("namespace Blazing.Extensions.DependencyInjection");
        generated.ShouldContain("static partial class BlazingDI");
        generated.ShouldContain("IServiceCollection Register(");
        generated.ShouldContain("return services;");
    }

    /// <summary>
    /// Verifies that a scoped service is emitted with <c>AddScoped</c>.
    /// </summary>
    [Fact]
    public async Task ScopedService_EmitsAddScoped()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Scoped, typeof(IUserService))]
                public class UserService : IUserService { }
                public interface IUserService { }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldContain("AddScoped<global::MyApp.IUserService, global::MyApp.UserService>");
    }

    /// <summary>
    /// Verifies that a <c>[CachingDecorator]</c>-annotated service is registered through
    /// the generated caching decorator factory for the interface registration.
    /// </summary>
    [Fact]
    public async Task CachingDecoratorService_EmitsDecoratorFactoryRegistration()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Singleton, typeof(IProductService))]
                [CachingDecorator(seconds: 30)]
                public class ProductService : IProductService
                {
                    public string GetName(int id) => "P";
                }

                public interface IProductService
                {
                    string GetName(int id);
                }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldContain("AddSingleton<global::MyApp.IProductService>(sp => new global::Blazing.Extensions.DependencyInjection.Generated.Blazing_CachingDecorator_ProductService(");
        generated.ShouldContain("GetRequiredService<global::MyApp.ProductService>()");
        generated.ShouldContain("IDecoratorCache");
    }

    /// <summary>
    /// Verifies that <c>LocalOnly = true</c> does NOT suppress registration in the declaring assembly.
    /// The service must still appear in its own <c>Register.g.cs</c>.
    /// </summary>
    [Fact]
    public async Task LocalOnly_StillRegisteredInDeclaringAssembly()
    {
        var source = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MyApp
            {
                [AutoRegister(ServiceLifetime.Scoped, LocalOnly = true)]
                public class LocalService { }
            }
            """;

        var generated = await RunGeneratorAsync(source);

        generated.ShouldContain("AddScoped<global::MyApp.LocalService, global::MyApp.LocalService>");
    }

    /// <summary>
    /// Verifies that a service with no <c>LocalOnly</c> flag from a referenced assembly
    /// IS included in the consuming assembly's <c>Register.g.cs</c>.
    /// </summary>
    [Fact]
    public async Task NonLocalOnly_FromReferencedAssembly_IsIncluded()
    {
        var referencedSource = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace LibA
            {
                [AutoRegister(ServiceLifetime.Singleton)]
                public class SharedService { }
            }
            """;

        // Main assembly needs at least one local [AutoRegister] so Register.g.cs is emitted
        var mainSource = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MainApp
            {
                [AutoRegister(ServiceLifetime.Transient)]
                public class LocalService { }
            }
            """;

        var generated = await RunGeneratorWithReferencedAssemblyAsync(referencedSource, mainSource);

        generated.ShouldNotBeNull();
        generated!.ShouldContain("AddSingleton<global::LibA.SharedService, global::LibA.SharedService>");
    }

    /// <summary>
    /// Verifies that a service with <c>LocalOnly = true</c> from a referenced assembly
    /// is NOT included in the consuming assembly's <c>Register.g.cs</c>.
    /// </summary>
    [Fact]
    public async Task LocalOnly_FromReferencedAssembly_IsExcluded()
    {
        var referencedSource = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace LibA
            {
                [AutoRegister(ServiceLifetime.Scoped, LocalOnly = true)]
                public class WasmOnlyService { }
            }
            """;

        // Main assembly needs at least one local [AutoRegister] so Register.g.cs is emitted
        var mainSource = """
            using Blazing.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection;

            namespace MainApp
            {
                [AutoRegister(ServiceLifetime.Transient)]
                public class LocalService { }
            }
            """;

        var generated = await RunGeneratorWithReferencedAssemblyAsync(referencedSource, mainSource);

        generated.ShouldNotBeNull();
        generated!.ShouldNotContain("WasmOnlyService");
        generated!.ShouldContain("AddTransient<global::MainApp.LocalService, global::MainApp.LocalService>");
    }

    // --- Infrastructure ---

    private static async Task<string> RunGeneratorAsync(string source)
    {
        var references = BuildReferences();
        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(source)],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new AutoRegisterGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(compilation, out _, out var diagnostics);

        // Filter to generator errors only (suppress binding errors from incomplete test sources)
        var generatorErrors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        generatorErrors.ShouldBeEmpty(
            $"Generator produced errors: {string.Join(", ", generatorErrors.Select(d => d.GetMessage(System.Globalization.CultureInfo.InvariantCulture)))}");

        var runResult = driver.GetRunResult();
        var generatedSource = runResult.GeneratedTrees
            .Select(t => t.GetText().ToString())
            .FirstOrDefault(t => t.Contains("BlazingDI", StringComparison.Ordinal));

        generatedSource.ShouldNotBeNull("Expected Register.g.cs to be emitted but no output found.");
        return generatedSource!;
    }

    /// <summary>
    /// Compiles <paramref name="referencedSource"/> into an in-memory assembly, then compiles
    /// <paramref name="mainSource"/> referencing it, and runs the generator on the main compilation.
    /// Returns the generated <c>Register.g.cs</c> content, or <see langword="null"/> if nothing
    /// was emitted.
    /// </summary>
    private static async Task<string?> RunGeneratorWithReferencedAssemblyAsync(
        string referencedSource,
        string mainSource)
    {
        var refs = BuildReferences();

        // Step 1 – compile the referenced assembly to an in-memory stream
        var refCompilation = CSharpCompilation.Create(
            assemblyName: "ReferencedAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(referencedSource)],
            references: refs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new System.IO.MemoryStream();
        var emitResult = refCompilation.Emit(ms);
        emitResult.Success.ShouldBeTrue(
            $"Referenced assembly failed to compile: {string.Join(", ", emitResult.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage(System.Globalization.CultureInfo.InvariantCulture)))}");
        ms.Seek(0, System.IO.SeekOrigin.Begin);

        // Step 2 – build the main compilation referencing the emitted assembly
        var allRefs = refs.ToList();
        allRefs.Add(MetadataReference.CreateFromStream(ms));

        var mainCompilation = CSharpCompilation.Create(
            assemblyName: "MainAssembly",
            syntaxTrees: [CSharpSyntaxTree.ParseText(mainSource)],
            references: allRefs,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        // Step 3 – run the generator on the main compilation
        var generator = new AutoRegisterGenerator();
        var driver = CSharpGeneratorDriver
            .Create(generator)
            .RunGeneratorsAndUpdateCompilation(mainCompilation, out _, out _);

        var runResult = driver.GetRunResult();
        return await Task.FromResult(
            runResult.GeneratedTrees
                .Select(t => t.GetText().ToString())
                .FirstOrDefault(t => t.Contains("BlazingDI", StringComparison.Ordinal)));
    }

    private static MetadataReference[] BuildReferences()
    {
        var netstandardPath = Path.Combine(
            Path.GetDirectoryName(typeof(object).Assembly.Location)!,
            "netstandard.dll");

        var refs = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Collections.Generic.List<>).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
        };

        // Add Microsoft.Extensions.DependencyInjection
        refs.Add(MetadataReference.CreateFromFile(
            typeof(Microsoft.Extensions.DependencyInjection.ServiceLifetime).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(
            typeof(Microsoft.Extensions.DependencyInjection.IServiceCollection).Assembly.Location));
        refs.Add(MetadataReference.CreateFromFile(
            typeof(Microsoft.Extensions.DependencyInjection.ServiceCollectionServiceExtensions).Assembly.Location));

        // Add Blazing.Extensions.DependencyInjection (for AutoRegisterAttribute)
        refs.Add(MetadataReference.CreateFromFile(
            typeof(Blazing.Extensions.DependencyInjection.AutoRegisterAttribute).Assembly.Location));

        // Add runtime references
        var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        foreach (var dll in new[] { "System.Runtime.dll", "System.Collections.dll" })
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
