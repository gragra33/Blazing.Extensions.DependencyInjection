namespace ConsoleExample.Data;

public class StringDataProvider : IDataProvider<string>
{
    public string GetData() => "Sample string data";
    public string GetProviderType() => "String Provider";
}