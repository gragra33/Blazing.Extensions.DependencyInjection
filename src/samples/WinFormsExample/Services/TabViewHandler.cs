namespace WinFormsExample.Services;

/// <summary>
/// Implementation of ITabViewHandler that uses dynamic discovery.
/// Discovers tab views automatically via dependency injection.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(ITabViewHandler))]
public class TabViewHandler : ITabViewHandler
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="TabViewHandler"/> class.
    /// </summary>
    public TabViewHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public IEnumerable<Type> GetAvailableTabViewTypes()
    {
        // Get all registered ITabView services and return their types
        var tabViews = _serviceProvider.GetServices<ITabView>();
        return tabViews.Select(view => view.GetType()).OrderBy(GetTabOrder);
    }

    public IEnumerable<TabInfo> GetTabMetadata()
    {
        // Get all registered ITabView services and create TabInfo records
        var tabViews = _serviceProvider.GetServices<ITabView>();
        return tabViews
            .Select(view => new TabInfo(view.GetType(), view.TabHeader, view.Order))
            .OrderBy(tab => tab.Order);
    }
    
    /// <summary>
    /// Gets the order of a tab view type by creating an instance and checking its Order property.
    /// This is used for sorting when we only have the Type.
    /// </summary>
    private int GetTabOrder(Type viewType)
    {
        try
        {
            var instance = _serviceProvider.GetService(viewType) as ITabView;
            return instance?.Order ?? int.MaxValue;
        }
        catch
        {
            return int.MaxValue;
        }
    }
}