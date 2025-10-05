namespace WpfExample.Services;

/// <summary>
/// Information about a tab view.
/// </summary>
/// <param name="ViewType">The type of the view</param>
/// <param name="Header">The tab header text</param>
/// <param name="Order">The display order</param>
public record TabInfo(Type ViewType, string Header, int Order);