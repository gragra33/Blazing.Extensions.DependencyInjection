namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Marks a class for source-generated logging decoration.
/// The Roslyn source generator will emit a concrete <c>Blazing_LoggingDecorator_{ImplName}</c> sealed
/// partial class that wraps every interface method with structured logging via
/// <see cref="Microsoft.Extensions.Logging.LoggerMessage"/> source-generated partial methods.
/// </summary>
/// <remarks>
/// <para>
/// The emitted decorator class is <c>sealed partial</c> so that the Microsoft logging source generator
/// can fill in the <c>[LoggerMessage]</c> partial method bodies — keeping all logging high-performance
/// and AOT-safe with no string interpolation at call sites and no boxing.
/// </para>
/// <para>
/// Three <c>[LoggerMessage]</c> partial methods are emitted per interface method:
/// <list type="bullet">
/// <item><description><c>LogCallingXxx</c> — <see cref="Microsoft.Extensions.Logging.LogLevel.Debug"/>, records parameters</description></item>
/// <item><description><c>LogCompletedXxx</c> — <see cref="Microsoft.Extensions.Logging.LogLevel.Debug"/>, records elapsed milliseconds</description></item>
/// <item><description><c>LogFailedXxx</c> — <see cref="Microsoft.Extensions.Logging.LogLevel.Error"/>, records elapsed milliseconds and error message</description></item>
/// </list>
/// </para>
/// <example>
/// <code>
/// [AutoRegister(ServiceLifetime.Singleton)]
/// [LoggingDecorator]
/// public class OrderService : IOrderService
/// {
///     public Task&lt;Order&gt; CreateOrderAsync(Cart cart) { /* ... */ }
/// }
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class LoggingDecoratorAttribute : Attribute { }
