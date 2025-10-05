namespace ConsoleExample.Examples;

[AutoRegister(ServiceLifetime.Transient, typeof(IExample))]
public class BasicServiceConfigurationExample : IExample
{
    public string Name => "Basic Service Configuration";
    
    public void Run()
    {
        Console.WriteLine("Setting up main service provider...");
        ConsoleHelper.LogTiming("Main ServiceProvider setup starting");
        
        var host = new ApplicationHost();
        host.ConfigureServices(services =>
        {
            services.AddSingleton<IRepository, DatabaseRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddTransient<INotificationService, EmailNotificationService>();
            services.AddTransient<ILoggingService, ConsoleLoggingService>();
            services.Register();
        });
        
        ConsoleHelper.LogTiming("Main ServiceProvider setup completed");
        
        Console.WriteLine("Testing basic service resolution...");
        var userService = host.GetRequiredService<IUserService>();
        var user = userService.GetUser(1);
        Console.WriteLine($"Retrieved user: {user.Name} ({user.Email})");
    }
}