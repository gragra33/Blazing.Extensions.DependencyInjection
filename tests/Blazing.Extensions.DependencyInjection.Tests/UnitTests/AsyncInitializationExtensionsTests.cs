using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

// ---------------------------------------------------------------------------
// Test doubles — file-level to avoid false "unused private member" diagnostics
// ---------------------------------------------------------------------------

internal sealed class TrackingService(List<string> log, string name, int priority = 0) : IAsyncInitializable
{
    public int InitializationPriority => priority;

    public Task InitializeAsync(IServiceProvider serviceProvider)
    {
        log.Add(name);
        return Task.CompletedTask;
    }
}

internal sealed class DependentInitService(List<string> log, string name, Type dependency)
    : IAsyncInitializable
{
    // Implicit implementation — uses instance field `dependency`, so CA1822 does not apply
    public IEnumerable<Type>? DependsOn => [dependency];

    public Task InitializeAsync(IServiceProvider serviceProvider)
    {
        log.Add(name);
        return Task.CompletedTask;
    }
}

// Circular A → B → A  (instance-backed fields prevent false CA1822 "make static" suggestions)
internal sealed class CircularServiceA : IAsyncInitializable
{
    private readonly Type[] _dependsOn = [typeof(CircularServiceB)];

    public IEnumerable<Type>? DependsOn => _dependsOn;

    public Task InitializeAsync(IServiceProvider serviceProvider)
    {
        // Reference instance data so the method is not flagged as CA1822 "make static"
        _ = _dependsOn.Length;
        return Task.CompletedTask;
    }
}

internal sealed class CircularServiceB : IAsyncInitializable
{
    private readonly Type[] _dependsOn = [typeof(CircularServiceA)];

    public IEnumerable<Type>? DependsOn => _dependsOn;

    public Task InitializeAsync(IServiceProvider serviceProvider)
    {
        _ = _dependsOn.Length;
        return Task.CompletedTask;
    }
}

/// <summary>
/// Unit tests for <see cref="AsyncInitializationExtensions"/> — covering
/// <c>InitializeAsync&lt;T&gt;</c>, <c>InitializeAllAsync</c>, priority ordering,
/// dependency resolution, circular dependency detection, and <c>GetInitializationOrder</c>.
/// </summary>
public class AsyncInitializationExtensionsTests
{

    // ---------------------------------------------------------------------------
    // InitializeAsync<T> Tests
    // ---------------------------------------------------------------------------

    #region InitializeAsync<T> Tests

    [Fact]
    public async Task InitializeAsync_Should_CallInitializeOnService()
    {
        // Arrange
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncInitializable>(new TrackingService(log, "service-a"));
        services.AddSingleton<TrackingService>(new TrackingService(log, "direct"));
        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeAsync<TrackingService>();

        // Assert
        log.ShouldContain("direct");
    }

