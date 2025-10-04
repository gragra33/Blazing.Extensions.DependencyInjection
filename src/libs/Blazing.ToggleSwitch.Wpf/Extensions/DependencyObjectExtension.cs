using System.Windows;
using System.Windows.Media;

namespace Blazing.ToggleSwitch.Wpf.Extensions;

/// <summary>
/// Provides extension methods for <see cref="DependencyObject"/> to facilitate type casting and visual tree navigation.
/// </summary>
public static class DependencyObjectExtension
{
    /// <summary>
    /// Attempts to cast a <see cref="DependencyObject"/> to the specified UI element type.
    /// </summary>
    /// <typeparam name="TElement">The type of UI element to cast to. Must inherit from <see cref="UIElement"/>.</typeparam>
    /// <param name="dObj">The dependency object to cast.</param>
    /// <param name="element">When this method returns, contains the cast element if the cast succeeded, or null if it failed.</param>
    /// <returns><c>true</c> if the cast succeeded; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This extension method provides a safe way to cast dependency objects to specific UI element types
    /// without throwing exceptions. It's commonly used in control template scenarios where you need to
    /// verify that a dependency object is of a specific type before operating on it.
    /// </remarks>
    /// <example>
    /// <code>
    /// if (dependencyObject.TryCast&lt;Button&gt;(out Button button))
    /// {
    ///     // Safely use the button instance
    ///     button.Click += OnButtonClick;
    /// }
    /// </code>
    /// </example>
    public static bool TryCast<TElement>(this DependencyObject dObj, out TElement element) where TElement : UIElement
    {
        element = dObj as TElement;
        return element != null;
    }

    /// <summary>
    /// Searches up the visual tree to find an ancestor of the specified type.
    /// </summary>
    /// <param name="o">The dependency object to start the search from.</param>
    /// <param name="ancestorType">The type of ancestor to search for.</param>
    /// <returns>
    /// The first ancestor of the specified type found in the visual tree, or <c>null</c> if no such ancestor exists.
    /// </returns>
    /// <remarks>
    /// This method recursively traverses up the visual tree using <see cref="VisualTreeHelper.GetParent(DependencyObject)"/>
    /// until it finds an ancestor of the specified type or reaches the root of the visual tree.
    /// The search includes both exact type matches and derived types.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Find the parent Window of a control
    /// var parentWindow = myControl.FindAncestorOfType(typeof(Window)) as Window;
    /// if (parentWindow != null)
    /// {
    ///     // Use the parent window
    ///     parentWindow.Title = "Found!";
    /// }
    /// </code>
    /// </example>
    public static DependencyObject FindAncestorOfType(this DependencyObject o, Type ancestorType)
    {
        var parent = VisualTreeHelper.GetParent(o);
        if (parent != null)
        {
            if (parent.GetType().IsSubclassOf(ancestorType) || parent.GetType() == ancestorType)
            {
                return parent;
            }
            return FindAncestorOfType(parent, ancestorType);
        }
        return null;
    }
}