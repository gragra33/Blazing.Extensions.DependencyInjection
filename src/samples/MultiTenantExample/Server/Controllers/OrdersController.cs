using Microsoft.AspNetCore.Mvc;
using MultiTenantExample.Server.Services;
using MultiTenantExample.Shared.DTOs;
using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Server.Controllers;

/// <summary>
/// API controller for multi-tenant order operations.
/// This demonstrates keyed services and service scoping features.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed partial class OrdersController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TenantDbContextFactory _dbContextFactory;
    private readonly ILogger<OrdersController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrdersController"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="dbContextFactory">The tenant database context factory.</param>
    /// <param name="logger">The logger instance.</param>
    public OrdersController(
        IServiceProvider serviceProvider,
        TenantDbContextFactory dbContextFactory,
        ILogger<OrdersController> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all orders for the current tenant.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A collection of orders for the tenant.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Order>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<Order>>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<IEnumerable<Order>>>> GetOrders(
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<IEnumerable<Order>>.FailureResponse("Tenant ID is required"));
        }

        LogGettingOrders(tenantId);

        try
        {
            // Demonstrate keyed services - get tenant-specific database context
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
            
            var orders = dbContext.Orders.ToList();
            
            return Ok(ApiResponse<IEnumerable<Order>>.SuccessResponse(orders, tenantId));
        }
        catch (Exception ex)
        {
            LogGetOrdersFailed(tenantId, ex);
            return StatusCode(500, ApiResponse<IEnumerable<Order>>.FailureResponse("Failed to retrieve orders", tenantId));
        }
    }

    /// <summary>
    /// Gets a specific order by ID.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The order if found; otherwise, not found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Order>>> GetOrder(
        int id,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<Order>.FailureResponse("Tenant ID is required"));
        }

        LogGettingOrder(tenantId, id);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
            
            var order = dbContext.Orders.FirstOrDefault(o => o.Id == id);
            
            if (order == null)
            {
                LogOrderNotFound(tenantId, id);
                return NotFound(ApiResponse<Order>.FailureResponse($"Order {id} not found", tenantId));
            }

            return Ok(ApiResponse<Order>.SuccessResponse(order, tenantId));
        }
        catch (Exception ex)
        {
            LogGetOrderFailed(tenantId, id, ex);
            return StatusCode(500, ApiResponse<Order>.FailureResponse("Failed to retrieve order", tenantId));
        }
    }

    /// <summary>
    /// Creates a new order for the current tenant.
    /// </summary>
    /// <param name="request">The order creation request.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The created order.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Order>>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<Order>.FailureResponse("Tenant ID is required"));
        }

        LogCreatingOrder(tenantId);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);

            // Create order with items
            var order = new Order
            {
                TenantId = tenantId,
                OrderNumber = $"{tenantId.ToUpperInvariant()}-ORD-{Guid.NewGuid():N}",
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Items = request.Items.Select((item, index) =>
                {
                    var product = dbContext.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    return new OrderItem
                    {
                        Id = index + 1,
                        ProductId = item.ProductId,
                        ProductName = product?.Name ?? "Unknown",
                        Quantity = item.Quantity,
                        UnitPrice = product?.Price ?? 0
                    };
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

            dbContext.Add(order);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            LogOrderCreated(tenantId, order.Id);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, 
                ApiResponse<Order>.SuccessResponse(order, tenantId));
        }
        catch (Exception ex)
        {
            LogCreateOrderFailed(tenantId, ex);
            return StatusCode(500, ApiResponse<Order>.FailureResponse("Failed to create order", tenantId));
        }
    }

    /// <summary>
    /// Updates an order status.
    /// </summary>
    /// <param name="id">The order identifier.</param>
    /// <param name="request">The status update request.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>The updated order.</returns>
    [HttpPut("{id}/status")]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<Order>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Order>>> UpdateOrderStatus(
        int id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var tenantId = GetTenantId();
        if (tenantId == null)
        {
            return BadRequest(ApiResponse<Order>.FailureResponse("Tenant ID is required"));
        }

        LogUpdatingOrderStatus(tenantId, id, request.Status);

        try
        {
            await using var dbContext = await _dbContextFactory.CreateContextAsync(tenantId, cancellationToken).ConfigureAwait(false);
            
            var order = dbContext.Orders.FirstOrDefault(o => o.Id == id);
            
            if (order == null)
            {
                LogOrderNotFound(tenantId, id);
                return NotFound(ApiResponse<Order>.FailureResponse($"Order {id} not found", tenantId));
            }

            order.Status = request.Status;
            dbContext.Update(order);
            await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            LogOrderStatusUpdated(tenantId, id, request.Status);

            return Ok(ApiResponse<Order>.SuccessResponse(order, tenantId));
        }
        catch (Exception ex)
        {
            LogUpdateOrderStatusFailed(tenantId, id, ex);
            return StatusCode(500, ApiResponse<Order>.FailureResponse("Failed to update order status", tenantId));
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting orders for tenant: '{TenantId}'")]
    partial void LogGettingOrders(string tenantId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get orders for tenant: '{TenantId}'")]
    partial void LogGetOrdersFailed(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting order {OrderId} for tenant: '{TenantId}'")]
    partial void LogGettingOrder(string tenantId, int orderId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Order {OrderId} not found for tenant: '{TenantId}'")]
    partial void LogOrderNotFound(string tenantId, int orderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to get order {OrderId} for tenant: '{TenantId}'")]
    partial void LogGetOrderFailed(string tenantId, int orderId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Creating order for tenant: '{TenantId}'")]
    partial void LogCreatingOrder(string tenantId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} created for tenant: '{TenantId}'")]
    partial void LogOrderCreated(string tenantId, int orderId);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to create order for tenant: '{TenantId}'")]
    partial void LogCreateOrderFailed(string tenantId, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Updating order {OrderId} status to {Status} for tenant: '{TenantId}'")]
    partial void LogUpdatingOrderStatus(string tenantId, int orderId, OrderStatus status);

    [LoggerMessage(Level = LogLevel.Information, Message = "Order {OrderId} status updated to {Status} for tenant: '{TenantId}'")]
    partial void LogOrderStatusUpdated(string tenantId, int orderId, OrderStatus status);

    [LoggerMessage(Level = LogLevel.Error, Message = "Failed to update order {OrderId} status for tenant: '{TenantId}'")]
    partial void LogUpdateOrderStatusFailed(string tenantId, int orderId, Exception exception);
}
