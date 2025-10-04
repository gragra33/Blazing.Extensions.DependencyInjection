using System.Globalization;
using System.Windows.Data;

namespace Blazing.ToggleSwitch.Wpf.Converters;

/// <summary>
/// Converts a width value to an offset for positioning toggle switch elements.
/// This converter is used to calculate the appropriate offset for toggle switch animations and positioning.
/// </summary>
/// <remarks>
/// The converter calculates an offset based on the input width, taking into account whether the offset
/// should be reversed. This is commonly used in toggle switch animations to position the moving thumb
/// element correctly within the switch track.
/// </remarks>
public class ToggleSwitchOffsetConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets a value indicating whether the offset calculation should be reversed.
    /// </summary>
    /// <value>
    /// <c>true</c> if the offset should be reversed (negative); otherwise, <c>false</c>.
    /// The default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// When set to <c>true</c>, the calculated offset will be negated, which is useful for
    /// positioning elements on the opposite side of the toggle switch.
    /// </remarks>
    public bool IsReversed { get; set; }

    /// <summary>
    /// Converts a width value to an offset value for toggle switch positioning.
    /// </summary>
    /// <param name="value">The width value to convert, expected to be a <see cref="double"/>.</param>
    /// <param name="targetType">The type of the binding target property (not used).</param>
    /// <param name="parameter">An optional converter parameter (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns>
    /// A <see cref="double"/> representing the calculated offset. If the input width is greater than 20,
    /// returns half the width minus 10 pixels, optionally negated if <see cref="IsReversed"/> is <c>true</c>.
    /// If the width is 20 or less, returns 0.
    /// </returns>
    /// <remarks>
    /// The calculation formula is:
    /// <list type="bullet">
    /// <item><description>If width &gt; 20: offset = (width / 2) - 10</description></item>
    /// <item><description>If <see cref="IsReversed"/> is true: offset = -offset</description></item>
    /// <item><description>If width ≤ 20: offset = 0</description></item>
    /// </list>
    /// </remarks>
    /// <example>
    /// <code>
    /// // For a width of 100 pixels and IsReversed = false:
    /// // Result = (100 / 2) - 10 = 40
    /// 
    /// // For a width of 100 pixels and IsReversed = true:
    /// // Result = -((100 / 2) - 10) = -40
    /// 
    /// // For a width of 15 pixels:
    /// // Result = 0
    /// </code>
    /// </example>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
        {
            return 0;
        }

        var width = (double)value;
        return width > 20D ? IsReversed ? -((width / 2) - 10) : (width / 2) - 10 : 0;

    }

    /// <summary>
    /// Converts an offset value back to a width value. This operation is not supported.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target (not used).</param>
    /// <param name="targetType">The type to convert to (not used).</param>
    /// <param name="parameter">The converter parameter to use (not used).</param>
    /// <param name="culture">The culture to use in the converter (not used).</param>
    /// <returns>Never returns a value.</returns>
    /// <exception cref="NotImplementedException">Always thrown as this operation is not supported.</exception>
    /// <remarks>
    /// This converter is designed for one-way binding only. The reverse conversion from offset to width
    /// is not implemented because it would be ambiguous and is not required for toggle switch functionality.
    /// </remarks>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}