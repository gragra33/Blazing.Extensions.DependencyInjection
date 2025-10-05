using System.Globalization;
using System.Windows.Data;

namespace WpfExample.Converters;

/// <summary>
/// Converts a View Type to its tab header text using dynamic discovery.
/// Uses dependency injection to get the actual ITabView instance.
/// </summary>
public class ViewTypeToTabHeaderConverter : IValueConverter
{
    /// <summary>
    /// Converts a Type to its TabHeader string using dynamic discovery.
    /// Gets the actual ITabView instance to read the TabHeader property.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Type viewType)
            return "Unknown Tab";

        try
        {
            // Get the service provider and find the ITabView instance of the requested type
            var serviceProvider = Application.Current.GetServices();
            var tabView = serviceProvider.GetServices<ITabView>()
                .FirstOrDefault(view => view.GetType() == viewType);
            
            return tabView?.TabHeader ?? viewType.Name;
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