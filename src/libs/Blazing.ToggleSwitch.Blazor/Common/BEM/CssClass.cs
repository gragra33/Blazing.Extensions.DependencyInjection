namespace Blazing.Common.BEM;

/// <summary>
/// Provides BEM (Block Element Modifier) CSS naming convention constants.
/// </summary>
public static class CssClass
{
    /// <summary>
    /// The library prefix for CSS classes.
    /// </summary>
    public const string LibPrefix = "c-";

    /// <summary>
    /// The string used to join block and element names in BEM notation.
    /// </summary>
    public const string NameJoin = "__";

    /// <summary>
    /// The string used to join element and modifier names in BEM notation.
    /// </summary>
    public const string ModifierJoin = "--";

    /// <summary>
    /// The CSS class for hidden elements.
    /// </summary>
    public const string Hidden = LibPrefix + "hidden";
}