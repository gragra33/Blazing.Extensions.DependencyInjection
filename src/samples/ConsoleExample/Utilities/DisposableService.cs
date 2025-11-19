namespace ConsoleExample.Utilities;

/// <summary>
/// Simple disposable service used by examples to demonstrate disposal and clearing behavior.
/// </summary>
public class DisposableService : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the service has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }
    
    /// <summary>
    /// Disposes the service and sets the <see cref="IsDisposed"/> flag.
    /// </summary>
    public void Dispose()
    {
        IsDisposed = true;
        Console.WriteLine("DisposableService disposed");
    }
}