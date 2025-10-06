namespace WinFormsExample.Services;

/// <summary>
/// Information about a tab view.
/// </summary>
public record TabInfo(Type ViewType, string TabHeader, int Order);