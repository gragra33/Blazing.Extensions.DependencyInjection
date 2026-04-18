namespace WpfExample.ViewModels;

/// <summary>
/// Demonstrates all three <see cref="IDecoratorCache"/> return-type variants (sync, Task&lt;T&gt;,
/// ValueTask&lt;T&gt;) together with per-key invalidation and runtime backend switching.
/// </summary>
[AutoRegister(ServiceLifetime.Transient)]
public partial class CacheViewModel : ViewModelBase
{
    private readonly IProductCatalogService _catalog;
    private readonly SwitchableDecoratorCache _switchable;
    private readonly IBlazingCacheInvalidator<IProductCatalogService> _invalidator;

    // ── observable properties ──────────────────────────────────────────────

    [ObservableProperty] private int _productId = 1;
    [ObservableProperty] private string _activeBackend = "Default";
    [ObservableProperty] private int _backendCallCount;
    [ObservableProperty] private bool _isBusy;

    // per-method results
    [ObservableProperty] private string _syncResult = string.Empty;
    [ObservableProperty] private string _syncStatus = string.Empty;
    [ObservableProperty] private string _taskResult = string.Empty;
    [ObservableProperty] private string _taskStatus = string.Empty;
    [ObservableProperty] private string _valueTaskResult = string.Empty;
    [ObservableProperty] private string _valueTaskStatus = string.Empty;

    /// <summary>Scrollable activity log shown in the ListView.</summary>
    public ObservableCollection<LogEntry> ActivityLog { get; } = [];

    /// <summary>
    /// Initialises a new instance of <see cref="CacheViewModel"/>.
    /// </summary>
    public CacheViewModel(
        IProductCatalogService catalog,
        SwitchableDecoratorCache switchable,
        IBlazingCacheInvalidator<IProductCatalogService> invalidator)
    {
        _catalog = catalog;
        _switchable = switchable;
        _invalidator = invalidator;
        ActiveBackend = _switchable.CurrentBackend.ToString();
    }

    // ── backend switching ─────────────────────────────────────────────────

    /// <summary>Switches the cache backend to <see cref="CacheBackend.Default"/>.</summary>
    [RelayCommand]
    private void SwitchToDefault()
    {
        _switchable.SwitchTo(CacheBackend.Default);
        ActiveBackend = _switchable.CurrentBackend.ToString();
        AddLog($"Switched backend → Default (ConcurrentDictionary)", isInfo: true);
    }

    /// <summary>Switches the cache backend to <see cref="CacheBackend.MemoryCache"/>.</summary>
    [RelayCommand]
    private void SwitchToMemoryCache()
    {
        _switchable.SwitchTo(CacheBackend.MemoryCache);
        ActiveBackend = _switchable.CurrentBackend.ToString();
        AddLog("Switched backend → MemoryCache (IMemoryCache)", isInfo: true);
    }

    /// <summary>Switches the cache backend to <see cref="CacheBackend.HybridCache"/>.</summary>
    [RelayCommand]
    private void SwitchToHybridCache()
    {
        _switchable.SwitchTo(CacheBackend.HybridCache);
        ActiveBackend = _switchable.CurrentBackend.ToString();
        AddLog("Switched backend → HybridCache (L1+L2)", isInfo: true);
    }

    // ── method calls ──────────────────────────────────────────────────────

    /// <summary>Calls the synchronous <c>GetName(id)</c> method and records hit/miss.</summary>
    [RelayCommand]
    private void ExecuteGetName()
    {
        IsBusy = true;
        var before = ProductCatalogService.TotalBackendCallCount;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = _catalog.GetName(ProductId);

        sw.Stop();
        var hit = ProductCatalogService.TotalBackendCallCount == before;
        BackendCallCount = ProductCatalogService.TotalBackendCallCount;
        SyncResult = result;
        SyncStatus = hit ? $"⚡ HIT ({sw.ElapsedMilliseconds} ms)" : $"💾 MISS ({sw.ElapsedMilliseconds} ms)";
        AddLog($"GetName(#{ProductId}) → \"{result}\" [{(hit ? "HIT" : "MISS")} {sw.ElapsedMilliseconds} ms]", hit);
        IsBusy = false;
    }