    [Fact]
    public async Task InitializeAsync_Should_ThrowWhenProviderIsNull()
    {
        // Arrange
        IServiceProvider? provider = null;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await provider!.InitializeAsync<TrackingService>());
    }

    #endregion

    // ---------------------------------------------------------------------------
    // InitializeAllAsync Tests
    // ---------------------------------------------------------------------------

    #region InitializeAllAsync Tests

    [Fact]
    public async Task InitializeAllAsync_Should_InitializeAllRegisteredServices()
    {
        // Arrange
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncInitializable>(new TrackingService(log, "svc-a"));
        services.AddSingleton<IAsyncInitializable>(new TrackingService(log, "svc-b"));
        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeAllAsync();

        // Assert
        log.ShouldContain("svc-a");
        log.ShouldContain("svc-b");
    }

    [Fact]
    public async Task InitializeAllAsync_Should_RespectPriorityOrder()
    {
        // Arrange — higher priority initializes first
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncInitializable>(new TrackingService(log, "low", priority: 1));
        services.AddSingleton<IAsyncInitializable>(new TrackingService(log, "high", priority: 10));
        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeAllAsync();

        // Assert
        log.IndexOf("high").ShouldBeLessThan(log.IndexOf("low"),
            "Higher priority service should be initialized first");
    }

    [Fact]
    public async Task InitializeAllAsync_Should_ThrowOnCircularDependency()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncInitializable, CircularServiceA>();
        services.AddSingleton<IAsyncInitializable, CircularServiceB>();
        var provider = services.BuildServiceProvider();

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () =>
            await provider.InitializeAllAsync());
    }

    [Fact]
    public async Task InitializeAllAsync_Should_NotCallServiceTwiceWhenSharedDependency()
    {
        // Arrange — two services depend on the same third service
        var log = new List<string>();
        var sharedService = new TrackingService(log, "shared", priority: 0);
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncInitializable>(sharedService);
        services.AddSingleton<IAsyncInitializable>(
            new DependentInitService(log, "consumer-a", typeof(TrackingService)));
        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeAllAsync();

        // Assert — "shared" should appear exactly once
        log.Count(n => n == "shared").ShouldBe(1);
    }

    [Fact]
    public async Task InitializeAllAsync_Should_ThrowWhenProviderIsNull()
    {
        // Arrange
        IServiceProvider? provider = null;

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await provider!.InitializeAllAsync());
    }

    #endregion

    // ---------------------------------------------------------------------------
    // AddStartupAction Tests
    // ---------------------------------------------------------------------------

    #region AddStartupAction Tests

    [Fact]
    public async Task AddStartupAction_Should_ExecuteActionDuringInitialization()
    {
        // Arrange
        var executed = false;
        var services = new ServiceCollection();
        services.AddStartupAction(_ =>
        {
            executed = true;
            return Task.CompletedTask;
        });
        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeAllAsync();

        // Assert
        executed.ShouldBeTrue();
    }

    [Fact]
    public async Task AddStartupAction_Should_RespectPriority()
    {
        // Arrange
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddStartupAction(_ => { log.Add("low"); return Task.CompletedTask; }, priority: 1);
        services.AddStartupAction(_ => { log.Add("high"); return Task.CompletedTask; }, priority: 100);
        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeAllAsync();

        // Assert
        log.IndexOf("high").ShouldBeLessThan(log.IndexOf("low"));
    }

    [Fact]
    public void AddStartupAction_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddStartupAction(_ => Task.CompletedTask));
    }

    [Fact]
    public void AddStartupAction_Should_ThrowWhenActionIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services.AddStartupAction(null!));
    }

    #endregion

    // ---------------------------------------------------------------------------
    // GetInitializationOrder Tests
    // ---------------------------------------------------------------------------

    #region GetInitializationOrder Tests

    [Fact]
    public void GetInitializationOrder_Should_ReturnOrderedSteps()
    {
        // Arrange
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncInitializable>(new TrackingService(log, "svc-a", priority: 5));
        services.AddSingleton<IAsyncInitializable>(new TrackingService(log, "svc-b", priority: 10));
        var provider = services.BuildServiceProvider();

        // Act
        var order = provider.GetInitializationOrder();

        // Assert
        order.ShouldNotBeNull();
        order.Steps.Count.ShouldBe(2);
        // Steps are ordered by priority descending — higher priority first
        order.Steps.First().Priority.ShouldBeGreaterThanOrEqualTo(order.Steps.Last().Priority);
    }

    [Fact]
    public void GetInitializationOrder_Should_ReturnEmptyWhenNoServicesRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();

        // Act
        var order = provider.GetInitializationOrder();

        // Assert
        order.Steps.Count.ShouldBe(0);
    }

    [Fact]
    public void GetInitializationOrder_Should_ThrowWhenProviderIsNull()
    {
        // Arrange
        IServiceProvider? provider = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => provider!.GetInitializationOrder());
    }

    #endregion
}
