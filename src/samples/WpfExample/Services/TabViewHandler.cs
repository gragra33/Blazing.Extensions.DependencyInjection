using System;
using System.Collections.Generic;
using System.Linq;
using WpfExample.ViewModels;

namespace WpfExample.Services;

/// <summary>
/// Implementation of ITabViewHandler that uses static metadata.
/// No View instantiation during startup - completely safe for DI resolution.
/// </summary>
public class TabViewHandler : ITabViewHandler
{
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
            .Select((tab, index) => new TabViewModel(tab.ViewType, tab.Header, tab.Order))
            .OrderBy(vm => vm.Order)
            .ToList();
        
        return tabViewModels;
    }
}