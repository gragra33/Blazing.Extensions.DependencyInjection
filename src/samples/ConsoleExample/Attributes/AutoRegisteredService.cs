namespace ConsoleExample.Attributes;

/// <summary>
/// Auto-registered demonstration service.
/// This service is registered via the <c>AutoRegister</c> attribute and can be resolved by DI.
/// </summary>
/// <param name="logger">An optional <see cref="ILoggingService"/> instance injected by DI; may be null.</param>
[AutoRegister(ServiceLifetime.Singleton)]
public class AutoRegisteredService(ILoggingService? logger = null)
{
    /// <summary>
    /// Performs the work action for this service.
    /// Logs a message using the injected <see cref="ILoggingService"/> when available and writes to the console.
    /// </summary>
    public void DoWork()
    {
        var message = "AutoRegistered service is working!";
        logger?.Log(message);
        Console.WriteLine(message);
    }
}