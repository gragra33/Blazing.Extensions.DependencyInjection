namespace WpfExample.Services;

/// <summary>
/// Implementation of ITabViewHandler that uses dynamic discovery.
/// Discovers tab views automatically via dependency injection.
/// </summary>
public class TabViewHandler : ITabViewHandler
{
    public IEnumerable<Type> GetAvailableTabViewTypes()
    {
        // Get all registered ITabView services and return their types
        var serviceProvider = Application.Current.GetServices();
        var tabViews = serviceProvider.GetServices<ITabView>();
        return tabViews.Select(view => view.GetType()).OrderBy(GetTabOrder);
    }

    public IEnumerable<TabInfo> GetTabMetadata()
    {
        // Get all registered ITabView services and create TabInfo records
        var serviceProvider = Application.Current.GetServices();
        var tabViews = serviceProvider.GetServices<ITabView>();
        return tabViews
            .Select(view => new TabInfo(view.GetType(), view.TabHeader, view.Order))
            .OrderBy(tab => tab.Order);
    }
    
    public IEnumerable<TabViewModel> GetTabViewModels()
    {
        // Get all registered ITabView services and create TabViewModel wrappers
        var serviceProvider = Application.Current.GetServices();
        var tabViews = serviceProvider.GetServices<ITabView>();
        var tabViewModels = tabViews
            .Select(view => new TabViewModel(view.GetType(), view.TabHeader, view.Order))
            .OrderBy(vm => vm.Order)
            .ToList();
        
        return tabViewModels;
    }

    /// <summary>
    /// Gets the order of a tab view type by creating an instance and checking its Order property.
    /// Used for sorting when we only have the Type.
    /// </summary>
    private int GetTabOrder(Type viewType)
    {
        try
        {
            var serviceProvider = Application.Current.GetServices();
            var view = serviceProvider.GetRequiredService(viewType) as ITabView;
            return view?.Order ?? int.MaxValue;
        }
        catch
        {
            return int.MaxValue;
        }
    }
}