namespace ConsoleExample.Processing;

public class IntegerProcessor : IGenericProcessor<int>
{
    public string Process(int data) => $"Processed integer: {data} (squared: {data * data})";
}