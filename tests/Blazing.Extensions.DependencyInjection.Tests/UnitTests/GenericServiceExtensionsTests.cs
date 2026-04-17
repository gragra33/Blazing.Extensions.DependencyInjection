using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="GenericServiceExtensions"/> — covering open-generic
/// singleton, transient, scoped, and batch <c>AddGenericServices</c> registrations,
/// as well as validation of non-generic and mismatched-arity types.
/// </summary>
public class GenericServiceExtensionsTests
{
    // ---------------------------------------------------------------------------
    // AddGenericSingleton Tests
    // ---------------------------------------------------------------------------

    #region AddGenericSingleton Tests

    [Fact]
    public void AddGenericSingleton_Should_ResolveClosedGenericType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
        var provider = services.BuildServiceProvider();

        // Act
        var userRepo = provider.GetRequiredService<IRepository<UserEntity>>();
        var productRepo = provider.GetRequiredService<IRepository<ProductEntity>>();

        // Assert
        userRepo.ShouldNotBeNull();
        userRepo.ShouldBeOfType<InMemoryRepository<UserEntity>>();
        productRepo.ShouldNotBeNull();
        productRepo.ShouldBeOfType<InMemoryRepository<ProductEntity>>();
    }

    [Fact]
    public void AddGenericSingleton_Should_ReturnSameInstanceForSameClosedType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));
        var provider = services.BuildServiceProvider();

        // Act
        var instance1 = provider.GetRequiredService<IRepository<UserEntity>>();
        var instance2 = provider.GetRequiredService<IRepository<UserEntity>>();

        // Assert — singleton returns same instance
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void AddGenericSingleton_Should_ThrowWhenNonGenericInterfaceTypePassed()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert — ITestService is not an open generic definition
        Should.Throw<ArgumentException>(() =>
            services.AddGenericSingleton(typeof(ITestService), typeof(TestService)));
    }

    [Fact]
    public void AddGenericSingleton_Should_ThrowWhenNonGenericImplementationTypePassed()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert — TestService is not an open generic definition
        Should.Throw<ArgumentException>(() =>
            services.AddGenericSingleton(typeof(IRepository<>), typeof(TestService)));
    }

    [Fact]
    public void AddGenericSingleton_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddGenericSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>)));
    }

    [Fact]
    public void AddGenericSingleton_Should_ThrowWhenInterfaceTypeIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services.AddGenericSingleton(null!, typeof(InMemoryRepository<>)));
    }

    [Fact]
    public void AddGenericSingleton_Should_ReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returned = services.AddGenericSingleton(typeof(IRepository<>), typeof(InMemoryRepository<>));

        // Assert
        returned.ShouldBeSameAs(services);
    }

    #endregion

    // ---------------------------------------------------------------------------
    // AddGenericTransient Tests
    // ---------------------------------------------------------------------------

    #region AddGenericTransient Tests

    [Fact]
    public void AddGenericTransient_Should_CreateNewInstanceEachResolution()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericTransient(typeof(IRepository<>), typeof(InMemoryRepository<>));
        var provider = services.BuildServiceProvider();

        // Act
        var instance1 = provider.GetRequiredService<IRepository<UserEntity>>();
        var instance2 = provider.GetRequiredService<IRepository<UserEntity>>();

        // Assert — transient returns different instances
        instance1.ShouldNotBeSameAs(instance2);
    }

    [Fact]
    public void AddGenericTransient_Should_ThrowWhenNonGenericTypePassed()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            services.AddGenericTransient(typeof(ITestService), typeof(InMemoryRepository<>)));
    }

    [Fact]
    public void AddGenericTransient_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddGenericTransient(typeof(IRepository<>), typeof(InMemoryRepository<>)));
    }

    #endregion

    // ---------------------------------------------------------------------------
    // AddGenericScoped Tests
    // ---------------------------------------------------------------------------

    #region AddGenericScoped Tests

    [Fact]
    public void AddGenericScoped_Should_ReturnSameInstanceWithinScope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericScoped(typeof(IRepository<>), typeof(InMemoryRepository<>));
        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var instance1 = scope.ServiceProvider.GetRequiredService<IRepository<UserEntity>>();
        var instance2 = scope.ServiceProvider.GetRequiredService<IRepository<UserEntity>>();

        // Assert — same instance within scope
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void AddGenericScoped_Should_CreateNewInstanceAcrossScopes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericScoped(typeof(IRepository<>), typeof(InMemoryRepository<>));
        var provider = services.BuildServiceProvider();

        // Act
        IRepository<UserEntity>? instance1;
        IRepository<UserEntity>? instance2;

        using (var scope1 = provider.CreateScope())
        {
            instance1 = scope1.ServiceProvider.GetRequiredService<IRepository<UserEntity>>();
        }

        using (var scope2 = provider.CreateScope())
        {
            instance2 = scope2.ServiceProvider.GetRequiredService<IRepository<UserEntity>>();
        }

        // Assert — different instances across scopes
        instance1.ShouldNotBeSameAs(instance2);
    }

    [Fact]
    public void AddGenericScoped_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddGenericScoped(typeof(IRepository<>), typeof(InMemoryRepository<>)));
    }

    #endregion

    // ---------------------------------------------------------------------------
    // AddGenericServices (batch) Tests
    // ---------------------------------------------------------------------------

    #region AddGenericServices Tests

    [Fact]
    public void AddGenericServices_Should_RegisterMultiplePairsAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericServices(
            ServiceLifetime.Singleton,
            (typeof(IRepository<>), typeof(InMemoryRepository<>)));
        var provider = services.BuildServiceProvider();

        // Act
        var userRepo = provider.GetRequiredService<IRepository<UserEntity>>();
        var productRepo = provider.GetRequiredService<IRepository<ProductEntity>>();

        // Assert
        userRepo.ShouldNotBeNull();
        productRepo.ShouldNotBeNull();
    }

    [Fact]
    public void AddGenericServices_Should_RegisterWithTransientLifetime()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericServices(
            ServiceLifetime.Transient,
            (typeof(IRepository<>), typeof(InMemoryRepository<>)));
        var provider = services.BuildServiceProvider();

        // Act
        var instance1 = provider.GetRequiredService<IRepository<UserEntity>>();
        var instance2 = provider.GetRequiredService<IRepository<UserEntity>>();

        // Assert
        instance1.ShouldNotBeSameAs(instance2);
    }

    [Fact]
    public void AddGenericServices_Should_ThrowWhenServicesIsNull()
    {
        // Arrange
        IServiceCollection? services = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            services!.AddGenericServices(
                ServiceLifetime.Singleton,
                (typeof(IRepository<>), typeof(InMemoryRepository<>))));
    }

    [Fact]
    public void AddGenericServices_Should_ReturnServiceCollectionForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var returned = services.AddGenericServices(
            ServiceLifetime.Singleton,
            (typeof(IRepository<>), typeof(InMemoryRepository<>)));

        // Assert
        returned.ShouldBeSameAs(services);
    }

    #endregion

    // ---------------------------------------------------------------------------
    // Validation — generic arg count mismatch
    // ---------------------------------------------------------------------------

    #region Generic Argument Count Mismatch Tests

    [Fact]
    public void AddGenericSingleton_Should_ThrowWhenGenericArgCountMismatches()
    {
        // Arrange — IRepository<T> (1 arg) vs a hypothetical 2-arg implementation
        // Use a built-in 2-arg generic to simulate the mismatch
        var services = new ServiceCollection();

        // Act & Assert — Dictionary<,> has 2 args; IRepository<> has 1 arg
        Should.Throw<ArgumentException>(() =>
            services.AddGenericSingleton(typeof(IRepository<>), typeof(Dictionary<,>)));
    }

    #endregion
}
