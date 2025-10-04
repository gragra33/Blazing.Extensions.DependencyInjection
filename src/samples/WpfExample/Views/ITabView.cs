namespace WpfExample.Views;

/// <summary>
/// Interface for views that can be displayed as tabs.
/// Provides complete decoupling from MainViewModel - tabs are discovered automatically.
/// </summary>
public interface ITabView
{
    /// <summary>
    /// The header text displayed in the tab.
    /// </summary>
    string TabHeader { get; }
    
    /// <summary>
    /// The display order of the tab (lower numbers appear first).
    /// </summary>
    int Order { get; }
}