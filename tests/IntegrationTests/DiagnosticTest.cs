using System;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Blazing.Extensions.DependencyInjection.Tests;

public class DiagnosticTest
{
    [Fact]
    public void Test_ConditionalWeakTable_Directly()
    {
        var table = new ConditionalWeakTable<object, IServiceProvider>();
        var host = new object();
        var services = new ServiceCollection();
        services.AddSingleton<string>("test");
        var provider = services.BuildServiceProvider();
        
        // Add to table
        table.AddOrUpdate(host, provider);
        
        // Try to get it back
        var result = table.TryGetValue(host, out var retrieved);
        
        Assert.True(result, "TryGetValue should return true");
        Assert.NotNull(retrieved);
        Assert.Same(provider, retrieved);
    }

    [Fact]
    public void Test_ConfigureServices_And_GetServices()
    {
        var host = new object();
        
        // Use ConfigureServices extension method
        var returned = host.ConfigureServices(s => s.AddSingleton<string>("test"));
        
        Assert.NotNull(returned);
        
        // Try to get it back with GetServices
        var retrieved = host.GetServices();
        
        Assert.NotNull(retrieved);
        Assert.Same(returned, retrieved);
    }
    
    [Fact]
    public void Test_SetServices_And_GetServices()
    {
        var host = new object();
        var services = new ServiceCollection();
        services.AddSingleton<string>("test");
        var provider = services.BuildServiceProvider();
        
        // Use SetServices
        host.SetServices(provider);
        
        // Try to get it back
        var retrieved = host.GetServices();
        
        Assert.NotNull(retrieved);
        Assert.Same(provider, retrieved);
    }
}
