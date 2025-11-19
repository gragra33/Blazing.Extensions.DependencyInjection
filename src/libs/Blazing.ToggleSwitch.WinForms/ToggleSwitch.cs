using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace Blazing.ToggleSwitch.WinForms;

/// <summary>
/// A modern toggle switch control for WinForms applications.
/// Provides smooth animations and customizable appearance options similar to the WPF version.
/// </summary>
[DefaultEvent("CheckedChanged")]
[DefaultProperty("Checked")]
[ToolboxItem(true)]
[Designer("System.Windows.Forms.Design.ControlDesigner, System.Design")]
public sealed class ToggleSwitch : Control, ISupportInitialize
{
    private bool _checked;
    private string _checkedText = "ON";
    private string _uncheckedText = "OFF";
    private Color _checkedBackColor = Color.FromArgb(0, 120, 215); // Windows blue
    private Color _uncheckedBackColor = Color.FromArgb(200, 200, 200);
    private Color _switchColor = Color.White;
    private System.Windows.Forms.Timer? _animationTimer;
    private float _switchPosition; // 0 = left, 1 = right
    private bool _isAnimating;
    private const int AnimationSteps = 10;
    private int _currentStep;
    private bool _initializing;
    private bool _autoSize = true;
    private Size _switchSize = new Size(44, 20); // Default switch size
    private bool _showText = true; // Show text labels next to switch

