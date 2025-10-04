using Microsoft.Extensions.DependencyInjection;
using System;

namespace Blazing.Extensions.DependencyInjection.Tests;

public class SimpleTest
{
    public interface ITestService { }
    public class TestService : ITestService { }

    [Fact]
    public void Show_Runtime_Version()
    {
        Console.WriteLine($".NET Runtime: {Environment.Version}");
        Console.WriteLine($"Framework Description: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
    }

    [Fact]
    public void Test_Service_Provider_Works()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var provider = services.BuildServiceProvider();
        
        var service = provider.GetService<ITestService>();
        
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }
    
    [Fact]
    public void Test_ConfigureServices_Returns_Working_Provider()
    {
        var host = new object();
        
        var returned = host.ConfigureServices(s => s.AddSingleton<ITestService, TestService>());
        
        Assert.NotNull(returned);
        
        var service = returned.GetService<ITestService>();
        Assert.NotNull(service);
        Assert.IsType<TestService>(service);
    }
}
