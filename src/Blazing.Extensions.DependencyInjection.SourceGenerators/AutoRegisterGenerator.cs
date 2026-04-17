using System.Collections.Immutable;
using Blazing.Extensions.DependencyInjection.SourceGenerators.Analyzers;
using Blazing.Extensions.DependencyInjection.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Blazing.Extensions.DependencyInjection.SourceGenerators;

/// <summary>
/// Roslyn incremental source generator that discovers all types marked with
/// <c>[AutoRegister]</c> — both in the current compilation and in all referenced assemblies —
/// and emits a <c>Register.g.cs</c> file containing a <c>Register()</c> extension method on
/// <c>IServiceCollection</c>.
/// </summary>
[Generator]
public sealed class AutoRegisterGenerator : IIncrementalGenerator
{
    private const string AutoRegisterAttributeFqn =
        "Blazing.Extensions.DependencyInjection.AutoRegisterAttribute";

    private const string CachingAttributeFqn =
        "Blazing.Extensions.DependencyInjection.CachingDecoratorAttribute";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Collect [AutoRegister] classes in the current compilation
        var localTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                AutoRegisterAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: TransformToModel)
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // 2. Discover attributed types from referenced assemblies
        var referencedTypes = context.CompilationProvider
            .Select(static (compilation, _) =>
                CompilationAnalyzer.DiscoverFromReferences(compilation));

        // 3. Collect [CachingDecorator] types for cache bootstrap registration
        var cachingTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                CachingAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformToCacheModel(ctx))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!)
            .Collect();

        // 4. Combine and emit Register.g.cs
        var allTypes = localTypes.Collect().Combine(referencedTypes).Combine(cachingTypes);

        context.RegisterSourceOutput(allTypes, static (spc, combined) =>
        {
            var ((local, referenced), caching) = combined;
            var all = local.AddRange(referenced);
            if (all.IsEmpty)
                return;

            spc.AddSource("Register.g.cs", Emit(all, caching));
        });
    }

    private static ServiceRegistrationModel? TransformToModel(
        GeneratorAttributeSyntaxContext ctx,
        CancellationToken cancellationToken)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var autoRegisterSymbol = ctx.SemanticModel.Compilation
            .GetTypeByMetadataName("Blazing.Extensions.DependencyInjection.AutoRegisterAttribute");

        if (autoRegisterSymbol is null)
            return null;

        return CompilationAnalyzer.TryBuildModel(typeSymbol, autoRegisterSymbol);
    }

    /// <summary>
    /// Collects the interface FQN and invalidator class name for each <c>[CachingDecorator]</c> class,
    /// used to emit <c>IBlazingCacheInvalidator&lt;T&gt;</c> registrations.
    /// </summary>
    private static CacheRegistrationModel? TransformToCacheModel(GeneratorAttributeSyntaxContext ctx)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var primaryInterface = ResolvePrimaryInterface(typeSymbol, ctx.SemanticModel.Compilation);
        if (primaryInterface is null)
            return null;

        var implFqn = StripGlobal(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        var ifaceFqn = StripGlobal(primaryInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        var simpleImplName = implFqn.Contains('.')
            ? implFqn.Substring(implFqn.LastIndexOf('.') + 1)
            : implFqn;

        return new CacheRegistrationModel(
            InterfaceFullyQualifiedName: ifaceFqn,
            ImplementationFullyQualifiedName: implFqn,
            DecoratorClassName: $"Blazing_CachingDecorator_{simpleImplName}",
            InvalidatorClassName: $"Blazing_CacheInvalidator_{simpleImplName}");
    }

    private static ITypeSymbol? ResolvePrimaryInterface(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var autoRegisterSymbol = compilation.GetTypeByMetadataName(AutoRegisterAttributeFqn);

        if (autoRegisterSymbol is not null)
        {
            var fromAttr = TryGetInterfaceFromAttribute(typeSymbol, autoRegisterSymbol);
            if (fromAttr is not null)
                return fromAttr;
        }

        return TryGetFirstNonDisposableInterface(typeSymbol);
    }

    private static ITypeSymbol? TryGetInterfaceFromAttribute(INamedTypeSymbol typeSymbol, INamedTypeSymbol autoRegisterSymbol)
    {
        foreach (var attr in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, autoRegisterSymbol))
                continue;

            if (attr.ConstructorArguments.Length > 1 &&
                attr.ConstructorArguments[1].Value is ITypeSymbol svcType)
                return svcType;
        }

        return null;
    }

    private static ITypeSymbol? TryGetFirstNonDisposableInterface(INamedTypeSymbol typeSymbol)
    {
        foreach (var iface in typeSymbol.Interfaces)
        {
            if (iface.Name is "IDisposable" or "IAsyncDisposable")
                continue;
            return iface;
        }

        return null;
    }

    private static string StripGlobal(string fqn)
        => fqn.StartsWith("global::", StringComparison.Ordinal) ? fqn.Substring(8) : fqn;

    private static string Emit(
        ImmutableArray<ServiceRegistrationModel> models,
        ImmutableArray<CacheRegistrationModel> cachingModels)
    {
        var cacheModelByImplementation = cachingModels
            .GroupBy(static m => m.ImplementationFullyQualifiedName, StringComparer.Ordinal)
            .ToDictionary(static g => g.Key, static g => g.First(), StringComparer.Ordinal);

        var registrations = string.Join("\n        ",
            models.SelectMany(m => BuildRegistrationLines(m, cacheModelByImplementation)));

        var cacheBootstrap = BuildCacheBootstrap(cachingModels);
        var usingExtensions = cachingModels.IsEmpty ? string.Empty : "\nusing Microsoft.Extensions.DependencyInjection.Extensions;";

        return $$$"""
            // <auto-generated/>
            // This file is generated by Blazing.Extensions.DependencyInjection.SourceGenerators.
            // Do not modify manually.
            #nullable enable
            using Microsoft.Extensions.DependencyInjection;{{{usingExtensions}}}

            namespace Blazing.Extensions.DependencyInjection
            {
                internal static partial class BlazingDI
                {
                    /// <summary>
                    /// Registers all services discovered at compile time via the <c>[AutoRegister]</c> attribute.
                    /// Call once at application startup.
                    /// </summary>
                    /// <param name="services">The service collection to register services into.</param>
                    /// <returns>The same <paramref name="services"/> instance for fluent chaining.</returns>
                    public static global::Microsoft.Extensions.DependencyInjection.IServiceCollection Register(
                        this global::Microsoft.Extensions.DependencyInjection.IServiceCollection services)
                    {
                        {{{registrations}}}
                        {{{cacheBootstrap}}}
                        return services;
                    }
                }
            }
            """;
    }

    private static string BuildCacheBootstrap(ImmutableArray<CacheRegistrationModel> cachingModels)
    {
        if (cachingModels.IsEmpty)
            return string.Empty;

        var lines = new List<string>
        {
            // Register the default cache if nothing else has been registered
            "services.TryAddSingleton<global::Blazing.Extensions.DependencyInjection.IDecoratorCache, global::Blazing.Extensions.DependencyInjection.DefaultDecoratorCache>();"
        };

        foreach (var cm in cachingModels)
        {
            lines.Add(
                $"services.AddSingleton<global::Blazing.Extensions.DependencyInjection.IBlazingCacheInvalidator<global::{cm.InterfaceFullyQualifiedName}>, " +
                $"global::Blazing.Extensions.DependencyInjection.Generated.{cm.InvalidatorClassName}>();");
        }

        return string.Join("\n        ", lines);
    }

    private static IEnumerable<string> BuildRegistrationLines(
        ServiceRegistrationModel m,
        IReadOnlyDictionary<string, CacheRegistrationModel> cacheModelByImplementation)
    {
        var lifetime = m.Lifetime;
        var impl = m.ImplFullyQualifiedName;
        var key = m.Key;
        cacheModelByImplementation.TryGetValue(impl, out var cacheModel);

        if (m.InterfaceFullyQualifiedNames.Length == 0)
        {
            yield return EmitPlainRegistration(lifetime, impl, impl, key);
            yield break;
        }

        foreach (var iface in m.InterfaceFullyQualifiedNames)
        {
            if (cacheModel is not null && iface.Equals(cacheModel.InterfaceFullyQualifiedName, StringComparison.Ordinal))
            {
                yield return EmitCachingDecoratorRegistration(lifetime, iface, impl, key, cacheModel.DecoratorClassName);
                continue;
            }

            yield return EmitPlainRegistration(lifetime, iface, impl, key);
        }

        // Also register as concrete type for direct resolution.
        // For decorated services, decorator factories resolve this concrete registration as the inner instance.
        yield return EmitPlainRegistration(lifetime, impl, impl, key);
    }

    private static string EmitPlainRegistration(string lifetime, string serviceType, string implementationType, string? key)
        => key is null
            ? $"services.Add{lifetime}<global::{serviceType}, global::{implementationType}>();"
            : $"services.AddKeyed{lifetime}<global::{serviceType}, global::{implementationType}>({key});";

    private static string EmitCachingDecoratorRegistration(
        string lifetime,
        string interfaceType,
        string implementationType,
        string? key,
        string decoratorClassName)
    {
        if (key is null)
        {
            return
                $"services.Add{lifetime}<global::{interfaceType}>(sp => new global::Blazing.Extensions.DependencyInjection.Generated.{decoratorClassName}(" +
                $"sp.GetRequiredService<global::{implementationType}>(), " +
                "sp.GetRequiredService<global::Blazing.Extensions.DependencyInjection.IDecoratorCache>()));";
        }

        return
            $"services.AddKeyed{lifetime}<global::{interfaceType}>({key}, (sp, _) => new global::Blazing.Extensions.DependencyInjection.Generated.{decoratorClassName}(" +
            $"sp.GetRequiredKeyedService<global::{implementationType}>({key}), " +
            "sp.GetRequiredService<global::Blazing.Extensions.DependencyInjection.IDecoratorCache>()));";
    }
}
