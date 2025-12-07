namespace MultiTenantExample.Shared.Models;

/// <summary>
/// Represents an order in the system.
/// </summary>
public sealed class Order
{
    /// <summary>
    /// Gets or sets the unique identifier for the order.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the tenant ID this order belongs to.
    /// </summary>
    public required string TenantId { get; set; }

    /// <summary>
    /// Gets or sets the order number.
    /// </summary>
    public required string OrderNumber { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public required string CustomerName { get; set; }

    /// <summary>
    /// Gets or sets the customer email.
    /// </summary>
    public required string CustomerEmail { get; set; }

    /// <summary>
    /// Gets or sets the total amount of the order.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Gets or sets the order status.
    /// </summary>
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    /// <summary>
    /// Gets or sets the date and time when the order was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the collection of order items.
    /// </summary>
    public List<OrderItem> Items { get; set; } = new();
}

/// <summary>
/// Represents an item in an order.
/// </summary>
public sealed class OrderItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the order item.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the order ID this item belongs to.
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    public required string ProductName { get; set; }

    /// <summary>
    /// Gets or sets the quantity ordered.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the unit price.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Gets the total price for this item (Quantity * UnitPrice).
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;
}

/// <summary>
/// Defines the possible statuses for an order.
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Order is pending processing.
    /// </summary>
    Pending,

    /// <summary>
    /// Order is being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// Order has been shipped.
    /// </summary>
    Shipped,

    /// <summary>
    /// Order has been delivered.
    /// </summary>
    Delivered,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Cancelled
}
