using BlazorServerExample.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add your assembly for scanning with Blazing.Extensions.DependencyInjection
builder.Services.AddAssembly(typeof(Program).Assembly);

// Register cache backends — must be done BEFORE Register() so TryAddSingleton<IDecoratorCache> inside
// the generated Register() call does not override the SwitchableDecoratorCache singleton.
builder.Services.AddMemoryCache();
builder.Services.AddHybridCache();
builder.Services.AddSingleton<SwitchableDecoratorCache>();
builder.Services.AddSingleton<IDecoratorCache>(sp => sp.GetRequiredService<SwitchableDecoratorCache>());

// Auto-register all services marked with [AutoRegister] attribute using Blazing.Extensions.DependencyInjection
builder.Services.Register();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
