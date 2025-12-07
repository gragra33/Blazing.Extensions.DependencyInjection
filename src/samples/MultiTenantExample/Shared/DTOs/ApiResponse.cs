namespace MultiTenantExample.Shared.DTOs;

/// <summary>
/// Represents a standardized API response.
/// </summary>
/// <typeparam name="T">The type of data in the response.</typeparam>
public sealed class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets the error message if the operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID for this response.
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The response data.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>A successful API response.</returns>
    public static ApiResponse<T> SuccessResponse(T data, string? tenantId = null) => new()
    {
        Success = true,
        Data = data,
        TenantId = tenantId
    };

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    /// <param name="tenantId">The tenant ID.</param>
    /// <returns>A failed API response.</returns>
    public static ApiResponse<T> FailureResponse(string errorMessage, string? tenantId = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        TenantId = tenantId
    };
}
