using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace WpfExample.Converters;

/// <summary>
/// Converts temperature values between Celsius and Fahrenheit in weather strings.
/// Supports both IValueConverter and IMultiValueConverter for flexible usage.
/// </summary>
public class TemperatureConverter : IValueConverter, IMultiValueConverter
{
    private static readonly Regex TemperatureRegex = new(@"(\d+)°C", RegexOptions.Compiled);

    /// <summary>
    /// Converts temperature in weather strings from Celsius to Fahrenheit if needed.
    /// </summary>
    /// <param name="value">The weather string containing temperature in Celsius.</param>
    /// <param name="targetType">Not used.</param>
    /// <param name="parameter">Boolean indicating if Celsius should be used (true) or Fahrenheit (false).</param>
    /// <param name="culture">Not used.</param>
    /// <returns>Weather string with temperature in the requested unit.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string weatherText)
            return value;

        // If parameter is true or null, use Celsius (default)
        bool useCelsius = parameter == null || parameter is bool and true;
        
        return ConvertTemperature(weatherText, useCelsius);
    }

    /// <summary>
    /// Converts temperature using multiple values (weather text and boolean flag).
    /// </summary>
    /// <param name="values">Array containing [0] weather text and [1] useCelsius boolean.</param>
    /// <param name="targetType">Not used.</param>
    /// <param name="parameter">Not used.</param>
    /// <param name="culture">Not used.</param>
    /// <returns>Weather string with temperature in the requested unit.</returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2 || values[0] is not string weatherText)
            return values[0];

        bool useCelsius = values[1] is bool and true;
        
        return ConvertTemperature(weatherText, useCelsius);
    }

    /// <summary>
    /// Performs the actual temperature conversion logic.
    /// </summary>
    /// <param name="weatherText">The weather string containing temperature in Celsius.</param>
    /// <param name="useCelsius">Whether to use Celsius (true) or Fahrenheit (false).</param>
    /// <returns>Weather string with temperature in the requested unit.</returns>
    private static string ConvertTemperature(string weatherText, bool useCelsius)
    {
        if (useCelsius)
            return weatherText; // Return original Celsius text

        // Convert Celsius to Fahrenheit
        return TemperatureRegex.Replace(weatherText, match =>
        {
            if (int.TryParse(match.Groups[1].Value, out int celsius))
            {
                int fahrenheit = (int)Math.Round(celsius * 9.0 / 5.0 + 32);
                return $"{fahrenheit}°F";
            }
            return match.Value; // Return original if parsing fails
        });
    }

    /// <summary>
    /// ConvertBack is not implemented as this is a one-way converter.
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("TemperatureConverter is a one-way converter.");
    }

    /// <summary>
    /// ConvertBack is not implemented as this is a one-way converter.
    /// </summary>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException("TemperatureConverter is a one-way converter.");
    }
}