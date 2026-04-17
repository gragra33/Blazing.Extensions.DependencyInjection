using Blazing.Extensions.DependencyInjection.SourceGenerators.Models;
using Microsoft.CodeAnalysis;

namespace Blazing.Extensions.DependencyInjection.SourceGenerators.Helpers;

/// <summary>
/// Converts an <see cref="IMethodSymbol"/> into a <see cref="MethodModel"/> suitable for
/// code emission by the decorator source generators.
/// </summary>
internal static class MethodEmitHelper
{
    /// <summary>
    /// Builds a <see cref="MethodModel"/> from the given method symbol.
    /// </summary>
    /// <param name="method">The interface method symbol to model.</param>
    /// <param name="decoratorClassName">
    /// The name of the decorator class that will declare the <c>[LoggerMessage]</c> partials.
    /// Used to construct the <see cref="MethodModel.LogParameterList"/>.
    /// </param>
    /// <returns>A fully-populated <see cref="MethodModel"/>.</returns>
    public static MethodModel BuildMethodModel(IMethodSymbol method, string decoratorClassName)
    {
        var returnType = method.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isAsync = IsAsyncReturnType(method.ReturnType);
        var isVoid = IsVoidReturnType(method.ReturnType);

        string? asyncResultType = null;
        if (isAsync && !isVoid && method.ReturnType is INamedTypeSymbol { IsGenericType: true } namedReturn)
            asyncResultType = namedReturn.TypeArguments[0]
                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        var paramCount = method.Parameters.Length;
        var paramNames = new string[paramCount];
        var paramDecls = new string[paramCount];
        var argDecls = new string[paramCount];
        var logParamDecls = new string[paramCount];

        for (var i = 0; i < paramCount; i++)
        {
            var p = method.Parameters[i];
            var typeName = p.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var name = p.Name;

            paramNames[i] = name;

            var modifier = p.RefKind switch
            {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => string.Empty,
            };

            var isParams = p.IsParams ? "params " : string.Empty;
            paramDecls[i] = $"{isParams}{modifier}{typeName} {name}";

            argDecls[i] = p.RefKind switch
            {
                RefKind.Ref => $"ref {name}",
                RefKind.Out => $"out {name}",
                RefKind.In => $"in {name}",
                _ => name,
            };

            logParamDecls[i] = $"{typeName} {name}";
        }

        var logSuffix = paramCount > 0
            ? ", " + string.Join(", ", logParamDecls)
            : string.Empty;

        var logParameterList =
            $"global::Microsoft.Extensions.Logging.ILogger<{decoratorClassName}> logger{logSuffix}";

        return new MethodModel(
            ReturnType: returnType,
            MethodName: method.Name,
            ParameterList: string.Join(", ", paramDecls),
            ArgumentList: string.Join(", ", argDecls),
            ParameterNames: paramNames,
            LogParameterList: logParameterList,
            IsAsync: isAsync,
            IsVoid: isVoid,
            AsyncResultType: asyncResultType);
    }

    private static bool IsAsyncReturnType(ITypeSymbol returnType)
        => returnType.Name is "Task" or "ValueTask";

    private static bool IsVoidReturnType(ITypeSymbol returnType)
    {
        if (returnType.SpecialType == SpecialType.System_Void)
            return true;

        // Non-generic Task or ValueTask — produces no value
        return returnType is INamedTypeSymbol { IsGenericType: false, Name: "Task" or "ValueTask" };
    }
}
