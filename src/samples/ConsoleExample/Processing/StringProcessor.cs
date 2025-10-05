namespace ConsoleExample.Processing;

public class StringProcessor : IGenericProcessor<string>
{
    public string Process(string data) => $"Processed string: '{data}' (length: {data.Length})";
}