using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Shared.DTOs;

/// <summary>
/// Data transfer object for creating a new order.
/// </summary>
public sealed class CreateOrderRequest
{
    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    public required string CustomerName { get; set; }

    /// <summary>
    /// Gets or sets the customer email.
    /// </summary>
    public required string CustomerEmail { get; set; }

    /// <summary>
    /// Gets or sets the collection of items to order.
    /// </summary>
    public required List<CreateOrderItemRequest> Items { get; set; }
}

/// <summary>
/// Data transfer object for an order item in a create request.
/// </summary>
public sealed class CreateOrderItemRequest
{
    /// <summary>
    /// Gets or sets the product ID.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity to order.
    /// </summary>
    public int Quantity { get; set; }
}

/// <summary>
/// Data transfer object for updating an order status.
/// </summary>
public sealed class UpdateOrderStatusRequest
{
    /// <summary>
    /// Gets or sets the new order status.
    /// </summary>
    public OrderStatus Status { get; set; }
}
