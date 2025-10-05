namespace ConsoleExample.Attributes;

[AutoRegister(ServiceLifetime.Singleton)]
public class AutoRegisteredService(ILoggingService? logger = null)
{
    public void DoWork()
    {
        var message = "AutoRegistered service is working!";
        logger?.Log(message);
        Console.WriteLine(message);
    }
}