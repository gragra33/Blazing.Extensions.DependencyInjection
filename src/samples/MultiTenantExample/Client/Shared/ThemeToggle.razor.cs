using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace MultiTenantExample.Client.Shared;

/// <summary>
/// Theme toggle component for switching between light and dark modes
/// </summary>
public partial class ThemeToggle : ComponentBase
{
    [Inject] protected IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Initialize theme on component load
            await JSRuntime.InvokeVoidAsync("initializeTheme");
        }
    }

    private async Task ToggleTheme()
    {
        await JSRuntime.InvokeVoidAsync("toggleTheme");
    }
}
