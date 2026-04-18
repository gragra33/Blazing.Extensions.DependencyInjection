using System.Collections.Immutable;
using Blazing.Extensions.DependencyInjection.SourceGenerators.Helpers;
using Blazing.Extensions.DependencyInjection.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Blazing.Extensions.DependencyInjection.SourceGenerators;

/// <summary>
/// Roslyn incremental source generator that discovers types marked with
/// <c>[CachingDecorator]</c> and/or <c>[LoggingDecorator]</c> and emits concrete
/// sealed decorator classes into <c>Decorators.g.cs</c>.
/// </summary>
[Generator]
public sealed class ServiceDecoratorGenerator : IIncrementalGenerator
{
    private const string CachingAttributeFqn =
        "Blazing.Extensions.DependencyInjection.CachingDecoratorAttribute";

    private const string LoggingAttributeFqn =
        "Blazing.Extensions.DependencyInjection.LoggingDecoratorAttribute";

    private const string AutoRegisterAttributeFqn =
        "Blazing.Extensions.DependencyInjection.AutoRegisterAttribute";

    private const string CachingKind = "Caching";
    private const string LoggingKind = "Logging";

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect types with [CachingDecorator]
        var cachingTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                CachingAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformToDecoratorModel(ctx, CachingKind))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Collect types with [LoggingDecorator]
        var loggingTypes = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                LoggingAttributeFqn,
                predicate: static (node, _) => node is ClassDeclarationSyntax,
                transform: static (ctx, _) => TransformToDecoratorModel(ctx, LoggingKind))
            .Where(static m => m is not null)
            .Select(static (m, _) => m!);

        // Combine both streams
        var allDecorators = cachingTypes.Collect().Combine(loggingTypes.Collect());

        context.RegisterSourceOutput(allDecorators, static (spc, combined) =>
        {
            var (caching, logging) = combined;
            var all = caching.AddRange(logging);
            if (all.IsEmpty)
                return;

            spc.AddSource("Decorators.g.cs", Emit(all));
        });
    }

    private static DecoratorModel? TransformToDecoratorModel(
        GeneratorAttributeSyntaxContext ctx,
        string kind)
    {
        if (ctx.TargetSymbol is not INamedTypeSymbol typeSymbol)
            return null;

        var primaryInterface = ResolveInterface(typeSymbol, ctx.SemanticModel.Compilation);
        if (primaryInterface is null)
            return null;

        var cacheSeconds = kind == CachingKind
            ? ReadCacheSeconds(typeSymbol, ctx.SemanticModel.Compilation)
            : 300;

        var implFqn = StripGlobal(typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));
        var ifaceFqn = StripGlobal(primaryInterface.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat));

        var simpleImplName = implFqn.Contains('.')
            ? implFqn.Substring(implFqn.LastIndexOf('.') + 1)
            : implFqn;
        var decoratorClassName = $"Blazing_{kind}Decorator_{simpleImplName}";

        var methods = CollectInterfaceMethods(primaryInterface as INamedTypeSymbol);
        var methodModels = new MethodModel[methods.Count];
        for (var i = 0; i < methods.Count; i++)
            methodModels[i] = MethodEmitHelper.BuildMethodModel(methods[i], decoratorClassName);

        return new DecoratorModel(
            ImplFullyQualifiedName: implFqn,
            InterfaceFullyQualifiedName: ifaceFqn,
            DecoratorKind: kind,
            CacheSeconds: cacheSeconds,
            Methods: methodModels);
    }

    private static string StripGlobal(string fqn)
        => fqn.StartsWith("global::", StringComparison.Ordinal) ? fqn.Substring(8) : fqn;

    private static ITypeSymbol? ResolveInterface(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var autoRegisterSymbol = compilation.GetTypeByMetadataName(AutoRegisterAttributeFqn);

        if (autoRegisterSymbol is not null)
        {
            foreach (var attr in typeSymbol.GetAttributes())
            {
                if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, autoRegisterSymbol))
                    continue;

                if (attr.ConstructorArguments.Length > 1 &&
                    attr.ConstructorArguments[1].Value is ITypeSymbol svcType)
                    return svcType;

                break;
            }
        }

        foreach (var iface in typeSymbol.Interfaces)
        {
            if (iface.Name is "IDisposable" or "IAsyncDisposable")
                continue;
            return iface;
        }

        return null;
    }

    private static int ReadCacheSeconds(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        var cachingSymbol = compilation.GetTypeByMetadataName(CachingAttributeFqn);
        if (cachingSymbol is null)
            return 300;

        foreach (var attr in typeSymbol.GetAttributes())
        {
            if (!SymbolEqualityComparer.Default.Equals(attr.AttributeClass, cachingSymbol))
                continue;

            if (attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is int seconds)
                return seconds;

            break;
        }

        return 300;
    }

    private static List<IMethodSymbol> CollectInterfaceMethods(
        INamedTypeSymbol? iface)
    {
        var result = new List<IMethodSymbol>();
        if (iface is null)
            return result;

        foreach (var member in iface.GetMembers())
        {
            if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false } method)
            {
                result.Add(method);
            }
        }

        // Collect methods from base interfaces
        foreach (var baseIface in iface.AllInterfaces)
        {
            foreach (var member in baseIface.GetMembers())
            {
                if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary, IsStatic: false } method)
                {
                    result.Add(method);
                }
            }
        }

        return result;
    }

    private static string Emit(ImmutableArray<DecoratorModel> models)
    {
        var classes = string.Join("\n\n", Enumerable.Select(models, EmitDecoratorClass));

        // Emit invalidator classes for every caching decorator
        var cachingModels = Enumerable.Where(models, m => m.DecoratorKind == CachingKind).ToArray();
        var invalidators = cachingModels.Length == 0
            ? string.Empty
            : "\n\n" + string.Join("\n\n", cachingModels.Select(EmitCachingInvalidator));

        return $$$"""
            // <auto-generated/>
            // This file is generated by Blazing.Extensions.DependencyInjection.SourceGenerators.
            // Do not modify manually.
            #nullable enable
            using System;
            using System.Collections.Generic;
            using System.Diagnostics;
            using System.Threading;
            using System.Threading.Tasks;
            using Microsoft.Extensions.Logging;

            namespace Blazing.Extensions.DependencyInjection.Generated
            {
                {{{classes}}}{{{invalidators}}}
            }
            """;
    }

    private static string EmitDecoratorClass(DecoratorModel m)
        => m.DecoratorKind == CachingKind
            ? EmitCachingDecorator(m)
            : EmitLoggingDecorator(m);

    private static string EmitCachingDecorator(DecoratorModel m)
    {
        var methods = string.Join("\n\n        ", m.Methods.Select(method => EmitCachingMethod(method, m.InterfaceSimpleName)));

        return $$$"""
            /// <summary>Source-generated caching decorator for <see cref="global::{{{m.InterfaceFullyQualifiedName}}}"/>.</summary>
                [global::System.CodeDom.Compiler.GeneratedCode("Blazing.DI.Generators", "3.0")]
                internal sealed class {{{m.DecoratorClassName}}} : global::{{{m.InterfaceFullyQualifiedName}}}, global::Blazing.Extensions.DependencyInjection.IBlazingInvalidatable
                {
                    private readonly global::{{{m.InterfaceFullyQualifiedName}}} _inner;
                    private readonly global::Blazing.Extensions.DependencyInjection.IDecoratorCache _cache;
                    private const int _cacheDurationSeconds = {{{m.CacheSeconds}}};
                    private const string _cacheKeyPrefix = "__{{{m.InterfaceSimpleName}}}__";

                    /// <summary>Initializes a new instance of <see cref="{{{m.DecoratorClassName}}}"/>.</summary>
                    public {{{m.DecoratorClassName}}}(
                        global::{{{m.InterfaceFullyQualifiedName}}} inner,
                        global::Blazing.Extensions.DependencyInjection.IDecoratorCache cache)
                    {
                        _inner = inner;
                        _cache = cache;
                    }

                    {{{methods}}}

                    /// <inheritdoc/>
                    public global::System.Threading.Tasks.Task InvalidateCacheAsync(string cacheKey, global::System.Threading.CancellationToken cancellationToken = default)
                        => _cache.RemoveAsync(cacheKey, cancellationToken);

                    /// <inheritdoc/>
                    public global::System.Threading.Tasks.Task InvalidateAllCacheAsync(global::System.Threading.CancellationToken cancellationToken = default)
                        => _cache.RemoveByPrefixAsync(_cacheKeyPrefix, cancellationToken);
                }
            """;
    }

    private static string EmitCachingMethod(MethodModel method, string interfaceSimpleName)
    {
        if (method.IsVoid || method is { IsAsync: true, AsyncResultType: null })
        {
            // Cannot cache void / non-generic async (Task, ValueTask) — delegate directly
            var awaitPrefix = method.IsAsync ? "await " : "";
            return $$$"""
                /// <inheritdoc/>
                        public {{{method.ReturnType}}} {{{method.MethodName}}}({{{method.ParameterList}}})
                            => {{{awaitPrefix}}}_inner.{{{method.MethodName}}}({{{method.ArgumentList}}});
                """;
        }

        // Build arg suffix: "_arg1_arg2" interpolated into the key
        var keyArgSuffix = method.ParameterNames.Length == 0
            ? string.Empty
            : "_" + string.Join("_",
                method.ParameterNames.Select(n => $"{{{n}}}"));

        var keyExpr = $"$\"__{interfaceSimpleName}__{method.MethodName}{keyArgSuffix}\"";
        var durationExpr = "global::System.TimeSpan.FromSeconds(_cacheDurationSeconds)";

        if (method.IsAsync)
        {
            // Find a CancellationToken parameter name, or use default
            var ctParam = method.ParameterNames.FirstOrDefault(static n => n.Equals("cancellationToken", StringComparison.OrdinalIgnoreCase)
                                                                           || n.Equals("ct", StringComparison.OrdinalIgnoreCase));
            var ctArg = ctParam ?? "default";

            return $$$"""
                /// <inheritdoc/>
                        public async {{{method.ReturnType}}} {{{method.MethodName}}}({{{method.ParameterList}}})
                        {
                            var _key = {{{keyExpr}}};
                            return await _cache.GetOrCreateAsync<{{{method.AsyncResultType}}}>(
                                _key,
                                async _ct => await _inner.{{{method.MethodName}}}({{{method.ArgumentList}}}).ConfigureAwait(false),
                                {{{durationExpr}}},
                                {{{ctArg}}}).ConfigureAwait(false);
                        }
                """;
        }

        // Synchronous non-void
        return $$$"""
            /// <inheritdoc/>
                    public {{{method.ReturnType}}} {{{method.MethodName}}}({{{method.ParameterList}}})
                    {
                        var _key = {{{keyExpr}}};
                        return _cache.GetOrCreate<{{{method.ReturnType}}}>(
                            _key,
                            () => _inner.{{{method.MethodName}}}({{{method.ArgumentList}}}),
                            {{{durationExpr}}});
                    }
            """;
    }

    private static string EmitCachingInvalidator(DecoratorModel m)
    {
        return $$$"""
            /// <summary>Source-generated cache invalidator for <see cref="global::{{{m.InterfaceFullyQualifiedName}}}"/>.</summary>
                [global::System.CodeDom.Compiler.GeneratedCode("Blazing.DI.Generators", "3.0")]
                internal sealed class {{{m.InvalidatorClassName}}} : global::Blazing.Extensions.DependencyInjection.IBlazingCacheInvalidator<global::{{{m.InterfaceFullyQualifiedName}}}>
                {
                    private readonly global::Blazing.Extensions.DependencyInjection.IDecoratorCache _cache;
                    private const string _keyPrefix = "__{{{m.InterfaceSimpleName}}}__";

                    /// <summary>Initializes a new instance of <see cref="{{{m.InvalidatorClassName}}}"/>.</summary>
                    public {{{m.InvalidatorClassName}}}(global::Blazing.Extensions.DependencyInjection.IDecoratorCache cache)
                        => _cache = cache;

                    /// <inheritdoc/>
                    public global::System.Threading.Tasks.Task InvalidateAsync(
                        string methodName,
                        global::System.ReadOnlySpan<string> argValues,
                        global::System.Threading.CancellationToken cancellationToken = default)
                    {
                        var _key = argValues.IsEmpty
                            ? $"__{{{m.InterfaceSimpleName}}}__{methodName}"
                            : $"__{{{m.InterfaceSimpleName}}}__{methodName}_" + global::System.String.Join("_", argValues.ToArray());
                        return _cache.RemoveAsync(_key, cancellationToken);
                    }

                    /// <inheritdoc/>
                    public global::System.Threading.Tasks.Task InvalidateAsync(string cacheKey, global::System.Threading.CancellationToken cancellationToken = default)
                        => _cache.RemoveAsync(cacheKey, cancellationToken);

                    /// <inheritdoc/>
                    public global::System.Threading.Tasks.Task InvalidateAllAsync(global::System.Threading.CancellationToken cancellationToken = default)
                        => _cache.RemoveByPrefixAsync(_keyPrefix, cancellationToken);
                }
            """;
    }

    private static string EmitLoggingDecorator(DecoratorModel m)
    {
        var logMethods = string.Join("\n\n        ",
            m.Methods.SelectMany(EmitLoggerMessageDeclarations));
        var methods = string.Join("\n\n        ",
            m.Methods.Select(EmitLoggingMethod));

        return $$$"""
            /// <summary>Source-generated logging decorator for <see cref="global::{{{m.InterfaceFullyQualifiedName}}}"/>.</summary>
                [global::System.CodeDom.Compiler.GeneratedCode("Blazing.DI.Generators", "3.0")]
                internal sealed partial class {{{m.DecoratorClassName}}} : global::{{{m.InterfaceFullyQualifiedName}}}
                {
                    private readonly global::{{{m.InterfaceFullyQualifiedName}}} _inner;
                    private readonly global::Microsoft.Extensions.Logging.ILogger<{{{m.DecoratorClassName}}}> _logger;

                    /// <summary>Initializes a new instance of <see cref="{{{m.DecoratorClassName}}}"/>.</summary>
                    public {{{m.DecoratorClassName}}}(
                        global::{{{m.InterfaceFullyQualifiedName}}} inner,
                        global::Microsoft.Extensions.Logging.ILogger<{{{m.DecoratorClassName}}}> logger)
                    {
                        _inner = inner;
                        _logger = logger;
                    }

                    // [LoggerMessage] partial declarations — bodies completed by Microsoft logging source generator
                    {{{logMethods}}}

                    {{{methods}}}
                }
            """;
    }

    private static IEnumerable<string> EmitLoggerMessageDeclarations(
        MethodModel method)
    {
        var name = method.MethodName;

        // Build structured log token list for the Calling message
        var msgTokens = method.ParameterNames.Length == 0
            ? string.Empty
            : " " + string.Join(" ", method.ParameterNames.Select(p => $"{{{p}}}"));

        yield return $$$"""
            [global::System.CodeDom.Compiler.GeneratedCode("Blazing.DI.Generators", "3.0")]
                    [global::Microsoft.Extensions.Logging.LoggerMessage(Level = global::Microsoft.Extensions.Logging.LogLevel.Debug,
                        Message = "Calling {{{name}}}{{{msgTokens}}}")]
                    private partial void LogCalling{{{name}}}({{{method.LogParameterList}}});
            """;

        yield return $$$"""
            [global::System.CodeDom.Compiler.GeneratedCode("Blazing.DI.Generators", "3.0")]
                    [global::Microsoft.Extensions.Logging.LoggerMessage(Level = global::Microsoft.Extensions.Logging.LogLevel.Debug,
                        Message = "{{{name}}} completed in {ElapsedMs}ms")]
                    private partial void LogCompleted{{{name}}}(global::Microsoft.Extensions.Logging.ILogger<{{{method.LogParameterList.Split(',')[0].Trim().Split('<')[1].TrimEnd('>')}}}> logger, long elapsedMs);
            """;

        yield return $$$"""
            [global::System.CodeDom.Compiler.GeneratedCode("Blazing.DI.Generators", "3.0")]
                    [global::Microsoft.Extensions.Logging.LoggerMessage(Level = global::Microsoft.Extensions.Logging.LogLevel.Error,
                        Message = "{{{name}}} failed after {ElapsedMs}ms: {ErrorMessage}")]
                    private partial void LogFailed{{{name}}}(global::Microsoft.Extensions.Logging.ILogger<{{{method.LogParameterList.Split(',')[0].Trim().Split('<')[1].TrimEnd('>')}}}> logger, long elapsedMs, string errorMessage);
            """;
    }

    private static string EmitLoggingMethod(MethodModel method)
    {
        var name = method.MethodName;
        var awaitKeyword = method.IsAsync ? "await " : string.Empty;
        var returnCapture = method.IsVoid ? string.Empty : "var _result = ";
        var returnStatement = method.IsVoid ? string.Empty : "\n                return _result;";
        var logArgs = method.ArgumentList.Length == 0
            ? string.Empty
            : $", {method.ArgumentList}";

        return $$$"""
            /// <inheritdoc/>
                    public {{{method.ReturnType}}} {{{name}}}({{{method.ParameterList}}})
                    {
                        var _sw = global::System.Diagnostics.Stopwatch.StartNew();
                        try
                        {
                            LogCalling{{{name}}}(_logger{{{logArgs}}});
                            {{{returnCapture}}}{{{awaitKeyword}}}_inner.{{{name}}}({{{method.ArgumentList}}});
                            LogCompleted{{{name}}}(_logger, _sw.ElapsedMilliseconds);{{{returnStatement}}}
                        }
                        catch (global::System.Exception _ex)
                        {
                            LogFailed{{{name}}}(_logger, _sw.ElapsedMilliseconds, _ex.Message);
                            throw;
                        }
                    }
            """;
    }
}
