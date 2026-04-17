using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Blazing.Extensions.DependencyInjection.Tests.Fixtures;

namespace Blazing.Extensions.DependencyInjection.Tests.UnitTests;

/// <summary>
/// Unit tests for <see cref="ServiceEnumerationExtensions"/> — verifying
/// <c>GetRequiredServices</c>, <c>GetServices</c>, <c>GetRequiredKeyedServices</c>,
/// <c>ForEachService</c>, and <c>ForEachServiceAsync</c>.
/// </summary>
public class ServiceEnumerationExtensionsTests
{
    #region GetRequiredServices Tests

    [Fact]
    public void GetRequiredServices_Should_ReturnAllRegisteredImplementations()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<ITestService, AnotherTestService>();
        });

        // Act
        var all = host.GetRequiredServices<ITestService>().ToList();

        // Assert
        all.Count.ShouldBe(2);
        all.ShouldContain(s => s is TestService);
        all.ShouldContain(s => s is AnotherTestService);
    }

    [Fact]
    public void GetRequiredServices_Should_ThrowWhenServiceProviderNotConfigured()
    {
        // Arrange
        var host = new TestHost();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            host.GetRequiredServices<ITestService>().ToList());
    }

    [Fact]
    public void GetRequiredServices_Should_ThrowWhenInstanceIsNull()
    {
        // Arrange
        TestHost? host = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            host!.GetRequiredServices<ITestService>().ToList());
    }

    [Fact]
    public void GetRequiredServices_Should_ReturnEmptyEnumerableWhenNoneRegistered()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(_ => { });

        // Act
        var all = host.GetRequiredServices<ITestService>().ToList();

        // Assert
        all.ShouldBeEmpty();
    }

    #endregion

    #region GetRequiredKeyedServices Tests

    [Fact]
    public void GetRequiredKeyedServices_Should_ReturnKeyedImplementations()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>("tier1");
            services.AddKeyedSingleton<IKeyedTestService, SecondaryKeyedService>("tier1");
        });

        // Act
        var all = host.GetRequiredKeyedServices<IKeyedTestService>("tier1").ToList();

        // Assert
        all.Count.ShouldBe(2);
        all.ShouldContain(s => s is PrimaryKeyedService);
        all.ShouldContain(s => s is SecondaryKeyedService);
    }

    [Fact]
    public void GetRequiredKeyedServices_Should_ThrowWhenServiceProviderNotConfigured()
    {
        // Arrange
        var host = new TestHost();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() =>
            host.GetRequiredKeyedServices<IKeyedTestService>("key").ToList());
    }

    #endregion

    #region GetServices (non-throwing) Tests

    [Fact]
    public void GetServices_Should_ReturnEmptyWhenNoneRegistered()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(_ => { });

        // Act
        var all = host.GetServices<ITestService>().ToList();

        // Assert
        all.ShouldBeEmpty();
    }

    [Fact]
    public void GetServices_Should_ReturnEmptyWhenServiceProviderNotConfigured()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var all = host.GetServices<ITestService>().ToList();

        // Assert
        all.ShouldBeEmpty();
    }

    [Fact]
    public void GetServices_Should_ReturnRegisteredImplementations()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
        {
            services.AddTransient<ITestService, TestService>();
            services.AddTransient<ITestService, AnotherTestService>();
        });

        // Act
        var all = host.GetServices<ITestService>().ToList();

        // Assert
        all.Count.ShouldBe(2);
    }

    #endregion

    #region ForEachService Tests

    [Fact]
    public void ForEachService_Should_ExecuteActionForEachRegisteredService()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<ITestService, AnotherTestService>();
        });

        var messages = new List<string>();

        // Act
        host.ForEachService<ITestService>(s => messages.Add(s.GetMessage()));

        // Assert
        messages.Count.ShouldBe(2);
        messages.ShouldContain("Hello from TestService");
        messages.ShouldContain("Hello from AnotherTestService");
    }

    [Fact]
    public void ForEachService_Should_ThrowWhenInstanceIsNull()
    {
        // Arrange
        TestHost? host = null;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            host!.ForEachService<ITestService>(_ => { }));
    }

    [Fact]
    public void ForEachService_Should_ThrowWhenActionIsNull()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(s => s.AddSingleton<ITestService, TestService>());

        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            host.ForEachService<ITestService>(null!));
    }

    #endregion

    #region ForEachServiceAsync Tests

    [Fact]
    public async Task ForEachServiceAsync_Should_ExecuteAsyncActionForEachService()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddSingleton<ITestService, AnotherTestService>();
        });

        var messages = new List<string>();

        // Act
        await host.ForEachServiceAsync<ITestService>(async s =>
        {
            await Task.Yield();
            messages.Add(s.GetMessage());
        });

        // Assert
        messages.Count.ShouldBe(2);
        messages.ShouldContain("Hello from TestService");
        messages.ShouldContain("Hello from AnotherTestService");
    }

    [Fact]
    public async Task ForEachServiceAsync_Should_ThrowWhenFuncIsNull()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(s => s.AddSingleton<ITestService, TestService>());

        // Act & Assert
        await Should.ThrowAsync<ArgumentNullException>(async () =>
            await host.ForEachServiceAsync<ITestService>(null!));
    }

    #endregion
}
