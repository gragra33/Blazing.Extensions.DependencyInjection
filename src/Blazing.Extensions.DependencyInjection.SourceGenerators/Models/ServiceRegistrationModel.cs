namespace Blazing.Extensions.DependencyInjection.SourceGenerators.Models;

/// <summary>
/// Immutable data record representing a single service registration discovered by <c>[AutoRegister]</c>.
/// </summary>
/// <param name="ImplFullyQualifiedName">Fully-qualified name of the concrete implementation type (e.g. <c>MyApp.Services.DataService</c>).</param>
/// <param name="InterfaceFullyQualifiedNames">Fully-qualified names of interface types to register the service as. Empty array means self-registration only.</param>
/// <param name="Lifetime">Service lifetime: <c>"Singleton"</c>, <c>"Scoped"</c>, or <c>"Transient"</c>.</param>
/// <param name="Key">When non-<see langword="null"/>, emits <c>AddKeyed{Lifetime}</c> with this key instead of <c>Add{Lifetime}</c>.</param>
internal sealed record ServiceRegistrationModel(
    string ImplFullyQualifiedName,
    string[] InterfaceFullyQualifiedNames,
    string Lifetime,
    string? Key
);
