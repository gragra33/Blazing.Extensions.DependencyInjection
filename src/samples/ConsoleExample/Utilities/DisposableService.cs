namespace ConsoleExample.Utilities;

public class DisposableService : IDisposable
{
    public bool IsDisposed { get; private set; }
    
    public void Dispose()
    {
        IsDisposed = true;
        Console.WriteLine("DisposableService disposed");
    }
}