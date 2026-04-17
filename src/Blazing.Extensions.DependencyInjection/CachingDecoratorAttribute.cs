namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Marks a class for source-generated caching decoration.
/// The Roslyn source generator will emit a concrete <c>Blazing_CachingDecorator_{ImplName}</c> class
/// that caches synchronous and async (<c>Task&lt;T&gt;</c> / <c>ValueTask&lt;T&gt;</c>) method results
/// in an in-memory dictionary for the configured duration.
/// </summary>
/// <remarks>
/// <para>
/// Apply together with <see cref="AutoRegisterAttribute"/> so that the generator also wires up the
/// decorated service in the emitted <c>Register()</c> extension method.
/// </para>
/// <para>
/// Synchronous methods that return a value (<c>T</c>) are cached with a
/// <c>lock (_cache)</c> block.
/// </para>
/// <para>
/// Async methods that return <c>Task&lt;T&gt;</c> or <c>ValueTask&lt;T&gt;</c> are also cached.
/// An <see cref="AsyncLock"/> provides single-flight semantics: concurrent cold-cache callers queue
/// on a <see cref="System.Threading.SemaphoreSlim"/>, only the first calls the inner service,
/// and subsequent callers return the already-cached result.
/// The awaited value (<c>T</c>) is stored — never the <c>Task</c> or <c>ValueTask</c> itself.
/// </para>
/// <para>
/// Void methods, non-generic <c>Task</c>, and non-generic <c>ValueTask</c> are always
/// delegated directly — they produce no cacheable value.
/// </para>
/// <example>
/// <code>
/// [AutoRegister(ServiceLifetime.Singleton)]
/// [CachingDecorator(seconds: 120)]
/// public class ProductService : IProductService
/// {
///     public Product GetById(int id) { /* hits database */ }
/// }
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CachingDecoratorAttribute : Attribute
{
    /// <summary>
    /// Gets how long to cache results, in seconds.
    /// </summary>
    public int Seconds { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CachingDecoratorAttribute"/>.
    /// </summary>
    /// <param name="seconds">How long to cache results, in seconds. Default is 300 seconds (5 minutes).</param>
    public CachingDecoratorAttribute(int seconds = 300) => Seconds = seconds;
}
