using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.WritingSystems.WSIdentifiers
{
	partial class IpaIdentifierView
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
			this._purposeComboBox = new System.Windows.Forms.ComboBox();
			this.betterLabel1 = new SIL.Windows.Forms.Widgets.BetterLabel();
			this._localizationHelper = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).BeginInit();
			this.SuspendLayout();
			// 
			// linkLabel1
			// 
			this.linkLabel1.AutoSize = true;
			this._localizationHelper.SetLocalizableToolTip(this.linkLabel1, null);
			this._localizationHelper.SetLocalizationComment(this.linkLabel1, "In writing system setup when IPA special option is selected: text used in link to" +
        " a webpage describing IPA transcription");
			this._localizationHelper.SetLocalizingId(this.linkLabel1, "IpaIdentifierView.linkLabel1");
			this.linkLabel1.Location = new System.Drawing.Point(3, 44);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(199, 13);
			this.linkLabel1.TabIndex = 2;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Read about IPA transcription on the web";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// _purposeComboBox
			// 
			this._purposeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._purposeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._purposeComboBox.FormattingEnabled = true;
			this._localizationHelper.SetLocalizableToolTip(this._purposeComboBox, null);
			this._localizationHelper.SetLocalizationComment(this._purposeComboBox, null);
			this._localizationHelper.SetLocalizingId(this._purposeComboBox, "IpaIdentifierView._purposeComboBox");
			this._purposeComboBox.Location = new System.Drawing.Point(84, 15);
			this._purposeComboBox.Name = "_purposeComboBox";
			this._purposeComboBox.Size = new System.Drawing.Size(202, 21);
			this._purposeComboBox.TabIndex = 3;
			this._purposeComboBox.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// betterLabel1
			// 
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.ForeColor = System.Drawing.SystemColors.ControlText;
			this.betterLabel1.IsTextSelectable = false;
			this._localizationHelper.SetLocalizableToolTip(this.betterLabel1, null);
			this._localizationHelper.SetLocalizationComment(this.betterLabel1, "In writing system setup when IPA special option is selected: Label for the purpos" +
        "e combo box");
			this._localizationHelper.SetLocalizingId(this.betterLabel1, "IpaIdentifierView.betterLabel1");
			this.betterLabel1.Location = new System.Drawing.Point(4, 18);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(101, 13);
			this.betterLabel1.TabIndex = 0;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "Purpose:";
			// 
			// _localizationHelper
			// 
			this._localizationHelper.LocalizationManagerId = null;
			this._localizationHelper.PrefixForNewItems = null;
			// 
			// IpaIdentifierView
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._purposeComboBox);
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this.betterLabel1);
			this._localizationHelper.SetLocalizableToolTip(this, null);
			this._localizationHelper.SetLocalizationComment(this, null);
			this._localizationHelper.SetLocalizingId(this, "IpaIdentifierView.IpaIdentifierView");
			this.Name = "IpaIdentifierView";
			this.Size = new System.Drawing.Size(286, 78);
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BetterLabel betterLabel1;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.ComboBox _purposeComboBox;
		private L10NSharp.Windows.Forms.L10NSharpExtender _localizationHelper;
	}
}
