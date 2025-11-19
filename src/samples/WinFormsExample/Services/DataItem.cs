namespace WinFormsExample.Services;

/// <summary>
/// Represents a data item for demonstration purposes.
/// </summary>
public class DataItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the data item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the data item.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the description of the data item.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date and time when the data item was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the data item is active.
    /// </summary>
    public bool IsActive { get; set; }
}