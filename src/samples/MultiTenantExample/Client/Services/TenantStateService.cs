using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Client.Services;

/// <summary>
/// Service for managing the current tenant state in the client application.
/// Registered using AutoRegister attribute for automatic discovery.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
public sealed class TenantStateService
{
    private Tenant? _currentTenant;

    /// <summary>
    /// Event raised when the current tenant changes.
    /// </summary>
    public event EventHandler? TenantChanged;

    /// <summary>
    /// Gets the current tenant.
    /// </summary>
    public Tenant? CurrentTenant => _currentTenant;

    /// <summary>
    /// Gets the current tenant ID.
    /// </summary>
    public string? CurrentTenantId => _currentTenant?.Id;

    /// <summary>
    /// Sets the current tenant.
    /// </summary>
    /// <param name="tenant">The tenant to set as current.</param>
    public void SetCurrentTenant(Tenant? tenant)
    {
        if (_currentTenant?.Id != tenant?.Id)
        {
            _currentTenant = tenant;
            OnTenantChanged();
        }
    }

    /// <summary>
    /// Clears the current tenant selection.
    /// </summary>
    public void ClearCurrentTenant()
    {
        if (_currentTenant != null)
        {
            _currentTenant = null;
            OnTenantChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether a tenant is currently selected.
    /// </summary>
    public bool HasTenant => _currentTenant != null;

    private void OnTenantChanged()
    {
        TenantChanged?.Invoke(this, EventArgs.Empty);
    }
}
