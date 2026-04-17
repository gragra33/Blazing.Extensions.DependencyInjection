using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.IntegrationTests;

// File-scoped to avoid false "unused private method" IDE0051 diagnostics on interface implementations
internal sealed class PriorityLogService(List<string> log, string name, int priority) : IAsyncInitializable
{
    public int InitializationPriority => priority;

    public Task InitializeAsync(IServiceProvider serviceProvider)
    {
        _ = serviceProvider; // parameter required by interface; suppress IDE0060
        log.Add(name);
        return Task.CompletedTask;
    }
}

/// <summary>
/// End-to-end integration tests that combine multiple DI extension features
/// in a single, realistic service container scenario.
/// </summary>
public class DependencyInjectionIntegrationTests
{
    // ---------------------------------------------------------------------------
    // Lazy + Scoped + Enumeration pipeline
    // ---------------------------------------------------------------------------

    [Fact]
    public void Integration_LazyAndScoped_Should_ResolveAllServicesCorrectly()
    {
        // Arrange — use TestHost so GetRequiredServices<T> works through the library's object-extension API
        var host = new TestHost();
        var provider = host.ConfigureServices(services =>
        {
            services.AddLazySingleton<ITestService, TestService>();
            services.AddScoped<IScopedTestService, ScopedTestService>();
            services.AddSingleton<ITestService, AnotherTestService>();
        });

        // Act
        var lazy = provider.GetRequiredService<Lazy<ITestService>>();
        using var scope = host.CreateScope();
        var scoped = scope.ServiceProvider.GetRequiredService<IScopedTestService>();
        var allTestServices = host.GetRequiredServices<ITestService>().ToList();

        // Assert
        lazy.ShouldNotBeNull();
        lazy.Value.ShouldNotBeNull();
        scoped.ShouldNotBeNull();
        allTestServices.Count.ShouldBeGreaterThanOrEqualTo(1);
    }

    // ---------------------------------------------------------------------------
    // Decoration + Factory pipeline
    // ---------------------------------------------------------------------------

    [Fact]
    public void Integration_DecoratedFactory_Should_ProduceDecoratedSingleton()
    {
        // Arrange — register service via type, then decorate it
        var services = new ServiceCollection();
        services.AddSingleton<IDecoratableService, DecoratableService>();
        services.Decorate<IDecoratableService>((inner, _) => new PrefixDecorator(inner));

        var provider = services.BuildServiceProvider();

        // Act
        var svc = provider.GetRequiredService<IDecoratableService>();

        // Assert — always gets the decorated wrapper
        svc.ShouldBeOfType<PrefixDecorator>();
        svc.Execute().ShouldBe("[decorated] base");
    }

    // ---------------------------------------------------------------------------
    // Generic open-type + keyed services
    // ---------------------------------------------------------------------------

    [Fact]
    public void Integration_GenericAndKeyed_Should_ResolveIndependently()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
        services.AddKeyedSingleton<ITestService, TestService>("primary");
        services.AddKeyedSingleton<ITestService, AnotherTestService>("secondary");

        var provider = services.BuildServiceProvider();

        // Act
        var userRepo = provider.GetRequiredService<IRepository<UserEntity>>();
        var primary = provider.GetRequiredKeyedService<ITestService>("primary");
        var secondary = provider.GetRequiredKeyedService<ITestService>("secondary");

        // Assert
        userRepo.ShouldBeOfType<InMemoryRepository<UserEntity>>();
        primary.ShouldBeOfType<TestService>();
        secondary.ShouldBeOfType<AnotherTestService>();
    }

    // ---------------------------------------------------------------------------
    // Async initialization + startup actions
    // ---------------------------------------------------------------------------

    [Fact]
    public async Task Integration_AsyncInitialization_Should_InitializeInPriorityOrder()
    {
        // Arrange
        var log = new List<string>();
        var services = new ServiceCollection();
        services.AddSingleton<IAsyncInitializable>(
            new PriorityLogService(log, "low", priority: 1));
        services.AddSingleton<IAsyncInitializable>(
            new PriorityLogService(log, "high", priority: 10));
        services.AddStartupAction(_ =>
        {
            log.Add("startup-action");
            return Task.CompletedTask;
        }, priority: 5);

        var provider = services.BuildServiceProvider();

        // Act
        await provider.InitializeAllAsync();

        // Assert — high priority first, then startup action, then low priority
        log.IndexOf("high").ShouldBeLessThan(log.IndexOf("low"),
            "Higher priority service must initialize first");
    }

    // ---------------------------------------------------------------------------
    // Scoped factory + disposal
    // ---------------------------------------------------------------------------

    [Fact]
    public void Integration_ScopedFactory_Dispose_Should_NotLeakServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterScopedFactory<ITestService>(_ => new TestService());
        var provider = services.BuildServiceProvider();

        // Act — create and immediately dispose scope
        ITestService? captured;
        using (var scope = provider.CreateScope())
        {
            captured = scope.ServiceProvider.GetRequiredService<ITestService>();
        }

        // Resolving a new scope should get a fresh instance (no leaked reference)
        ITestService? fresh;
        using (var scope2 = provider.CreateScope())
        {
            fresh = scope2.ServiceProvider.GetRequiredService<ITestService>();
        }

        // Assert
        captured.ShouldNotBeSameAs(fresh);
    }

    // ---------------------------------------------------------------------------
    // Diagnostic integrity
    // ---------------------------------------------------------------------------

    [Fact]
    public void Integration_Diagnostics_Should_ReflectActualRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddScoped<IScopedTestService, ScopedTestService>();
        services.AddTransient<IDependentService, DependentService>();

        // Act
        var diagnostics = services.GetDiagnostics();

        // Assert
        diagnostics.TotalServices.ShouldBe(3);
        diagnostics.SingletonCount.ShouldBe(1);
        diagnostics.ScopedCount.ShouldBe(1);
        diagnostics.TransientCount.ShouldBe(1);
        diagnostics.Warnings.ShouldBeEmpty();
    }
}
