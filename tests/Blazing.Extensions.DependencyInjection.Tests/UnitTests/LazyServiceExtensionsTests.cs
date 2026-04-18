using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="LazyServiceExtensions"/> — verifying lazy singleton,
/// transient, scoped, and keyed registration variants.
/// </summary>
public class LazyServiceExtensionsTests
{
    #region AddLazySingleton Tests

    [Fact]
    public void AddLazySingleton_Should_RegisterLazyWrappedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazySingleton<ITestService, TestService>();
        var provider = services.BuildServiceProvider();

        // Act
        var lazy = provider.GetRequiredService<Lazy<ITestService>>();

        // Assert
        lazy.ShouldNotBeNull();
        lazy.Value.ShouldNotBeNull();
        lazy.Value.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void AddLazySingleton_Should_ReturnSameInstanceOnMultipleValueAccesses()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazySingleton<ITestService, TestService>();
        var provider = services.BuildServiceProvider();
        var lazy = provider.GetRequiredService<Lazy<ITestService>>();

        // Act
        var instance1 = lazy.Value;
        var instance2 = lazy.Value;

        // Assert
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void AddLazySingleton_Should_NotCreateServiceUntilValueAccessed()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazySingleton<ITestService, TestService>();
        var provider = services.BuildServiceProvider();

        // Act
        var lazy = provider.GetRequiredService<Lazy<ITestService>>();

        // Assert — Lazy not yet evaluated
        lazy.IsValueCreated.ShouldBeFalse();
        _ = lazy.Value;
        lazy.IsValueCreated.ShouldBeTrue();
    }

    [Fact]
    public void AddLazySingleton_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddLazySingleton<ITestService, TestService>());
    }

    [Fact]
    public void AddLazySingleton_Should_ReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returned = services.AddLazySingleton<ITestService, TestService>();

        // Assert
        returned.ShouldBeSameAs(services);
    }

    #endregion

    #region AddLazyKeyedSingleton Tests

    [Fact]
    public void AddLazyKeyedSingleton_Should_RegisterKeyedLazyService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyKeyedSingleton<ITestService, TestService>("primary");
        var provider = services.BuildServiceProvider();

        // Act
        var lazy = provider.GetRequiredKeyedService<Lazy<ITestService>>("primary");

        // Assert
        lazy.ShouldNotBeNull();
        lazy.Value.ShouldNotBeNull();
        lazy.Value.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void AddLazyKeyedSingleton_DifferentKeys_Should_BeIndependent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyKeyedSingleton<ITestService, TestService>("a");
        services.AddLazyKeyedSingleton<ITestService, AnotherTestService>("b");
        var provider = services.BuildServiceProvider();

        // Act
        var lazyA = provider.GetRequiredKeyedService<Lazy<ITestService>>("a");
        var lazyB = provider.GetRequiredKeyedService<Lazy<ITestService>>("b");

        // Assert
        lazyA.Value.ShouldBeOfType<TestService>();
        lazyB.Value.ShouldBeOfType<AnotherTestService>();
    }

    #endregion

    #region AddLazyTransient Tests

    [Fact]
    public void AddLazyTransient_Should_RegisterLazyTransientService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyTransient<ITestService, TestService>();
        var provider = services.BuildServiceProvider();

        // Act
        var lazy = provider.GetRequiredService<Lazy<ITestService>>();

        // Assert
        lazy.ShouldNotBeNull();
        lazy.Value.ShouldNotBeNull();
        lazy.Value.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void AddLazyTransient_Should_CreateNewLazyForEachResolution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyTransient<ITestService, TestService>();
        var provider = services.BuildServiceProvider();

        // Act — each GetRequiredService call returns a different Lazy wrapper
        var lazy1 = provider.GetRequiredService<Lazy<ITestService>>();
        var lazy2 = provider.GetRequiredService<Lazy<ITestService>>();

        // Assert
        lazy1.ShouldNotBeSameAs(lazy2);
    }

    [Fact]
    public void AddLazyTransient_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddLazyTransient<ITestService, TestService>());
    }

    #endregion

    #region AddLazyKeyedTransient Tests

    [Fact]
    public void AddLazyKeyedTransient_Should_RegisterKeyedLazyTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyKeyedTransient<ITestService, TestService>("key1");
        var provider = services.BuildServiceProvider();

        // Act
        var lazy = provider.GetRequiredKeyedService<Lazy<ITestService>>("key1");

        // Assert
        lazy.ShouldNotBeNull();
        lazy.Value.ShouldBeOfType<TestService>();
    }

    #endregion

    #region AddLazyScoped Tests

    [Fact]
    public void AddLazyScoped_Should_RegisterLazyScopedService()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyScoped<ITestService, TestService>();
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var lazy = scope.ServiceProvider.GetRequiredService<Lazy<ITestService>>();

        // Assert
        lazy.ShouldNotBeNull();
        lazy.Value.ShouldNotBeNull();
        lazy.Value.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void AddLazyScoped_Should_CreateNewInstancePerScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyScoped<ITestService, TestService>();
        var provider = services.BuildServiceProvider();

        // Act
        ITestService? instanceFromScope1;
        ITestService? instanceFromScope2;

        using (var scope1 = provider.CreateScope())
        {
            instanceFromScope1 = scope1.ServiceProvider.GetRequiredService<Lazy<ITestService>>().Value;
        }

        using (var scope2 = provider.CreateScope())
        {
            instanceFromScope2 = scope2.ServiceProvider.GetRequiredService<Lazy<ITestService>>().Value;
        }

        // Assert
        instanceFromScope1.ShouldNotBeSameAs(instanceFromScope2);
    }

    [Fact]
    public void AddLazyScoped_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddLazyScoped<ITestService, TestService>());
    }

    #endregion
}
