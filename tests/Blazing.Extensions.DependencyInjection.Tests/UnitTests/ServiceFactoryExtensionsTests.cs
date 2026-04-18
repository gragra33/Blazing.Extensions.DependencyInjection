using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ServiceFactoryExtensions"/> — verifying
/// <c>RegisterFactory</c>, <c>RegisterTransientFactory</c>, <c>RegisterScopedFactory</c>,
/// and their keyed counterparts.
/// </summary>
public class ServiceFactoryExtensionsTests
{
    #region RegisterFactory (singleton) Tests

    [Fact]
    public void RegisterFactory_Should_RegisterSingletonServiceUsingFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterFactory<ITestService>(_ => new TestService());
        var provider = services.BuildServiceProvider();

        // Act
        var service = provider.GetRequiredService<ITestService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void RegisterFactory_Should_ReturnSameInstanceOnMultipleResolutions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterFactory<ITestService>(_ => new TestService());
        var provider = services.BuildServiceProvider();

        // Act
        var instance1 = provider.GetRequiredService<ITestService>();
        var instance2 = provider.GetRequiredService<ITestService>();

        // Assert — factory is registered as singleton
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void RegisterFactory_Should_ProvideServiceProviderToFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton("factory-context");
        services.RegisterFactory<ITestService>(provider =>
        {
            var ctx = provider.GetRequiredService<string>();
            ctx.ShouldBe("factory-context");
            return new TestService();
        });
        var provider = services.BuildServiceProvider();

        // Act
        var service = provider.GetRequiredService<ITestService>();

        // Assert
        service.ShouldNotBeNull();
    }

    [Fact]
    public void RegisterFactory_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.RegisterFactory<ITestService>(_ => new TestService()));
    }

    [Fact]
    public void RegisterFactory_Should_ThrowWhenFactoryIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services.RegisterFactory<ITestService>(null!));
    }

    [Fact]
    public void RegisterFactory_Should_ReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returned = services.RegisterFactory<ITestService>(_ => new TestService());

        // Assert
        returned.ShouldBeSameAs(services);
    }

    #endregion

    #region RegisterTransientFactory Tests

    [Fact]
    public void RegisterTransientFactory_Should_CreateNewInstanceEachTime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterTransientFactory<ITestService>(_ => new TestService());
        var provider = services.BuildServiceProvider();

        // Act
        var instance1 = provider.GetRequiredService<ITestService>();
        var instance2 = provider.GetRequiredService<ITestService>();

        // Assert — transient creates new instance each time
        instance1.ShouldNotBeSameAs(instance2);
    }

    [Fact]
    public void RegisterTransientFactory_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.RegisterTransientFactory<ITestService>(_ => new TestService()));
    }

    [Fact]
    public void RegisterTransientFactory_Should_ThrowWhenFactoryIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services.RegisterTransientFactory<ITestService>(null!));
    }

    #endregion

    #region RegisterScopedFactory Tests

    [Fact]
    public void RegisterScopedFactory_Should_CreateInstancePerScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterScopedFactory<ITestService>(_ => new TestService());
        var provider = services.BuildServiceProvider();

        // Act
        ITestService? instanceFromScope1;
        ITestService? instanceFromScope2;

        using (var scope1 = provider.CreateScope())
        {
            instanceFromScope1 = scope1.ServiceProvider.GetRequiredService<ITestService>();
        }

        using (var scope2 = provider.CreateScope())
        {
            instanceFromScope2 = scope2.ServiceProvider.GetRequiredService<ITestService>();
        }

        // Assert
        instanceFromScope1.ShouldNotBeSameAs(instanceFromScope2);
    }

    [Fact]
    public void RegisterScopedFactory_Should_ReturnSameInstanceWithinScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterScopedFactory<ITestService>(_ => new TestService());
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var instance1 = scope.ServiceProvider.GetRequiredService<ITestService>();
        var instance2 = scope.ServiceProvider.GetRequiredService<ITestService>();

        // Assert — scoped service is same within the same scope
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void RegisterScopedFactory_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.RegisterScopedFactory<ITestService>(_ => new TestService()));
    }

    #endregion

    #region RegisterKeyedFactory Tests

    [Fact]
    public void RegisterKeyedFactory_Should_RegisterKeyedServiceUsingFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterKeyedFactory<ITestService>("primary", (_, _) => new TestService());
        services.RegisterKeyedFactory<ITestService>("secondary", (_, _) => new AnotherTestService());
        var provider = services.BuildServiceProvider();

        // Act
        var primary = provider.GetRequiredKeyedService<ITestService>("primary");
        var secondary = provider.GetRequiredKeyedService<ITestService>("secondary");

        // Assert
        primary.ShouldBeOfType<TestService>();
        secondary.ShouldBeOfType<AnotherTestService>();
    }

    [Fact]
    public void RegisterKeyedTransientFactory_Should_CreateNewInstanceEachTime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterKeyedTransientFactory<ITestService>("key", (_, _) => new TestService());
        var provider = services.BuildServiceProvider();

        // Act
        var instance1 = provider.GetRequiredKeyedService<ITestService>("key");
        var instance2 = provider.GetRequiredKeyedService<ITestService>("key");

        // Assert
        instance1.ShouldNotBeSameAs(instance2);
    }

    [Fact]
    public void RegisterKeyedScopedFactory_Should_ReturnSameInstanceWithinScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterKeyedScopedFactory<ITestService>("key", (_, _) => new TestService());
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var instance1 = scope.ServiceProvider.GetRequiredKeyedService<ITestService>("key");
        var instance2 = scope.ServiceProvider.GetRequiredKeyedService<ITestService>("key");

        // Assert — same instance within a scope
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void RegisterKeyedFactory_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.RegisterKeyedFactory<ITestService>("key", (_, _) => new TestService()));
    }

    [Fact]
    public void RegisterKeyedFactory_Should_ThrowWhenFactoryIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services.RegisterKeyedFactory<ITestService>("key", null!));
    }

    #endregion
}
