namespace Blazing.Common.BEM;

/// <summary>
/// Provides extension methods for BEM CSS class naming conventions.
/// </summary>
public static class CssClassExtensions
{
    /// <summary>
    /// Joins a name with the current string using BEM element notation.
    /// </summary>
    /// <param name="this">The base CSS class name.</param>
    /// <param name="name">The element name to join.</param>
    /// <returns>The combined BEM class name.</returns>
    public static string JoinName(this string @this, string name)
        => @this + CssClass.NameJoin + name;

    /// <summary>
    /// Joins a modifier name with the current string using BEM modifier notation.
    /// </summary>
    /// <param name="this">The base CSS class name.</param>
    /// <param name="modifierName">The modifier name to join.</param>
    /// <returns>The combined BEM modifier class name.</returns>
    public static string JoinModifier(this string @this, string modifierName)
        => @this + CssClass.ModifierJoin + modifierName;
}