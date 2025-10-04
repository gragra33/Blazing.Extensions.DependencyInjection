using System;
using System.Windows.Controls;

namespace WpfExample.Views;

/// <summary>
/// HomeView demonstrates View-First pattern where TabViewModel automatically sets the correct ViewModel as DataContext.
/// Implements ITabView for automatic discovery by TabViewHandler.
/// Shows complete decoupling - MainViewModel has no knowledge of this View.
/// </summary>
public partial class HomeView : UserControl, ITabView
{
    /// <inheritdoc/>
    public string TabHeader => "🏠 Home";
    
    /// <inheritdoc/>
    public int Order => 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomeView"/> class.
    /// TabViewModel will automatically resolve and assign the correct HomeViewModel as DataContext.
    /// </summary>
    public HomeView()
    {
        InitializeComponent();
        Console.WriteLine("HomeView: Constructor called - TabViewModel will set DataContext");
    }
}
