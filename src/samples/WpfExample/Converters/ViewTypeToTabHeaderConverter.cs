using System;
using System.Globalization;
using System.Windows.Data;
using WpfExample.Services;

namespace WpfExample.Converters;

/// <summary>
/// Converts a View Type to its tab header text using static metadata.
/// NO service resolution - completely safe for startup!
/// </summary>
public class ViewTypeToTabHeaderConverter : IValueConverter
{
    /// <summary>
    /// Converts a Type to its TabHeader string using static metadata.
    /// SAFE: No service resolution - uses TabMetadata.GetTabHeader()
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Type viewType)
            return "Unknown Tab";

        try
        {
            // SAFE: No service resolution - uses static metadata
            return TabMetadata.GetTabHeader(viewType);
        }
        catch (Exception ex)
        {
            // Log error or handle gracefully
            System.Diagnostics.Debug.WriteLine($"Failed to get tab header for {viewType.Name}: {ex.Message}");
            return viewType.Name;
        }
    }

    /// <summary>
    /// Not implemented - one-way binding only.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("ViewTypeToTabHeaderConverter is one-way only");
    }
}