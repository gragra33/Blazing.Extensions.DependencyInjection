using MultiTenantExample.Shared.DTOs;
using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Client.Services;

/// <summary>
/// Service for making API calls to the tenant endpoints.
/// Registered using AutoRegister attribute for automatic discovery.
/// Changed to Scoped to match HttpClient's lifetime in Blazor WASM.
/// </summary>
[AutoRegister(ServiceLifetime.Scoped)]
public sealed class TenantApiService
{
    private readonly HttpClient _httpClient;
    private readonly TenantStateService _tenantState;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantApiService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="tenantState">The tenant state service.</param>
    public TenantApiService(HttpClient httpClient, TenantStateService tenantState)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _tenantState = tenantState ?? throw new ArgumentNullException(nameof(tenantState));
    }

    /// <summary>
    /// Gets all tenants from the API.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>API response containing all tenants.</returns>
    public async Task<ApiResponse<IEnumerable<Tenant>>?> GetAllTenantsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var requestUri = "api/tenants";
            Console.WriteLine($"Requesting: {_httpClient.BaseAddress}{requestUri}");
            
            var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            
            Console.WriteLine($"Response Status: {response.StatusCode}");
            Console.WriteLine($"Response Headers: {response.Headers}");
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Error Response Content: {content}");
                return ApiResponse<IEnumerable<Tenant>>.FailureResponse($"HTTP {response.StatusCode}: {content}");
            }
            
            return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<Tenant>>>(
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tenants: {ex.Message}");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Gets a specific tenant by ID.
    /// </summary>
    /// <param name="tenantId">The tenant identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>API response containing the tenant.</returns>
    public async Task<ApiResponse<Tenant>?> GetTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ApiResponse<Tenant>>(
                $"api/tenants/{tenantId}",
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tenant: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets all orders for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>API response containing orders.</returns>
    public async Task<ApiResponse<IEnumerable<Order>>?> GetOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        if (_tenantState.CurrentTenantId == null)
        {
            return ApiResponse<IEnumerable<Order>>.FailureResponse("No tenant selected");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/orders");
            request.Headers.Add("X-Tenant-Id", _tenantState.CurrentTenantId);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<Order>>>(
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting orders: {ex.Message}");
            return ApiResponse<IEnumerable<Order>>.FailureResponse(ex.Message, _tenantState.CurrentTenantId);
        }
    }

    /// <summary>
    /// Gets a specific order by ID for the current tenant.
    /// </summary>
    /// <param name="orderId">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>API response containing the order.</returns>
    public async Task<ApiResponse<Order>?> GetOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        if (_tenantState.CurrentTenantId == null)
        {
            return ApiResponse<Order>.FailureResponse("No tenant selected");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/orders/{orderId}");
            request.Headers.Add("X-Tenant-Id", _tenantState.CurrentTenantId);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<Order>>(
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting order: {ex.Message}");
            return ApiResponse<Order>.FailureResponse(ex.Message, _tenantState.CurrentTenantId);
        }
    }

    /// <summary>
    /// Gets all products for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>API response containing products.</returns>
    public async Task<ApiResponse<IEnumerable<Product>>?> GetProductsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_tenantState.CurrentTenantId == null)
        {
            return ApiResponse<IEnumerable<Product>>.FailureResponse("No tenant selected");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "api/products");
            request.Headers.Add("X-Tenant-Id", _tenantState.CurrentTenantId);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<Product>>>(
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting products: {ex.Message}");
            return ApiResponse<IEnumerable<Product>>.FailureResponse(ex.Message, _tenantState.CurrentTenantId);
        }
    }

    /// <summary>
    /// Gets a specific product by ID for the current tenant.
    /// </summary>
    /// <param name="productId">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>API response containing the product.</returns>
    public async Task<ApiResponse<Product>?> GetProductAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        if (_tenantState.CurrentTenantId == null)
        {
            return ApiResponse<Product>.FailureResponse("No tenant selected");
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, $"api/products/{productId}");
            request.Headers.Add("X-Tenant-Id", _tenantState.CurrentTenantId);

            var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ApiResponse<Product>>(
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting product: {ex.Message}");
            return ApiResponse<Product>.FailureResponse(ex.Message, _tenantState.CurrentTenantId);
        }
    }

    /// <summary>
    /// Gets the configuration for the current tenant.
    /// This demonstrates lazy initialization - the configuration is loaded on first access.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>API response containing the tenant configuration.</returns>
    public async Task<ApiResponse<Dictionary<string, object>>?> GetTenantConfigurationAsync(
        CancellationToken cancellationToken = default)
    {
        if (_tenantState.CurrentTenantId == null)
        {
            return ApiResponse<Dictionary<string, object>>.FailureResponse("No tenant selected");
        }

        try
        {
            var requestUri = $"api/tenants/{_tenantState.CurrentTenantId}/configuration";
            Console.WriteLine($"Requesting configuration: {_httpClient.BaseAddress}{requestUri}");
            
            var response = await _httpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                Console.WriteLine($"Error Response: {content}");
                return ApiResponse<Dictionary<string, object>>.FailureResponse($"HTTP {response.StatusCode}: {content}", _tenantState.CurrentTenantId);
            }
            
            return await response.Content.ReadFromJsonAsync<ApiResponse<Dictionary<string, object>>>(
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting tenant configuration: {ex.Message}");
            return ApiResponse<Dictionary<string, object>>.FailureResponse(ex.Message, _tenantState.CurrentTenantId);
        }
    }
}
