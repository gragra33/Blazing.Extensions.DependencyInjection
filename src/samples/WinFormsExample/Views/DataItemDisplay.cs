namespace WinFormsExample.Views;

/// <summary>
/// Display wrapper for <see cref="DataItem"/> to provide proper ListBox display text.
/// </summary>
internal class DataItemDisplay
{
    /// <summary>
    /// Gets the underlying <see cref="DataItem"/> instance.
    /// </summary>
    public DataItem Data { get; }

    /// <summary>
    /// Gets the display text for the ListBox, showing name and active status.
    /// </summary>
    public string DisplayText => $"{Data.Name} ({(Data.IsActive ? "Active" : "Inactive")})";

    /// <summary>
    /// Initializes a new instance of the <see cref="DataItemDisplay"/> class.
    /// </summary>
    /// <param name="data">The <see cref="DataItem"/> to wrap for display.</param>
    public DataItemDisplay(DataItem data)
    {
        Data = data;
    }

    /// <summary>
    /// Returns the display text for the ListBox.
    /// </summary>
    /// <returns>The formatted display text.</returns>
    public override string ToString()
    {
        return DisplayText;
    }
}