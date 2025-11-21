using System.Globalization;

namespace Blazing.Common;

/// <summary>
/// Base class for Blazor component controls, providing common properties and methods.
/// </summary>
public abstract class ComponentControlBase : ComponentBase, IDisposable
{
    #region Properties

    /// <summary>
    /// Gets the unique identifier for the component instance.
    /// </summary>
    public string? ComponentUniqueId { get; private set; }

    /// <summary>
    /// Gets or sets the reference to the component's root element.
    /// </summary>
    [Parameter]
    public ElementReference Reference { get; set; }

    /// <summary>
    /// Gets or sets the child content to be rendered inside the component.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the CSS class for the component.
    /// </summary>
    [Parameter]
    public virtual string? CssClass { get; set; }

    /// <summary>
    /// Gets or sets the inline style for the component.
    /// </summary>
    [Parameter]
    public virtual string? Style { get; set; }

    /// <summary>
    /// Gets or sets additional attributes that are not matched by other parameters.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Gets the current style as a dictionary of CSS property-value pairs.
    /// </summary>
    public IDictionary<string, string> CurrentStyle
    {
        get
        {
            Dictionary<string, string> currentStyle = new();

            if (string.IsNullOrEmpty(Style)) return currentStyle;
            
            foreach (string pair in Style.Split(';'))
            {
                string[] keyAndValue = pair.Split(':');
                    
                if (keyAndValue.Length != 2) continue;
                    
                string key = keyAndValue[0].Trim();
                string value = keyAndValue[1].Trim();

                currentStyle[key] = value;
            }

            return currentStyle;
        }
    }
    
    #endregion

    #region Overrides
    
    /// <summary>
    /// Initializes the component and sets a unique identifier.
    /// </summary>
    protected override void OnInitialized()
    {
        ComponentUniqueId = GetUniqueId();
        base.OnInitialized();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets the component's id attribute, falling back to the unique id if not set.
    /// </summary>
    /// <returns>The id attribute or unique id.</returns>
    protected string? GetId()
        => AdditionalAttributes is not null
           && AdditionalAttributes.TryGetValue("id", out object? id)
           && !string.IsNullOrEmpty(Convert.ToString(id, CultureInfo.InvariantCulture))
            ? $"{id}"
            : ComponentUniqueId;

    /// <summary>
    /// Generates a new unique identifier string for the component.
    /// </summary>
    /// <returns>A unique identifier string.</returns>
    public static string GetUniqueId()
        => Guid.NewGuid().ToString();

    /// <summary>
    /// Invokes StateHasChanged asynchronously.
    /// </summary>
    public new void StateHasChanged()
        => InvokeAsync(base.StateHasChanged);

    /// <summary>
    /// Asynchronously invokes StateHasChanged.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StateHasChangedAsync()
        => await InvokeAsync(base.StateHasChanged).ConfigureAwait(false);

    /// <summary>
    /// Gets the CSS class for the component.
    /// </summary>
    /// <returns>The CSS class string.</returns>
    public virtual string GetComponentCssClass() => CssClass ?? "";

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources used by the component.
    /// </summary>
    /// <param name="disposing">True if called from Dispose; false if called from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        // Dispose logic if needed
    }

    #endregion
}