# Blazing.ToggleSwitch.Wpf

A modern, flexible WPF ToggleSwitch control inspired by Windows 10/11 design patterns. This lookless control provides complete customization capabilities while maintaining the familiar toggle switch UX found in modern applications.

## Table of Contents

-   [Overview](#overview)
-   [Features](#features)
-   [Installation](#installation)
-   [Quick Start](#quick-start)
-   [Basic Usage](#basic-usage)
-   [Advanced Usage](#advanced-usage)
-   [Properties Reference](#properties-reference)
-   [Styling and Theming](#styling-and-theming)
-   [Examples](#examples)
-   [Architecture](#architecture)
-   [Credits](#credits)

## Overview

**Blazing.ToggleSwitch.Wpf** is a comprehensive WPF toggle switch control that brings modern UI patterns to your desktop applications. Built as a lookless control, it provides the flexibility to completely customize its appearance while maintaining consistent behavior and accessibility features.

### Key Inspiration

This control is inspired by the original CodeProject article "[Flexible WPF ToggleSwitch Lookless Control in C# & VB](https://www.codeproject.com/articles/WPF-ToggleSwitch-Control)" and has been modernized with enhanced features, improved performance, and comprehensive documentation.

## Features

-   ✅ **Modern Design**: Sleek toggle switch control for contemporary WPF applications
-   ✅ **Highly Customizable**: Extensive styling options for colors, text, positioning, and animations
-   ✅ **Flexible Layout**: Position header content and switch elements in any arrangement
-   ✅ **Data Binding**: Full support for WPF data binding with dependency properties
-   ✅ **Smooth Animations**: Built-in animations for state changes and hover effects
-   ✅ **Accessibility**: Proper keyboard navigation and screen reader support
-   ✅ **Lookless Control**: Complete template customization capability
-   ✅ **SharedSizeGroup**: Consistent alignment across multiple controls
-   ✅ **Well Documented**: Comprehensive XML documentation and examples

## Installation

### Project Reference

Add a project reference to include the ToggleSwitch control:

```xml
<ItemGroup>
  <ProjectReference Include="path\to\Blazing.ToggleSwitch.Wpf\Blazing.ToggleSwitch.Wpf.csproj" />
</ItemGroup>
```

### Requirements

-   .NET 8.0 or later
-   WPF application target (`UseWPF` enabled)

## Quick Start

### 1. Add Namespace Reference

In your XAML file, add the namespace reference:

```xml
<Window xmlns:toggleSwitch="clr-namespace:Blazing.ToggleSwitch.Wpf;assembly=Blazing.ToggleSwitch.Wpf"
        ... >
```

### 2. Use the Control

```xml
<toggleSwitch:ToggleSwitch
    IsChecked="{Binding IsEnabled}"
    Header="Enable Feature:"
    CheckedText="On"
    UncheckedText="Off" />
```

## Basic Usage

### Simple Toggle Switch

```xml
<!-- Basic toggle switch -->
<toggleSwitch:ToggleSwitch IsChecked="{Binding IsEnabled}" />
```

### Toggle Switch with Header

```xml
<!-- Toggle switch with descriptive header -->
<toggleSwitch:ToggleSwitch
    IsChecked="{Binding EnableNotifications}"
    Header="Enable Notifications:"
    HeaderContentPlacement="Left" />
```

### Customized Appearance

```xml
<!-- Customized colors and text -->
<toggleSwitch:ToggleSwitch
    IsChecked="{Binding DarkMode}"
    Header="Dark Mode"
    CheckedText="On"
    UncheckedText="Off"
    CheckedBackground="#FF2196F3"
    CheckedForeground="White"
    UncheckedBackground="#FFCCCCCC"
    UncheckedForeground="Black"
    SwitchWidth="60" />
```

## Advanced Usage

### Flexible Positioning

The control supports four positioning options for the header relative to the switch:

```xml
<!-- Header on the left (default) -->
<toggleSwitch:ToggleSwitch
    Header="Left Header:"
    HeaderContentPlacement="Left"
    IsChecked="{Binding Setting1}" />

<!-- Header on the right -->
<toggleSwitch:ToggleSwitch
    Header="Right Header:"
    HeaderContentPlacement="Right"
    IsChecked="{Binding Setting2}" />

<!-- Header on top -->
<toggleSwitch:ToggleSwitch
    Header="Top Header:"
    HeaderContentPlacement="Top"
    IsChecked="{Binding Setting3}" />

<!-- Header on bottom -->
<toggleSwitch:ToggleSwitch
    Header="Bottom Header:"
    HeaderContentPlacement="Bottom"
    IsChecked="{Binding Setting4}" />
```

### Content Alignment

Control the horizontal alignment of header content and switch elements:

```xml
<toggleSwitch:ToggleSwitch
    Header="Aligned Content:"
    HeaderHorizontalAlignment="Center"
    SwitchHorizontalAlignment="Right"
    HeaderContentPlacement="Top"
    IsChecked="{Binding IsEnabled}" />
```

### SharedSizeGroup for Consistent Layout

Use SharedSizeGroup to align multiple toggle switches consistently:

```xml
<Grid Grid.IsSharedSizeScope="True">
    <Grid.Resources>
        <sys:String x:Key="ToggleSwitchGroup">MainGroup</sys:String>
    </Grid.Resources>

    <StackPanel>
        <toggleSwitch:ToggleSwitch
            Header="Short Name:"
            SharedSizeGroupName="{StaticResource ToggleSwitchGroup}"
            IsChecked="{Binding Setting1}" />

        <toggleSwitch:ToggleSwitch
            Header="Very Long Descriptive Name:"
            SharedSizeGroupName="{StaticResource ToggleSwitchGroup}"
            IsChecked="{Binding Setting2}" />

        <toggleSwitch:ToggleSwitch
            Header="Medium Name:"
            SharedSizeGroupName="{StaticResource ToggleSwitchGroup}"
            IsChecked="{Binding Setting3}" />
    </StackPanel>
</Grid>
```

### Custom Content

Use complex content for the header:

```xml
<toggleSwitch:ToggleSwitch IsChecked="{Binding IsEnabled}">
    <toggleSwitch:ToggleSwitch.Content>
        <StackPanel Orientation="Horizontal">
            <Image Source="icon.png" Width="16" Height="16" Margin="0,0,5,0" />
            <TextBlock Text="Enable Feature" FontWeight="Bold" />
        </StackPanel>
    </toggleSwitch:ToggleSwitch.Content>
</toggleSwitch:ToggleSwitch>
```

## Properties Reference

### Content Properties

| Property        | Type     | Default | Description                                   |
| --------------- | -------- | ------- | --------------------------------------------- |
| `Header`        | `object` | `null`  | Header content displayed alongside the switch |
| `CheckedText`   | `string` | `"On"`  | Text displayed when the switch is checked     |
| `UncheckedText` | `string` | `"Off"` | Text displayed when the switch is unchecked   |

### Layout Properties

| Property                    | Type                  | Default                    | Description                                                      |
| --------------------------- | --------------------- | -------------------------- | ---------------------------------------------------------------- |
| `HeaderContentPlacement`    | `Dock`                | `Dock.Left`                | Position of header relative to switch (Left, Right, Top, Bottom) |
| `SwitchContentPlacement`    | `Dock`                | `Dock.Left`                | Position of switch content relative to switch                    |
| `HeaderHorizontalAlignment` | `HorizontalAlignment` | `HorizontalAlignment.Left` | Horizontal alignment of header content                           |
| `SwitchHorizontalAlignment` | `HorizontalAlignment` | `HorizontalAlignment.Left` | Horizontal alignment of switch content                           |
| `CheckHorizontalAlignment`  | `HorizontalAlignment` | `HorizontalAlignment.Left` | Horizontal alignment of check content                            |

### Sizing Properties

| Property              | Type        | Default               | Description                                             |
| --------------------- | ----------- | --------------------- | ------------------------------------------------------- |
| `SwitchWidth`         | `double`    | `44.0`                | Width of the switch control in device-independent units |
| `HeaderPadding`       | `Thickness` | `(0,0,0,0)`           | Padding around header content                           |
| `SwitchPadding`       | `Thickness` | `(8,0,8,0)`           | Padding around switch content                           |
| `CheckPadding`        | `Thickness` | `(0,0,0,0)`           | Padding around check content                            |
| `SharedSizeGroupName` | `string`    | `"ToggleSwitchGroup"` | SharedSizeGroup name for consistent sizing              |

### Appearance Properties

| Property               | Type    | Default   | Description                     |
| ---------------------- | ------- | --------- | ------------------------------- |
| `CheckedBackground`    | `Brush` | `#0063B1` | Background brush when checked   |
| `CheckedForeground`    | `Brush` | `White`   | Foreground brush when checked   |
| `CheckedBorderBrush`   | `Brush` | `#0063B1` | Border brush when checked       |
| `UncheckedBackground`  | `Brush` | `#CCCCCC` | Background brush when unchecked |
| `UncheckedForeground`  | `Brush` | `Black`   | Foreground brush when unchecked |
| `UncheckedBorderBrush` | `Brush` | `#CCCCCC` | Border brush when unchecked     |

### Internal Properties

| Property          | Type   | Description                                      |
| ----------------- | ------ | ------------------------------------------------ |
| `IsHeaderStretch` | `bool` | Internal property for header stretching behavior |

## Styling and Theming

### Custom Styles

Create custom styles to apply consistent appearance across your application:

```xml
<Style x:Key="CustomToggleSwitchStyle" TargetType="{x:Type toggleSwitch:ToggleSwitch}">
    <Setter Property="CheckedBackground" Value="#FF4CAF50" />
    <Setter Property="CheckedForeground" Value="White" />
    <Setter Property="UncheckedBackground" Value="#FFFF5722" />
    <Setter Property="UncheckedForeground" Value="White" />
    <Setter Property="SwitchWidth" Value="60" />
    <Setter Property="HeaderContentPlacement" Value="Left" />
    <Setter Property="HeaderPadding" Value="0,0,10,0" />
</Style>

<!-- Apply the style -->
<toggleSwitch:ToggleSwitch
    Style="{StaticResource CustomToggleSwitchStyle}"
    Header="Custom Styled:"
    IsChecked="{Binding IsEnabled}" />
```

### Control Template Customization

For complete customization, override the control template:

```xml
<Style TargetType="{x:Type toggleSwitch:ToggleSwitch}">
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="{x:Type toggleSwitch:ToggleSwitch}">
                <!-- Custom template implementation -->
                <!-- See Themes/Generic.xaml for reference -->
            </ControlTemplate>
        </Setter.Value>
    </Setter>
</Style>
```

## Examples

### Settings Page

Create a settings page with multiple toggle switches:

```xml
<ScrollViewer>
    <StackPanel Margin="20">
        <TextBlock Text="Application Settings" FontSize="24" FontWeight="Bold" Margin="0,0,0,20"/>

        <Grid Grid.IsSharedSizeScope="True">
            <Grid.Resources>
                <sys:String x:Key="SettingsGroup">SettingsGroup</sys:String>
                <Style TargetType="{x:Type toggleSwitch:ToggleSwitch}">
                    <Setter Property="Margin" Value="0,5" />
                    <Setter Property="SharedSizeGroupName" Value="{StaticResource SettingsGroup}" />
                    <Setter Property="HeaderContentPlacement" Value="Left" />
                </Style>
            </Grid.Resources>

            <StackPanel>
                <toggleSwitch:ToggleSwitch
                    Header="Enable Notifications:"
                    IsChecked="{Binding EnableNotifications}" />

                <toggleSwitch:ToggleSwitch
                    Header="Auto-save Documents:"
                    IsChecked="{Binding AutoSave}" />

                <toggleSwitch:ToggleSwitch
                    Header="Show Advanced Options:"
                    IsChecked="{Binding ShowAdvanced}" />

                <toggleSwitch:ToggleSwitch
                    Header="Enable Dark Theme:"
                    IsChecked="{Binding DarkTheme}"
                    CheckedBackground="#FF2196F3"
                    CheckedText="Dark"
                    UncheckedText="Light" />
            </StackPanel>
        </Grid>
    </StackPanel>
</ScrollViewer>
```

### Data Entry Form

Use toggle switches in forms with other controls:

```xml
<Grid Grid.IsSharedSizeScope="True">
    <Grid.Resources>
        <sys:String x:Key="FormGroup">FormGroup</sys:String>
    </Grid.Resources>

    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" SharedSizeGroup="{StaticResource FormGroup}" />
        <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" />
        <RowDefinition Height="Auto" />
        <RowDefinition Height="Auto" />
    </Grid.RowDefinitions>

    <!-- Regular form fields -->
    <Label Grid.Row="0" Grid.Column="0" Content="Name:" />
    <TextBox Grid.Row="0" Grid.Column="1" Text="{Binding UserName}" />

    <Label Grid.Row="1" Grid.Column="0" Content="Email:" />
    <TextBox Grid.Row="1" Grid.Column="1" Text="{Binding Email}" />

    <!-- Toggle switch integrated into form -->
    <toggleSwitch:ToggleSwitch
        Grid.Row="2" Grid.ColumnSpan="2"
        Header="Subscribe to Newsletter:"
        HeaderContentPlacement="Left"
        SharedSizeGroupName="{StaticResource FormGroup}"
        IsChecked="{Binding SubscribeNewsletter}"
        Margin="0,10,0,0" />
</Grid>
```

## Architecture

### Design Principles

The ToggleSwitch control follows WPF best practices:

1. **Lookless Control**: Extends `ToggleButton` with complete template customization
2. **Dependency Properties**: All customizable aspects exposed as dependency properties
3. **Visual States**: Uses Visual State Manager for animations and state management
4. **MVVM Friendly**: Full data binding support with change notifications
5. **Accessibility**: Proper keyboard navigation and screen reader support

### Control Structure

```
ToggleSwitch (extends ToggleButton)
├── Header Content (ContentPresenter)
├── Switch Mechanism (Border with animations)
│   ├── Switch Background (animated)
│   ├── Switch Thumb (animated position)
│   └── State Text (CheckedText/UncheckedText)
└── Visual States
    ├── CheckStates (Checked, Unchecked, Indeterminate)
    ├── CommonStates (Normal, MouseOver, Pressed, Disabled)
    ├── FocusStates (Focused, Unfocused)
    └── ValidationStates (Valid, InvalidFocused, InvalidUnfocused)
```

### Key Classes

-   **`ToggleSwitch`**: Main control class extending `ToggleButton`
-   **`ToggleSwitchOffsetConverter`**: Value converter for switch thumb positioning
-   **`DependencyObjectExtension`**: Extension methods for visual tree navigation

### Files Structure

```
Blazing.ToggleSwitch.Wpf/
├── ToggleSwitch.cs                    # Main control implementation
├── Converters/
│   └── ToggleSwitchOffsetConverter.cs # Position calculation converter
├── Extensions/
│   └── DependencyObjectExtension.cs  # Helper extension methods
├── Resources/
│   └── ColorStyles.xaml              # Color resource definitions
├── Themes/
│   └── Generic.xaml                  # Default control template
├── Properties/
│   ├── Resources.resx                # Embedded resources
│   └── Settings.settings             # Project settings
├── GlobalSuppressions.cs             # Code analysis suppressions
└── README.md                         # This documentation
```

## Credits

This control is inspired by and builds upon the excellent work from:

-   **Original Article**: "[Flexible WPF ToggleSwitch Lookless Control in C# & VB](https://www.codeproject.com/articles/WPF-ToggleSwitch-Control)" by Graeme Grant
-   **Microsoft Design Guidelines**: Windows 10/11 toggle switch patterns
-   **WPF Community**: Best practices and patterns from the WPF development community

### Enhancements in This Version

-   Comprehensive XML documentation
-   Modern C# language features and patterns
-   Enhanced property system with better defaults
-   Improved performance and memory efficiency
-   Extended customization options
-   Better accessibility support
-   Cleaner architecture with separation of concerns

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests to improve the control.

## License

[Specify your license here]

---

_Part of the Blazing.Extensions.DependencyInjection solution - see the [main README](../../../README.md) for the complete solution overview._
