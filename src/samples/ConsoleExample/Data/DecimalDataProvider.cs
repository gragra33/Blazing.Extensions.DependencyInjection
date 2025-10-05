namespace ConsoleExample.Data;

public class DecimalDataProvider : IDataProvider<decimal>
{
    public decimal GetData() => 3.14159m;
    public string GetProviderType() => "Decimal Provider";
}