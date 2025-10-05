namespace ConsoleExample.Processing;

public interface IGenericProcessor<T> where T : notnull
{
    string Process(T data);
}