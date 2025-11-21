using CSS = Blazing.ToggleSwitch.Blazor.Css.CssClass.Toggle;

namespace Blazing.ToggleSwitch.Blazor;

/// <summary>
/// Represents a toggle switch component for Blazor UI.
/// </summary>
public partial class Toggle
{
    #region Fields

    /// <summary>
    /// The unique identifier for the label element.
    /// </summary>
    private string? _labelId;
    /// <summary>
    /// The unique identifier for the pill element.
    /// </summary>
    private string? _pillId;
    /// <summary>
    /// The unique identifier for the state label element.
    /// </summary>
    private string? _stateLabelId;

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the content to be rendered as the label.
    /// </summary>
    [Parameter]
    public RenderFragment? LabelContent { get; set; }

    /// <summary>
    /// Gets or sets the label text for the toggle.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the text displayed when the toggle is on.
    /// </summary>
    [Parameter]
    public string? OnText { get; set; }

    /// <summary>
    /// Gets or sets the text displayed when the toggle is off.
    /// </summary>
    [Parameter]
    public string? OffText { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the label is displayed inline.
    /// </summary>
    [Parameter]
    public bool InlineLabel { get; set; }

    /// <summary>
    /// Gets or sets the position of the toggle control.
    /// </summary>
    [Parameter]
    public TogglePosition Position { get; set; }

    /// <summary>
    /// Gets or sets the current value of the toggle.
    /// </summary>
    [Parameter]
    public bool Value { get; set; }

    /// <summary>
    /// Gets or sets the event callback for value changes.
    /// </summary>
    [Parameter]
    public EventCallback<bool> ValueChanged { get; set; }

    /// <summary>
    /// Gets or sets the default value of the toggle.
    /// </summary>
    [Parameter]
    public bool DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the toggle is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the toggle is read-only.
    /// </summary>
    [Parameter]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// Gets the label element's unique identifier.
    /// </summary>
    private string? LabelId => _labelId;
    /// <summary>
    /// Gets the pill element's unique identifier.
    /// </summary>
    private string? PillId => _pillId;
    /// <summary>
    /// Gets the state label element's unique identifier.
    /// </summary>
    private string? StateLabelId => _stateLabelId;

    /// <summary>
    /// Gets the CSS class string for the toggle component.
    /// </summary>
    private string Classname
    {
        get
        {
            CssBuilder builder = new CssBuilder(CSS.Root);

            if (Disabled)
                builder.AddClass(CSS.Modifier.Disabled);

            if (InlineLabel)
                builder.AddClass(CSS.Modifier.InlineLabel);

            if (!HasOnOffLabel())
                builder.AddClass(CSS.Modifier.NoOnOffLabel);

            if (!string.IsNullOrEmpty(CssClass))
                builder.AddClass(CssClass);

            return builder.Build();
        }
    }

    /// <summary>
    /// Gets the current state label text (OnText or OffText).
    /// </summary>
    private string? StateLabel
        => Value ? OnText : OffText;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes the component and sets unique identifiers for elements.
    /// </summary>
    protected override void OnInitialized()
    {
        _labelId = GetUniqueId();
        _pillId = GetUniqueId();
        _stateLabelId = GetUniqueId();

        if (DefaultValue)
            Value = true;

        base.OnInitialized();
    }

    #endregion

    #region Events

    /// <summary>
    /// Handles the change event for the toggle, updating its value and invoking the callback.
    /// </summary>
    private void OnChange()
    {
        // check is here for browsers that do not manage the input disabled state
        if (Disabled)
            return;

        Value = !Value;

        InvokeAsync(async () => await ValueChanged.InvokeAsync(Value).ConfigureAwait(false));
    }

    /// <summary>
    /// Handles key down events for toggling the value with keyboard input.
    /// </summary>
    /// <param name="arg">The keyboard event arguments.</param>
    private void OnKeyDownAsync(KeyboardEventArgs arg)
    {
        //Console.WriteLine($"** KEY: {arg.Code} | {arg.Key}");

        switch (arg.Code)
        {
            case "Space":
            case "Enter":
                OnChange();
                break;
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Determines whether the toggle has a label.
    /// </summary>
    /// <returns>True if a label or label content is present; otherwise, false.</returns>
    private bool HasLabel()
        => !string.IsNullOrWhiteSpace(Label) || LabelContent is not null;

    /// <summary>
    /// Determines whether the toggle has On/Off label text.
    /// </summary>
    /// <returns>True if OnText or OffText is present; otherwise, false.</returns>
    private bool HasOnOffLabel()
        => !string.IsNullOrWhiteSpace(OnText) || !string.IsNullOrWhiteSpace(OffText);

    #endregion
}