using Blazing.ToggleSwitch.WinForms;

namespace WinFormsExample.Views;

/// <summary>
/// SettingsView demonstrates the custom ToggleSwitch control and various settings.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ITabView))]
public partial class SettingsView : UserControl, ITabView
{
    private readonly IDialogService _dialogService;

    /// <summary>
    /// The header text displayed in the tab.
    /// </summary>
    public string TabHeader => "⚙️ Settings";

    /// <summary>
    /// The display order of the tab (lower numbers appear first).
    /// </summary>
    public int Order => 4;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsView"/> class.
    /// </summary>
    public SettingsView(IDialogService dialogService)
    {
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        
        InitializeComponent();
        SetupEventHandlers();
        LoadSettings();
        Console.WriteLine("SettingsView: Constructor called - Dialog service injected successfully");
    }

    private void SetupEventHandlers()
    {
        Console.WriteLine("SettingsView: Setting up event handlers for toggle switches and controls");
        if (notificationsToggle != null)
            notificationsToggle.CheckedChanged += NotificationsToggle_CheckedChanged;
        
        if (autoSaveToggle != null)
            autoSaveToggle.CheckedChanged += AutoSaveToggle_CheckedChanged;
        
        if (darkModeToggle != null)
            darkModeToggle.CheckedChanged += DarkModeToggle_CheckedChanged;
        
        if (soundToggle != null)
            soundToggle.CheckedChanged += SoundToggle_CheckedChanged;
        
        if (volumeTrackBar != null)
            volumeTrackBar.Scroll += VolumeTrackBar_Scroll;
        
        if (saveButton != null)
            saveButton.Click += SaveButton_Click;
        
        if (resetButton != null)
            resetButton.Click += ResetButton_Click;
    }

    private void LoadSettings()
    {
        Console.WriteLine("SettingsView: LoadSettings called - Loading default settings");
        // Load default settings
        if (notificationsToggle != null) notificationsToggle.Checked = true;
        if (autoSaveToggle != null) autoSaveToggle.Checked = true;
        if (darkModeToggle != null) darkModeToggle.Checked = false;
        if (soundToggle != null) soundToggle.Checked = true;
        if (volumeTrackBar != null) volumeTrackBar.Value = 50;
        if (volumeLabel != null) volumeLabel.Text = "50%";
        if (autoSaveIntervalTextBox != null) autoSaveIntervalTextBox.Text = "5";
        if (themeComboBox != null) themeComboBox.SelectedIndex = 0;
        if (languageComboBox != null) languageComboBox.SelectedIndex = 0;
        Console.WriteLine("SettingsView: Default settings loaded successfully");
    }

    private void NotificationsToggle_CheckedChanged(object? sender, EventArgs e)
    {
        Console.WriteLine("SettingsView: NotificationsToggle_CheckedChanged executed!");
        var toggle = sender as ToggleSwitch;
        var status = toggle?.Checked == true ? "Enabled" : "Disabled";
        Console.WriteLine($"SettingsView: Notifications {status}");
    }

    private void AutoSaveToggle_CheckedChanged(object? sender, EventArgs e)
    {
        Console.WriteLine("SettingsView: AutoSaveToggle_CheckedChanged executed!");
        var toggle = sender as ToggleSwitch;
        var isEnabled = toggle?.Checked == true;
        if (autoSaveIntervalTextBox != null)
        {
            autoSaveIntervalTextBox.Enabled = isEnabled;
            Console.WriteLine($"SettingsView: Auto-save interval textbox {(isEnabled ? "enabled" : "disabled")}");
        }
        Console.WriteLine($"SettingsView: Auto Save {(isEnabled ? "Enabled" : "Disabled")}");
    }

    private void DarkModeToggle_CheckedChanged(object? sender, EventArgs e)
    {
        Console.WriteLine("SettingsView: DarkModeToggle_CheckedChanged executed!");
        var toggle = sender as ToggleSwitch;
        var status = toggle?.Checked == true ? "Enabled" : "Disabled";
        Console.WriteLine($"SettingsView: Dark Mode {status}");
    }

    private void SoundToggle_CheckedChanged(object? sender, EventArgs e)
    {
        Console.WriteLine("SettingsView: SoundToggle_CheckedChanged executed!");
        var toggle = sender as ToggleSwitch;
        var isEnabled = toggle?.Checked == true;
        if (volumeTrackBar != null)
        {
            volumeTrackBar.Enabled = isEnabled;
            Console.WriteLine($"SettingsView: Volume trackbar {(isEnabled ? "enabled" : "disabled")}");
        }
        Console.WriteLine($"SettingsView: Sound {(isEnabled ? "Enabled" : "Disabled")}");
    }

    private void VolumeTrackBar_Scroll(object? sender, EventArgs e)
    {
        Console.WriteLine("SettingsView: VolumeTrackBar_Scroll executed!");
        if (volumeTrackBar != null && volumeLabel != null)
        {
            volumeLabel.Text = $"{volumeTrackBar.Value}%";
            Console.WriteLine($"SettingsView: Volume changed to {volumeTrackBar.Value}%");
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("SettingsView: SaveButton_Click executed!");
        _dialogService.ShowMessage("Settings Saved", 
            "Your settings have been saved successfully!\n\n" +
            "This demonstrates how the ToggleSwitch controls can be used " +
            "to manage application settings with a modern UI.");
        Console.WriteLine("SettingsView: Settings saved successfully");
    }

    private void ResetButton_Click(object? sender, EventArgs e)
    {
        Console.WriteLine("SettingsView: ResetButton_Click executed!");
        if (_dialogService.ShowConfirmation("Reset Settings", 
            "Are you sure you want to reset all settings to their default values?"))
        {
            Console.WriteLine("SettingsView: User confirmed settings reset");
            LoadSettings();
            Console.WriteLine("SettingsView: Settings reset to defaults");
        }
        else
        {
            Console.WriteLine("SettingsView: User cancelled settings reset");
        }
    }
}