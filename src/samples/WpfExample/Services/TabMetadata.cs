using System;
using System.Collections.Generic;
using System.Linq;
using WpfExample.Views;

namespace WpfExample.Services;

/// <summary>
/// Static tab metadata - completely avoids View instantiation during startup.
/// This ensures zero dependency resolution during app startup.
/// </summary>
public static class TabMetadata
{
    /// <summary>
    /// Static tab definitions - no View instantiation required!
    /// </summary>
    private static readonly TabInfo[] StaticTabDefinitions = 
    {
        new(typeof(HomeView), "🏠 Home", 1),
        new(typeof(WeatherView), "🌤️ Weather", 2), 
        new(typeof(DataView), "📊 Data", 3),
        new(typeof(SettingsView), "⚙️ Settings", 4)
    };

    /// <summary>
    /// Gets all tab metadata in the correct order without any View instantiation.
    /// </summary>
    public static IEnumerable<TabInfo> GetAllTabs()
    {
        return StaticTabDefinitions.OrderBy(tab => tab.Order).ToList();
    }

    /// <summary>
    /// Gets tab view types in the correct order.
    /// </summary>
    public static IEnumerable<Type> GetTabViewTypes()
    {
        return GetAllTabs().Select(tab => tab.ViewType);
    }

    /// <summary>
    /// Gets the tab header for a specific view type without instantiation.
    /// </summary>
    public static string GetTabHeader(Type viewType)
    {
        var tabInfo = StaticTabDefinitions.FirstOrDefault(tab => tab.ViewType == viewType);
        return tabInfo?.Header ?? viewType.Name;
    }
}