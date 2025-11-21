using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftServiceProvider = Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions;

namespace Blazing.Extensions.DependencyInjection.Tests;

public class ServiceExtensionsTests : IDisposable
{
    private readonly List<object> _testInstances = new();

    // Helper method to create and track test instances
    private T CreateTestInstance<T>() where T : new()
    {
        var instance = new T();
        _testInstances.Add(instance);
        return instance;
    }

    // Cleanup after each test
    public void Dispose()
    {
        foreach (var instance in _testInstances)
        {
            try
            {
                // Clear the service provider for this instance to prevent test interference
                instance.SetServices(null);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        _testInstances.Clear();
    }

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

    public class DependentService(ITestService testService) : IDependentService
    {
        public string GetValue() => $"Dependent: {testService.GetMessage()}";
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
        var host = CreateTestInstance<TestHost>();

        // Act
        var services = host.GetServices();

        // Assert
        services.ShouldBeNull();
    }

    [Fact]
    public void Services_Property_Should_Return_ConfiguredProvider()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();

        // Act
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
        });

        // Assert
        host.GetServices().ShouldNotBeNull();
        MicrosoftServiceProvider.GetService<ITestService>(host.GetServices()!).ShouldNotBeNull();
    }

    [Fact]
    public void Services_Property_Can_Be_Set_To_Null()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
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
        var host = CreateTestInstance<TestHost>();

        // Act
        var serviceProvider = host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
        });

        // Assert
        serviceProvider.ShouldNotBeNull();
        MicrosoftServiceProvider.GetService<ITestService>(serviceProvider).ShouldBeOfType<TestService>();
    }

    [Fact]
    public void ConfigureServices_Should_RegisterSingletonServices()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();

        // Act
        var serviceProvider = host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
        });

        // Assert
        var service1 = MicrosoftServiceProvider.GetRequiredService<ITestService>(serviceProvider);
        var service2 = MicrosoftServiceProvider.GetRequiredService<ITestService>(serviceProvider);
        service1.ShouldBeSameAs(service2);
    }

    [Fact]
    public void ConfigureServices_Should_RegisterTransientServices()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();

        // Act
        var serviceProvider = host.ConfigureServices(services =>
        {
            services.AddTransient<ITestService, TestService>();
        });

        // Assert
        var service1 = MicrosoftServiceProvider.GetRequiredService<ITestService>(serviceProvider);
        var service2 = MicrosoftServiceProvider.GetRequiredService<ITestService>(serviceProvider);
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
                MicrosoftServiceProvider.GetRequiredService<ITestService>(provider).ShouldNotBeNull();
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
                warmedUpService = MicrosoftServiceProvider.GetRequiredService<ITestService>(provider);
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
        MicrosoftServiceProvider.GetService<ITestService>(serviceProvider).ShouldNotBeNull();
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

    #region GetServiceCollection and BuildServiceProvider Tests

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
    public void BuildServiceProvider_Should_BuildAndAssignProvider()
    {
        // Arrange
        var host = new TestHost();
        var services = host.GetServiceCollection(s =>
        {
            s.AddSingleton<ITestService, TestService>();
        });

        // Act
        var serviceProvider = host.BuildServiceProvider(services);

        // Assert
        serviceProvider.ShouldNotBeNull();
        host.GetServices().ShouldBe(serviceProvider);
        host.GetRequiredService<ITestService>().ShouldNotBeNull();
    }

    [Fact]
    public void BuildServiceProvider_Should_AcceptServiceProviderOptions()
    {
        // Arrange
        var host = new TestHost();
        var services = host.GetServiceCollection(s =>
        {
            s.AddTransient<ITestService, TestService>();
        });

        // Act
        var serviceProvider = host.BuildServiceProvider(services, new ServiceProviderOptions
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
        ex.Message.ShouldBe("Service provider is not configured for TestHost. Call ConfigureServices() first or use serviceProvider.GetRequiredService<T>() directly.");
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

        // Act - BuildServiceProvider
        host.BuildServiceProvider(services);

        // Assert
        host.GetRequiredService<ITestService>().ShouldNotBeNull();
        host.GetRequiredService<IDependentService>().ShouldNotBeNull();
    }

    #endregion

    #region Diagnostic Tests (ConditionalWeakTable behavior)

    [Fact]
    public void ConditionalWeakTable_Should_Work_Directly()
    {
        // Arrange
        var table = new System.Runtime.CompilerServices.ConditionalWeakTable<object, IServiceProvider>();
        var host = new object();
        var services = new ServiceCollection();
        services.AddSingleton<string>("test");
        var provider = services.BuildServiceProvider();
        
        // Act
        table.AddOrUpdate(host, provider);
        var result = table.TryGetValue(host, out var retrieved);
        
        // Assert
        result.ShouldBeTrue();
        retrieved.ShouldNotBeNull();
        retrieved.ShouldBeSameAs(provider);
    }

    [Fact]
    public void ConfigureServices_And_GetServices_Should_Work_Together()
    {
        // Arrange
        var host = new object();
        
        // Act
        var returned = host.ConfigureServices(s => s.AddSingleton<string>("test"));
        var retrieved = host.GetServices();
        
        // Assert
        returned.ShouldNotBeNull();
        retrieved.ShouldNotBeNull();
        retrieved.ShouldBeSameAs(returned);
    }
    
    [Fact]
    public void SetServices_And_GetServices_Should_Work_Together()
    {
        // Arrange
        var host = new object();
        var services = new ServiceCollection();
        services.AddSingleton<string>("test");
        var provider = services.BuildServiceProvider();
        
        // Act
        host.SetServices(provider);
        var retrieved = host.GetServices();
        
        // Assert
        retrieved.ShouldNotBeNull();
        retrieved.ShouldBeSameAs(provider);
    }

    #endregion

    #region Keyed Services Tests

    public interface IKeyedTestService
    {
        string GetMessage();
    }

    public class PrimaryKeyedService : IKeyedTestService
    {
        public string GetMessage() => "Primary Service";
    }

    public class SecondaryKeyedService : IKeyedTestService
    {
        public string GetMessage() => "Secondary Service";
    }

    public class DatabaseService : IKeyedTestService
    {
        public string GetMessage() => "Database Service";
    }

    [Fact]
    public void GetRequiredKeyedService_Should_Return_Correct_Service()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>("primary");
            services.AddKeyedSingleton<IKeyedTestService, SecondaryKeyedService>("secondary");
        });

        // Act
        var primaryService = host.GetRequiredKeyedService<IKeyedTestService>("primary");
        var secondaryService = host.GetRequiredKeyedService<IKeyedTestService>("secondary");

        // Assert
        primaryService.ShouldNotBeNull();
        primaryService.ShouldBeOfType<PrimaryKeyedService>();
        primaryService.GetMessage().ShouldBe("Primary Service");

        secondaryService.ShouldNotBeNull();
        secondaryService.ShouldBeOfType<SecondaryKeyedService>();
        secondaryService.GetMessage().ShouldBe("Secondary Service");
    }

    [Fact]
    public void GetKeyedService_Should_Return_Correct_Service_Or_Null()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>("primary");
        });

        // Act
        var existingService = host.GetKeyedService<IKeyedTestService>("primary");
        var nonExistentService = host.GetKeyedService<IKeyedTestService>("nonexistent");

        // Assert
        existingService.ShouldNotBeNull();
        existingService.ShouldBeOfType<PrimaryKeyedService>();
        nonExistentService.ShouldBeNull();
    }

    [Fact]
    public void GetRequiredKeyedService_Should_Throw_When_Service_Not_Found()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>("primary");
        });

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => 
            host.GetRequiredKeyedService<IKeyedTestService>("nonexistent"));
    }

    [Fact]
    public void GetRequiredKeyedService_Should_Throw_When_No_ServiceProvider()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => 
            host.GetRequiredKeyedService<IKeyedTestService>("primary"));
    }

    [Fact]
    public void Multiple_Keyed_Services_Should_Be_Independent()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddKeyedTransient<IKeyedTestService, PrimaryKeyedService>("primary");
            services.AddKeyedTransient<IKeyedTestService, SecondaryKeyedService>("secondary");
            services.AddKeyedTransient<IKeyedTestService, DatabaseService>("database");
        });

        // Act
        var primary1 = host.GetRequiredKeyedService<IKeyedTestService>("primary");
        var primary2 = host.GetRequiredKeyedService<IKeyedTestService>("primary");
        var secondary = host.GetRequiredKeyedService<IKeyedTestService>("secondary");
        var database = host.GetRequiredKeyedService<IKeyedTestService>("database");

        // Assert
        primary1.ShouldNotBeNull();
        primary1.ShouldBeOfType<PrimaryKeyedService>();
        
        primary2.ShouldNotBeNull();
        primary2.ShouldBeOfType<PrimaryKeyedService>();
        primary1.ShouldNotBeSameAs(primary2); // Transient services should be different instances

        secondary.ShouldNotBeNull();
        secondary.ShouldBeOfType<SecondaryKeyedService>();

        database.ShouldNotBeNull();
        database.ShouldBeOfType<DatabaseService>();

        primary1.GetMessage().ShouldBe("Primary Service");
        secondary.GetMessage().ShouldBe("Secondary Service");
        database.GetMessage().ShouldBe("Database Service");
    }

    [Fact]
    public void Keyed_Singleton_Services_Should_Return_Same_Instance()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>("primary");
        });

        // Act
        var instance1 = host.GetRequiredKeyedService<IKeyedTestService>("primary");
        var instance2 = host.GetRequiredKeyedService<IKeyedTestService>("primary");

        // Assert
        instance1.ShouldNotBeNull();
        instance2.ShouldNotBeNull();
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void Keyed_Services_Can_Coexist_With_Regular_Services()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddSingleton<ITestService, TestService>();
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>("primary");
        });

        // Act
        var regularService = host.GetRequiredService<ITestService>();
        var keyedService = host.GetRequiredKeyedService<IKeyedTestService>("primary");

        // Assert
        regularService.ShouldNotBeNull();
        regularService.ShouldBeOfType<TestService>();
        
        keyedService.ShouldNotBeNull();
        keyedService.ShouldBeOfType<PrimaryKeyedService>();
    }

    [Fact]
    public void Keyed_Services_Work_With_Null_Keys()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>(null);
        });

        // Act
        var service = host.GetRequiredKeyedService<IKeyedTestService>(null);

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<PrimaryKeyedService>();
    }

    [Fact]
    public void Keyed_Services_Work_With_Complex_Keys()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        var complexKey = new { Environment = "Production", Region = "US-East" };
        
        host.ConfigureServices(services =>
        {
            services.AddKeyedSingleton<IKeyedTestService, PrimaryKeyedService>(complexKey);
        });

        // Act
        var service = host.GetRequiredKeyedService<IKeyedTestService>(complexKey);

        // Assert
        service.ShouldNotBeNull();
        service.ShouldBeOfType<PrimaryKeyedService>();
    }

    #endregion

    #region AutoRegister Attribute Tests

    public interface IAutoTestService
    {
        string GetMessage();
    }

    [AutoRegister(ServiceLifetime.Singleton)]
    public class AutoSingletonService : IAutoTestService
    {
        public string GetMessage() => "Auto Singleton Service";
    }

    [AutoRegister(ServiceLifetime.Transient)]
    public class AutoTransientService : IAutoTestService
    {
        public string GetMessage() => "Auto Transient Service";
    }

    [AutoRegister(ServiceLifetime.Scoped, typeof(IAutoTestService))]
    public class AutoScopedWithInterfaceService : IAutoTestService
    {
        public string GetMessage() => "Auto Scoped Service";
    }

    [AutoRegister]
    public class AutoDefaultService : IAutoTestService
    {
        public string GetMessage() => "Auto Default Service";
    }

    [Fact]
    public void AutoRegister_Should_Register_Services_From_Current_Assembly()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.Register();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var singletonService = MicrosoftServiceProvider.GetService<AutoSingletonService>(serviceProvider);
        singletonService.ShouldNotBeNull();
        singletonService.ShouldBeOfType<AutoSingletonService>();

        var singletonAsInterface = MicrosoftServiceProvider.GetService<IAutoTestService>(serviceProvider);
        // Note: Multiple registrations of same interface - will get the last one registered
        singletonAsInterface.ShouldNotBeNull();
    }

    [Fact]
    public void AutoRegister_Should_Register_Singleton_Service_Once()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.Register();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var instance1 = MicrosoftServiceProvider.GetService<AutoSingletonService>(serviceProvider);
        var instance2 = MicrosoftServiceProvider.GetService<AutoSingletonService>(serviceProvider);

        instance1.ShouldNotBeNull();
        instance2.ShouldNotBeNull();
        instance1.ShouldBeSameAs(instance2);
    }

    [Fact]
    public void AutoRegister_Should_Register_Transient_Service_Multiple_Times()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.Register();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var instance1 = MicrosoftServiceProvider.GetService<AutoTransientService>(serviceProvider);
        var instance2 = MicrosoftServiceProvider.GetService<AutoTransientService>(serviceProvider);

        instance1.ShouldNotBeNull();
        instance2.ShouldNotBeNull();
        instance1.ShouldNotBeSameAs(instance2);
    }

    [Fact]
    public void AutoRegister_Should_Register_Service_With_Specific_Interface()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.Register();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Service should be registered as the specified interface type
        var service = MicrosoftServiceProvider.GetService<IAutoTestService>(serviceProvider);
        service.ShouldNotBeNull();
        
        // Should also be able to get the concrete type
        var concreteService = MicrosoftServiceProvider.GetService<AutoScopedWithInterfaceService>(serviceProvider);
        concreteService.ShouldNotBeNull();
    }

    [Fact]
    public void AutoRegister_Should_Use_Default_Transient_lifetime()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.Register();
        var serviceProvider = services.BuildServiceProvider();

        // Assert - Default scope should be Transient
        var instance1 = MicrosoftServiceProvider.GetService<AutoDefaultService>(serviceProvider);
        var instance2 = MicrosoftServiceProvider.GetService<AutoDefaultService>(serviceProvider);

        instance1.ShouldNotBeNull();
        instance2.ShouldNotBeNull();
        instance1.ShouldNotBeSameAs(instance2); // Transient = different instances
    }

    [Fact]
    public void AutoRegister_Should_Work_With_Specific_Assembly()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentAssembly = typeof(ServiceExtensionsTests).Assembly;

        // Act
        services.Register(currentAssembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = MicrosoftServiceProvider.GetService<AutoSingletonService>(serviceProvider);
        service.ShouldNotBeNull();
    }

    [Fact]
    public void AutoRegister_Should_Work_With_Multiple_Assemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var currentAssembly = typeof(ServiceExtensionsTests).Assembly;
        var dependencyInjectionAssembly = typeof(ServiceExtensions).Assembly;

        // Act
        services.Register(currentAssembly, dependencyInjectionAssembly);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var service = MicrosoftServiceProvider.GetService<AutoSingletonService>(serviceProvider);
        service.ShouldNotBeNull();
    }

    #endregion

    #region Service Scope Tests

    public interface IScopedService
    {
        string GetScopedId();
    }

    public class ScopedService : IScopedService
    {
        private readonly string _id = Guid.NewGuid().ToString();
        public string GetScopedId() => _id;
    }

    [Fact]
    public void CreateScope_Should_Create_New_ServiceScope()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddScoped<IScopedService, ScopedService>();
        });

        // Act
        using var scope = host.CreateScope();
        var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();

        // Assert
        scope.ShouldNotBeNull();
        scopedService.ShouldNotBeNull();
    }

    [Fact]
    public void GetScopedService_WithResult_Should_Return_Value()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddScoped<IScopedService, ScopedService>();
        });

        // Act
        var scopedId = host.GetScopedService<IScopedService, string>(service =>
            service.GetScopedId());

        // Assert
        scopedId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetScopedServiceAsync_WithResult_Should_Return_Value()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddScoped<IScopedService, ScopedService>();
        });

        // Act
        var scopedId = await host.GetScopedServiceAsync<IScopedService, string>(async service =>
        {
            await Task.CompletedTask;
            return service.GetScopedId();
        });

        // Assert
        scopedId.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public void GetScopedKeyedService_Should_Work_With_Keyed_Services()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddKeyedScoped<IScopedService, ScopedService>("scoped");
        });

        // Act
        var result = string.Empty;
        host.GetScopedKeyedService<IScopedService>("scoped", service =>
        {
            result = service.GetScopedId();
        });

        // Assert
        result.ShouldNotBeNullOrEmpty();
    }

    #endregion

    #region Lazy Service Tests

    public interface IExpensiveService
    {
        string GetData();
    }

    public class ExpensiveService : IExpensiveService
    {
        public ExpensiveService()
        {
            // Simulate expensive operation
            System.Threading.Thread.Sleep(10);
        }

        public string GetData() => "Expensive Data";
    }

    [Fact]
    public void AddLazySingleton_Should_Defer_Service_Creation()
    {
        // Arrange
        var services = new ServiceCollection();
        var creationTime = DateTime.UtcNow;
        services.AddLazySingleton<IExpensiveService, ExpensiveService>();

        // Act
        var provider = services.BuildServiceProvider();
        var lazyService = ServiceProviderServiceExtensions.GetRequiredService<Lazy<IExpensiveService>>(provider);

        // Assert
        lazyService.ShouldNotBeNull();
        lazyService.IsValueCreated.ShouldBeFalse();

        // Act - Access the value
        var service = lazyService.Value;

        // Assert
        service.ShouldNotBeNull();
        lazyService.IsValueCreated.ShouldBeTrue();
    }

    [Fact]
    public void AddLazySingleton_Should_Return_Same_Instance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazySingleton<IExpensiveService, ExpensiveService>();
        var provider = services.BuildServiceProvider();

        // Act
        var lazy1 = ServiceProviderServiceExtensions.GetRequiredService<Lazy<IExpensiveService>>(provider);
        var lazy2 = ServiceProviderServiceExtensions.GetRequiredService<Lazy<IExpensiveService>>(provider);

        // Assert
        lazy1.Value.ShouldBeSameAs(lazy2.Value);
    }

    [Fact]
    public void AddLazyTransient_Should_Create_New_Instances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLazyTransient<IExpensiveService, ExpensiveService>();
        var provider = services.BuildServiceProvider();

        // Act
        var lazy1 = ServiceProviderServiceExtensions.GetRequiredService<Lazy<IExpensiveService>>(provider);
        var lazy2 = ServiceProviderServiceExtensions.GetRequiredService<Lazy<IExpensiveService>>(provider);

        // Assert
        lazy1.Value.ShouldNotBeSameAs(lazy2.Value);
    }

    [Fact]
    public void GetLazyService_Should_Return_Lazy_Wrapper()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddLazySingleton<IExpensiveService, ExpensiveService>();
        });

        // Act
        var lazyService = host.GetLazyService<IExpensiveService>();

        // Assert
        lazyService.ShouldNotBeNull();
        lazyService.IsValueCreated.ShouldBeFalse();
    }

    [Fact]
    public void GetLazyKeyedService_Should_Return_Lazy_Keyed_Service()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddLazyKeyedSingleton<IExpensiveService, ExpensiveService>("expensive");
        });

        // Act
        var lazyService = host.GetLazyKeyedService<IExpensiveService>("expensive");

        // Assert
        lazyService.ShouldNotBeNull();
        lazyService.IsValueCreated.ShouldBeFalse();
    }

    #endregion

    #region Generic Service Tests

    public interface IRepository<T>
    {
        string GetTypeName();
    }

    public class Repository<T> : IRepository<T>
    {
        public string GetTypeName() => typeof(T).Name;
    }

    public class User { }
    public class Product { }

    [Fact]
    public void AddGenericSingleton_Should_Register_Open_Generic()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGenericSingleton(typeof(IRepository<>), typeof(Repository<>));
        var provider = services.BuildServiceProvider();

        // Assert
        var userRepo = ServiceProviderServiceExtensions.GetRequiredService<IRepository<User>>(provider);
        userRepo.ShouldNotBeNull();
        userRepo.GetTypeName().ShouldBe("User");
    }

    [Fact]
    public void AddGenericSingleton_Should_Return_Same_Instance()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericSingleton(typeof(IRepository<>), typeof(Repository<>));
        var provider = services.BuildServiceProvider();

        // Act
        var repo1 = ServiceProviderServiceExtensions.GetRequiredService<IRepository<User>>(provider);
        var repo2 = ServiceProviderServiceExtensions.GetRequiredService<IRepository<User>>(provider);

        // Assert
        repo1.ShouldBeSameAs(repo2);
    }

    [Fact]
    public void AddGenericTransient_Should_Create_New_Instances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddGenericTransient(typeof(IRepository<>), typeof(Repository<>));
        var provider = services.BuildServiceProvider();

        // Act
        var repo1 = ServiceProviderServiceExtensions.GetRequiredService<IRepository<User>>(provider);
        var repo2 = ServiceProviderServiceExtensions.GetRequiredService<IRepository<User>>(provider);

        // Assert
        repo1.ShouldNotBeSameAs(repo2);
    }

    [Fact]
    public void AddGenericServices_Should_Register_Multiple_Generics()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddGenericServices(ServiceLifetime.Singleton,
            (typeof(IRepository<>), typeof(Repository<>)));
        var provider = services.BuildServiceProvider();

        // Assert
        var userRepo = ServiceProviderServiceExtensions.GetRequiredService<IRepository<User>>(provider);
        var productRepo = ServiceProviderServiceExtensions.GetRequiredService<IRepository<Product>>(provider);
        userRepo.ShouldNotBeNull();
        productRepo.ShouldNotBeNull();
    }

    #endregion

    #region Service Factory Tests

    public interface IConfigurableService
    {
        string GetConfig();
    }

    public class ConfigurableService : IConfigurableService
    {
        private readonly string _config;
        public ConfigurableService(string config) => _config = config;
        public string GetConfig() => _config;
    }

    [Fact]
    public void RegisterFactory_Should_Register_With_Factory()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterFactory<IConfigurableService>(provider =>
            new ConfigurableService("test-config"));

        // Act
        var provider = services.BuildServiceProvider();
        var service = ServiceProviderServiceExtensions.GetRequiredService<IConfigurableService>(provider);

        // Assert
        service.ShouldNotBeNull();
        service.GetConfig().ShouldBe("test-config");
    }

    [Fact]
    public void RegisterTransientFactory_Should_Create_New_Instances()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterTransientFactory<IConfigurableService>(provider =>
            new ConfigurableService("test"));

        // Act
        var provider = services.BuildServiceProvider();
        var service1 = ServiceProviderServiceExtensions.GetRequiredService<IConfigurableService>(provider);
        var service2 = ServiceProviderServiceExtensions.GetRequiredService<IConfigurableService>(provider);

        // Assert
        service1.ShouldNotBeSameAs(service2);
    }

    [Fact]
    public void RegisterScopedFactory_Should_Create_Per_Scope()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterScopedFactory<IConfigurableService>(provider =>
            new ConfigurableService("test"));

        // Act
        var provider = services.BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        var service1a = ServiceProviderServiceExtensions.GetRequiredService<IConfigurableService>(scope1.ServiceProvider);
        var service1b = ServiceProviderServiceExtensions.GetRequiredService<IConfigurableService>(scope1.ServiceProvider);

        using var scope2 = provider.CreateScope();
        var service2 = ServiceProviderServiceExtensions.GetRequiredService<IConfigurableService>(scope2.ServiceProvider);

        // Assert
        service1a.ShouldBeSameAs(service1b);
        service1a.ShouldNotBeSameAs(service2);
    }

    [Fact]
    public void RegisterConditionalFactory_Should_Return_Based_On_Condition()
    {
        // Arrange
        var services = new ServiceCollection();
        var useAlt = false;
        services.RegisterConditionalFactory<IConfigurableService>(provider =>
            useAlt ? new ConfigurableService("alt") : new ConfigurableService("primary"));

        // Act
        var provider = services.BuildServiceProvider();
        var service = ServiceProviderServiceExtensions.GetRequiredService<IConfigurableService>(provider);

        // Assert
        service.GetConfig().ShouldBe("primary");
    }

    [Fact]
    public void RegisterKeyedFactory_Should_Register_Keyed_Service()
    {
        // Arrange
        var services = new ServiceCollection();
        services.RegisterKeyedFactory<IConfigurableService>("primary", (provider, key) =>
            new ConfigurableService("primary-config"));

        // Act
        var provider = services.BuildServiceProvider();
        var service = ServiceProviderKeyedServiceExtensions.GetRequiredKeyedService<IConfigurableService>(
            provider, "primary");

        // Assert
        service.ShouldNotBeNull();
        service.GetConfig().ShouldBe("primary-config");
    }

    #endregion

    #region Service Enumeration Tests

    public interface IHandler
    {
        string Handle(string input);
    }

    public class PrimaryHandler : IHandler
    {
        public string Handle(string input) => $"Primary: {input}";
    }

    public class SecondaryHandler : IHandler
    {
        public string Handle(string input) => $"Secondary: {input}";
    }

    [Fact]
    public void GetServices_Should_Return_All_Or_Empty()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddTransient<IHandler, PrimaryHandler>();
        });

        // Act
        var handlers = host.GetServices<IHandler>();

        // Assert
        handlers.ShouldNotBeNull();
        handlers.Count().ShouldBeGreaterThanOrEqualTo(1);
    }

    [Fact]
    public void GetServices_WithPredicate_Should_Filter()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddTransient<IHandler, PrimaryHandler>();
            services.AddTransient<IHandler, SecondaryHandler>();
        });

        // Act
        var filtered = host.GetServices<IHandler>(h => h is PrimaryHandler);

        // Assert
        filtered.Count().ShouldBe(1);
    }

    [Fact]
    public void ForEachService_Should_Execute_For_Each()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        var count = 0;
        host.ConfigureServices(services =>
        {
            services.AddTransient<IHandler, PrimaryHandler>();
            services.AddTransient<IHandler, SecondaryHandler>();
        });

        // Act
        host.ForEachService<IHandler>(h => count++);

        // Assert
        count.ShouldBe(2);
    }

    [Fact]
    public void MapServices_Should_Transform_Services()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddTransient<IHandler, PrimaryHandler>();
            services.AddTransient<IHandler, SecondaryHandler>();
        });

        // Act
        var results = host.MapServices<IHandler, string>(h => h.Handle("test"));

        // Assert
        results.Count().ShouldBe(2);
        results.ShouldContain(r => r.Contains("Primary"));
        results.ShouldContain(r => r.Contains("Secondary"));
    }

    [Fact]
    public void GetFirstService_Should_Return_First_Matching()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddTransient<IHandler, PrimaryHandler>();
            services.AddTransient<IHandler, SecondaryHandler>();
        });

        // Act
        var handler = host.GetFirstService<IHandler>(h => h is PrimaryHandler);

        // Assert
        handler.ShouldNotBeNull();
        handler.ShouldBeOfType<PrimaryHandler>();
    }

    [Fact]
    public void GetFirstServiceOrDefault_Should_Return_Null_If_Not_Found()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddTransient<IHandler, PrimaryHandler>();
        });

        // Act
        var handler = host.GetFirstServiceOrDefault<IHandler>(h => h is SecondaryHandler);

        // Assert
        handler.ShouldBeNull();
    }

    [Fact]
    public void GetServiceCount_Should_Return_Count()
    {
        // Arrange
        var host = CreateTestInstance<TestHost>();
        host.ConfigureServices(services =>
        {
            services.AddTransient<IHandler, PrimaryHandler>();
            services.AddTransient<IHandler, SecondaryHandler>();
        });

        // Act
        var count = host.GetServiceCount<IHandler>();

        // Assert
        count.ShouldBe(2);
    }

    #endregion
}

