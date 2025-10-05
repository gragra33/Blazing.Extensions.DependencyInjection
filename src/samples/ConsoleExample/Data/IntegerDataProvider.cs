namespace ConsoleExample.Data;

public class IntegerDataProvider : IDataProvider<int>
{
    public int GetData() => 42;
    public string GetProviderType() => "Integer Provider";
}