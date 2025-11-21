using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace Blazing.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for managing service scopes with Blazing.Extensions.DependencyInjection.
/// Provides convenient methods for creating and working with service scopes, including async disposal support.
/// 
/// Service scopes are essential for:
/// - Blazor Server applications (per-request scoping)
/// - Background services and hosted services
/// - Multi-tenancy scenarios
/// - Resource isolation and cleanup
/// 
/// Usage:
///   using var scope = instance.CreateScope();
///   var scopedService = scope.ServiceProvider.GetRequiredService&lt;IScopedService&gt;();
///   
///   await using var asyncScope = instance.CreateAsyncScope();
///   var service = asyncScope.ServiceProvider.GetRequiredService&lt;IScopedService&gt;();
/// </summary>
public static class ServiceScopeExtensions
{
    // Thread-local storage for tracking the current scope
    private static readonly ThreadLocal<Stack<IServiceScope>> _scopeStack = new(() => new Stack<IServiceScope>());
    private static readonly ThreadLocal<Stack<AsyncServiceScope>> _asyncScopeStack = new(() => new Stack<AsyncServiceScope>());

    /// <summary>
    /// Creates a new service scope from the instance's service provider.
    /// Use with 'using' statement to ensure proper disposal.
    /// 
    /// Usage:
    ///   using var scope = instance.CreateScope();
    ///   var service = scope.ServiceProvider.GetRequiredService&lt;IScopedService&gt;();
    /// </summary>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>A new service scope</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static IServiceScope CreateScope(this object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        
        // Don't intercept direct IServiceProvider calls - let Microsoft's methods handle those
        if (instance is IServiceProvider directProvider)
        {
            return ServiceProviderServiceExtensions.CreateScope(directProvider);
        }
        
        var serviceProvider = instance.GetServices();
        if (serviceProvider == null)
        {
            throw new InvalidOperationException(
                "Service provider is not configured. Call ConfigureServices() first.");
        }

