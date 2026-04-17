using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ServiceScopeExtensions"/> — verifying
/// <c>CreateScope</c>, <c>CreateAsyncScope</c>, <c>GetScopedService</c>,
/// <c>GetOptionalScopedService</c>, and async variants.
/// </summary>
public class ServiceScopeExtensionsTests
{
    #region CreateScope Tests

    [Fact]
    public void CreateScope_Should_CreateNewScope()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        // Act
        using var scope = host.CreateScope();

        // Assert
        scope.ShouldNotBeNull();
        scope.ServiceProvider.ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IScopedTestService>().ShouldNotBeNull();
    }

    [Fact]
    public void CreateScope_Should_ThrowWhenServiceProviderNotConfigured()
    {
        // Arrange
        var host = new TestHost();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => host.CreateScope());
    }

    [Fact]
    public void CreateScope_Should_ThrowWhenInstanceIsNull()
    {
        // Arrange
        TestHost? host = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => host!.CreateScope());
    }

    [Fact]
    public void CreateScope_Should_ProvideScopedIsolation()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        // Act
        IScopedTestService? instanceFromScope1;
        IScopedTestService? instanceFromScope2;

        using (var scope1 = host.CreateScope())
        {
            instanceFromScope1 = scope1.ServiceProvider.GetRequiredService<IScopedTestService>();
        }

        using (var scope2 = host.CreateScope())
        {
            instanceFromScope2 = scope2.ServiceProvider.GetRequiredService<IScopedTestService>();
        }

        // Assert
        instanceFromScope1.ShouldNotBeSameAs(instanceFromScope2);
    }

    #endregion

    #region CreateAsyncScope Tests

    [Fact]
    public async Task CreateAsyncScope_Should_CreateNewAsyncScope()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        // Act
        await using var asyncScope = host.CreateAsyncScope();

        // Assert
        asyncScope.ServiceProvider.ShouldNotBeNull();
        asyncScope.ServiceProvider.GetRequiredService<IScopedTestService>().ShouldNotBeNull();
    }

    [Fact]
    public void CreateAsyncScope_Should_ThrowWhenServiceProviderNotConfigured()
    {
        // Arrange
        var host = new TestHost();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => host.CreateAsyncScope());
    }

    #endregion

    #region GetScopedService (action overload) Tests

    [Fact]
    public void GetScopedService_Should_ResolveServiceAndExecuteAction()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        string? capturedValue = null;

        // Act
        host.GetScopedService<IScopedTestService>(svc => capturedValue = svc.GetValue());

        // Assert
        capturedValue.ShouldBe("Scoped Value");
    }

    [Fact]
    public void GetScopedService_Should_ThrowWhenInstanceIsNull()
    {
        // Arrange
        TestHost? host = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            host!.GetScopedService<IScopedTestService>(_ => { }));
    }

    [Fact]
    public void GetScopedService_Should_ThrowWhenActionIsNull()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            host.GetScopedService<IScopedTestService>(null!));
    }

    #endregion

    #region GetScopedService (func/return overload) Tests

    [Fact]
    public void GetScopedService_WithReturn_Should_ReturnFunctionResult()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        // Act
        var result = host.GetScopedService<IScopedTestService, string>(svc => svc.GetValue());

        // Assert
        result.ShouldBe("Scoped Value");
    }

    #endregion

    #region GetOptionalScopedService Tests

    [Fact]
    public void GetOptionalScopedService_Should_ResolveServiceWhenRegistered()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        IScopedTestService? captured = null;

        // Act
        host.GetOptionalScopedService<IScopedTestService>(svc => captured = svc);

        // Assert
        captured.ShouldNotBeNull();
    }

    [Fact]
    public void GetOptionalScopedService_Should_PassNullWhenNotRegistered()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(_ => { });

        IScopedTestService? captured = new ScopedTestService(); // non-null sentinel

        // Act
        host.GetOptionalScopedService<IScopedTestService>(svc => captured = svc);

        // Assert
        captured.ShouldBeNull();
    }

    #endregion

    #region GetScopedServiceAsync Tests

    [Fact]
    public async Task GetScopedServiceAsync_Should_ResolveServiceAndExecuteAsyncFunc()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        string? capturedValue = null;

        // Act
        await host.GetScopedServiceAsync<IScopedTestService>(async svc =>
        {
            await Task.Yield();
            capturedValue = svc.GetValue();
        });

        // Assert
        capturedValue.ShouldBe("Scoped Value");
    }

    [Fact]
    public async Task GetScopedServiceAsync_WithReturn_Should_ReturnAsyncFunctionResult()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
            services.AddScoped<IScopedTestService, ScopedTestService>());

        // Act
        var result = await host.GetScopedServiceAsync<IScopedTestService, string>(async svc =>
        {
            await Task.Yield();
            return svc.GetValue();
        });

        // Assert
        result.ShouldBe("Scoped Value");
    }

    #endregion
}
