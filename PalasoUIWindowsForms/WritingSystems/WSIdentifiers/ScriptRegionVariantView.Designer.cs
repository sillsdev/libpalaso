namespace Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers
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
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this._scriptCombo = new System.Windows.Forms.ComboBox();
			this._variant = new System.Windows.Forms.TextBox();
			this._region = new System.Windows.Forms.TextBox();
			this.betterLabel3 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel2 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel1 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.SuspendLayout();
			//
			// linkLabel1
			//
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Location = new System.Drawing.Point(-1, 85);
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
			this._scriptCombo.FormattingEnabled = true;
			this._scriptCombo.Location = new System.Drawing.Point(68, 3);
			this._scriptCombo.Name = "_scriptCombo";
			this._scriptCombo.Size = new System.Drawing.Size(161, 21);
			this._scriptCombo.TabIndex = 6;
			this._scriptCombo.SelectedIndexChanged += new System.EventHandler(this._scriptCombo_SelectedIndexChanged);
			//
			// _variant
			//
			this._variant.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._variant.Location = new System.Drawing.Point(68, 56);
			this._variant.Name = "_variant";
			this._variant.Size = new System.Drawing.Size(161, 20);
			this._variant.TabIndex = 7;
			this._variant.TextChanged += new System.EventHandler(this._variant_TextChanged);
			//
			// _region
			//
			this._region.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._region.Location = new System.Drawing.Point(68, 30);
			this._region.Name = "_region";
			this._region.Size = new System.Drawing.Size(161, 20);
			this._region.TabIndex = 8;
			this._region.TextChanged += new System.EventHandler(this._region_TextChanged);
			//
			// betterLabel3
			//
			this.betterLabel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel3.Location = new System.Drawing.Point(1, 32);
			this.betterLabel3.Multiline = true;
			this.betterLabel3.Name = "betterLabel3";
			this.betterLabel3.ReadOnly = true;
			this.betterLabel3.Size = new System.Drawing.Size(55, 18);
			this.betterLabel3.TabIndex = 5;
			this.betterLabel3.TabStop = false;
			this.betterLabel3.Text = "Region:";
			//
			// betterLabel2
			//
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Location = new System.Drawing.Point(1, 57);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(55, 18);
			this.betterLabel2.TabIndex = 4;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "Variant:";
			//
			// betterLabel1
			//
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Location = new System.Drawing.Point(1, 3);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(55, 18);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Script:";
			//
			// ScriptRegionVariantView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._region);
			this.Controls.Add(this._variant);
			this.Controls.Add(this._scriptCombo);
			this.Controls.Add(this.betterLabel3);
			this.Controls.Add(this.betterLabel2);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.betterLabel1);
			this.Name = "ScriptRegionVariantView";
			this.Size = new System.Drawing.Size(232, 107);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel2;
		private Palaso.UI.WindowsForms.Widgets.BetterLabel betterLabel3;
		private System.Windows.Forms.ComboBox _scriptCombo;
		private System.Windows.Forms.TextBox _variant;
		private System.Windows.Forms.TextBox _region;
	}
}