    /// <summary>
    /// Initializes a new instance of the <see cref="ToggleSwitch"/> class.
    /// </summary>
    public ToggleSwitch()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.StandardClick |
                 ControlStyles.StandardDoubleClick, true);
        
        TabStop = true;
        Size = _switchSize;
        BackColor = Color.Transparent;
        
        InitializeAnimation();
    }

    #region ISupportInitialize Implementation

    /// <summary>
    /// Signals the object that initialization is starting.
    /// </summary>
    public void BeginInit()
    {
        _initializing = true;
    }

    /// <summary>
    /// Signals the object that initialization is complete.
    /// </summary>
    public void EndInit()
    {
        _initializing = false;
        UpdateSize();
    }

    #endregion

    private void InitializeAnimation()
    {
        _animationTimer = new System.Windows.Forms.Timer();
        _animationTimer.Interval = 20; // 50 FPS
        _animationTimer.Tick += AnimationTimer_Tick;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the toggle switch is in the checked state.
    /// </summary>
    [DefaultValue(false)]
    [Description("Indicates whether the toggle switch is in the checked state.")]
    public bool Checked
    {
        get => _checked;
        set
        {
            if (_checked != value)
            {
                _checked = value;
                StartAnimation();
                OnCheckedChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the text displayed when the toggle switch is checked.
    /// </summary>
    [DefaultValue("ON")]
    [Description("The text displayed when the toggle switch is checked.")]
    public string? CheckedText
    {
        get => _checkedText;
        set
        {
            _checkedText = value ?? "ON";
            if (_showText)
            {
                UpdateSize();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the text displayed when the toggle switch is unchecked.
    /// </summary>
    [DefaultValue("OFF")]
    [Description("The text displayed when the toggle switch is unchecked.")]
    public string? UncheckedText
    {
        get => _uncheckedText;
        set
        {
            _uncheckedText = value ?? "OFF";
            if (_showText)
            {
                UpdateSize();
                Invalidate();
            }
        }
    }

    /// <summary>
    /// Gets or sets the background color when the toggle switch is checked.
    /// </summary>
    [Description("The background color when the toggle switch is checked.")]
    public Color CheckedBackColor
    {
        get => _checkedBackColor;
        set
        {
            _checkedBackColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the background color when the toggle switch is unchecked.
    /// </summary>
    [Description("The background color when the toggle switch is unchecked.")]
    public Color UncheckedBackColor
    {
        get => _uncheckedBackColor;
        set
        {
            _uncheckedBackColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the color of the switch button.
    /// </summary>
    [DefaultValue(typeof(Color), "White")]
    [Description("The color of the switch button.")]
    public Color SwitchColor
    {
        get => _switchColor;
        set
        {
            _switchColor = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets whether to show text labels next to the switch.
    /// </summary>
    [DefaultValue(true)]
    [Description("Indicates whether to show text labels next to the switch.")]
    public bool ShowText
    {
        get => _showText;
        set
        {
            _showText = value;
            UpdateSize();
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the size of the switch control.
    /// This includes both the switch and text if visible.
    /// </summary>
    [Description("The size of the switch control.")]
    public new Size Size
    {
        get => base.Size;
        set
        {
            // Don't change _switchSize when setting Size
            base.Size = value;
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets the size of just the switch part (without text).
    /// </summary>
    [Description("The size of just the switch part (without text).")]
    public Size SwitchSize
    {
        get => _switchSize;
        set
        {
            _switchSize = new Size(Math.Max(20, value.Width), Math.Max(5, value.Height));
            UpdateSize();
            Invalidate();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the control automatically resizes to fit its content.
    /// </summary>
    [DefaultValue(true)]
    [Description("Indicates whether the control automatically resizes to fit its content.")]
    public override bool AutoSize
    {
        get => _autoSize;
        set
        {
            if (_autoSize != value)
            {
                _autoSize = value;
                SetAutoSizeMode(AutoSizeMode.GrowAndShrink);
                UpdateSize();
                OnAutoSizeChanged(EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets the current accessibility state text.
    /// </summary>
    [Browsable(false)]
    public string AccessibilityText => _checked ? _checkedText : _uncheckedText;

    /// <summary>
    /// Occurs when the value of the <see cref="Checked"/> property changes.
    /// </summary>
    [Description("Occurs when the value of the Checked property changes.")]
    public event EventHandler? CheckedChanged;

    /// <summary>
    /// Raises the <see cref="CheckedChanged"/> event.
    /// </summary>
    /// <param name="e">Event arguments.</param>
    private void OnCheckedChanged(EventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }

    private void UpdateSize()
    {
        if (_initializing || !_autoSize) return;

        Size totalSize;
        if (_showText)
        {
            // Measure text size
            using (var g = CreateGraphics())
            {
                var checkedSize = TextRenderer.MeasureText(g, _checkedText, Font);
                var uncheckedSize = TextRenderer.MeasureText(g, _uncheckedText, Font);
                var maxTextWidth = Math.Max(checkedSize.Width, uncheckedSize.Width);
                var maxTextHeight = Math.Max(checkedSize.Height, uncheckedSize.Height);
                
                // Total width = switch width + padding + text width
                // Total height = max of switch height and text height
                totalSize = new Size(
                    _switchSize.Width + 8 + maxTextWidth, // 8px padding between switch and text
                    Math.Max(_switchSize.Height, maxTextHeight));
            }
        }
        else
        {
            totalSize = _switchSize;
        }

        if (base.Size != totalSize)
        {
            base.Size = totalSize;
        }
    }

    /// <summary>
    /// Gets the preferred size of the control.
    /// </summary>
    public override Size GetPreferredSize(Size proposedSize)
    {
        if (_showText)
        {
            using (var g = CreateGraphics())
            {
                var checkedSize = TextRenderer.MeasureText(g, _checkedText, Font);
                var uncheckedSize = TextRenderer.MeasureText(g, _uncheckedText, Font);
                var maxTextWidth = Math.Max(checkedSize.Width, uncheckedSize.Width);
                var maxTextHeight = Math.Max(checkedSize.Height, uncheckedSize.Height);
                
                return new Size(
                    _switchSize.Width + 8 + maxTextWidth,
                    Math.Max(_switchSize.Height, maxTextHeight));
            }
        }
        return _switchSize;
    }

    /// <summary>
    /// Sets the size of the control.
    /// </summary>
    protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
    {
        if (_autoSize)
        {
            // Force the size to match our preferred size when AutoSize is true
            var preferredSize = GetPreferredSize(Size.Empty);
            width = preferredSize.Width;
            height = preferredSize.Height;
        }
        base.SetBoundsCore(x, y, width, height, specified);
    }

    private void StartAnimation()
    {
        if (_animationTimer == null) return;

        _isAnimating = true;
        _currentStep = 0;
        _animationTimer.Start();
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (_animationTimer == null) return;

        _currentStep++;
        float progress = (float)_currentStep / AnimationSteps;
        
        if (_checked)
        {
            _switchPosition = EaseInOut(progress);
        }
        else
        {
            _switchPosition = 1 - EaseInOut(progress);
        }

        Invalidate();

        if (_currentStep >= AnimationSteps)
        {
            _animationTimer.Stop();
            _isAnimating = false;
            _switchPosition = _checked ? 1 : 0;
            Invalidate();
        }
    }

    private static float EaseInOut(float t)
    {
        return t * t * (3.0f - 2.0f * t);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var switchRect = new Rectangle(0, 0, _switchSize.Width, _switchSize.Height);
        
        // Draw switch background
        var backColor = _checked ? _checkedBackColor : _uncheckedBackColor;
        if (_isAnimating)
        {
            // Blend colors during animation
            var alpha = _switchPosition;
            var r = (int)(_uncheckedBackColor.R + (_checkedBackColor.R - _uncheckedBackColor.R) * alpha);
            var g1 = (int)(_uncheckedBackColor.G + (_checkedBackColor.G - _uncheckedBackColor.G) * alpha);
            var b = (int)(_uncheckedBackColor.B + (_checkedBackColor.B - _uncheckedBackColor.B) * alpha);
            backColor = Color.FromArgb(r, g1, b);
        }

        // Draw background with rounded rectangle
        var cornerRadius = _switchSize.Height / 2;
        using (var brush = new SolidBrush(backColor))
        {
            g.FillRoundedRectangle(brush, switchRect, cornerRadius);
        }

        // Draw border
        using (var pen = new Pen(Color.FromArgb(160, 160, 160), 1))
        {
            g.DrawRoundedRectangle(pen, switchRect, cornerRadius);
        }

        // Draw switch button (circle)
        var buttonSize = _switchSize.Height - 4;
        var buttonX = 2 + (_switchSize.Width - buttonSize - 4) * _switchPosition;
        var buttonY = 2;
        var buttonRect = new RectangleF(buttonX, buttonY, buttonSize, buttonSize);

        // Draw button shadow
        var shadowRect = new RectangleF(buttonX + 1, buttonY + 1, buttonSize, buttonSize);
        using (var shadowBrush = new SolidBrush(Color.FromArgb(50, 0, 0, 0)))
        {
            g.FillEllipse(shadowBrush, shadowRect);
        }

        // Draw button
        using (var brush = new SolidBrush(_switchColor))
        {
            g.FillEllipse(brush, buttonRect);
        }

        // Draw button border
        using (var pen = new Pen(Color.FromArgb(200, 200, 200), 1))
        {
            g.DrawEllipse(pen, buttonRect);
        }

        // Draw text if enabled
        if (_showText)
        {
            var textToRender = _checked ? _checkedText : _uncheckedText;
            var textRect = new Rectangle(_switchSize.Width + 8, 0, 
                                       Width - _switchSize.Width - 8, Height);
            
            var textFlags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | 
                           TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix;
            
            TextRenderer.DrawText(g, textToRender, Font, textRect, ForeColor, textFlags);
        }

        // Focus rectangle
        if (Focused)
        {
            var focusRect = new Rectangle(-2, -2, Width + 4, Height + 4);
            ControlPaint.DrawFocusRectangle(g, focusRect);
        }
    }

    protected override void OnClick(EventArgs e)
    {
        Checked = !Checked;
        base.OnClick(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
        {
            Checked = !Checked;
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }

    protected override void OnGotFocus(EventArgs e)
    {
        base.OnGotFocus(e);
        Invalidate(); // Redraw to show focus rectangle
    }

    protected override void OnLostFocus(EventArgs e)
    {
        base.OnLostFocus(e);
        Invalidate(); // Redraw to hide focus rectangle
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _animationTimer?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override bool IsInputKey(Keys keyData)
    {
        return keyData == Keys.Space || keyData == Keys.Enter || base.IsInputKey(keyData);
    }

    /// <summary>
    /// Gets the accessibility information for the control.
    /// </summary>
    protected override AccessibleObject CreateAccessibilityInstance()
    {
        return new ToggleSwitchAccessibleObject(this);
    }
}

/// <summary>
/// Accessibility object for the ToggleSwitch control.
/// </summary>
internal sealed class ToggleSwitchAccessibleObject : Control.ControlAccessibleObject
{
    private readonly ToggleSwitch _owner;

    public ToggleSwitchAccessibleObject(ToggleSwitch owner) : base(owner)
    {
        _owner = owner;
    }

    public override string DefaultAction => "Toggle";

    public override string Name => _owner.AccessibilityText;

    public override AccessibleRole Role => AccessibleRole.CheckButton;

    public override AccessibleStates State
    {
        get
        {
            var state = AccessibleStates.Focusable;
            if (_owner.Checked) state |= AccessibleStates.Checked;
            if (_owner.Focused) state |= AccessibleStates.Focused;
            return state;
        }
    }

    public override void DoDefaultAction()
    {
        _owner.Checked = !_owner.Checked;
    }
}

/// <summary>
/// Extension methods for graphics operations.
/// </summary>
internal static class GraphicsExtensions
{
    /// <summary>
    /// Fills a rounded rectangle.
    /// </summary>
    public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle rect, int cornerRadius)
    {
        if (cornerRadius <= 0)
        {
            graphics.FillRectangle(brush, rect);
            return;
        }

        using (var path = CreateRoundedRectanglePath(rect, cornerRadius))
        {
            graphics.FillPath(brush, path);
        }
    }

    /// <summary>
    /// Draws a rounded rectangle.
    /// </summary>
    public static void DrawRoundedRectangle(this Graphics graphics, Pen pen, Rectangle rect, int cornerRadius)
    {
        if (cornerRadius <= 0)
        {
            graphics.DrawRectangle(pen, rect);
            return;
        }

        using (var path = CreateRoundedRectanglePath(rect, cornerRadius))
        {
            graphics.DrawPath(pen, path);
        }
    }

    private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
    {
        var path = new GraphicsPath();
        var diameter = Math.Min(cornerRadius * 2, Math.Min(rect.Width, rect.Height));

        if (diameter <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
        path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
        path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
        path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);

        path.CloseFigure();
        return path;
    }
}