namespace MultiTenantExample.Shared.DTOs;

/// <summary>
/// Data transfer object for creating a new product.
/// </summary>
public sealed class CreateProductRequest
{
    /// <summary>
    /// Gets or sets the product SKU.
    /// </summary>
    public required string Sku { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the initial stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the product category.
    /// </summary>
    public required string Category { get; set; }
}

/// <summary>
/// Data transfer object for updating a product.
/// </summary>
public sealed class UpdateProductRequest
{
    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the product price.
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Gets or sets the stock quantity.
    /// </summary>
    public int? StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the product category.
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets whether the product is active.
    /// </summary>
    public bool? IsActive { get; set; }
}
