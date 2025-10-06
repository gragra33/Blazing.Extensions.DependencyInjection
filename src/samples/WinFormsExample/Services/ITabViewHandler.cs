namespace WinFormsExample.Services;

/// <summary>
/// Interface for handling tab view discovery and management.
/// </summary>
public interface ITabViewHandler
{
    /// <summary>
    /// Gets all available tab view types.
    /// </summary>
    IEnumerable<Type> GetAvailableTabViewTypes();
    
    /// <summary>
    /// Gets metadata for all tab views.
    /// </summary>
    IEnumerable<TabInfo> GetTabMetadata();
}