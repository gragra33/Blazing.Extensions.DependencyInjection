#nullable disable
namespace WinFormsExample.Views
{
    partial class HomeView
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
            patternPanel = new Panel();
            pattern4Label = new Label();
            pattern3Label = new Label();
            pattern2Label = new Label();
            pattern1Label = new Label();
            patternTitleLabel = new Label();
            separator = new Label();
            showInfoButton = new Button();
            refreshButton = new Button();
            lastRefreshedLabel = new Label();
            welcomeLabel = new Label();
            benefitsPanel = new Panel();
            benefit4Label = new Label();
            benefit3Label = new Label();
            benefit2Label = new Label();
            benefit1Label = new Label();
            benefitsTitleLabel = new Label();
            descriptionLabel = new Label();
            titleLabel = new Label();
            mainPanel.SuspendLayout();
            patternPanel.SuspendLayout();
            benefitsPanel.SuspendLayout();
            SuspendLayout();
            // 
            // mainPanel
            // 
            mainPanel.AutoScroll = true;
            mainPanel.Controls.Add(patternPanel);
            mainPanel.Controls.Add(patternTitleLabel);
            mainPanel.Controls.Add(separator);
            mainPanel.Controls.Add(showInfoButton);
            mainPanel.Controls.Add(refreshButton);
            mainPanel.Controls.Add(lastRefreshedLabel);
            mainPanel.Controls.Add(welcomeLabel);
            mainPanel.Controls.Add(benefitsPanel);
            mainPanel.Controls.Add(descriptionLabel);
            mainPanel.Controls.Add(titleLabel);
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.Location = new System.Drawing.Point(0, 0);
            mainPanel.Name = "mainPanel";
            mainPanel.Padding = new Padding(20);
            mainPanel.Size = new System.Drawing.Size(800, 795);
            mainPanel.TabIndex = 0;
            // 
            // patternPanel
            // 
            patternPanel.BackColor = System.Drawing.Color.FromArgb(255, 249, 230);
            patternPanel.BorderStyle = BorderStyle.FixedSingle;
            patternPanel.Controls.Add(pattern4Label);
            patternPanel.Controls.Add(pattern3Label);
            patternPanel.Controls.Add(pattern2Label);
            patternPanel.Controls.Add(pattern1Label);
            patternPanel.Location = new System.Drawing.Point(20, 480);
            patternPanel.Name = "patternPanel";
            patternPanel.Padding = new Padding(15);
            patternPanel.Size = new System.Drawing.Size(740, 120);
            patternPanel.TabIndex = 9;
            // 
            // pattern4Label
            // 
            pattern4Label.Location = new System.Drawing.Point(15, 75);
            pattern4Label.Name = "pattern4Label";
            pattern4Label.Size = new System.Drawing.Size(700, 20);
            pattern4Label.TabIndex = 3;
            pattern4Label.Text = "✓ Easy Testing: Views can be tested independently";
            // 
            // pattern3Label
            // 
            pattern3Label.Location = new System.Drawing.Point(15, 55);
            pattern3Label.Name = "pattern3Label";
            pattern3Label.Size = new System.Drawing.Size(700, 20);
            pattern3Label.TabIndex = 2;
            pattern3Label.Text = "✓ Separation of Concerns: Each tab manages its own state";
            // 
            // pattern2Label
            // 
            pattern2Label.Location = new System.Drawing.Point(15, 35);
            pattern2Label.Name = "pattern2Label";
            pattern2Label.Size = new System.Drawing.Size(700, 20);
            pattern2Label.TabIndex = 1;
            pattern2Label.Text = "✓ Independent Resolution: Each View resolves its own dependencies via DI";
            // 
            // pattern1Label
            // 
            pattern1Label.Location = new System.Drawing.Point(15, 15);
            pattern1Label.Name = "pattern1Label";
            pattern1Label.Size = new System.Drawing.Size(700, 20);
            pattern1Label.TabIndex = 0;
            pattern1Label.Text = "✓ Loose Coupling: MainForm doesn't know about specific View types";
            // 
            // patternTitleLabel
            // 
            patternTitleLabel.AutoSize = true;
            patternTitleLabel.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            patternTitleLabel.Location = new System.Drawing.Point(20, 440);
            patternTitleLabel.Name = "patternTitleLabel";
            patternTitleLabel.Size = new System.Drawing.Size(275, 25);
            patternTitleLabel.TabIndex = 8;
            patternTitleLabel.Text = "🎯 View-First Pattern Benefits";
            // 
            // separator
            // 
            separator.BorderStyle = BorderStyle.Fixed3D;
            separator.Location = new System.Drawing.Point(20, 420);
            separator.Name = "separator";
            separator.Size = new System.Drawing.Size(740, 2);
            separator.TabIndex = 7;
            // 
            // showInfoButton
            // 
            showInfoButton.Location = new System.Drawing.Point(220, 360);
            showInfoButton.Name = "showInfoButton";
            showInfoButton.Size = new System.Drawing.Size(150, 35);
            showInfoButton.TabIndex = 6;
            showInfoButton.Text = "ℹ️ Show Info";
            showInfoButton.UseVisualStyleBackColor = true;
            // 
            // refreshButton
            // 
            refreshButton.Location = new System.Drawing.Point(20, 360);
            refreshButton.Name = "refreshButton";
            refreshButton.Size = new System.Drawing.Size(180, 35);
            refreshButton.TabIndex = 5;
            refreshButton.Text = "🔄 Refresh Welcome";
            refreshButton.UseVisualStyleBackColor = true;
            // 
            // lastRefreshedLabel
            // 
            lastRefreshedLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            lastRefreshedLabel.ForeColor = System.Drawing.Color.Gray;
            lastRefreshedLabel.Location = new System.Drawing.Point(20, 330);
            lastRefreshedLabel.Name = "lastRefreshedLabel";
            lastRefreshedLabel.Size = new System.Drawing.Size(740, 20);
            lastRefreshedLabel.TabIndex = 4;
            lastRefreshedLabel.Text = "Not yet refreshed";
            // 
            // welcomeLabel
            // 
            welcomeLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Italic);
            welcomeLabel.ForeColor = System.Drawing.Color.FromArgb(47, 79, 79);
            welcomeLabel.Location = new System.Drawing.Point(20, 280);
            welcomeLabel.Name = "welcomeLabel";
            welcomeLabel.Size = new System.Drawing.Size(740, 40);
            welcomeLabel.TabIndex = 3;
            welcomeLabel.Text = "Welcome to the Blazing.Extensions.DependencyInjection Demo!";
            // 
            // benefitsPanel
            // 
            benefitsPanel.BackColor = System.Drawing.Color.FromArgb(240, 248, 255);
            benefitsPanel.BorderStyle = BorderStyle.FixedSingle;
            benefitsPanel.Controls.Add(benefit4Label);
            benefitsPanel.Controls.Add(benefit3Label);
            benefitsPanel.Controls.Add(benefit2Label);
            benefitsPanel.Controls.Add(benefit1Label);
            benefitsPanel.Controls.Add(benefitsTitleLabel);
            benefitsPanel.Location = new System.Drawing.Point(20, 120);
            benefitsPanel.Name = "benefitsPanel";
            benefitsPanel.Padding = new Padding(15);
            benefitsPanel.Size = new System.Drawing.Size(740, 140);
            benefitsPanel.TabIndex = 2;
            // 
            // benefit4Label
            // 
            benefit4Label.Location = new System.Drawing.Point(15, 105);
            benefit4Label.Name = "benefit4Label";
            benefit4Label.Size = new System.Drawing.Size(700, 20);
            benefit4Label.TabIndex = 4;
            benefit4Label.Text = "• Lifetime management: Scoped, transient, and singleton services work seamlessly";
            // 
            // benefit3Label
            // 
            benefit3Label.Location = new System.Drawing.Point(15, 85);
            benefit3Label.Name = "benefit3Label";
            benefit3Label.Size = new System.Drawing.Size(700, 20);
            benefit3Label.TabIndex = 3;
            benefit3Label.Text = "• Service injection: Services are injected into controls, not manually created";
            // 
            // benefit2Label
            // 
            benefit2Label.Location = new System.Drawing.Point(15, 65);
            benefit2Label.Name = "benefit2Label";
            benefit2Label.Size = new System.Drawing.Size(700, 20);
            benefit2Label.TabIndex = 2;
            benefit2Label.Text = "• Tab-based architecture: Each tab is a separate UserControl, loaded dynamically";
            // 
            // benefit1Label
            // 
            benefit1Label.Location = new System.Drawing.Point(15, 45);
            benefit1Label.Name = "benefit1Label";
            benefit1Label.Size = new System.Drawing.Size(700, 20);
            benefit1Label.TabIndex = 1;
            benefit1Label.Text = "• Automatic View discovery: Each view implementing ITabView is automatically discovered and registered";
            // 
            // benefitsTitleLabel
            // 
            benefitsTitleLabel.AutoSize = true;
            benefitsTitleLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            benefitsTitleLabel.Location = new System.Drawing.Point(15, 15);
            benefitsTitleLabel.Name = "benefitsTitleLabel";
            benefitsTitleLabel.Size = new System.Drawing.Size(135, 21);
            benefitsTitleLabel.TabIndex = 0;
            benefitsTitleLabel.Text = "✨ Key Benefits:";
            // 
            // descriptionLabel
            // 
            descriptionLabel.Location = new System.Drawing.Point(20, 65);
            descriptionLabel.Name = "descriptionLabel";
            descriptionLabel.Size = new System.Drawing.Size(740, 40);
            descriptionLabel.TabIndex = 1;
            descriptionLabel.Text = "This application demonstrates how Blazing.Extensions.DependencyInjection simplifies dependency injection in WinForms applications with multiple tabs and user controls.";
            // 
            // titleLabel
            // 
            titleLabel.AutoSize = true;
            titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            titleLabel.Location = new System.Drawing.Point(20, 20);
            titleLabel.Name = "titleLabel";
            titleLabel.Size = new System.Drawing.Size(321, 30);
            titleLabel.TabIndex = 0;
            titleLabel.Text = "Welcome to the DI Demo App";
            // 
            // HomeView
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(mainPanel);
            Name = "HomeView";
            Size = new System.Drawing.Size(800, 795);
            mainPanel.ResumeLayout(false);
            mainPanel.PerformLayout();
            patternPanel.ResumeLayout(false);
            benefitsPanel.ResumeLayout(false);
            benefitsPanel.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.Panel benefitsPanel;
        private System.Windows.Forms.Label benefitsTitleLabel;
        private System.Windows.Forms.Label benefit1Label;
        private System.Windows.Forms.Label benefit2Label;
        private System.Windows.Forms.Label benefit3Label;
        private System.Windows.Forms.Label benefit4Label;
        private System.Windows.Forms.Label welcomeLabel;
        private System.Windows.Forms.Label lastRefreshedLabel;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button showInfoButton;
        private System.Windows.Forms.Label separator;
        private System.Windows.Forms.Label patternTitleLabel;
        private System.Windows.Forms.Panel patternPanel;
        private System.Windows.Forms.Label pattern1Label;
        private System.Windows.Forms.Label pattern2Label;
        private System.Windows.Forms.Label pattern3Label;
        private System.Windows.Forms.Label pattern4Label;
    }
}