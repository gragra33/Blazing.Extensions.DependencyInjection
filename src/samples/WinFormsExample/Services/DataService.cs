using System.Threading.Tasks;

namespace WinFormsExample.Services;

/// <summary>
/// Implementation of IDataService with mock data.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(IDataService))]
public class DataService : IDataService
{
    private readonly List<DataItem> _data =
    [
        new DataItem
        {
            Id = 1, Name = "Sample Item 1", Description = "First sample item", CreatedDate = DateTime.Now.AddDays(-5),
            IsActive = true
        },
        new DataItem
        {
            Id = 2, Name = "Sample Item 2", Description = "Second sample item", CreatedDate = DateTime.Now.AddDays(-3),
            IsActive = false
        },
        new DataItem
        {
            Id = 3, Name = "Sample Item 3", Description = "Third sample item", CreatedDate = DateTime.Now.AddDays(-1),
            IsActive = true
        }
    ];

    public DataService()
    {
        Console.WriteLine(@"DataService: Constructor called - Service registered as singleton");
        Console.WriteLine($@"DataService: Initialized with {_data.Count} sample data items");
    }

    public async Task<IEnumerable<DataItem>> GetDataAsync()
    {
        Console.WriteLine(@"DataService: GetDataAsync called");
        // Simulate async operation
        Console.WriteLine(@"DataService: Simulating database query delay (100ms)...");
        await Task.Delay(100);
        
        var result = _data.ToList();
        Console.WriteLine($@"DataService: Returning {result.Count} data items");
        foreach (var item in result)
        {
            Console.WriteLine($@"  - {item.Name} (ID: {item.Id}, Active: {item.IsActive})");
        }
        
        return result;
    }

    public async Task SaveDataAsync(DataItem item)
    {
        Console.WriteLine($@"DataService: SaveDataAsync called for item: {item.Name}");
        await Task.Delay(50);
        
        var existing = _data.FirstOrDefault(d => d.Id == item.Id);
        if (existing != null)
        {
            Console.WriteLine($@"DataService: Updating existing item (ID: {item.Id})");
            existing.Name = item.Name;
            existing.Description = item.Description;
            existing.IsActive = item.IsActive;
            Console.WriteLine(@"DataService: Item updated successfully");
        }
        else
        {
            item.Id = _data.Count > 0 ? _data.Max(d => d.Id) + 1 : 1;
            item.CreatedDate = DateTime.Now;
            _data.Add(item);
            Console.WriteLine($@"DataService: Added new item with ID: {item.Id}");
        }

        Console.WriteLine($@"DataService: Total items in collection: {_data.Count}");
    }

    public async Task DeleteDataAsync(int id)
    {
        Console.WriteLine($@"DataService: DeleteDataAsync called for ID: {id}");
        await Task.Delay(50);
        
        var item = _data.FirstOrDefault(d => d.Id == id);
        if (item != null)
        {
            _data.Remove(item);
            Console.WriteLine($@"DataService: Deleted item '{item.Name}' (ID: {id})");
            Console.WriteLine($@"DataService: Remaining items in collection: {_data.Count}");
        }
        else
        {
            Console.WriteLine($@"DataService: Item with ID {id} not found - deletion skipped");
        }
    }
}