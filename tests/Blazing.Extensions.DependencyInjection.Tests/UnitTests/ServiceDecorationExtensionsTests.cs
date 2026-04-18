using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ServiceDecorationExtensions"/> — verifying
/// the <c>Decorate&lt;T&gt;</c> factory-based decorator pattern.
/// </summary>
public class ServiceDecorationExtensionsTests
{
    #region Decorate Tests

    [Fact]
    public void Decorate_Should_WrapServiceWithDecorator()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDecoratableService, DecoratableService>();
        services.Decorate<IDecoratableService>((inner, _) => new PrefixDecorator(inner));
        var provider = services.BuildServiceProvider();

        // Act
        var service = provider.GetRequiredService<IDecoratableService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<PrefixDecorator>();
        service.Execute().ShouldBe("[decorated] base");
    }

    [Fact]
    public void Decorate_Should_ProvideServiceProviderToDecoratorFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton("context-value");
        services.AddSingleton<IDecoratableService, DecoratableService>();
        services.Decorate<IDecoratableService>((inner, provider) =>
        {
            // Factory can access other services via the provider
            var ctx = provider.GetRequiredService<string>();
            ctx.ShouldBe("context-value");
            return new PrefixDecorator(inner);
        });
        var provider = services.BuildServiceProvider();

        // Act
        var service = provider.GetRequiredService<IDecoratableService>();

        // Assert
        service.ShouldBeOfType<PrefixDecorator>();
    }

    [Fact]
    public void Decorate_Should_ThrowWhenServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            services.Decorate<IDecoratableService>((inner, _) => new PrefixDecorator(inner)));
    }

    [Fact]
    public void Decorate_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.Decorate<IDecoratableService>((inner, _) => new PrefixDecorator(inner)));
    }

    [Fact]
    public void Decorate_Should_ThrowWhenDecoratorFactoryIsNull()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDecoratableService, DecoratableService>();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services.Decorate<IDecoratableService>(null!));
    }

    [Fact]
    public void Decorate_Should_ReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDecoratableService, DecoratableService>();

        // Act
        var returned = services.Decorate<IDecoratableService>((inner, _) => new PrefixDecorator(inner));

        // Assert
        returned.ShouldBeSameAs(services);
    }

    [Fact]
    public void Decorate_Should_RegisterDecoratedServiceAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IDecoratableService, DecoratableService>();
        services.Decorate<IDecoratableService>((inner, _) => new PrefixDecorator(inner));
        var provider = services.BuildServiceProvider();

        // Act
        var instance1 = provider.GetRequiredService<IDecoratableService>();
        var instance2 = provider.GetRequiredService<IDecoratableService>();

        // Assert — decorated service is registered as singleton
        instance1.ShouldBeSameAs(instance2);
    }

    #endregion
}
