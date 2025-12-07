using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MultiTenantExample.Client.Shared;

/// <summary>
/// Navigation bar component with responsive menu
/// </summary>
public partial class NavBar : ComponentBase, IDisposable
{
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private bool isMenuOpen = false;
    private DotNetObjectReference<NavBar>? dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("registerNavBarEvents", dotNetRef);
            await JSRuntime.InvokeVoidAsync("reinitializeIcons");
        }
    }

    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        StateHasChanged();
    }

    private void CloseMenu()
    {
        if (isMenuOpen)
        {
            isMenuOpen = false;
            StateHasChanged();
        }
    }

    [JSInvokable]
    public void OnOutsideClick()
    {
        CloseMenu();
    }

    [JSInvokable]
    public void OnEscapeKey()
    {
        CloseMenu();
    }

    [JSInvokable]
    public void OnWindowResize(int width)
    {
        if (width > 991 && isMenuOpen)
        {
            CloseMenu();
        }
    }

    public void Dispose()
    {
        dotNetRef?.Dispose();
    }
}
