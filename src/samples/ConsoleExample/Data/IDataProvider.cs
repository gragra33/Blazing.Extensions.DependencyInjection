namespace ConsoleExample.Data;

public interface IDataProvider<T>
{
    T GetData();
    string GetProviderType();
}