        var scope = serviceProvider.CreateScope();
        _scopeStack.Value?.Push(scope);
        return scope;
    }

    /// <summary>
    /// Creates a new async-capable service scope from the instance's service provider.
    /// Use with 'await using' statement for proper async disposal.
    /// 
    /// Essential for Blazor Server and async initialization scenarios.
    /// 
    /// Usage:
    ///   await using var asyncScope = instance.CreateAsyncScope();
    ///   var service = asyncScope.ServiceProvider.GetRequiredService&lt;IScopedService&gt;();
    /// </summary>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>A new async service scope</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured</exception>
    public static AsyncServiceScope CreateAsyncScope(this object instance)
    {
        ArgumentNullException.ThrowIfNull(instance);
        
        // Don't intercept direct IServiceProvider calls - let Microsoft's methods handle those
        if (instance is IServiceProvider directProvider)
        {
            return ServiceProviderServiceExtensions.CreateAsyncScope(directProvider);
        }
        
        var serviceProvider = instance.GetServices();
        if (serviceProvider == null)
        {
            throw new InvalidOperationException(
                "Service provider is not configured. Call ConfigureServices() first.");
        }

        var asyncScope = serviceProvider.CreateAsyncScope();
        _asyncScopeStack.Value?.Push(asyncScope);
        return asyncScope;
    }

    /// <summary>
    /// Gets a required scoped service within a new scope, automatically disposing the scope after use.
    /// This is a convenience method that combines CreateScope and service resolution.
    /// 
    /// Usage:
    ///   var result = instance.GetScopedService&lt;IScopedService&gt;(service => service.DoWork());
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="action">Action to perform with the scoped service</param>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not found</exception>
    public static void GetScopedService<TService>(this object instance, Action<TService> action) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(action);

        using var scope = instance.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        action(service);
    }

    /// <summary>
    /// Gets a required scoped service within a new scope and returns a value, automatically disposing the scope after use.
    /// 
    /// Usage:
    ///   var result = instance.GetScopedService&lt;IScopedService, string&gt;(service => service.GetData());
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <typeparam name="TResult">The return type</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="func">Function to execute with the scoped service</param>
    /// <returns>The result returned by the function</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not found</exception>
    public static TResult GetScopedService<TService, TResult>(this object instance, Func<TService, TResult> func) 
        where TService : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(func);

        using var scope = instance.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<TService>();
        return func(service);
    }

    /// <summary>
    /// Gets an optional scoped service within a new scope, automatically disposing the scope after use.
    /// Returns null if the service is not registered.
    /// 
    /// Usage:
    ///   instance.GetOptionalScopedService&lt;IScopedService&gt;(service => 
    ///   {
    ///       service?.DoWork();
    ///   });
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="action">Action to perform with the scoped service (null if not registered)</param>
    public static void GetOptionalScopedService<TService>(this object instance, Action<TService?> action) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(action);

        using var scope = instance.CreateScope();
        var service = scope.ServiceProvider.GetService<TService>();
        action(service);
    }

    /// <summary>
    /// Asynchronously gets a required scoped service within a new async scope, automatically disposing the scope after use.
    /// 
    /// Usage:
    ///   await instance.GetScopedServiceAsync&lt;IScopedService&gt;(async service => await service.DoWorkAsync());
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="func">Async function to execute with the scoped service</param>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not found</exception>
    public static async Task GetScopedServiceAsync<TService>(this object instance, Func<TService, Task> func) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(func);

        var scope = instance.CreateAsyncScope();
        try
        {
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            await func(service).ConfigureAwait(false);
        }
        finally
        {
            await scope.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously gets a required scoped service within a new async scope and returns a value, automatically disposing the scope after use.
    /// 
    /// Usage:
    ///   var result = await instance.GetScopedServiceAsync&lt;IScopedService, string&gt;(async service => await service.GetDataAsync());
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <typeparam name="TResult">The return type</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="func">Async function to execute with the scoped service</param>
    /// <returns>The result returned by the function</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not found</exception>
    public static async Task<TResult> GetScopedServiceAsync<TService, TResult>(this object instance, Func<TService, Task<TResult>> func) 
        where TService : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(func);

        var scope = instance.CreateAsyncScope();
        try
        {
            var service = scope.ServiceProvider.GetRequiredService<TService>();
            return await func(service).ConfigureAwait(false);
        }
        finally
        {
            await scope.DisposeAsync().ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Gets the current active service scope from the scope stack, if any.
    /// Used internally to track scope context in multi-scoped scenarios.
    /// </summary>
    /// <returns>The current scope, or null if no scope is active</returns>
    public static IServiceScope? GetCurrentScope()
    {
        var stack = _scopeStack.Value;
        return stack?.Count > 0 ? stack.Peek() : null;
    }

    /// <summary>
    /// Gets the current active async scope from the scope stack, if any.
    /// Used internally to track async scope context in multi-scoped scenarios.
    /// </summary>
    /// <returns>The current async scope, or null if no async scope is active</returns>
    public static AsyncServiceScope? GetCurrentAsyncScope()
    {
        var stack = _asyncScopeStack.Value;
        return stack?.Count > 0 ? stack.Peek() : null;
    }

    /// <summary>
    /// Gets a required service from the current scope or the root provider.
    /// Falls back to root provider if no scope is active.
    /// 
    /// Useful in middleware and request-level processing where scope context is available.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>The requested service</returns>
    /// <exception cref="InvalidOperationException">Thrown when service is not found</exception>
    public static TService GetScopedOrRootService<TService>(this object instance) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        // Try to get from current scope first
        var currentScope = GetCurrentScope();
        if (currentScope != null)
        {
            return currentScope.ServiceProvider.GetRequiredService<TService>();
        }

        // Fall back to root provider
        return instance.GetRequiredService<TService>();
    }

    /// <summary>
    /// Gets an optional service from the current scope or the root provider.
    /// Falls back to root provider if no scope is active.
    /// Returns null if the service is not registered.
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <returns>The requested service, or null if not found</returns>
    public static TService? GetOptionalScopedOrRootService<TService>(this object instance) where TService : class
    {
        ArgumentNullException.ThrowIfNull(instance);

        // Try to get from current scope first
        var currentScope = GetCurrentScope();
        if (currentScope != null)
        {
            return currentScope.ServiceProvider.GetService<TService>();
        }

        // Fall back to root provider
        return instance.GetService<TService>();
    }

    /// <summary>
    /// Gets a required keyed service from a new scope, automatically disposing the scope after use.
    /// 
    /// Usage:
    ///   instance.GetScopedKeyedService&lt;IRepository&gt;("sql", repo => repo.SaveData());
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="action">Action to perform with the scoped service</param>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not found</exception>
    public static void GetScopedKeyedService<TService>(this object instance, object? serviceKey, Action<TService> action) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(action);

        using var scope = instance.CreateScope();
        var service = scope.ServiceProvider.GetRequiredKeyedService<TService>(serviceKey);
        action(service);
    }

    /// <summary>
    /// Gets a required keyed service from a new scope and returns a value, automatically disposing the scope after use.
    /// 
    /// Usage:
    ///   var count = instance.GetScopedKeyedService&lt;IRepository, int&gt;("sql", repo => repo.GetCount());
    /// </summary>
    /// <typeparam name="TService">The type of service to resolve</typeparam>
    /// <typeparam name="TResult">The return type</typeparam>
    /// <param name="instance">The object instance with a configured service provider</param>
    /// <param name="serviceKey">The key for the keyed service</param>
    /// <param name="func">Function to execute with the scoped service</param>
    /// <returns>The result returned by the function</returns>
    /// <exception cref="InvalidOperationException">Thrown when service provider is not configured or service not found</exception>
    public static TResult GetScopedKeyedService<TService, TResult>(this object instance, object? serviceKey, Func<TService, TResult> func) 
        where TService : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(func);

        using var scope = instance.CreateScope();
        var service = scope.ServiceProvider.GetRequiredKeyedService<TService>(serviceKey);
        return func(service);
    }
}
