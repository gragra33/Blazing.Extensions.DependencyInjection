#nullable disable
namespace WinFormsExample.Views
{
    partial class DataView
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
            this.mainPanel = new System.Windows.Forms.Panel();
            this.titleLabel = new System.Windows.Forms.Label();
            this.listLabel = new System.Windows.Forms.Label();
            this.dataListBox = new System.Windows.Forms.ListBox();
            this.formPanel = new System.Windows.Forms.Panel();
            this.nameLabel = new System.Windows.Forms.Label();
            this.nameTextBox = new System.Windows.Forms.TextBox();
            this.descLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.isActiveCheckBox = new System.Windows.Forms.CheckBox();
            this.refreshButton = new System.Windows.Forms.Button();
            this.addButton = new System.Windows.Forms.Button();
            this.mainPanel.SuspendLayout();
            this.formPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainPanel
            // 
            this.mainPanel.Controls.Add(this.addButton);
            this.mainPanel.Controls.Add(this.refreshButton);
            this.mainPanel.Controls.Add(this.formPanel);
            this.mainPanel.Controls.Add(this.dataListBox);
            this.mainPanel.Controls.Add(this.listLabel);
            this.mainPanel.Controls.Add(this.titleLabel);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Padding = new System.Windows.Forms.Padding(20);
            this.mainPanel.Size = new System.Drawing.Size(760, 480);
            this.mainPanel.TabIndex = 0;
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.titleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(215)))));
            this.titleLabel.Location = new System.Drawing.Point(20, 20);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(148, 30);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "Data Management";
            // 
            // listLabel
            // 
            this.listLabel.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.listLabel.Location = new System.Drawing.Point(20, 70);
            this.listLabel.Name = "listLabel";
            this.listLabel.Size = new System.Drawing.Size(100, 20);
            this.listLabel.TabIndex = 1;
            this.listLabel.Text = "Data Items:";
            // 
            // dataListBox
            // 
            this.dataListBox.DisplayMember = "DisplayText";
            this.dataListBox.Location = new System.Drawing.Point(20, 95);
            this.dataListBox.Name = "dataListBox";
            this.dataListBox.Size = new System.Drawing.Size(300, 200);
            this.dataListBox.TabIndex = 2;
            // 
            // formPanel
            // 
            this.formPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(249)))), ((int)(((byte)(250)))));
            this.formPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.formPanel.Controls.Add(this.isActiveCheckBox);
            this.formPanel.Controls.Add(this.descriptionTextBox);
            this.formPanel.Controls.Add(this.descLabel);
            this.formPanel.Controls.Add(this.nameTextBox);
            this.formPanel.Controls.Add(this.nameLabel);
            this.formPanel.Location = new System.Drawing.Point(340, 95);
            this.formPanel.Name = "formPanel";
            this.formPanel.Size = new System.Drawing.Size(300, 200);
            this.formPanel.TabIndex = 3;
            // 
            // nameLabel
            // 
            this.nameLabel.Location = new System.Drawing.Point(10, 15);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(50, 20);
            this.nameLabel.TabIndex = 0;
            this.nameLabel.Text = "Name:";
            // 
            // nameTextBox
            // 
            this.nameTextBox.Location = new System.Drawing.Point(70, 12);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = new System.Drawing.Size(200, 23);
            this.nameTextBox.TabIndex = 1;
            // 
            // descLabel
            // 
            this.descLabel.Location = new System.Drawing.Point(10, 50);
            this.descLabel.Name = "descLabel";
            this.descLabel.Size = new System.Drawing.Size(70, 20);
            this.descLabel.TabIndex = 2;
            this.descLabel.Text = "Description:";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Location = new System.Drawing.Point(10, 75);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(260, 50);
            this.descriptionTextBox.TabIndex = 3;
            // 
            // isActiveCheckBox
            // 
            this.isActiveCheckBox.Location = new System.Drawing.Point(10, 135);
            this.isActiveCheckBox.Name = "isActiveCheckBox";
            this.isActiveCheckBox.Size = new System.Drawing.Size(80, 20);
            this.isActiveCheckBox.TabIndex = 4;
            this.isActiveCheckBox.Text = "Is Active";
            this.isActiveCheckBox.UseVisualStyleBackColor = true;
            // 
            // refreshButton
            // 
            this.refreshButton.Location = new System.Drawing.Point(20, 310);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(100, 30);
            this.refreshButton.TabIndex = 4;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            // 
            // addButton
            // 
            this.addButton.Location = new System.Drawing.Point(140, 310);
            this.addButton.Name = "addButton";
            this.addButton.Size = new System.Drawing.Size(100, 30);
            this.addButton.TabIndex = 5;
            this.addButton.Text = "Add New";
            this.addButton.UseVisualStyleBackColor = true;
            // 
            // DataView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.mainPanel);
            this.Name = "DataView";
            this.Size = new System.Drawing.Size(760, 480);
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.formPanel.ResumeLayout(false);
            this.formPanel.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label listLabel;
        private System.Windows.Forms.ListBox dataListBox;
        private System.Windows.Forms.Panel formPanel;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.TextBox nameTextBox;
        private System.Windows.Forms.Label descLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.CheckBox isActiveCheckBox;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button addButton;
    }
}