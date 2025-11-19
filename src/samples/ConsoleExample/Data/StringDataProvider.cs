namespace ConsoleExample.Data;

/// <summary>
/// Provides string data for demonstration purposes.
/// </summary>
public class StringDataProvider : IDataProvider<string>
{
    /// <inheritdoc/>
    public string GetData() => "Sample string data";

    /// <inheritdoc/>
    public string GetProviderType() => "String Provider";
}