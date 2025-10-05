using System.Collections.ObjectModel;

namespace WpfExample.ViewModels;

/// <summary>
/// DataViewModel demonstrates data management operations with IDataService dependency injection.
/// Shows how tab ViewModels can manage their own data collections independently.
/// </summary>
public partial class DataViewModel : ViewModelBase
{
    private readonly IDataService _dataService;
    private ObservableCollection<string> _dataItems = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataViewModel"/> class.
    /// Automatically loads initial data from the data service.
    /// </summary>
    /// <param name="dataService">Service for data management operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when dataService is null.</exception>
    public DataViewModel(IDataService dataService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        LoadData();
    }

    /// <summary>
    /// Gets or sets the collection of data items displayed in the UI.
    /// Bound to ListBox or other collection controls.
    /// </summary>
    public ObservableCollection<string> DataItems
    {
        get => _dataItems;
        set => SetProperty(ref _dataItems, value);
    }

    /// <summary>
    /// Loads all data items from the data service and updates the UI collection.
    /// Demonstrates service method invocation and data binding updates.
    /// </summary>
    [RelayCommand]
    private void LoadData()
    {
        Console.WriteLine("DataViewModel: LoadData command executed!");
        var items = _dataService.GetAllData();
        DataItems = new ObservableCollection<string>(items);
    }

    /// <summary>
    /// Adds a new data item with current timestamp to the collection.
    /// Demonstrates data modification through service layer.
    /// </summary>
    [RelayCommand]
    private void AddDataItem()
    {
        Console.WriteLine("DataViewModel: AddDataItem command executed!");
        var itemName = $"Item {DateTime.Now:HH:mm:ss}";
        _dataService.AddData(itemName);
        LoadData();
    }

    /// <summary>
    /// Clears all data items from the collection.
    /// Demonstrates bulk operations through service layer.
    /// </summary>
    [RelayCommand]
    private void ClearData()
    {
        Console.WriteLine("DataViewModel: ClearData command executed!");
        _dataService.ClearData();
        LoadData();
    }
}
