namespace WinFormsExample.Views;

/// <summary>
/// Display wrapper for DataItem to provide proper ListBox display text.
/// </summary>
internal class DataItemDisplay
{
    public DataItem Data { get; }
    public string DisplayText => $"{Data.Name} ({(Data.IsActive ? "Active" : "Inactive")})";

    public DataItemDisplay(DataItem data)
    {
        Data = data;
    }

    public override string ToString()
    {
        return DisplayText;
    }
}