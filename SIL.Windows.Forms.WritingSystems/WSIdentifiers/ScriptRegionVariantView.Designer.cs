using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	partial class ScriptRegionVariantView
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
			this.components = new System.ComponentModel.Container();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this._scriptCombo = new System.Windows.Forms.ComboBox();
			this._variant = new System.Windows.Forms.TextBox();
			this.betterLabel3 = new SIL.Windows.Forms.Widgets.BetterLabel();
			this.betterLabel2 = new SIL.Windows.Forms.Widgets.BetterLabel();
			this.betterLabel1 = new SIL.Windows.Forms.Widgets.BetterLabel();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this._regionCombo = new System.Windows.Forms.ComboBox();
			this._warningLabel = new SIL.Windows.Forms.Widgets.BetterLabel();
			this.SuspendLayout();
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(1, 80);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(213, 13);
			this.linkLabel1.TabIndex = 3;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Read about language identifiers on the web";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// _scriptCombo
			// 
			this._scriptCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._scriptCombo.ForeColor = System.Drawing.SystemColors.WindowText;
			this._scriptCombo.Location = new System.Drawing.Point(68, 3);
			this._scriptCombo.Name = "_scriptCombo";
			this._scriptCombo.Size = new System.Drawing.Size(161, 21);
			this._scriptCombo.TabIndex = 6;
			this._scriptCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._scriptCombo.SelectedIndexChanged += new System.EventHandler(this.ScriptCombo_OnSelectedIndexChanged);
			// 
			// _variant
			// 
			this._variant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._variant.Location = new System.Drawing.Point(68, 55);
			this._variant.Name = "_variant";
			this._variant.Size = new System.Drawing.Size(161, 20);
			this._variant.TabIndex = 7;
			this._variant.Leave += new System.EventHandler(this.Variant_OnLeave);
			// 
			// betterLabel3
			// 
			this.betterLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel3.Enabled = false;
			this.betterLabel3.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel3.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betterLabel3.IsTextSelectable = false;
			this.betterLabel3.Location = new System.Drawing.Point(1, 33);
			this.betterLabel3.Multiline = true;
			this.betterLabel3.Name = "betterLabel3";
			this.betterLabel3.ReadOnly = true;
			this.betterLabel3.Size = new System.Drawing.Size(55, 15);
			this.betterLabel3.TabIndex = 5;
			this.betterLabel3.TabStop = false;
			this.betterLabel3.Text = "Region:";
			// 
			// betterLabel2
			// 
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Enabled = false;
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betterLabel2.IsTextSelectable = false;
			this.betterLabel2.Location = new System.Drawing.Point(1, 58);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(55, 15);
			this.betterLabel2.TabIndex = 4;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Variant:";
			// 
			// betterLabel1
			// 
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betterLabel1.IsTextSelectable = false;
			this.betterLabel1.Location = new System.Drawing.Point(1, 8);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(55, 15);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Script:";
			// 
			// _regionCombo
			// 
			this._regionCombo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._regionCombo.FormattingEnabled = true;
			this._regionCombo.Location = new System.Drawing.Point(68, 29);
			this._regionCombo.Name = "_regionCombo";
			this._regionCombo.Size = new System.Drawing.Size(161, 21);
			this._regionCombo.TabIndex = 9;
			this._regionCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._regionCombo.SelectedIndexChanged += new System.EventHandler(this.RegionCombo_OnSelectedIndexChanged);
			// 
			// _warningLabel
			// 
			this._warningLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._warningLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._warningLabel.Enabled = false;
			this._warningLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._warningLabel.ForeColor = System.Drawing.SystemColors.ControlText;
			this._warningLabel.IsTextSelectable = false;
			this._warningLabel.Location = new System.Drawing.Point(1, 95);
			this._warningLabel.Multiline = true;
			this._warningLabel.Name = "_warningLabel";
			this._warningLabel.ReadOnly = true;
			this._warningLabel.Size = new System.Drawing.Size(213, 45);
			this._warningLabel.TabIndex = 9;
			this._warningLabel.TabStop = false;
			this._warningLabel.Text = "Do not change anything here if this project is shared with other people.  Data lo" +
    "ss or corruption may result.";
			// 
			// ScriptRegionVariantView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._regionCombo);
			this.Controls.Add(this._variant);
			this.Controls.Add(this._scriptCombo);
			this.Controls.Add(this.betterLabel3);
			this.Controls.Add(this.betterLabel2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.betterLabel1);
			this.Controls.Add(this._warningLabel);
			this.Name = "ScriptRegionVariantView";
			this.Size = new System.Drawing.Size(232, 144);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BetterLabel betterLabel1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private BetterLabel betterLabel2;
		private BetterLabel betterLabel3;
		private System.Windows.Forms.ComboBox _scriptCombo;
		private System.Windows.Forms.TextBox _variant;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ComboBox _regionCombo;
		private BetterLabel _warningLabel;
	}
}
