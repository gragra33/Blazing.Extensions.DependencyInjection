namespace WinFormsExample.Views
{
    partial class SettingsView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            mainPanel = new Panel();
            titleLabel = new Label();
            notificationsPanel = new Panel();
            notificationsLabel = new Label();
            notificationsToggle = new Blazing.ToggleSwitch.WinForms.ToggleSwitch();
            autoSavePanel = new Panel();
            autoSaveLabel = new Label();
            autoSaveToggle = new Blazing.ToggleSwitch.WinForms.ToggleSwitch();
            intervalLabel = new Label();
            autoSaveIntervalTextBox = new TextBox();
            appearancePanel = new Panel();
            darkModeLabel = new Label();
            darkModeToggle = new Blazing.ToggleSwitch.WinForms.ToggleSwitch();
            themeLabel = new Label();
            themeComboBox = new ComboBox();
            languageLabel = new Label();
            languageComboBox = new ComboBox();
            audioPanel = new Panel();
            soundLabel = new Label();
            soundToggle = new Blazing.ToggleSwitch.WinForms.ToggleSwitch();
            volumeLabelTitle = new Label();
            volumeTrackBar = new TrackBar();
            volumeLabel = new Label();
            buttonPanel = new Panel();
            saveButton = new Button();
            resetButton = new Button();
            mainPanel.SuspendLayout();
            notificationsPanel.SuspendLayout();
            autoSavePanel.SuspendLayout();
            appearancePanel.SuspendLayout();
            audioPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)volumeTrackBar).BeginInit();
            buttonPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.AutoScroll = true;
            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(notificationsPanel);
            mainPanel.Controls.Add(autoSavePanel);
            mainPanel.Controls.Add(appearancePanel);
            mainPanel.Controls.Add(audioPanel);
            mainPanel.Controls.Add(buttonPanel);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(20);
            mainPanel.Size = new System.Drawing.Size(760, 713);
            mainPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            titleLabel.Location = new System.Drawing.Point(20, 20);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(223, 30);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Application Settings";
            // 
            // notificationsPanel
            // 
            notificationsPanel.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            notificationsPanel.BorderStyle = BorderStyle.FixedSingle;
            notificationsPanel.Controls.Add(notificationsLabel);
            notificationsPanel.Controls.Add(notificationsToggle);
            notificationsPanel.Location = new System.Drawing.Point(20, 70);
            notificationsPanel.Name = "notificationsPanel";
            notificationsPanel.Size = new System.Drawing.Size(400, 80);
            notificationsPanel.TabIndex = 1;
            // 
            // notificationsLabel
            // 
            notificationsLabel.AutoSize = true;
            notificationsLabel.Location = new System.Drawing.Point(15, 20);
            notificationsLabel.Name = "notificationsLabel";
            notificationsLabel.Size = new System.Drawing.Size(116, 15);
            notificationsLabel.TabIndex = 0;
            notificationsLabel.Text = "Enable Notifications:";
            // 
            // notificationsToggle
            // 
            notificationsToggle.BackColor = System.Drawing.Color.Transparent;
            notificationsToggle.CheckedBackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            notificationsToggle.Location = new System.Drawing.Point(200, 15);
            notificationsToggle.Name = "notificationsToggle";
            notificationsToggle.Size = new System.Drawing.Size(44, 20);
            notificationsToggle.TabIndex = 1;
            notificationsToggle.UncheckedBackColor = System.Drawing.Color.Gray;
            // 
            // autoSavePanel
            // 
            autoSavePanel.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            autoSavePanel.BorderStyle = BorderStyle.FixedSingle;
            autoSavePanel.Controls.Add(autoSaveLabel);
            autoSavePanel.Controls.Add(autoSaveToggle);
            autoSavePanel.Controls.Add(intervalLabel);
            autoSavePanel.Controls.Add(autoSaveIntervalTextBox);
            autoSavePanel.Location = new System.Drawing.Point(20, 170);
            autoSavePanel.Name = "autoSavePanel";
            autoSavePanel.Size = new System.Drawing.Size(400, 80);
            autoSavePanel.TabIndex = 2;
            // 
            // autoSaveLabel
            // 
            autoSaveLabel.AutoSize = true;
            autoSaveLabel.Location = new System.Drawing.Point(15, 20);
            autoSaveLabel.Name = "autoSaveLabel";
            autoSaveLabel.Size = new System.Drawing.Size(127, 15);
            autoSaveLabel.TabIndex = 0;
            autoSaveLabel.Text = "Auto Save Documents:";
            // 
            // autoSaveToggle
            // 
            autoSaveToggle.BackColor = System.Drawing.Color.Transparent;
            autoSaveToggle.CheckedBackColor = System.Drawing.Color.FromArgb(76, 175, 80);
            autoSaveToggle.Location = new System.Drawing.Point(200, 15);
            autoSaveToggle.Name = "autoSaveToggle";
            autoSaveToggle.Size = new System.Drawing.Size(44, 20);
            autoSaveToggle.TabIndex = 1;
            autoSaveToggle.UncheckedBackColor = System.Drawing.Color.Gray;
            // 
            // intervalLabel
            // 
            intervalLabel.AutoSize = true;
            intervalLabel.Location = new System.Drawing.Point(15, 55);
            intervalLabel.Name = "intervalLabel";
            intervalLabel.Size = new System.Drawing.Size(103, 15);
            intervalLabel.TabIndex = 2;
            intervalLabel.Text = "Interval (minutes):";
            // 
            // autoSaveIntervalTextBox
            // 
            autoSaveIntervalTextBox.Location = new System.Drawing.Point(200, 52);
            autoSaveIntervalTextBox.Name = "autoSaveIntervalTextBox";
            autoSaveIntervalTextBox.Size = new System.Drawing.Size(60, 23);
            autoSaveIntervalTextBox.TabIndex = 3;
            autoSaveIntervalTextBox.Text = "5";
            // 
            // appearancePanel
            // 
            appearancePanel.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            appearancePanel.BorderStyle = BorderStyle.FixedSingle;
            appearancePanel.Controls.Add(darkModeLabel);
            appearancePanel.Controls.Add(darkModeToggle);
            appearancePanel.Controls.Add(themeLabel);
            appearancePanel.Controls.Add(themeComboBox);
            appearancePanel.Controls.Add(languageLabel);
            appearancePanel.Controls.Add(languageComboBox);
            appearancePanel.Location = new System.Drawing.Point(20, 270);
            appearancePanel.Name = "appearancePanel";
            appearancePanel.Size = new System.Drawing.Size(400, 120);
            appearancePanel.TabIndex = 3;
            // 
            // darkModeLabel
            // 
            darkModeLabel.AutoSize = true;
            darkModeLabel.Location = new System.Drawing.Point(15, 20);
            darkModeLabel.Name = "darkModeLabel";
            darkModeLabel.Size = new System.Drawing.Size(68, 15);
            darkModeLabel.TabIndex = 0;
            darkModeLabel.Text = "Dark Mode:";
            // 
            // darkModeToggle
            // 
            darkModeToggle.BackColor = System.Drawing.Color.Transparent;
            darkModeToggle.CheckedBackColor = System.Drawing.Color.FromArgb(33, 37, 41);
            darkModeToggle.CheckedText = "DARK";
            darkModeToggle.Location = new System.Drawing.Point(200, 15);
            darkModeToggle.Name = "darkModeToggle";
            darkModeToggle.Size = new System.Drawing.Size(44, 20);
            darkModeToggle.TabIndex = 1;
            darkModeToggle.UncheckedBackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            darkModeToggle.UncheckedText = "LIGHT";
            // 
            // themeLabel
            // 
            themeLabel.AutoSize = true;
            themeLabel.Location = new System.Drawing.Point(15, 55);
            themeLabel.Name = "themeLabel";
            themeLabel.Size = new System.Drawing.Size(47, 15);
            themeLabel.TabIndex = 2;
            themeLabel.Text = "Theme:";
            // 
            // themeComboBox
            // 
            themeComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            themeComboBox.FormattingEnabled = true;
            themeComboBox.Items.AddRange(new object[] { "Default", "Blue", "Green", "Purple" });
            themeComboBox.Location = new System.Drawing.Point(200, 52);
            themeComboBox.Name = "themeComboBox";
            themeComboBox.Size = new System.Drawing.Size(120, 23);
            themeComboBox.TabIndex = 3;
            // 
            // languageLabel
            // 
            languageLabel.AutoSize = true;
            languageLabel.Location = new System.Drawing.Point(15, 85);
            languageLabel.Name = "languageLabel";
            languageLabel.Size = new System.Drawing.Size(62, 15);
            languageLabel.TabIndex = 4;
            languageLabel.Text = "Language:";
            // 
            // languageComboBox
            // 
            languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            languageComboBox.FormattingEnabled = true;
            languageComboBox.Items.AddRange(new object[] { "English", "Spanish", "French", "German" });
            languageComboBox.Location = new System.Drawing.Point(200, 82);
            languageComboBox.Name = "languageComboBox";
            languageComboBox.Size = new System.Drawing.Size(120, 23);
            languageComboBox.TabIndex = 5;
            // 
            // audioPanel
            // 
            audioPanel.BackColor = System.Drawing.Color.FromArgb(248, 249, 250);
            audioPanel.BorderStyle = BorderStyle.FixedSingle;
            audioPanel.Controls.Add(soundLabel);
            audioPanel.Controls.Add(soundToggle);
            audioPanel.Controls.Add(volumeLabelTitle);
            audioPanel.Controls.Add(volumeTrackBar);
            audioPanel.Controls.Add(volumeLabel);
            audioPanel.Location = new System.Drawing.Point(20, 400);
            audioPanel.Name = "audioPanel";
            audioPanel.Size = new System.Drawing.Size(400, 100);
            audioPanel.TabIndex = 4;
            // 
            // soundLabel
            // 
            soundLabel.AutoSize = true;
            soundLabel.Location = new System.Drawing.Point(15, 20);
            soundLabel.Name = "soundLabel";
            soundLabel.Size = new System.Drawing.Size(82, 15);
            soundLabel.TabIndex = 0;
            soundLabel.Text = "Sound Effects:";
            // 
            // soundToggle
            // 
            soundToggle.BackColor = System.Drawing.Color.Transparent;
            soundToggle.CheckedBackColor = System.Drawing.Color.FromArgb(255, 152, 0);
            soundToggle.Location = new System.Drawing.Point(200, 15);
            soundToggle.Name = "soundToggle";
            soundToggle.Size = new System.Drawing.Size(44, 20);
            soundToggle.TabIndex = 1;
            soundToggle.UncheckedBackColor = System.Drawing.Color.Gray;
            // 
            // volumeLabelTitle
            // 
            volumeLabelTitle.AutoSize = true;
            volumeLabelTitle.Location = new System.Drawing.Point(15, 55);
            volumeLabelTitle.Name = "volumeLabelTitle";
            volumeLabelTitle.Size = new System.Drawing.Size(50, 15);
            volumeLabelTitle.TabIndex = 2;
            volumeLabelTitle.Text = "Volume:";
            // 
            // volumeTrackBar
            // 
            volumeTrackBar.Location = new System.Drawing.Point(200, 50);
            volumeTrackBar.Maximum = 100;
            volumeTrackBar.Name = "volumeTrackBar";
            volumeTrackBar.Size = new System.Drawing.Size(120, 45);
            volumeTrackBar.TabIndex = 3;
            volumeTrackBar.TickFrequency = 10;
            volumeTrackBar.Value = 50;
            // 
            // volumeLabel
            // 
            volumeLabel.AutoSize = true;
            volumeLabel.Location = new System.Drawing.Point(330, 55);
            volumeLabel.Name = "volumeLabel";
            volumeLabel.Size = new System.Drawing.Size(29, 15);
            volumeLabel.TabIndex = 4;
            volumeLabel.Text = "50%";
            // 
            // buttonPanel
            // 
            buttonPanel.Controls.Add(saveButton);
            buttonPanel.Controls.Add(resetButton);
            buttonPanel.Location = new System.Drawing.Point(20, 520);
            buttonPanel.Name = "buttonPanel";
            buttonPanel.Size = new System.Drawing.Size(400, 50);
            buttonPanel.TabIndex = 5;
            // 
            // saveButton
            // 
            saveButton.Location = new System.Drawing.Point(0, 10);
            saveButton.Name = "saveButton";
            saveButton.Size = new System.Drawing.Size(100, 30);
            saveButton.TabIndex = 0;
            saveButton.Text = "Save Settings";
            saveButton.UseVisualStyleBackColor = true;
            // 
            // resetButton
            // 
            resetButton.Location = new System.Drawing.Point(120, 10);
            resetButton.Name = "resetButton";
            resetButton.Size = new System.Drawing.Size(120, 30);
            resetButton.TabIndex = 1;
            resetButton.Text = "Reset to Defaults";
            resetButton.UseVisualStyleBackColor = true;
            // 
            // SettingsView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            Controls.Add(mainPanel);
            Name = "SettingsView";
            Size = new System.Drawing.Size(760, 713);
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            notificationsPanel.ResumeLayout(false);
            notificationsPanel.PerformLayout();
            autoSavePanel.ResumeLayout(false);
            autoSavePanel.PerformLayout();
            appearancePanel.ResumeLayout(false);
            appearancePanel.PerformLayout();
            audioPanel.ResumeLayout(false);
            audioPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)volumeTrackBar).EndInit();
            buttonPanel.ResumeLayout(false);
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Panel notificationsPanel;
        private System.Windows.Forms.Label notificationsLabel;
        private Blazing.ToggleSwitch.WinForms.ToggleSwitch notificationsToggle;
        private System.Windows.Forms.Panel autoSavePanel;
        private System.Windows.Forms.Label autoSaveLabel;
        private Blazing.ToggleSwitch.WinForms.ToggleSwitch autoSaveToggle;
        private System.Windows.Forms.Label intervalLabel;
        private System.Windows.Forms.TextBox autoSaveIntervalTextBox;
        private System.Windows.Forms.Panel appearancePanel;
        private System.Windows.Forms.Label darkModeLabel;
        private Blazing.ToggleSwitch.WinForms.ToggleSwitch darkModeToggle;
        private System.Windows.Forms.Label themeLabel;
        private System.Windows.Forms.ComboBox themeComboBox;
        private System.Windows.Forms.Label languageLabel;
        private System.Windows.Forms.ComboBox languageComboBox;
        private System.Windows.Forms.Panel audioPanel;
        private System.Windows.Forms.Label soundLabel;
        private Blazing.ToggleSwitch.WinForms.ToggleSwitch soundToggle;
        private System.Windows.Forms.Label volumeLabelTitle;
        private System.Windows.Forms.TrackBar volumeTrackBar;
        private System.Windows.Forms.Label volumeLabel;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Button resetButton;
    }
}