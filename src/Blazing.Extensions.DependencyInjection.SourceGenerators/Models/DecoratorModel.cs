namespace Blazing.Extensions.DependencyInjection.SourceGenerators.Models;

/// <summary>
/// Immutable data record representing the data needed to emit one decorator class.
/// </summary>
/// <param name="ImplFullyQualifiedName">Fully-qualified name of the concrete implementation (e.g. <c>MyApp.Services.ProductService</c>).</param>
/// <param name="InterfaceFullyQualifiedName">Fully-qualified name of the interface the decorator implements.</param>
/// <param name="DecoratorKind">Decorator kind: <c>"Caching"</c> or <c>"Logging"</c>.</param>
/// <param name="CacheSeconds">Cache duration in seconds. Relevant only when <paramref name="DecoratorKind"/> is <c>"Caching"</c>.</param>
/// <param name="Methods">Methods of the interface that the decorator must implement.</param>
internal sealed record DecoratorModel(
    string ImplFullyQualifiedName,
    string InterfaceFullyQualifiedName,
    string DecoratorKind,
    int CacheSeconds,
    MethodModel[] Methods
)
{
    /// <summary>Gets the generated class name for this decorator.</summary>
    public string DecoratorClassName => $"Blazing_{DecoratorKind}Decorator_{SimpleImplName}";

    /// <summary>Gets the generated invalidator class name for this caching decorator.</summary>
    public string InvalidatorClassName => $"Blazing_CacheInvalidator_{SimpleImplName}";

    /// <summary>Gets the simple (unqualified) name of the decorated interface.</summary>
    public string InterfaceSimpleName => InterfaceFullyQualifiedName.Contains('.')
        ? InterfaceFullyQualifiedName.Substring(InterfaceFullyQualifiedName.LastIndexOf('.') + 1)
        : InterfaceFullyQualifiedName;

    private string SimpleImplName => ImplFullyQualifiedName.Contains('.')
        ? ImplFullyQualifiedName.Substring(ImplFullyQualifiedName.LastIndexOf('.') + 1)
        : ImplFullyQualifiedName;
}

/// <summary>
/// Immutable data record representing one method to be wrapped or delegated by a generated decorator.
/// </summary>
/// <param name="ReturnType">Display string for the return type (e.g. <c>"void"</c>, <c>"Task"</c>, <c>"Task&lt;string&gt;"</c>).</param>
/// <param name="MethodName">Name of the method (e.g. <c>"GetById"</c>).</param>
/// <param name="ParameterList">Formatted parameter declaration list (e.g. <c>"int id, string name"</c>).</param>
/// <param name="ArgumentList">Formatted argument pass-through list (e.g. <c>"id, name"</c>).</param>
/// <param name="ParameterNames">Individual parameter names used as structured logging message tokens.</param>
/// <param name="LogParameterList">Parameter list for the <c>[LoggerMessage]</c> partial calling-log declaration.</param>
/// <param name="IsAsync"><see langword="true"/> when the return type is <c>Task</c>, <c>ValueTask</c>, or a generic async variant.</param>
/// <param name="IsVoid"><see langword="true"/> when the return type is <c>void</c> or non-generic <c>Task</c> / <c>ValueTask</c>.</param>
/// <param name="AsyncResultType">
/// Fully-qualified type argument of <c>Task&lt;T&gt;</c> or <c>ValueTask&lt;T&gt;</c> when
/// <paramref name="IsAsync"/> is <see langword="true"/> and <paramref name="IsVoid"/> is
/// <see langword="false"/>; otherwise <see langword="null"/>.
/// </param>
internal sealed record MethodModel(
    string ReturnType,
    string MethodName,
    string ParameterList,
    string ArgumentList,
    string[] ParameterNames,
    string LogParameterList,
    bool IsAsync,
    bool IsVoid,
    string? AsyncResultType
);
