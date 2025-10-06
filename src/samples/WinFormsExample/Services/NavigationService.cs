namespace WinFormsExample.Services;

/// <summary>
/// Implementation of INavigationService for WinForms TabControl.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton, typeof(INavigationService))]
public class NavigationService : INavigationService
{
    public int CurrentTabIndex { get; private set; }

    public event EventHandler<int>? NavigationChanged;

    public void NavigateTo(int tabIndex)
    {
        if (CurrentTabIndex != tabIndex)
        {
            CurrentTabIndex = tabIndex;
            NavigationChanged?.Invoke(this, tabIndex);
        }
    }
}