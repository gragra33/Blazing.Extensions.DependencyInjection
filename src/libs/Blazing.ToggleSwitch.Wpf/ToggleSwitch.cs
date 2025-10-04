using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Blazing.ToggleSwitch.Wpf.Extensions;

namespace Blazing.ToggleSwitch.Wpf;

/// <summary>
/// Represents a toggle switch control that allows users to choose between two states (On/Off, Yes/No, etc.).
/// This control extends the ToggleButton with additional styling and layout options for creating modern UI experiences.
/// </summary>
/// <remarks>
/// The ToggleSwitch control provides the following features:
/// <list type="bullet">
/// <item><description>Customizable text for checked and unchecked states</description></item>
/// <item><description>Configurable header and switch positioning</description></item>
/// <item><description>Customizable colors and brushes for different states</description></item>
/// <item><description>Support for horizontal alignment and padding customization</description></item>
/// <item><description>Shared size group support for consistent alignment across multiple controls</description></item>
/// </list>
/// </remarks>
public class ToggleSwitch : ToggleButton
{
    #region Constructor

    /// <summary>
    /// Initializes static members of the <see cref="ToggleSwitch"/> class.
    /// This static constructor sets up the default style key for the control.
    /// </summary>
    static ToggleSwitch()
    {
        DefaultStyleKeyProperty.OverrideMetadata(ctrlType, new FrameworkPropertyMetadata(ctrlType));
    }

    #endregion

    #region Properties

    /// <summary>
    /// The name of the template part used for check label animation.
    /// </summary>
    private const string CheckLabeLAnimationName = "PART_CheckLabeLAnimation";
        
    /// <summary>
    /// The name of the template part used for shared group state management.
    /// </summary>
    private const string SharedGroupStateName = "PART_SharedGroupSize";

    /// <summary>
    /// The base name for header placement visual states.
    /// </summary>
    private const string HeaderPlacementVisualState = "HeaderContentPlacementAt";
        
    /// <summary>
    /// The base name for header stretch visual states.
    /// </summary>
    private const string HeaderStretchVisualState = "HeaderStretchAt";
        
    /// <summary>
    /// The base name for switch placement visual states.
    /// </summary>
    private const string SwitchPlacementVisualState = "SwitchContentPlacementAt";

    /// <summary>
    /// The default width value for the switch control.
    /// </summary>
    private const double DefaultSwitchWidthValue = 44.0D;
        
    /// <summary>
    /// The default text displayed when the switch is in checked state.
    /// </summary>
    private const string DefaultCheckedTextValue = "On";
        
    /// <summary>
    /// The default text displayed when the switch is in unchecked state.
    /// </summary>
    private const string DefaultUncheckedTextValue = "Off";
        
    /// <summary>
    /// The default placement position for header content.
    /// </summary>
    private const Dock DefaultHeaderContentPlacementValue = Dock.Left;
        
    /// <summary>
    /// The default placement position for switch content.
    /// </summary>
    private const Dock DefaultSwitchContentPlacementValue = Dock.Left;
        
    /// <summary>
    /// The default horizontal alignment for header content.
    /// </summary>
    private const HorizontalAlignment DefaultHeaderHorizontalValue = HorizontalAlignment.Left;
        
    /// <summary>
    /// The default horizontal alignment for check content.
    /// </summary>
    private const HorizontalAlignment DefaultCheckHorizontalValue = HorizontalAlignment.Left;
        
    /// <summary>
    /// The default horizontal alignment for switch content.
    /// </summary>
    private const HorizontalAlignment DefaultSwitchHorizontalValue = HorizontalAlignment.Left;
        
    /// <summary>
    /// The default shared size group name for consistent sizing across multiple ToggleSwitch controls.
    /// </summary>
    private const string DefaultSharedSizeGroupName = "ToggleSwitchGroup";
        
    /// <summary>
    /// The default padding value for header content.
    /// </summary>
    private static readonly Thickness DefaultHeaderPaddingValue = new Thickness(0D);
        
    /// <summary>
    /// The default padding value for check content.
    /// </summary>
    private static readonly Thickness DefaultCheckPaddingValue = new Thickness(0D);
        
    /// <summary>
    /// The default padding value for switch content.
    /// </summary>
    private static readonly Thickness DefaultSwitchPaddingValue = new Thickness(8D, 0D, 8D, 0D);