    /// <summary>Calls the <c>Task&lt;string&gt; GetNameAsync(id)</c> method and records hit/miss.</summary>
    [RelayCommand]
    private async Task ExecuteGetNameTaskAsync()
    {
        IsBusy = true;
        var before = ProductCatalogService.TotalBackendCallCount;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await _catalog.GetNameAsync(ProductId);

        sw.Stop();
        var hit = ProductCatalogService.TotalBackendCallCount == before;
        BackendCallCount = ProductCatalogService.TotalBackendCallCount;
        TaskResult = result;
        TaskStatus = hit ? $"⚡ HIT ({sw.ElapsedMilliseconds} ms)" : $"💾 MISS ({sw.ElapsedMilliseconds} ms)";
        AddLog($"GetNameAsync(#{ProductId}) → \"{result}\" [{(hit ? "HIT" : "MISS")} {sw.ElapsedMilliseconds} ms]", hit);
        IsBusy = false;
    }

    /// <summary>Calls the <c>ValueTask&lt;int&gt; GetCountAsync(id)</c> method and records hit/miss.</summary>
    [RelayCommand]
    private async Task ExecuteGetCountValueTaskAsync()
    {
        IsBusy = true;
        var before = ProductCatalogService.TotalBackendCallCount;
        var sw = System.Diagnostics.Stopwatch.StartNew();

        var result = await _catalog.GetCountAsync(ProductId);

        sw.Stop();
        var hit = ProductCatalogService.TotalBackendCallCount == before;
        BackendCallCount = ProductCatalogService.TotalBackendCallCount;
        ValueTaskResult = result.ToString();
        ValueTaskStatus = hit ? $"⚡ HIT ({sw.ElapsedMilliseconds} ms)" : $"💾 MISS ({sw.ElapsedMilliseconds} ms)";
        AddLog($"GetCountAsync(#{ProductId}) → stock: {result} [{(hit ? "HIT" : "MISS")} {sw.ElapsedMilliseconds} ms]", hit);
        IsBusy = false;
    }

    // ── per-key invalidation ──────────────────────────────────────────────

    /// <summary>Invalidates the <c>GetName</c> cache entry for the current product ID.</summary>
    [RelayCommand]
    private async Task InvalidateGetNameSyncAsync()
    {
        await _invalidator.InvalidateAsync(nameof(IProductCatalogService.GetName), [ProductId.ToString()]);
        SyncStatus = string.Empty;
        AddLog($"Invalidated GetName(#{ProductId})", isInfo: true);
    }

    /// <summary>Invalidates the <c>GetNameAsync</c> cache entry for the current product ID.</summary>
    [RelayCommand]
    private async Task InvalidateGetNameTaskAsync()
    {
        await _invalidator.InvalidateAsync(nameof(IProductCatalogService.GetNameAsync), [ProductId.ToString()]);
        TaskStatus = string.Empty;
        AddLog($"Invalidated GetNameAsync(#{ProductId})", isInfo: true);
    }

    /// <summary>Invalidates the <c>GetCountAsync</c> cache entry for the current product ID.</summary>
    [RelayCommand]
    private async Task InvalidateGetCountValueTaskAsync()
    {
        await _invalidator.InvalidateAsync(nameof(IProductCatalogService.GetCountAsync), [ProductId.ToString()]);
        ValueTaskStatus = string.Empty;
        AddLog($"Invalidated GetCountAsync(#{ProductId})", isInfo: true);
    }

    // ── log helper ────────────────────────────────────────────────────────
    private void AddLog(string message, bool isHit = false, bool isInfo = false)
    {
        var entry = new LogEntry(
            Time: DateTime.Now.ToString("HH:mm:ss.fff"),
            Message: message,
            IsHit: isHit,
            IsInfo: isInfo && !isHit);

        Application.Current.Dispatcher.Invoke(() =>
        {
            ActivityLog.Insert(0, entry);
            if (ActivityLog.Count > 50) ActivityLog.RemoveAt(ActivityLog.Count - 1);
        });
    }

    /// <summary>A single entry in the activity log.</summary>
    /// <param name="Time">Timestamp of the operation.</param>
    /// <param name="Message">Human-readable description.</param>
    /// <param name="IsHit">True when the result came from cache.</param>
    /// <param name="IsInfo">True for informational entries (backend switch / invalidation).</param>
    public record LogEntry(string Time, string Message, bool IsHit, bool IsInfo);
}
