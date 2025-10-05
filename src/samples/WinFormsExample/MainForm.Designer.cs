using System.Windows.Forms;

namespace WinFormsExample
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Main TabControl for the application.
        /// </summary>
        private System.Windows.Forms.TabControl mainTabControl;
        
        /// <summary>
        /// Main panel that contains all UI elements.
        /// </summary>
        private System.Windows.Forms.Panel mainPanel;
        
        /// <summary>
        /// Title label for the application.
        /// </summary>
        private System.Windows.Forms.Label titleLabel;
        
        /// <summary>
        /// Subtitle label with framework info.
        /// </summary>
        private System.Windows.Forms.Label subtitleLabel;
        
        /// <summary>
        /// Info panel with TabViewHandler pattern description.
        /// </summary>
        private System.Windows.Forms.Panel infoPanel;
        
        /// <summary>
        /// Pattern description label.
        /// </summary>
        private System.Windows.Forms.Label patternLabel;
        
        /// <summary>
        /// Description label.
        /// </summary>
        private System.Windows.Forms.Label descriptionLabel;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.subtitleLabel = new System.Windows.Forms.Label();
            this.infoPanel = new System.Windows.Forms.Panel();
            this.patternLabel = new System.Windows.Forms.Label();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.mainTabControl = new System.Windows.Forms.TabControl();
            this.mainPanel.SuspendLayout();
            this.infoPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.mainTabControl);
            this.mainPanel.Controls.Add(this.infoPanel);
            this.mainPanel.Controls.Add(this.subtitleLabel);
            this.mainPanel.Controls.Add(this.titleLabel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(20);
            this.mainPanel.Size = new System.Drawing.Size(950, 700);
            this.mainPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.titleLabel.Location = new System.Drawing.Point(20, 20);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(577, 32);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "WinForms Example - Dynamic Tab Discovery";
            this.titleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.titleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // subtitleLabel
            // 
            this.subtitleLabel.AutoSize = true;
            this.subtitleLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.subtitleLabel.ForeColor = System.Drawing.Color.Gray;
            this.subtitleLabel.Location = new System.Drawing.Point(20, 60);
            this.subtitleLabel.Name = "subtitleLabel";
            this.subtitleLabel.Size = new System.Drawing.Size(334, 19);
            this.subtitleLabel.TabIndex = 1;
            this.subtitleLabel.Text = "Powered by Blazing.Extensions.DependencyInjection";
            this.subtitleLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.subtitleLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // infoPanel
            // 
            this.infoPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(249)))), ((int)(((byte)(230)))));
            this.infoPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.infoPanel.Controls.Add(this.descriptionLabel);
            this.infoPanel.Controls.Add(this.patternLabel);
            this.infoPanel.Location = new System.Drawing.Point(20, 95);
            this.infoPanel.Name = "infoPanel";
            this.infoPanel.Padding = new System.Windows.Forms.Padding(12);
            this.infoPanel.Size = new System.Drawing.Size(910, 80);
            this.infoPanel.TabIndex = 2;
            this.infoPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // patternLabel
            // 
            this.patternLabel.AutoSize = true;
            this.patternLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.patternLabel.Location = new System.Drawing.Point(12, 12);
            this.patternLabel.Name = "patternLabel";
            this.patternLabel.Size = new System.Drawing.Size(182, 19);
            this.patternLabel.TabIndex = 0;
            this.patternLabel.Text = "🎯 TabViewHandler Pattern:";
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.descriptionLabel.Location = new System.Drawing.Point(12, 35);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(815, 30);
            this.descriptionLabel.TabIndex = 1;
            this.descriptionLabel.Text = "Complete decoupling achieved! MainForm has ZERO knowledge of View types. Tabs are discovered automatically via ITabViewHandler service.\r\nAdd new tabs by implementing ITabView - no MainForm changes needed!";
            // 
            // mainTabControl
            // 
            this.mainTabControl.Location = new System.Drawing.Point(20, 190);
            this.mainTabControl.Name = "mainTabControl";
            this.mainTabControl.SelectedIndex = 0;
            this.mainTabControl.Size = new System.Drawing.Size(910, 480);
            this.mainTabControl.TabIndex = 3;
            this.mainTabControl.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(950, 700);
            this.Controls.Add(this.mainPanel);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinForms Example - Dynamic Tab Discovery";
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.infoPanel.ResumeLayout(false);
            this.infoPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}