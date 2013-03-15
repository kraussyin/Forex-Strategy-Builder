﻿namespace FSB_Launcher
{
    sealed partial class LauncherForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LauncherForm));
            this.lblApplicationName = new System.Windows.Forms.Label();
            this.tbxOutput = new System.Windows.Forms.ListBox();
            this.lblCompany = new System.Windows.Forms.Label();
            this.linkWebsite = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // lblApplicationName
            // 
            this.lblApplicationName.AutoSize = true;
            this.lblApplicationName.Font = new System.Drawing.Font("Segoe UI", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblApplicationName.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblApplicationName.Location = new System.Drawing.Point(64, 35);
            this.lblApplicationName.Name = "lblApplicationName";
            this.lblApplicationName.Size = new System.Drawing.Size(232, 30);
            this.lblApplicationName.TabIndex = 0;
            this.lblApplicationName.Text = "Forex Strategy Builder";
            // 
            // tbxOutput
            // 
            this.tbxOutput.BackColor = System.Drawing.SystemColors.Control;
            this.tbxOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbxOutput.ForeColor = System.Drawing.SystemColors.ControlText;
            this.tbxOutput.FormattingEnabled = true;
            this.tbxOutput.ItemHeight = 15;
            this.tbxOutput.Location = new System.Drawing.Point(96, 120);
            this.tbxOutput.Name = "tbxOutput";
            this.tbxOutput.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.tbxOutput.Size = new System.Drawing.Size(230, 135);
            this.tbxOutput.TabIndex = 3;
            this.tbxOutput.TabStop = false;
            // 
            // lblCompany
            // 
            this.lblCompany.AutoSize = true;
            this.lblCompany.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCompany.Location = new System.Drawing.Point(12, 276);
            this.lblCompany.Name = "lblCompany";
            this.lblCompany.Size = new System.Drawing.Size(104, 15);
            this.lblCompany.TabIndex = 4;
            this.lblCompany.Text = "Forex Software Ltd";
            // 
            // linkWebsite
            // 
            this.linkWebsite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.linkWebsite.AutoSize = true;
            this.linkWebsite.ForeColor = System.Drawing.SystemColors.ControlText;
            this.linkWebsite.Location = new System.Drawing.Point(301, 276);
            this.linkWebsite.Name = "linkWebsite";
            this.linkWebsite.Size = new System.Drawing.Size(107, 15);
            this.linkWebsite.TabIndex = 5;
            this.linkWebsite.TabStop = true;
            this.linkWebsite.Text = "http://forexsb.com";
            this.linkWebsite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkWebsite_LinkClicked);
            // 
            // LauncherForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(420, 300);
            this.Controls.Add(this.linkWebsite);
            this.Controls.Add(this.lblCompany);
            this.Controls.Add(this.tbxOutput);
            this.Controls.Add(this.lblApplicationName);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LauncherForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FSB Pro Launcher";
            this.TopMost = true;
            this.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.FormLauncher_MouseDoubleClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblApplicationName;
        private System.Windows.Forms.ListBox tbxOutput;
        private System.Windows.Forms.Label lblCompany;
        private System.Windows.Forms.LinkLabel linkWebsite;
    }
}

