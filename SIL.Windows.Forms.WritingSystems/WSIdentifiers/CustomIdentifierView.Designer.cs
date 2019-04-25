using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	partial class CustomIdentifierView
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
			this._languageTag = new System.Windows.Forms.TextBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.betterLabel1 = new SIL.Windows.Forms.Widgets.BetterLabel();
			this._l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._l10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			// 
			// _languageTag
			// 
			this._languageTag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._l10NSharpExtender.SetLocalizableToolTip(this._languageTag, null);
			this._l10NSharpExtender.SetLocalizationComment(this._languageTag, null);
			this._l10NSharpExtender.SetLocalizingId(this._languageTag, "CustomIdentifierView._languageTag");
			this._languageTag.Location = new System.Drawing.Point(84, 18);
			this._languageTag.Name = "_languageTag";
			this._languageTag.Size = new System.Drawing.Size(183, 20);
			this._languageTag.TabIndex = 1;
			this._languageTag.TextChanged += new System.EventHandler(this._languageTag_TextChanged);
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this._l10NSharpExtender.SetLocalizableToolTip(this.linkLabel1, null);
			this._l10NSharpExtender.SetLocalizationComment(this.linkLabel1, "text used in the link to a webpage that describes the language identifiers");
			this._l10NSharpExtender.SetLocalizingId(this.linkLabel1, "CustomIdentifierView.linkLabel1");
			this.linkLabel1.Location = new System.Drawing.Point(-1, 44);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(213, 13);
			this.linkLabel1.TabIndex = 2;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Read about language identifiers on the web";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// betterLabel1
			// 
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betterLabel1.IsTextSelectable = false;
			this._l10NSharpExtender.SetLocalizableToolTip(this.betterLabel1, null);
			this._l10NSharpExtender.SetLocalizationComment(this.betterLabel1, "label describes the code for a custom identifier. Translate only \'code\'");
			this._l10NSharpExtender.SetLocalizingId(this.betterLabel1, "CustomIdentifierView.betterLabel1");
			this.betterLabel1.Location = new System.Drawing.Point(4, 18);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(86, 13);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "RFC5646 code:";
			// 
			// _l10NSharpExtender
			// 
			this._l10NSharpExtender.LocalizationManagerId = null;
			this._l10NSharpExtender.PrefixForNewItems = null;
			// 
			// CustomIdentifierView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this._languageTag);
			this.Controls.Add(this.betterLabel1);
			this._l10NSharpExtender.SetLocalizableToolTip(this, null);
			this._l10NSharpExtender.SetLocalizationComment(this, null);
			this._l10NSharpExtender.SetLocalizingId(this, "CustomIdentifierView.CustomIdentifierView");
			this.Name = "CustomIdentifierView";
			this.Size = new System.Drawing.Size(270, 76);
			((System.ComponentModel.ISupportInitialize)(this._l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BetterLabel betterLabel1;
		private System.Windows.Forms.TextBox _languageTag;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private L10NSharp.UI.L10NSharpExtender _l10NSharpExtender;
	}
}
