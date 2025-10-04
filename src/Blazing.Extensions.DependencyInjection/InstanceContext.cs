using System.Reflection;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Record that holds both the service provider and assemblies for an instance.
/// </summary>
/// <param name="ServiceProvider">The service provider for the instance</param>
/// <param name="Assemblies">The assemblies to scan for auto-discovery</param>
internal sealed record InstanceContext(IServiceProvider? ServiceProvider, HashSet<Assembly> Assemblies);