using System.Threading.Tasks;

namespace WinFormsExample.Views;

/// <summary>
/// DataView demonstrates data operations and list management.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ITabView))]
public partial class DataView : UserControl, ITabView
{
    private readonly IDataService _dataService;
    private readonly IDialogService _dialogService;

    /// <summary>
    /// The header text displayed in the tab.
    /// </summary>
    public string TabHeader => "📊 Data";

    /// <summary>
    /// The display order of the tab (lower numbers appear first).
    /// </summary>
    public int Order => 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataView"/> class.
    /// </summary>
    public DataView(IDataService dataService, IDialogService dialogService)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        InitializeComponent();
        SetupEventHandlers();
        _ = LoadDataAsync();
        Console.WriteLine(@"DataView: Constructor called - Services injected successfully");
    }

    private void SetupEventHandlers()
    {
        Console.WriteLine(@"DataView: Setting up event handlers");
        if (dataListBox != null)
            dataListBox.SelectedIndexChanged += DataListBox_SelectedIndexChanged;
        
        if (refreshButton != null)
            refreshButton.Click += RefreshButton_Click;
        
        if (addButton != null)
            addButton.Click += AddButton_Click;
    }

    private async Task LoadDataAsync()
    {
        Console.WriteLine(@"DataView: LoadDataAsync called");
        try
        {
            var data = await _dataService.GetDataAsync();
            var dataItems = data.Select(d => new DataItemDisplay(d)).ToList();
            
            if (dataListBox != null)
            {
                dataListBox.DataSource = dataItems;
                Console.WriteLine($@"DataView: Loaded {dataItems.Count} data items");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"DataView: Error loading data - {ex.Message}");
            _dialogService.ShowMessage("Error", $"Failed to load data: {ex.Message}");
        }
    }

    private void DataListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        Console.WriteLine(@"DataView: DataListBox_SelectedIndexChanged executed!");

        if (dataListBox?.SelectedItem is DataItemDisplay selectedItem && nameTextBox != null && descriptionTextBox != null && isActiveCheckBox != null)
        {
            Console.WriteLine($@"DataView: Selected item - {selectedItem.Data.Name}");
            nameTextBox.Text = selectedItem.Data.Name;
            descriptionTextBox.Text = selectedItem.Data.Description;
            isActiveCheckBox.Checked = selectedItem.Data.IsActive;
        }
        else
        {
            ClearForm();
        }
    }

    private void ClearForm()
    {
        Console.WriteLine(@"DataView: ClearForm called");
        if (nameTextBox != null) nameTextBox.Text = "";
        if (descriptionTextBox != null) descriptionTextBox.Text = "";
        if (isActiveCheckBox != null) isActiveCheckBox.Checked = false;
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine(@"DataView: RefreshButton_Click executed!");
        await LoadDataAsync();
    }

    private async void AddButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine(@"DataView: AddButton_Click executed!");
        if (string.IsNullOrWhiteSpace(nameTextBox?.Text))
        {
            Console.WriteLine(@"DataView: Validation failed - Name is required");
            _dialogService.ShowMessage("Validation Error", "Name is required.");
            return;
        }

        try
        {
            var newItem = new DataItem
            {
                Name = nameTextBox.Text.Trim(),
                Description = descriptionTextBox?.Text?.Trim() ?? "",
                IsActive = isActiveCheckBox?.Checked ?? false
            };

            Console.WriteLine($@"DataView: Adding new item - {newItem.Name}");
            await _dataService.SaveDataAsync(newItem);
            ClearForm();
            await LoadDataAsync();
            _dialogService.ShowMessage("Success", "Item added successfully!");
            Console.WriteLine(@"DataView: Item added successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($@"DataView: Error adding item - {ex.Message}");
            _dialogService.ShowMessage("Error", $"Failed to add item: {ex.Message}");
        }
    }
}