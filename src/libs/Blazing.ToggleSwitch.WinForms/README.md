# Blazing.ToggleSwitch.WinForms

A modern toggle switch control for WinForms applications, providing smooth animations and customizable appearance options.

## Features

- **Smooth Animations**: Fluid transitions between checked and unchecked states
- **Customizable Appearance**: Configure colors, text, and dimensions
- **Keyboard Support**: Space and Enter key support for accessibility
- **Event Handling**: Standard CheckedChanged event for integration
- **Modern Design**: Clean, contemporary styling that fits modern applications

## Usage

```csharp
var toggleSwitch = new ToggleSwitch
{
    CheckedText = "ON",
    UncheckedText = "OFF",
    CheckedBackColor = Color.FromArgb(0, 120, 215),
    UncheckedBackColor = Color.Gray,
    SwitchWidth = 60,
    SwitchHeight = 30
};

toggleSwitch.CheckedChanged += (sender, e) =>
{
    var toggle = sender as ToggleSwitch;
    Console.WriteLine($"Toggle is now: {(toggle.Checked ? "ON" : "OFF")}");
};
```

## Properties

- `Checked` - Gets or sets whether the toggle is checked
- `CheckedText` - Text displayed when checked (default: "ON")
- `UncheckedText` - Text displayed when unchecked (default: "OFF")
- `CheckedBackColor` - Background color when checked
- `UncheckedBackColor` - Background color when unchecked
- `SwitchColor` - Color of the switch button
- `TextColor` - Color of the text
- `SwitchWidth` - Width of the control
- `SwitchHeight` - Height of the control

## Events

- `CheckedChanged` - Raised when the Checked property changes