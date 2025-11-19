namespace ConsoleExample.Data;

/// <summary>
/// Provides integer data for demonstration purposes.
/// </summary>
public class IntegerDataProvider : IDataProvider<int>
{
    /// <inheritdoc/>
    public int GetData() => 42;

    /// <inheritdoc/>
    public string GetProviderType() => "Integer Provider";
}