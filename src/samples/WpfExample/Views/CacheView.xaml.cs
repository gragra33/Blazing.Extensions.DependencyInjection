namespace WpfExample.Views;

/// <summary>
/// Interactive demo showing <see cref="IDecoratorCache"/> with sync, Task&lt;T&gt;, and
/// ValueTask&lt;T&gt; method caching plus per-key invalidation and runtime backend switching.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ITabView))]
public partial class CacheView : UserControl, ITabView
{
    /// <inheritdoc/>
    public string TabHeader => "🗄️ Caching";

    /// <inheritdoc/>
    public int Order => 5;

    /// <summary>
    /// Initialises a new instance of <see cref="CacheView"/>.
    /// Resolves <see cref="CacheViewModel"/> via dependency injection and sets it as DataContext.
    /// </summary>
    public CacheView()
    {
        InitializeComponent();

        // VIEW-FIRST PATTERN: Resolve CacheViewModel via DI
        DataContext = Application.Current.GetRequiredService<CacheViewModel>();

        Console.WriteLine("CacheView: Constructor called — CacheViewModel resolved via DI");
    }
}
