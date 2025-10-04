using System;
using System.Reflection;
using System.Windows;

namespace Blazing.ToggleSwitch.Wpf.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing ToggleSwitch Assembly Attributes");
            Console.WriteLine("=========================================");

            // Get the assembly containing ToggleSwitch
            var assembly = typeof(ToggleSwitch).Assembly;
            Console.WriteLine($"Assembly: {assembly.FullName}");
            
            // Check for ThemeInfoAttribute
            var themeInfoAttributes = assembly.GetCustomAttributes(typeof(ThemeInfoAttribute), false);
            if (themeInfoAttributes.Length > 0)
            {
                var themeInfo = (ThemeInfoAttribute)themeInfoAttributes[0];
                Console.WriteLine($"ThemeInfoAttribute found:");
                Console.WriteLine($"  GenericDictionaryLocation: {themeInfo.GenericDictionaryLocation}");
                Console.WriteLine($"  ThemeDictionaryLocation: {themeInfo.ThemeDictionaryLocation}");
            }
            else
            {
                Console.WriteLine("ThemeInfoAttribute NOT found!");
            }
            
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}