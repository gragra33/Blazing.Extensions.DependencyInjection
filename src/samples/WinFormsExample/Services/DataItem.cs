namespace WinFormsExample.Services;

/// <summary>
/// Represents a data item for demonstration purposes.
/// </summary>
public class DataItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}