namespace WpfExample.Services;

/// <summary>
/// Service for discovering and managing tab views.
/// Provides complete decoupling - MainViewModel doesn't need to know about specific View types.
/// </summary>
public interface ITabViewHandler
{
    /// <summary>
    /// Gets the available tab view types in the correct order.
    /// </summary>
    IEnumerable<Type> GetAvailableTabViewTypes();

    /// <summary>
    /// Gets metadata for all available tab views.
    /// </summary>
    IEnumerable<TabInfo> GetTabMetadata();
    
    /// <summary>
    /// Gets TabViewModel instances that can be safely bound in XAML without converters.
    /// </summary>
    IEnumerable<TabViewModel> GetTabViewModels();
}