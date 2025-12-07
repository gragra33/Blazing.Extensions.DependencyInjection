using MultiTenantExample.Shared.Interfaces;
using MultiTenantExample.Shared.Models;

namespace MultiTenantExample.Server.Services;

/// <summary>
/// In-memory implementation of tenant-specific database context.
/// In a real application, this would be Entity Framework DbContext with actual database connections.
/// This demonstrates Keyed Services feature of Blazing.Extensions.DependencyInjection.
/// </summary>
public sealed partial class TenantDbContext : ITenantDbContext
{
    private readonly string _tenantId;
    private readonly ILogger<TenantDbContext> _logger;
    private readonly List<Order> _orders;
    private readonly List<Product> _products;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantDbContext"/> class.
    /// </summary>
    /// <param name="tenantId">The tenant ID this context is for.</param>
    /// <param name="logger">The logger instance.</param>
    public TenantDbContext(string tenantId, ILogger<TenantDbContext> logger)
    {
        _tenantId = tenantId ?? throw new ArgumentNullException(nameof(tenantId));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _orders = new List<Order>();
        _products = new List<Product>();

        InitializeSampleData();

        LogContextCreated(tenantId);
    }

    /// <inheritdoc/>
    public string TenantId => _tenantId;

    /// <inheritdoc/>
    public IQueryable<Order> Orders => _orders.AsQueryable();

    /// <inheritdoc/>
    public IQueryable<Product> Products => _products.AsQueryable();

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In a real implementation, this would save to database
        LogSavingChanges(_tenantId);
        return Task.FromResult(0);
    }

    /// <inheritdoc/>
    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is Order order)
        {
            order.TenantId = _tenantId;
            order.Id = _orders.Count + 1;
            _orders.Add(order);
        }
        else if (entity is Product product)
        {
            product.TenantId = _tenantId;
            product.Id = _products.Count + 1;
            _products.Add(product);
        }
    }

    /// <inheritdoc/>
    public void Update<TEntity>(TEntity entity) where TEntity : class
    {
        // In-memory implementation doesn't need explicit update
        if (entity is Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <inheritdoc/>
    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        if (entity is Order order)
        {
            _orders.Remove(order);
        }
        else if (entity is Product product)
        {
            _products.Remove(product);
        }
    }

    private void InitializeSampleData()
    {
        // Add sample products for each tenant
        var productData = _tenantId.ToLowerInvariant() switch
        {
            "tenant-a" => new[]
            {
                ("SKU-A001", "Enterprise License", "Annual enterprise software license", 9999.99m, "Software"),
                ("SKU-A002", "Professional Support", "24/7 professional support package", 2499.99m, "Services"),
                ("SKU-A003", "Training Package", "Comprehensive training program", 1499.99m, "Services")
            },
            "tenant-b" => new[]
            {
                ("SKU-B001", "Cloud Storage 100GB", "100GB cloud storage plan", 9.99m, "Storage"),
                ("SKU-B002", "Cloud Storage 1TB", "1TB cloud storage plan", 49.99m, "Storage"),
                ("SKU-B003", "Backup Service", "Automated backup service", 19.99m, "Services")
            },
            "tenant-c" => new[]
            {
                ("SKU-C001", "Mountain Bike", "Professional mountain bike", 1299.99m, "Bikes"),
                ("SKU-C002", "Road Bike", "Lightweight road bike", 1899.99m, "Bikes"),
                ("SKU-C003", "Bike Helmet", "Safety certified helmet", 79.99m, "Accessories")
            },
            _ => Array.Empty<(string, string, string, decimal, string)>()
        };

        foreach (var (sku, name, description, price, category) in productData)
        {
            _products.Add(new Product
            {
                Id = _products.Count + 1,
                TenantId = _tenantId,
                Sku = sku,
                Name = name,
                Description = description,
                Price = price,
                StockQuantity = 100,
                Category = category,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Add sample orders
        if (_products.Count > 0)
        {
            _orders.Add(new Order
            {
                Id = 1,
                TenantId = _tenantId,
                OrderNumber = $"{_tenantId.ToUpperInvariant()}-ORD-001",
                CustomerName = "John Doe",
                CustomerEmail = "john.doe@example.com",
                TotalAmount = _products[0].Price,
                Status = OrderStatus.Processing,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                Items = new List<OrderItem>
                {
                    new()
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = _products[0].Id,
                        ProductName = _products[0].Name,
                        Quantity = 1,
                        UnitPrice = _products[0].Price
                    }
                }
            });
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        LogContextDisposed(_tenantId);
        _disposed = true;
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "TenantDbContext created for tenant: '{TenantId}'")]
    partial void LogContextCreated(string tenantId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Saving changes for tenant: '{TenantId}'")]
    partial void LogSavingChanges(string tenantId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "TenantDbContext disposed for tenant: '{TenantId}'")]
    partial void LogContextDisposed(string tenantId);
}
