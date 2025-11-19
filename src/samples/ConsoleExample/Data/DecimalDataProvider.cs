namespace ConsoleExample.Data;

/// <summary>
/// Provides decimal data for demonstration purposes.
/// </summary>
public class DecimalDataProvider : IDataProvider<decimal>
{
    /// <inheritdoc/>
    public decimal GetData() => 3.14159m;

    /// <inheritdoc/>
    public string GetProviderType() => "Decimal Provider";
}