namespace WpfExample.ViewModels;

/// <summary>
/// Base class for all ViewModels using CommunityToolkit.Mvvm.
/// Provides INotifyPropertyChanged implementation and other MVVM infrastructure.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    // ObservableObject from CommunityToolkit.Mvvm provides:
    // - INotifyPropertyChanged implementation
    // - SetProperty methods
    // - OnPropertyChanged methods
    // All automatically with source generators for optimal performance
}
