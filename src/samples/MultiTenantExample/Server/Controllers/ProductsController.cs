using Microsoft.AspNetCore.Mvc;
using MultiTenantExample.Server.Services;
using MultiTenantExample.Shared.DTOs;
using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Server.Controllers;

/// <summary>
/// API controller for multi-tenant product operations.
/// This demonstrates keyed services for tenant-specific data access.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed partial class ProductsController : ControllerBase
{
    private readonly TenantDbContextFactory _dbContextFactory;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    /// <param name="dbContextFactory">The tenant database context factory.</param>
    /// <param name="logger">The logger instance.</param>
    public ProductsController(
        TenantDbContextFactory dbContextFactory,
        ILogger<ProductsController> logger)
    {
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all products for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A collection of products for the tenant.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Product>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Product>>>> GetProducts(
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<IEnumerable<Product>>.FailureResponse("Tenant ID is required"));
        }

        LogGettingProducts(tenantId);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
            
            var products = dbContext.Products.Where(p => p.IsActive).ToList();
            
            return Ok(ApiResponse<IEnumerable<Product>>.SuccessResponse(products, tenantId));
        }
        catch (Exception ex)
        {
            LogGetProductsFailed(tenantId, ex);
            return StatusCode(500, ApiResponse<IEnumerable<Product>>.FailureResponse("Failed to retrieve products", tenantId));
        }
    }

    /// <summary>
    /// Gets a specific product by ID.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The product if found; otherwise, not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Product>>> GetProduct(
        int id,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<Product>.FailureResponse("Tenant ID is required"));
        }

        LogGettingProduct(tenantId, id);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
            
            var product = dbContext.Products.FirstOrDefault(p => p.Id == id);
            
            if (product == null)
            {
                LogProductNotFound(tenantId, id);
                return NotFound(ApiResponse<Product>.FailureResponse($"Product {id} not found", tenantId));
            }

            return Ok(ApiResponse<Product>.SuccessResponse(product, tenantId));
        }
        catch (Exception ex)
        {
            LogGetProductFailed(tenantId, id, ex);
            return StatusCode(500, ApiResponse<Product>.FailureResponse("Failed to retrieve product", tenantId));
        }
    }

    /// <summary>
    /// Creates a new product for the current tenant.
    /// </summary>
    /// <param name="request">The product creation request.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The created product.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Product>>> CreateProduct(
        [FromBody] CreateProductRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<Product>.FailureResponse("Tenant ID is required"));
        }

        LogCreatingProduct(tenantId);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);

            var product = new Product
            {
                TenantId = tenantId,
                Sku = request.Sku,
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Category = request.Category,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Add(product);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            LogProductCreated(tenantId, product.Id);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, 
                ApiResponse<Product>.SuccessResponse(product, tenantId));
        }
        catch (Exception ex)
        {
            LogCreateProductFailed(tenantId, ex);
            return StatusCode(500, ApiResponse<Product>.FailureResponse("Failed to create product", tenantId));
        }
    }

    /// <summary>
    /// Updates a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="request">The product update request.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The updated product.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Product>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Product>>> UpdateProduct(
        int id,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<Product>.FailureResponse("Tenant ID is required"));
        }

        LogUpdatingProduct(tenantId, id);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
            
            var product = dbContext.Products.FirstOrDefault(p => p.Id == id);
            
            if (product == null)
            {
                LogProductNotFound(tenantId, id);
                return NotFound(ApiResponse<Product>.FailureResponse($"Product {id} not found", tenantId));
            }

            // Update only provided fields
            if (request.Name != null)
                product.Name = request.Name;
            if (request.Description != null)
                product.Description = request.Description;
            if (request.Price.HasValue)
                product.Price = request.Price.Value;
            if (request.StockQuantity.HasValue)
                product.StockQuantity = request.StockQuantity.Value;
            if (request.Category != null)
                product.Category = request.Category;
            if (request.IsActive.HasValue)
                product.IsActive = request.IsActive.Value;

            dbContext.Update(product);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            LogProductUpdated(tenantId, id);

            return Ok(ApiResponse<Product>.SuccessResponse(product, tenantId));
        }
        catch (Exception ex)
        {
            LogUpdateProductFailed(tenantId, id, ex);
            return StatusCode(500, ApiResponse<Product>.FailureResponse("Failed to update product", tenantId));
        }
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(
        int id,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<object>.FailureResponse("Tenant ID is required"));
        }

        LogDeletingProduct(tenantId, id);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
            
            var product = dbContext.Products.FirstOrDefault(p => p.Id == id);
            
            if (product == null)
            {
                LogProductNotFound(tenantId, id);
                return NotFound(ApiResponse<object>.FailureResponse($"Product {id} not found", tenantId));
            }

            // Soft delete by marking as inactive
            product.IsActive = false;
            dbContext.Update(product);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            LogProductDeleted(tenantId, id);

            return NoContent();
        }
        catch (Exception ex)
        {
            LogDeleteProductFailed(tenantId, id, ex);
            return StatusCode(500, ApiResponse<object>.FailureResponse("Failed to delete product", tenantId));
        }
    }

    private string? GetTenantId()
    {
        if (HttpContext.Items.TryGetValue("TenantId", out var tenantIdObj) &&
            tenantIdObj is string tenantId)
        {
            return tenantId;
        }

        return null;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting products for tenant: '{TenantId}'")]
    partial void LogGettingProducts(string tenantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get products for tenant: '{TenantId}'")]
    partial void LogGetProductsFailed(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting product {ProductId} for tenant: '{TenantId}'")]
    partial void LogGettingProduct(string tenantId, int productId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Product {ProductId} not found for tenant: '{TenantId}'")]
    partial void LogProductNotFound(string tenantId, int productId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get product {ProductId} for tenant: '{TenantId}'")]
    partial void LogGetProductFailed(string tenantId, int productId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating product for tenant: '{TenantId}'")]
    partial void LogCreatingProduct(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} created for tenant: '{TenantId}'")]
    partial void LogProductCreated(string tenantId, int productId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create product for tenant: '{TenantId}'")]
    partial void LogCreateProductFailed(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating product {ProductId} for tenant: '{TenantId}'")]
    partial void LogUpdatingProduct(string tenantId, int productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} updated for tenant: '{TenantId}'")]
    partial void LogProductUpdated(string tenantId, int productId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to update product {ProductId} for tenant: '{TenantId}'")]
    partial void LogUpdateProductFailed(string tenantId, int productId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Deleting product {ProductId} for tenant: '{TenantId}'")]
    partial void LogDeletingProduct(string tenantId, int productId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product {ProductId} deleted for tenant: '{TenantId}'")]
    partial void LogProductDeleted(string tenantId, int productId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to delete product {ProductId} for tenant: '{TenantId}'")]
    partial void LogDeleteProductFailed(string tenantId, int productId, Exception exception);
}
