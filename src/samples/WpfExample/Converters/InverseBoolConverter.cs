using System.Globalization;
using System.Windows.Data;

namespace WpfExample.Converters;

/// <summary>
/// Inverts a <see cref="bool"/> value for use with <see cref="System.Windows.UIElement.IsEnabled"/>
/// bindings where the source property represents a busy/loading state.
/// </summary>
[ValueConversion(typeof(bool), typeof(bool))]
public sealed class InverseBoolConverter : IValueConverter
{
    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is bool b && !b;
}
