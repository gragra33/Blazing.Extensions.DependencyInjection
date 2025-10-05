namespace ConsoleExample.Processing;

public class DecimalProcessor : IGenericProcessor<decimal>
{
    public string Process(decimal data) => $"Processed decimal: {data:F4} (rounded: {Math.Round(data, 2)})";
}