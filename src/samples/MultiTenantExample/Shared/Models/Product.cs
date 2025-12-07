namespace MultiTenantExample.Shared.Models;

/// <summary>
/// Represents a product in the system.
/// </summary>
public sealed class Product
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID this product belongs to.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the product SKU (Stock Keeping Unit).
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
    /// Gets or sets the available stock quantity.
    /// </summary>
    public int StockQuantity { get; set; }

    /// <summary>
    /// Gets or sets the product category.
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the product is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the date and time when the product was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when the product was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