    /// <summary>
    /// The default background brush for checked state.
    /// </summary>
    private static readonly Brush CheckedBackgroundBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x63, 0xB1));
        
    /// <summary>
    /// The default foreground brush for checked state.
    /// </summary>
    private static readonly Brush CheckedForegroundBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        
    /// <summary>
    /// The default border brush for checked state.
    /// </summary>
    private static readonly Brush CheckedBorderBrushBrush = new SolidColorBrush(Color.FromRgb(0x00, 0x63, 0xB1));
        
    /// <summary>
    /// The default background brush for unchecked state.
    /// </summary>
    private static readonly Brush UncheckedBackgroundBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xFF, 0xFF));
        
    /// <summary>
    /// The default foreground brush for unchecked state.
    /// </summary>
    private static readonly Brush UncheckedForegroundBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));
        
    /// <summary>
    /// The default border brush for unchecked state.
    /// </summary>
    private static readonly Brush UncheckedBorderBrushBrush = new SolidColorBrush(Color.FromRgb(0x33, 0x33, 0x33));

    /// <summary>
    /// The type of this control, used for template and metadata operations.
    /// </summary>
    private static readonly Type ctrlType = typeof(ToggleSwitch);
        
    /// <summary>
    /// The name of this control, used for categorization in design tools.
    /// </summary>
    private const string ctrlName = nameof(ToggleSwitch);

    /// <summary>
    /// Identifies the <see cref="HeaderHorizontalAlignment"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(HeaderHorizontalAlignment), typeof(HorizontalAlignment), ctrlType, new PropertyMetadata(DefaultHeaderHorizontalValue, OnHeaderHorizontalAlignmentChanged));

    /// <summary>
    /// Identifies the <see cref="HeaderPadding"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderPaddingProperty =
        DependencyProperty.Register(nameof(HeaderPadding), typeof(Thickness), ctrlType, new PropertyMetadata(DefaultHeaderPaddingValue));

    /// <summary>
    /// Identifies the <see cref="HeaderContentPlacement"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HeaderContentPlacementProperty =
        DependencyProperty.Register(nameof(HeaderContentPlacement), typeof(Dock), ctrlType, new PropertyMetadata(DefaultHeaderContentPlacementValue, OnHeaderContentPlacementPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="CheckedText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CheckedTextProperty =
        DependencyProperty.Register(nameof(CheckedText), typeof(string), ctrlType, new PropertyMetadata(DefaultCheckedTextValue, OnCheckTextChanged));

    /// <summary>
    /// Identifies the <see cref="UncheckedText"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UncheckedTextProperty =
        DependencyProperty.Register(nameof(UncheckedText), typeof(string), ctrlType, new PropertyMetadata(DefaultUncheckedTextValue));

    /// <summary>
    /// Identifies the <see cref="CheckHorizontalAlignment"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CheckHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(CheckHorizontalAlignment), typeof(HorizontalAlignment), ctrlType, new PropertyMetadata(DefaultCheckHorizontalValue));

    /// <summary>
    /// Identifies the <see cref="CheckPadding"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CheckPaddingProperty =
        DependencyProperty.Register(nameof(CheckPadding), typeof(Thickness), ctrlType, new PropertyMetadata(DefaultCheckPaddingValue));

    /// <summary>
    /// Identifies the <see cref="CheckedBackground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CheckedBackgroundProperty =
        DependencyProperty.Register(nameof(CheckedBackground), typeof(Brush), ctrlType, new PropertyMetadata(CheckedBackgroundBrush));

    /// <summary>
    /// Identifies the <see cref="CheckedForeground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CheckedForegroundProperty =
        DependencyProperty.Register(nameof(CheckedForeground), typeof(Brush), ctrlType, new PropertyMetadata(CheckedForegroundBrush));

    /// <summary>
    /// Identifies the <see cref="CheckedBorderBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty CheckedBorderBrushProperty =
        DependencyProperty.Register(nameof(CheckedBorderBrush), typeof(Brush), ctrlType, new PropertyMetadata(CheckedBorderBrushBrush));

    /// <summary>
    /// Identifies the <see cref="UncheckedBackground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UncheckedBackgroundProperty =
        DependencyProperty.Register(nameof(UncheckedBackground), typeof(Brush), ctrlType, new PropertyMetadata(UncheckedBackgroundBrush));

    /// <summary>
    /// Identifies the <see cref="UncheckedForeground"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UncheckedForegroundProperty =
        DependencyProperty.Register(nameof(UncheckedForeground), typeof(Brush), ctrlType, new PropertyMetadata(UncheckedForegroundBrush));

    /// <summary>
    /// Identifies the <see cref="UncheckedBorderBrush"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty UncheckedBorderBrushProperty =
        DependencyProperty.Register(nameof(UncheckedBorderBrush), typeof(Brush), ctrlType, new PropertyMetadata(UncheckedBorderBrushBrush));

    /// <summary>
    /// Identifies the <see cref="SwitchWidth"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SwitchWidthProperty =
        DependencyProperty.Register(nameof(SwitchWidth), typeof(Double), ctrlType, new PropertyMetadata(DefaultSwitchWidthValue));

    /// <summary>
    /// Identifies the <see cref="SwitchPadding"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SwitchPaddingProperty =
        DependencyProperty.Register(nameof(SwitchPadding), typeof(Thickness), ctrlType, new PropertyMetadata(DefaultSwitchPaddingValue));

    /// <summary>
    /// Identifies the <see cref="SwitchContentPlacement"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SwitchContentPlacementProperty =
        DependencyProperty.Register(nameof(SwitchContentPlacement), typeof(Dock), ctrlType, new PropertyMetadata(DefaultSwitchContentPlacementValue, OnSwitchContentPlacementPropertyChanged));

    /// <summary>
    /// Identifies the <see cref="SwitchHorizontalAlignment"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SwitchHorizontalAlignmentProperty =
        DependencyProperty.Register(nameof(SwitchHorizontalAlignment), typeof(HorizontalAlignment), ctrlType, new PropertyMetadata(DefaultSwitchHorizontalValue));

    /// <summary>
    /// Identifies the <see cref="SharedSizeGroupName"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty SharedSizeGroupNameProperty =
        DependencyProperty.Register(nameof(SharedSizeGroupName), typeof(string), ctrlType, new PropertyMetadata(DefaultSharedSizeGroupName));

    /// <summary>
    /// Identifies the <see cref="IsHeaderStretch"/> dependency property.
    /// This property is used internally to manage header stretching behavior.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static readonly DependencyProperty IsHeaderStretchProperty = DependencyProperty.Register(nameof(IsHeaderStretch), typeof(bool), ctrlType, null);

    /// <summary>
    /// Gets or sets the horizontal alignment of the header content.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.HorizontalAlignment"/> value that specifies the horizontal alignment.
    /// The default is <see cref="HorizontalAlignment.Left"/>.
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the horizontal alignment of the Header's content."), Category(ctrlName)]
    public HorizontalAlignment HeaderHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(HeaderHorizontalAlignmentProperty);
        set => SetValue(HeaderHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding around the header content.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Thickness"/> value that specifies the padding.
    /// The default is no padding (0,0,0,0).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the spacing around the header text."), Category(ctrlName)]
    public Thickness HeaderPadding
    {
        get => (Thickness)GetValue(HeaderPaddingProperty);
        set => SetValue(HeaderPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the placement of the header content relative to the switch.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Controls.Dock"/> value that specifies the placement.
    /// The default is <see cref="Dock.Left"/>.
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the placement of the Header Content to the Switch."), Category(ctrlName)]
    public Dock HeaderContentPlacement
    {
        get => (Dock)GetValue(HeaderContentPlacementProperty);
        set => SetValue(HeaderContentPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the text displayed when the switch is in the checked state.
    /// </summary>
    /// <value>
    /// A string that represents the checked state text. The default is "On".
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the Switch text for checked state."), Category(ctrlName)]
    public string CheckedText
    {
        get => (string)GetValue(CheckedTextProperty);
        set => SetValue(CheckedTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text displayed when the switch is in the unchecked state.
    /// </summary>
    /// <value>
    /// A string that represents the unchecked state text. The default is "Off".
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the Switch text for Unchecked state."), Category(ctrlName)]
    public string UncheckedText
    {
        get => (string)GetValue(UncheckedTextProperty);
        set => SetValue(UncheckedTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the check content.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.HorizontalAlignment"/> value that specifies the horizontal alignment.
    /// The default is <see cref="HorizontalAlignment.Left"/>.
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the horizontal alignment of the Check's content."), Category(ctrlName)]
    public HorizontalAlignment CheckHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(CheckHorizontalAlignmentProperty);
        set => SetValue(CheckHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding around the check content.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Thickness"/> value that specifies the padding.
    /// The default is no padding (0,0,0,0).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the spacing around the switch text."), Category(ctrlName)]
    public Thickness CheckPadding
    {
        get => (Thickness)GetValue(CheckPaddingProperty);
        set => SetValue(CheckPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush for the switch when it is in the checked state.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Media.Brush"/> that paints the background.
    /// The default is a blue brush (#0063B1).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the graphical switch checked background brush."), Category(ctrlName)]
    public Brush CheckedBackground
    {
        get => (Brush)GetValue(CheckedBackgroundProperty);
        set => SetValue(CheckedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush for the switch when it is in the checked state.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Media.Brush"/> that paints the foreground.
    /// The default is white brush (#FFFFFF).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the graphical switch checked foreground brush."), Category(ctrlName)]
    public Brush CheckedForeground
    {
        get => (Brush)GetValue(CheckedForegroundProperty);
        set => SetValue(CheckedForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the border brush for the switch when it is in the checked state.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Media.Brush"/> that paints the border.
    /// The default is a blue brush (#0063B1).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the graphical switch checked border brush."), Category(ctrlName)]
    public Brush CheckedBorderBrush
    {
        get => (Brush)GetValue(CheckedBorderBrushProperty);
        set => SetValue(CheckedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the background brush for the switch when it is in the unchecked state.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Media.Brush"/> that paints the background.
    /// The default is white brush (#FFFFFF).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the graphical switch Unchecked background brush."), Category(ctrlName)]
    public Brush UncheckedBackground
    {
        get => (Brush)GetValue(UncheckedBackgroundProperty);
        set => SetValue(UncheckedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the foreground brush for the switch when it is in the unchecked state.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Media.Brush"/> that paints the foreground.
    /// The default is a dark gray brush (#333333).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the graphical switch Unchecked foreground brush."), Category(ctrlName)]
    public Brush UncheckedForeground
    {
        get => (Brush)GetValue(UncheckedForegroundProperty);
        set => SetValue(UncheckedForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the border brush for the switch when it is in the unchecked state.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Media.Brush"/> that paints the border.
    /// The default is a dark gray brush (#333333).
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the graphical switch Unchecked border brush."), Category(ctrlName)]
    public Brush UncheckedBorderBrush
    {
        get => (Brush)GetValue(UncheckedBorderBrushProperty);
        set => SetValue(UncheckedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the graphical switch element.
    /// </summary>
    /// <value>
    /// A <see cref="System.Double"/> that specifies the width in device-independent units.
    /// The default is 44.0.
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the width of the graphical switch. (Default: 44)"), Category(ctrlName)]
    public Double SwitchWidth
    {
        get => (Double)GetValue(SwitchWidthProperty);
        set => SetValue(SwitchWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding around the switch element.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Thickness"/> value that specifies the padding.
    /// The default is padding of 8 pixels on the left and right sides.
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the spacing around the graphical switch."), Category(ctrlName)]
    public Thickness SwitchPadding
    {
        get => (Thickness)GetValue(SwitchPaddingProperty);
        set => SetValue(SwitchPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the placement of the switch element relative to the state text.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.Controls.Dock"/> value that specifies the placement.
    /// The default is <see cref="Dock.Left"/>.
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the placement of the graphical switch to the state text."), Category(ctrlName)]
    public Dock SwitchContentPlacement
    {
        get => (Dock)GetValue(SwitchContentPlacementProperty);
        set => SetValue(SwitchContentPlacementProperty, value);
    }

    /// <summary>
    /// Gets or sets the horizontal alignment of the switch content.
    /// </summary>
    /// <value>
    /// A <see cref="System.Windows.HorizontalAlignment"/> value that specifies the horizontal alignment.
    /// The default is <see cref="HorizontalAlignment.Left"/>.
    /// </value>
    [Bindable(true)]
    [Description("Gets or sets the horizontal alignment of the Switch's content."), Category(ctrlName)]
    public HorizontalAlignment SwitchHorizontalAlignment
    {
        get => (HorizontalAlignment)GetValue(SwitchHorizontalAlignmentProperty);
        set => SetValue(SwitchHorizontalAlignmentProperty, value);
    }

    /// <summary>
    /// Gets or sets the name of the shared size group for consistent alignment across multiple controls.
    /// </summary>
    /// <value>
    /// A string that specifies the shared size group name. The default is an empty string.
    /// </value>
    /// <remarks>
    /// This property is used to synchronize the size of the left column (header or switch) 
    /// across multiple ToggleSwitch controls, depending on the HeaderContentPlacement property.
    /// </remarks>
    [Bindable(true)]
    [Description("Name of Shared Size Group for the Left column (header or switch). Depends on HeaderContentPlacement property."), Category(ctrlName)]
    public string SharedSizeGroupName
    {
        get => (string)GetValue(SharedSizeGroupNameProperty);
        set => SetValue(SharedSizeGroupNameProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the header should stretch.
    /// This property is used internally and should not be used directly.
    /// </summary>
    /// <value>
    /// <c>true</c> if the header should stretch; otherwise, <c>false</c>.
    /// </value>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool IsHeaderStretch
    {
        get => (bool)GetValue(IsHeaderStretchProperty);
        set => SetValue(IsHeaderStretchProperty, value);
    }

    #endregion

    #region Events

    /// <summary>
    /// When overridden in a derived class, is invoked whenever application code or internal processes call ApplyTemplate.
    /// </summary>
    /// <remarks>
    /// This method is called when the control template is applied to the control.
    /// It initializes the control's visual state and sets up bindings for template parts.
    /// </remarks>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        // Initialize control state and bindings
        CheckLabeLAnimationValue(this);
        SharedGroupStateValue(this, HeaderContentPlacement);
        CoerceHeaderSizing();
        UpdatePlacementVisualState();
    }

    /// <summary>
    /// Called when the HeaderHorizontalAlignment property changes.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">Event arguments containing the old and new values.</param>
    private static void OnHeaderHorizontalAlignmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d.TryCast(out ToggleSwitch ctrl))
        {
            ctrl.CoerceHeaderSizing();
        }
    }

    /// <summary>
    /// Determines whether the header should stretch based on its content placement.
    /// </summary>
    private void CoerceHeaderSizing()
    {
        SetValue(IsHeaderStretchProperty, HeaderContentPlacement == Dock.Left || HeaderContentPlacement == Dock.Right);
    }

    /// <summary>
    /// Called when the CheckedText property changes.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">Event arguments containing the old and new values.</param>
    private static void OnCheckTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d.TryCast(out ToggleSwitch ctrl))
        {
            CheckLabeLAnimationValue(ctrl);
        }
    }

    /// <summary>
    /// Called when the HeaderContentPlacement property changes.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">Event arguments containing the old and new values.</param>
    private static void OnHeaderContentPlacementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d.TryCast(out ToggleSwitch ctrl))
        {
            var oldValue = (Dock)e.OldValue;
            var newValue = (Dock)e.NewValue;

            ChangeSharedGroupStateValue(ctrl, newValue, oldValue);
            ctrl.OnHeaderContentPlacementChanged(newValue, oldValue);
        }
    }

    /// <summary>
    /// Called when the SwitchContentPlacement property changes.
    /// </summary>
    /// <param name="d">The dependency object where the property changed.</param>
    /// <param name="e">Event arguments containing the old and new values.</param>
    private static void OnSwitchContentPlacementPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d.TryCast(out ToggleSwitch ctrl))
        {
            ctrl.OnSwitchContentPlacementChanged((Dock)e.NewValue, (Dock)e.OldValue);
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Updates the shared group state when the placement changes.
    /// </summary>
    /// <param name="ctrl">The ToggleSwitch control to update.</param>
    /// <param name="newValue">The new placement value.</param>
    /// <param name="oldValue">The old placement value.</param>
    private static void ChangeSharedGroupStateValue(ToggleSwitch ctrl, Dock newValue, Dock oldValue)
    {
        // Remove binding from old placement
        SharedGroupStateValue(ctrl, oldValue, false);
        // Add binding to new placement
        SharedGroupStateValue(ctrl, newValue);
    }

    /// <summary>
    /// Sets up or removes shared size group binding for the specified placement.
    /// </summary>
    /// <param name="ts">The ToggleSwitch control.</param>
    /// <param name="placement">The dock placement for the shared group.</param>
    /// <param name="IsBound">True to bind the shared size group; false to remove the binding.</param>
    private static void SharedGroupStateValue(ToggleSwitch ts, Dock placement, bool IsBound = true)
    {
        var field = (DiscreteObjectKeyFrame)ts.Template?.FindName(SharedGroupStateName + placement.ToString(), ts);
        if (field != null)
        {
            var binding = new Binding(nameof(SharedSizeGroupName)) { Source = ts };
            BindingOperations.SetBinding(field, ObjectKeyFrame.ValueProperty, IsBound ? binding : new Binding());
        }
    }

    /// <summary>
    /// Sets up the check label animation binding.
    /// </summary>
    /// <param name="ts">The ToggleSwitch control.</param>
    private static void CheckLabeLAnimationValue(ToggleSwitch ts)
    {
        var checkLabelAnimation = (DiscreteObjectKeyFrame)ts.Template?.FindName(CheckLabeLAnimationName, ts);
        if (checkLabelAnimation != null)
        {
            var binding = new Binding(nameof(CheckedText)) { Source = ts };
            BindingOperations.SetBinding(checkLabelAnimation, ObjectKeyFrame.ValueProperty, binding);
        }
    }

    /// <summary>
    /// Called when the header content placement changes.
    /// </summary>
    /// <param name="newValue">The new placement value.</param>
    /// <param name="oldValue">The old placement value.</param>
    protected virtual void OnHeaderContentPlacementChanged(Dock newValue, Dock oldValue)
    {
        CoerceHeaderSizing();
        UpdateHeaderVisualState(newValue);
    }

    /// <summary>
    /// Called when the switch content placement changes.
    /// </summary>
    /// <param name="newValue">The new placement value.</param>
    /// <param name="oldValue">The old placement value.</param>
    protected virtual void OnSwitchContentPlacementChanged(Dock newValue, Dock oldValue)
    {
        UpdateSwitchPlacementVisualState(newValue);
    }

    /// <summary>
    /// Updates all placement-related visual states.
    /// </summary>
    private void UpdatePlacementVisualState()
    {
        UpdateHeaderVisualState(HeaderContentPlacement);
        UpdateSwitchPlacementVisualState(SwitchContentPlacement);
    }

    /// <summary>
    /// Updates the header-related visual states based on the placement.
    /// </summary>
    /// <param name="newPlacement">The new header placement.</param>
    private void UpdateHeaderVisualState(Dock newPlacement)
    {
        // Update header placement visual state
        _ = GoToState($"{HeaderPlacementVisualState}{newPlacement.ToString()}", false);

        // Update header stretch visual state based on placement and stretch property
        if (IsHeaderStretch)
        {
            string stretchState = newPlacement switch
            {
                Dock.Right or Dock.Left => $"{HeaderStretchVisualState}{newPlacement.ToString()}",
                Dock.Top or Dock.Bottom or _ => $"{HeaderStretchVisualState}Middle"
            };
            _ = GoToState(stretchState, false);
        }
        else
        {
            _ = GoToState($"{HeaderStretchVisualState}Middle", false);
        }
    }

    /// <summary>
    /// Updates the switch placement visual state.
    /// </summary>
    /// <param name="newPlacement">The new switch placement.</param>
    private void UpdateSwitchPlacementVisualState(Dock newPlacement)
    {
        _ = GoToState($"{SwitchPlacementVisualState}{newPlacement.ToString()}", false);
    }

    /// <summary>
    /// Transitions the control to the specified visual state.
    /// </summary>
    /// <param name="stateName">The name of the state to transition to.</param>
    /// <param name="useTransitions">True to use transitions when changing states; false otherwise.</param>
    /// <returns>True if the state change was successful; false otherwise.</returns>
    internal bool GoToState(string stateName, bool useTransitions)
    {
        return VisualStateManager.GoToState(this, stateName, useTransitions);
    }

    #endregion
}