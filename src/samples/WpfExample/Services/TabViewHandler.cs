using System;
using System.Collections.Generic;
using System.Linq;
using WpfExample.ViewModels;

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

/// <summary>
/// Implementation of ITabViewHandler that uses static metadata.
/// No View instantiation during startup - completely safe for DI resolution.
/// </summary>
public class TabViewHandler : ITabViewHandler
{
    public TabViewHandler()
    {
        // No dependencies needed - uses static metadata
    }

    public IEnumerable<Type> GetAvailableTabViewTypes()
    {
        return TabMetadata.GetTabViewTypes();
    }

    public IEnumerable<TabInfo> GetTabMetadata()
    {
        return TabMetadata.GetAllTabs();
    }
    
    public IEnumerable<TabViewModel> GetTabViewModels()
    {
        var tabViewModels = TabMetadata.GetAllTabs()
            .Select((tab, index) => new TabViewModel(tab.ViewType, tab.Header, tab.Order, isFirstTab: index == 0))
            .OrderBy(vm => vm.Order)
            .ToList();
        
        return tabViewModels;
    }
}