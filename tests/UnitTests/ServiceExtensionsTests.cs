using System;
using System.Linq;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Extensions.DependencyInjection.Tests;

public class ServiceExtensionsTests
{
    // Test helper classes
    public interface ITestService
    {
        string GetMessage();
    }

    public class TestService : ITestService
    {
        public string GetMessage() => "Hello from TestService";
    }

    public class AnotherTestService : ITestService
    {
        public string GetMessage() => "Hello from AnotherTestService";
    }

    public interface IDependentService
    {
        string GetValue();
    }

    public class DependentService : IDependentService
    {
        private readonly ITestService _testService;

        public DependentService(ITestService testService)
        {
            _testService = testService;
        }

        public string GetValue() => $"Dependent: {_testService.GetMessage()}";
    }

    public class TestHost { }

    public class DisposableService : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose() => IsDisposed = true;
    }

    #region Services Property Tests

    [Fact]
    public void Services_Property_Should_BeNull_Initially()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var services = host.GetServices();

        // Assert
        services.ShouldBeNull();
    }

    [Fact]
    public void Services_Property_Should_Return_ConfiguredProvider()
    {
        // Arrange
        var host = new TestHost();

        // Act
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
        });

        // Assert
        host.GetServices().ShouldNotBeNull();
        host.GetServices()!.GetService<ITestService>().ShouldNotBeNull();
    }

    [Fact]
    public void Services_Property_Can_Be_Set_To_Null()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Act
        host.SetServices(null);

        // Assert
        host.GetServices().ShouldBeNull();
    }

    #endregion

    #region ConfigureServices Tests

    [Fact]
    public void ConfigureServices_Should_ConfigureAndReturnServiceProvider()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var serviceProvider = host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
        });

        // Assert
        serviceProvider.ShouldNotBeNull();
        serviceProvider.GetService<ITestService>().ShouldBeOfType<TestService>();
    }

    [Fact]
    public void ConfigureServices_Should_RegisterSingletonServices()
    {
        // Arrange
        var host = new TestHost();

        // Act
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
        });

        // Assert
        var service1 = host.GetRequiredService<ITestService>();
        var service2 = host.GetRequiredService<ITestService>();
        service1.ShouldBeSameAs(service2);
    }

    [Fact]
    public void ConfigureServices_Should_RegisterTransientServices()
    {
        // Arrange
        var host = new TestHost();

        // Act
        host.ConfigureServices(services =>
        {
            services.AddTransient<ITestService, TestService>();
        });

        // Assert
        var service1 = host.GetRequiredService<ITestService>();
        var service2 = host.GetRequiredService<ITestService>();
        service1.ShouldNotBeSameAs(service2);
    }

    [Fact]
    public void ConfigureServices_Should_SupportDependencyInjection()
    {
        // Arrange
        var host = new TestHost();

        // Act
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddTransient<IDependentService, DependentService>();
        });

        // Assert
        var dependentService = host.GetRequiredService<IDependentService>();
        dependentService.GetValue().ShouldContain("Hello from TestService");
    }

    #endregion

    #region ConfigureServices with PostBuildAction Tests

    [Fact]
    public void ConfigureServices_WithPostBuild_Should_ExecutePostBuildAction()
    {
        // Arrange
        var host = new TestHost();
        var postBuildExecuted = false;

        // Act
        host.ConfigureServices(
            services => services.AddSingleton<ITestService, TestService>(),
            provider =>
            {
                postBuildExecuted = true;
                provider.GetRequiredService<ITestService>().ShouldNotBeNull();
            });

        // Assert
        postBuildExecuted.ShouldBeTrue();
    }

    [Fact]
    public void ConfigureServices_WithPostBuild_Should_AllowServiceWarmup()
    {
        // Arrange
        var host = new TestHost();
        ITestService warmedUpService = null;

        // Act
        host.ConfigureServices(
            services => services.AddSingleton<ITestService, TestService>(),
            provider =>
            {
                warmedUpService = provider.GetRequiredService<ITestService>();
            });

        // Assert
        warmedUpService.ShouldNotBeNull();
        warmedUpService.ShouldBeSameAs(host.GetRequiredService<ITestService>());
    }

    #endregion

    #region ConfigureServicesAdvanced Tests

    [Fact]
    public void ConfigureServicesAdvanced_Should_AllowCustomServiceProviderOptions()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var serviceProvider = host.ConfigureServicesAdvanced(services =>
        {
            services.AddTransient<ITestService, TestService>();
            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateOnBuild = true
            });
        });

        // Assert
        serviceProvider.ShouldNotBeNull();
        serviceProvider.GetService<ITestService>().ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureServicesAdvanced_Should_AllowScopeValidation()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var serviceProvider = host.ConfigureServicesAdvanced(services =>
        {
            services.AddScoped<ITestService, TestService>();
            return services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true
            });
        });

        // Assert
        serviceProvider.ShouldNotBeNull();
    }

    #endregion

    #region GetServiceCollection and BuildServices Tests

    [Fact]
    public void GetServiceCollection_Should_ReturnConfigurableServiceCollection()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var services = host.GetServiceCollection(s =>
        {
            s.AddSingleton<ITestService, TestService>();
        });

        // Assert
        services.ShouldNotBeNull();
        services.ShouldHaveSingleItem();
        services.First().ServiceType.ShouldBe(typeof(ITestService));
    }

    [Fact]
    public void BuildServices_Should_BuildAndAssignProvider()
    {
        // Arrange
        var host = new TestHost();
        var services = host.GetServiceCollection(s =>
        {
            s.AddSingleton<ITestService, TestService>();
        });

        // Act
        var serviceProvider = host.BuildServices(services);

        // Assert
        serviceProvider.ShouldNotBeNull();
        host.GetServices().ShouldBe(serviceProvider);
        host.GetRequiredService<ITestService>().ShouldNotBeNull();
    }

    [Fact]
    public void BuildServices_Should_AcceptServiceProviderOptions()
    {
        // Arrange
        var host = new TestHost();
        var services = host.GetServiceCollection(s =>
        {
            s.AddTransient<ITestService, TestService>();
        });

        // Act
        var serviceProvider = host.BuildServices(services, new ServiceProviderOptions
        {
            ValidateOnBuild = true
        });

        // Assert
        serviceProvider.ShouldNotBeNull();
        host.GetRequiredService<ITestService>().ShouldNotBeNull();
    }

    #endregion

    #region GetRequiredService Tests

    [Fact]
    public void GetRequiredService_Should_ResolveRegisteredService()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Act
        var service = host.GetRequiredService<ITestService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void GetRequiredService_Should_ThrowWhenServiceNotRegistered()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => { });

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => host.GetRequiredService<ITestService>());
    }

    [Fact]
    public void GetRequiredService_Should_ThrowWhenServicesNotConfigured()
    {
        // Arrange
        var host = new TestHost();

        // Act & Assert
        var ex = Should.Throw<InvalidOperationException>(() => host.GetRequiredService<ITestService>());
        ex.Message.ShouldBe("Service provider is not configured.");
    }

    [Fact]
    public void GetRequiredService_ConvenienceOverload_Should_Work()
    {
        // Arrange
        object host = new TestHost();
        ((TestHost)host).ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Act
        var service = host.GetRequiredService<ITestService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<TestService>();
    }

    #endregion

    #region GetService Tests

    [Fact]
    public void GetService_Should_ResolveRegisteredService()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Act
        var service = host.GetService<ITestService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void GetService_Should_ReturnNullWhenServiceNotRegistered()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => { });

        // Act
        var service = host.GetService<ITestService>();

        // Assert
        service.ShouldBeNull();
    }

    [Fact]
    public void GetService_Should_ReturnNullWhenServicesNotConfigured()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var service = host.GetService<ITestService>();

        // Assert
        service.ShouldBeNull();
    }

    [Fact]
    public void GetService_ConvenienceOverload_Should_Work()
    {
        // Arrange
        object host = new TestHost();
        ((TestHost)host).ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Act
        var service = host.GetService<ITestService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<TestService>();
    }

    #endregion

    #region Multiple Instances Tests

    [Fact]
    public void Multiple_Instances_Should_HaveIsolatedServiceProviders()
    {
        // Arrange
        var host1 = new TestHost();
        var host2 = new TestHost();

        // Act
        host1.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());
        host2.ConfigureServices(services => services.AddSingleton<ITestService, AnotherTestService>());

        // Assert
        host1.GetRequiredService<ITestService>().ShouldBeOfType<TestService>();
        host2.GetRequiredService<ITestService>().ShouldBeOfType<AnotherTestService>();
    }

    [Fact]
    public void Multiple_Instances_Should_NotShareServices()
    {
        // Arrange
        var host1 = new TestHost();
        var host2 = new TestHost();

        // Act
        host1.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());
        host2.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Assert
        var service1 = host1.GetRequiredService<ITestService>();
        var service2 = host2.GetRequiredService<ITestService>();
        service1.ShouldNotBeSameAs(service2);
    }

    #endregion

    #region Memory Management Tests

    [Fact]
    public void Services_Should_BeRemovedWhenInstanceIsGarbageCollected()
    {
        // Arrange
        WeakReference CreateAndConfigureHost()
        {
            var host = new TestHost();
            host.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());
            host.GetRequiredService<ITestService>().ShouldNotBeNull();
            return new WeakReference(host);
        }

        var weakRef = CreateAndConfigureHost();

        // Act
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        // Assert
        weakRef.IsAlive.ShouldBeFalse();
    }

    [Fact]
    public void Reconfiguring_Services_Should_Replace_Provider()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());
        var firstService = host.GetRequiredService<ITestService>();

        // Act
        host.ConfigureServices(services => services.AddSingleton<ITestService, AnotherTestService>());
        var secondService = host.GetRequiredService<ITestService>();

        // Assert
        firstService.ShouldBeOfType<TestService>();
        secondService.ShouldBeOfType<AnotherTestService>();
        firstService.ShouldNotBeSameAs(secondService);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ConfigureServices_Should_HandleEmptyConfiguration()
    {
        // Arrange
        var host = new TestHost();

        // Act
        var serviceProvider = host.ConfigureServices(services => { });

        // Assert
        serviceProvider.ShouldNotBeNull();
        host.GetServices().ShouldNotBeNull();
    }

    [Fact]
    public void ConfigureServices_Should_HandleMultipleServiceRegistrations()
    {
        // Arrange
        var host = new TestHost();

        // Act
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddTransient<IDependentService, DependentService>();
            services.AddScoped<TestHost>();
        });

        // Assert
        host.GetRequiredService<ITestService>().ShouldNotBeNull();
        host.GetRequiredService<IDependentService>().ShouldNotBeNull();
    }

    [Fact]
    public void Services_Can_Be_Replaced_Explicitly()
    {
        // Arrange
        var host = new TestHost();
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var customProvider = services.BuildServiceProvider();

        // Act
        host.SetServices(customProvider);

        // Assert
        host.GetServices().ShouldBe(customProvider);
        host.GetRequiredService<ITestService>().ShouldNotBeNull();
    }

    [Fact]
    public void GetRequiredService_With_Generic_Type_Constraint_Should_Work()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Act
        var service = host.GetRequiredService<TestHost, ITestService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<TestService>();
    }

    [Fact]
    public void GetService_With_Generic_Type_Constraint_Should_Work()
    {
        // Arrange
        var host = new TestHost();
        host.ConfigureServices(services => services.AddSingleton<ITestService, TestService>());

        // Act
        var service = host.GetService<TestHost, ITestService>();

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<TestService>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Complete_Workflow_Should_Work_EndToEnd()
    {
        // Arrange
        var host = new TestHost();

        // Act - Configure
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddTransient<IDependentService, DependentService>();
        });

        // Act - Resolve
        var testService = host.GetRequiredService<ITestService>();
        var dependentService = host.GetRequiredService<IDependentService>();

        // Assert
        testService.ShouldNotBeNull();
        testService.GetMessage().ShouldBe("Hello from TestService");
        dependentService.ShouldNotBeNull();
        dependentService.GetValue().ShouldContain("Hello from TestService");
    }

    [Fact]
    public void Manual_ServiceCollection_Workflow_Should_Work()
    {
        // Arrange
        var host = new TestHost();

        // Act - Get collection
        var services = host.GetServiceCollection(s =>
        {
            s.AddSingleton<ITestService, TestService>();
        });

        // Act - Add more services
        services.AddTransient<IDependentService, DependentService>();

        // Act - BuildServices
        host.BuildServices(services);

        // Assert
        host.GetRequiredService<ITestService>().ShouldNotBeNull();
        host.GetRequiredService<IDependentService>().ShouldNotBeNull();
    }

    #endregion
}

