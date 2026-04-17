using System.Collections.Immutable;
using Blazing.Extensions.DependencyInjection.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace Blazing.Extensions.DependencyInjection.SourceGenerators.Analyzers;

/// <summary>
/// Walks <see cref="Compilation.References"/> to discover all types decorated with
/// <c>[AutoRegister]</c> in assemblies that reference <c>Blazing.Extensions.DependencyInjection</c>.
/// </summary>
internal static class CompilationAnalyzer
{
    private const string AutoRegisterAttributeName = "Blazing.Extensions.DependencyInjection.AutoRegisterAttribute";
    private const string BlazingDIAssemblyName = "Blazing.Extensions.DependencyInjection";

    /// <summary>
    /// Discovers all <c>[AutoRegister]</c>-attributed types from the referenced assemblies
    /// of the given compilation and returns them as an immutable array of registration models.
    /// </summary>
    /// <param name="compilation">The compilation whose references to inspect.</param>
    /// <returns>
    /// An immutable array of <see cref="ServiceRegistrationModel"/> instances, one per
    /// discovered attributed class. Returns an empty array if no matches are found.
    /// </returns>
    public static ImmutableArray<ServiceRegistrationModel> DiscoverFromReferences(Compilation compilation)
    {
        var autoRegisterSymbol = compilation.GetTypeByMetadataName(AutoRegisterAttributeName);
        if (autoRegisterSymbol is null)
            return ImmutableArray<ServiceRegistrationModel>.Empty;

        var results = ImmutableArray.CreateBuilder<ServiceRegistrationModel>();

        foreach (var reference in compilation.References)
        {
            if (compilation.GetAssemblyOrModuleSymbol(reference) is not IAssemblySymbol assemblySymbol)
                continue;

            // Only process assemblies that themselves reference Blazing.Extensions.DependencyInjection
            if (!ReferencesBlazingDI(assemblySymbol))
                continue;

            WalkNamespace(assemblySymbol.GlobalNamespace, autoRegisterSymbol, results);
        }

        return results.ToImmutable();
    }

    private static bool ReferencesBlazingDI(IAssemblySymbol assembly)
    {
        foreach (var module in assembly.Modules)
        {
            foreach (var referenced in module.ReferencedAssemblies)
            {
                if (referenced.Name == BlazingDIAssemblyName)
                    return true;
            }
        }

        return false;
    }

    private static void WalkNamespace(
        INamespaceSymbol ns,
        INamedTypeSymbol autoRegisterSymbol,
        ImmutableArray<ServiceRegistrationModel>.Builder results)
    {
        foreach (var member in ns.GetMembers())
        {
            if (member is INamespaceSymbol childNs)
            {
                WalkNamespace(childNs, autoRegisterSymbol, results);
            }
            else if (member is INamedTypeSymbol { TypeKind: TypeKind.Class, IsAbstract: false } type)
            {
                if (IsLocalOnly(type, autoRegisterSymbol))
                    continue;

                var model = TryBuildModel(type, autoRegisterSymbol);
                if (model is not null)
                    results.Add(model);
            }
        }
    }

    /// <summary>
    /// Attempts to build a <see cref="ServiceRegistrationModel"/> for a type that may carry
    /// the <c>[AutoRegister]</c> attribute. Returns <see langword="null"/> if the attribute is absent.
    /// </summary>
    internal static ServiceRegistrationModel? TryBuildModel(
        INamedTypeSymbol type,
        INamedTypeSymbol autoRegisterSymbol)
    {
        AttributeData? attrData = null;
        foreach (var attr in type.GetAttributes())
        {
            if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, autoRegisterSymbol))
            {
                attrData = attr;
                break;
            }
        }

        if (attrData is null)
            return null;

        var lifetime = ExtractLifetime(attrData);
        var serviceType = ExtractServiceType(attrData);
        var key = ExtractKey(attrData);

        var implFqn = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .TrimStart("global::".ToCharArray());

        string[] interfaces;
        if (serviceType is not null)
        {
            interfaces = [serviceType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                .TrimStart("global::".ToCharArray())];
        }
        else
        {
            // Collect directly implemented interfaces, excluding IDisposable and IAsyncDisposable
            var ifaceList = new List<string>();
            foreach (var iface in type.Interfaces)
            {
                var ifaceName = iface.Name;
                if (ifaceName is "IDisposable" or "IAsyncDisposable")
                    continue;
                ifaceList.Add(iface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                    .TrimStart("global::".ToCharArray()));
            }

            interfaces = ifaceList.ToArray();
        }

        return new ServiceRegistrationModel(implFqn, interfaces, lifetime, key);
    }

    private static string ExtractLifetime(AttributeData attrData)
    {
        // Constructor arg 0 is ServiceLifetime (int)
        if (attrData.ConstructorArguments.Length > 0 &&
            attrData.ConstructorArguments[0].Value is int lifetimeValue)
        {
            return lifetimeValue switch
            {
                0 => "Singleton",
                1 => "Scoped",
                _ => "Transient",
            };
        }

        return "Transient";
    }

    private static ITypeSymbol? ExtractServiceType(AttributeData attrData)
    {
        // Constructor arg 1 (if present) is typeof(TService) → TypedConstant.Value is ITypeSymbol
        if (attrData.ConstructorArguments.Length > 1 &&
            attrData.ConstructorArguments[1].Value is ITypeSymbol svcType)
        {
            return svcType;
        }

        return null;
    }

    private static string? ExtractKey(AttributeData attrData)
    {
        foreach (var namedArg in attrData.NamedArguments)
        {
            if (namedArg is { Key: "Key", Value.IsNull: false })
            {
                var val = namedArg.Value.Value;
                return val is string s ? $"\"{s}\"" : val?.ToString();
            }
        }

        return null;
    }
    /// <summary>
    /// Returns <see langword="true" /> if the type's <c>[AutoRegister]</c> attribute has
    /// <c>LocalOnly = true</c>, meaning it should be skipped when scanning referenced assemblies.
    /// </summary>
    private static bool IsLocalOnly(INamedTypeSymbol type, INamedTypeSymbol autoRegisterSymbol)
    {
        foreach (var attr in type.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, autoRegisterSymbol))
                continue;

            foreach (var namedArg in attr.NamedArguments)
            {
                if (namedArg is { Key: "LocalOnly", Value.Value: true })
                    return true;
            }
        }

        return false;
    }}
