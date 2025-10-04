using System.Diagnostics.CodeAnalysis;
using System.Windows;

// Suppress IDE0130 for the entire assembly as we want a custom namespace structure
[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure",
    Justification = "Custom namespace structure for better API organization")]

// Specify that theme resources are in the same assembly as the control
[assembly: ThemeInfo(ResourceDictionaryLocation.SourceAssembly, ResourceDictionaryLocation.SourceAssembly)]