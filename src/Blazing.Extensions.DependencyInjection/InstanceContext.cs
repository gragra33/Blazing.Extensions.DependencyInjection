namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Record that holds the service provider for an instance.
/// </summary>
/// <param name="ServiceProvider">The service provider for the instance</param>
internal sealed record InstanceContext(IServiceProvider? ServiceProvider);