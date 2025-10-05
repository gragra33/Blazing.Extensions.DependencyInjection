namespace WinFormsExample.Views
{
    partial class WeatherView
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
            weatherInfoPanel = new Panel();
            lastUpdatedLabel = new Label();
            windSpeedLabel = new Label();
            humidityLabel = new Label();
            conditionLabel = new Label();
            temperatureLabel = new Label();
            temperatureUnitToggle = new Blazing.ToggleSwitch.WinForms.ToggleSwitch();
            temperatureUnitLabel = new Label();
            fetchButton = new Button();
            locationComboBox = new ComboBox();
            locationLabel = new Label();
            titleLabel = new Label();
            mainPanel.SuspendLayout();
            weatherInfoPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.Controls.Add(weatherInfoPanel);
            mainPanel.Controls.Add(temperatureUnitToggle);
            mainPanel.Controls.Add(temperatureUnitLabel);
            mainPanel.Controls.Add(fetchButton);
            mainPanel.Controls.Add(locationComboBox);
            mainPanel.Controls.Add(locationLabel);
            mainPanel.Controls.Add(titleLabel);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(20);
            mainPanel.Size = new System.Drawing.Size(760, 480);
            mainPanel.TabIndex = 0;
            // 
            // weatherInfoPanel
            // 
            weatherInfoPanel.BackColor = System.Drawing.Color.FromArgb(245, 250, 255);
            weatherInfoPanel.BorderStyle = BorderStyle.FixedSingle;
            weatherInfoPanel.Controls.Add(lastUpdatedLabel);
            weatherInfoPanel.Controls.Add(windSpeedLabel);
            weatherInfoPanel.Controls.Add(humidityLabel);
            weatherInfoPanel.Controls.Add(conditionLabel);
            weatherInfoPanel.Controls.Add(temperatureLabel);
            weatherInfoPanel.Location = new System.Drawing.Point(20, 140);
            weatherInfoPanel.Name = "weatherInfoPanel";
            weatherInfoPanel.Size = new System.Drawing.Size(500, 200);
            weatherInfoPanel.TabIndex = 6;
            weatherInfoPanel.Visible = false;
            // 
            // lastUpdatedLabel
            // 
            lastUpdatedLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            lastUpdatedLabel.ForeColor = System.Drawing.Color.Gray;
            lastUpdatedLabel.Location = new System.Drawing.Point(15, 160);
            lastUpdatedLabel.Name = "lastUpdatedLabel";
            lastUpdatedLabel.Size = new System.Drawing.Size(300, 20);
            lastUpdatedLabel.TabIndex = 4;
            lastUpdatedLabel.Text = "Last Updated: --";
            // 
            // windSpeedLabel
            // 
            windSpeedLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            windSpeedLabel.Location = new System.Drawing.Point(15, 95);
            windSpeedLabel.Name = "windSpeedLabel";
            windSpeedLabel.Size = new System.Drawing.Size(200, 20);
            windSpeedLabel.TabIndex = 3;
            windSpeedLabel.Text = "Wind Speed: -- km/h";
            // 
            // humidityLabel
            // 
            humidityLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            humidityLabel.Location = new System.Drawing.Point(15, 70);
            humidityLabel.Name = "humidityLabel";
            humidityLabel.Size = new System.Drawing.Size(200, 20);
            humidityLabel.TabIndex = 2;
            humidityLabel.Text = "Humidity: --%";
            // 
            // conditionLabel
            // 
            conditionLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            conditionLabel.Location = new System.Drawing.Point(15, 45);
            conditionLabel.Name = "conditionLabel";
            conditionLabel.Size = new System.Drawing.Size(200, 20);
            conditionLabel.TabIndex = 1;
            conditionLabel.Text = "Condition: --";
            // 
            // temperatureLabel
            // 
            temperatureLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            temperatureLabel.Location = new System.Drawing.Point(15, 15);
            temperatureLabel.Name = "temperatureLabel";
            temperatureLabel.Size = new System.Drawing.Size(200, 25);
            temperatureLabel.TabIndex = 0;
            temperatureLabel.Text = "Temperature: --°C";
            // 
            // temperatureUnitToggle
            // 
            temperatureUnitToggle.BackColor = System.Drawing.Color.Transparent;
            temperatureUnitToggle.CheckedBackColor = System.Drawing.Color.FromArgb(255, 87, 34);
            temperatureUnitToggle.CheckedText = "°F";
            temperatureUnitToggle.Location = new System.Drawing.Point(360, 95);
            temperatureUnitToggle.Name = "temperatureUnitToggle";
            temperatureUnitToggle.Size = new System.Drawing.Size(44, 20);
            temperatureUnitToggle.TabIndex = 5;
            temperatureUnitToggle.UncheckedBackColor = System.Drawing.Color.FromArgb(33, 150, 243);
            temperatureUnitToggle.UncheckedText = "°C";
            // 
            // temperatureUnitLabel
            // 
            temperatureUnitLabel.AutoSize = true;
            temperatureUnitLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            temperatureUnitLabel.Location = new System.Drawing.Point(360, 70);
            temperatureUnitLabel.Name = "temperatureUnitLabel";
            temperatureUnitLabel.Size = new System.Drawing.Size(119, 19);
            temperatureUnitLabel.TabIndex = 4;
            temperatureUnitLabel.Text = "Temperature Unit:";
            // 
            // fetchButton
            // 
            fetchButton.Location = new System.Drawing.Point(240, 92);
            fetchButton.Name = "fetchButton";
            fetchButton.Size = new System.Drawing.Size(100, 30);
            fetchButton.TabIndex = 3;
            fetchButton.Text = "Get Weather";
            fetchButton.UseVisualStyleBackColor = true;
            // 
            // locationComboBox
            // 
            locationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            locationComboBox.Location = new System.Drawing.Point(20, 95);
            locationComboBox.Name = "locationComboBox";
            locationComboBox.Size = new System.Drawing.Size(200, 23);
            locationComboBox.TabIndex = 2;
            // 
            // locationLabel
            // 
            locationLabel.AutoSize = true;
            locationLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            locationLabel.Location = new System.Drawing.Point(20, 70);
            locationLabel.Name = "locationLabel";
            locationLabel.Size = new System.Drawing.Size(103, 19);
            locationLabel.TabIndex = 1;
            locationLabel.Text = "Select Location:";
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            titleLabel.ForeColor = System.Drawing.Color.FromArgb(0, 120, 215);
            titleLabel.Location = new System.Drawing.Point(20, 20);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(230, 30);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Weather Information";
            // 
            // WeatherView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.White;
            Controls.Add(mainPanel);
            Name = "WeatherView";
            Size = new System.Drawing.Size(760, 480);
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            weatherInfoPanel.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label locationLabel;
        private System.Windows.Forms.ComboBox locationComboBox;
        private System.Windows.Forms.Button fetchButton;
        private System.Windows.Forms.Label temperatureUnitLabel;
        private Blazing.ToggleSwitch.WinForms.ToggleSwitch temperatureUnitToggle;
        private System.Windows.Forms.Panel weatherInfoPanel;
        private System.Windows.Forms.Label temperatureLabel;
        private System.Windows.Forms.Label conditionLabel;
        private System.Windows.Forms.Label humidityLabel;
        private System.Windows.Forms.Label windSpeedLabel;
        private System.Windows.Forms.Label lastUpdatedLabel;
    }
}