using BEM = Blazing.Common.BEM.CssClass;

namespace Blazing.ToggleSwitch.Blazor.Css;

/// <summary>
/// Provides BEM-style CSS class names for the ToggleSwitch Blazor components.
/// </summary>
public static class CssClass // BEM Naming Convention
{
    /// <summary>
    /// Contains CSS class names for the Toggle component and its elements.
    /// </summary>
    internal static class Toggle
    {
        /// <summary>
        /// The root CSS class for the toggle component.
        /// </summary>
        public const string Root = BEM.LibPrefix + "toggle";

        /// <summary>
        /// The CSS class for the toggle label.
        /// </summary>
        public const string Label = BEM.NameJoin + "label";
        /// <summary>
        /// The CSS class for the toggle container.
        /// </summary>
        public const string Container = Root + BEM.NameJoin + "container";
        /// <summary>
        /// The CSS class for the toggle pill element.
        /// </summary>
        public const string Pill = Root + BEM.NameJoin + "pill";
        /// <summary>
        /// The CSS class for the toggle thumb element.
        /// </summary>
        public const string Thumb = Root + BEM.NameJoin + "thumb";
        /// <summary>
        /// The CSS class for the inner part of the toggle thumb.
        /// </summary>
        public const string ThumbInner = Root + BEM.NameJoin + "thumb-inner";
        /// <summary>
        /// The CSS class for the toggle state text.
        /// </summary>
        public const string State = Root + BEM.NameJoin + "state-text";

        /// <summary>
        /// Contains modifier CSS class names for the toggle component.
        /// </summary>
        internal static class Modifier
        {
            /// <summary>
            /// The CSS class for the disabled toggle modifier.
            /// </summary>
            public const string Disabled = Root + BEM.ModifierJoin + "disabled";
            /// <summary>
            /// The CSS class for the inline label modifier.
            /// </summary>
            public const string InlineLabel = Root + BEM.ModifierJoin + "inline-label";
            /// <summary>
            /// The CSS class for the modifier when on/off label is missing.
            /// </summary>
            public const string NoOnOffLabel = Root + BEM.ModifierJoin + "on-off-missing";
        }
    }
}