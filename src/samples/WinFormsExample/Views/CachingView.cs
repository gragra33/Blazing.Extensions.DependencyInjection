using System.Diagnostics;

namespace WinFormsExample.Views;

/// <summary>
/// Interactive caching demo tab for WinForms.
/// Shows backend switching, cache hits/misses, and per-key invalidation.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ITabView))]
public sealed class CachingView : UserControl, ITabView
{
    private readonly IProductCatalogService _catalogService;
    private readonly SwitchableDecoratorCache _switchableCache;
    private readonly IBlazingCacheInvalidator<IProductCatalogService> _cacheInvalidator;

    private readonly NumericUpDown _productIdInput;
    private readonly ComboBox _backendSelect;
    private readonly Label _activeBackendValue;
    private readonly Label _backendCallsValue;
    private readonly Label _cacheHitsValue;
    private readonly TextBox _activityLog;

    private int _backendCalls;
    private int _cacheHits;

    /// <summary>
    /// Gets the tab header text.
    /// </summary>
    public string TabHeader => "🗄️ Caching";

    /// <summary>
    /// Gets the tab sort order.
    /// </summary>
    public int Order => 5;

    /// <summary>
    /// Initializes a new instance of the <see cref="CachingView"/> class.
    /// </summary>
    /// <param name="catalogService">Product catalog service (cached via decorator).</param>
    /// <param name="switchableCache">Switchable cache backend wrapper.</param>
    /// <param name="cacheInvalidator">Per-service cache invalidator.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public CachingView(
        IProductCatalogService catalogService,
        SwitchableDecoratorCache switchableCache,
        IBlazingCacheInvalidator<IProductCatalogService> cacheInvalidator)
    {
        _catalogService = catalogService ?? throw new ArgumentNullException(nameof(catalogService));
        _switchableCache = switchableCache ?? throw new ArgumentNullException(nameof(switchableCache));
        _cacheInvalidator = cacheInvalidator ?? throw new ArgumentNullException(nameof(cacheInvalidator));

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 7,
            AutoScroll = true,
            Padding = new Padding(16),
        };

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(new Label
        {
            Text = "🗄️ Caching Decorator Demo",
            AutoSize = true,
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8),
        });

        root.Controls.Add(new Label
        {
            Text = "Switch backends at runtime and compare cache hits vs backend misses.",
            AutoSize = true,
            ForeColor = Color.DimGray,
            Margin = new Padding(0, 0, 0, 12),
        });

        var backendPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 8),
        };

        backendPanel.Controls.Add(new Label { Text = "Backend:", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        _backendSelect = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220,
            Margin = new Padding(0, 2, 8, 0),
        };
        _backendSelect.Items.AddRange([
            "Default (ConcurrentDictionary)",
            "MemoryCache (IMemoryCache)",
            "HybridCache (L1+L2)",
        ]);
        _backendSelect.SelectedIndex = 0;
        _backendSelect.SelectedIndexChanged += BackendSelect_SelectedIndexChanged;
        backendPanel.Controls.Add(_backendSelect);

        _activeBackendValue = new Label { AutoSize = true, Margin = new Padding(0, 7, 16, 0) };
        _backendCallsValue = new Label { AutoSize = true, Margin = new Padding(0, 7, 16, 0) };
        _cacheHitsValue = new Label { AutoSize = true, Margin = new Padding(0, 7, 0, 0) };
        backendPanel.Controls.Add(_activeBackendValue);
        backendPanel.Controls.Add(_backendCallsValue);
        backendPanel.Controls.Add(_cacheHitsValue);
        root.Controls.Add(backendPanel);

        var productPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0, 0, 0, 10),
        };

        productPanel.Controls.Add(new Label { Text = "Product ID:", AutoSize = true, Margin = new Padding(0, 7, 6, 0) });
        _productIdInput = new NumericUpDown
        {
            Minimum = 1,
            Maximum = 5,
            Value = 1,
            Width = 70,
        };
        productPanel.Controls.Add(_productIdInput);
        root.Controls.Add(productPanel);

        var methodPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 10),
        };

        methodPanel.Controls.Add(CreateActionButton("Call GetName", (_, _) => CallGetName()));
        methodPanel.Controls.Add(CreateActionButton("Call GetNameAsync", async (_, _) => await CallGetNameAsync()));
        methodPanel.Controls.Add(CreateActionButton("Call GetCountAsync", async (_, _) => await CallGetCountAsync()));
        root.Controls.Add(methodPanel);

        var invalidationPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 0, 0, 10),
        };

        invalidationPanel.Controls.Add(CreateActionButton("Invalidate GetName", async (_, _) => await InvalidateAsync(nameof(IProductCatalogService.GetName))));
        invalidationPanel.Controls.Add(CreateActionButton("Invalidate GetNameAsync", async (_, _) => await InvalidateAsync(nameof(IProductCatalogService.GetNameAsync))));
        invalidationPanel.Controls.Add(CreateActionButton("Invalidate GetCountAsync", async (_, _) => await InvalidateAsync(nameof(IProductCatalogService.GetCountAsync))));
        root.Controls.Add(invalidationPanel);

        root.Controls.Add(new Label
        {
            Text = "Activity Log",
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Margin = new Padding(0, 2, 0, 6),
        });

        _activityLog = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
            Font = new Font("Consolas", 9),
            MinimumSize = new Size(300, 180),
        };
        root.Controls.Add(_activityLog);

        Controls.Add(root);

        UpdateCounters();
        Log($"Caching tab initialised with backend: {_switchableCache.CurrentBackend}");
    }

    private Button CreateActionButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            Margin = new Padding(0, 0, 8, 8),
            Padding = new Padding(8, 4, 8, 4),
        };

        button.Click += onClick;
        return button;
    }

    private int CurrentProductId => (int)_productIdInput.Value;

    private void BackendSelect_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var backend = _backendSelect.SelectedIndex switch
        {
            0 => CacheBackend.Default,
            1 => CacheBackend.MemoryCache,
            2 => CacheBackend.HybridCache,
            _ => CacheBackend.Default,
        };

        _switchableCache.SwitchTo(backend);
        UpdateCounters();
        Log($"Switched backend → {backend}");
    }

    private void CallGetName()
    {
        var before = ProductCatalogService.TotalBackendCallCount;
        var timer = Stopwatch.StartNew();

        var result = _catalogService.GetName(CurrentProductId);

        timer.Stop();
        ApplyCounters(before);
        Log($"GetName(#{CurrentProductId}) → '{result}' [{HitOrMiss(before)} {timer.ElapsedMilliseconds} ms]");
    }

    private async Task CallGetNameAsync()
    {
        var before = ProductCatalogService.TotalBackendCallCount;
        var timer = Stopwatch.StartNew();

        var result = await _catalogService.GetNameAsync(CurrentProductId);

        timer.Stop();
        ApplyCounters(before);
        Log($"GetNameAsync(#{CurrentProductId}) → '{result}' [{HitOrMiss(before)} {timer.ElapsedMilliseconds} ms]");
    }

    private async Task CallGetCountAsync()
    {
        var before = ProductCatalogService.TotalBackendCallCount;
        var timer = Stopwatch.StartNew();

        var result = await _catalogService.GetCountAsync(CurrentProductId);

        timer.Stop();
        ApplyCounters(before);
        Log($"GetCountAsync(#{CurrentProductId}) → stock: {result} [{HitOrMiss(before)} {timer.ElapsedMilliseconds} ms]");
    }

    private async Task InvalidateAsync(string methodName)
    {
        await _cacheInvalidator.InvalidateAsync(methodName, [CurrentProductId.ToString()]);
        Log($"Invalidated {methodName}(#{CurrentProductId})");
    }

    private static string HitOrMiss(int backendCallsBefore)
        => ProductCatalogService.TotalBackendCallCount == backendCallsBefore ? "HIT" : "MISS";

    private void ApplyCounters(int backendCallsBefore)
    {
        var backendCallsAfter = ProductCatalogService.TotalBackendCallCount;
        if (backendCallsAfter == backendCallsBefore)
        {
            _cacheHits++;
        }

        _backendCalls = backendCallsAfter;
        UpdateCounters();
    }

    private void UpdateCounters()
    {
        _activeBackendValue.Text = $"Active: {_switchableCache.CurrentBackend}";
        _backendCallsValue.Text = $"Backend calls: {_backendCalls}";
        _cacheHitsValue.Text = $"Cache hits: {_cacheHits}";
    }

    private void Log(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        _activityLog.AppendText(line + Environment.NewLine);
    }
}
