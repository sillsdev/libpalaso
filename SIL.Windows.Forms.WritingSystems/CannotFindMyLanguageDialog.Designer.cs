using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.WritingSystems
{
	partial class CannotFindMyLanguageDialog
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CannotFindMyLanguageDialog));
			this._okButton = new System.Windows.Forms.Button();
			this._L10NSharpExtender = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
			this.betterLinkLabel3 = new BetterLinkLabel();
			this.betterLinkLabel1 = new BetterLinkLabel();
			this.betterLabel1 = new BetterLabel();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._L10NSharpExtender.SetLocalizableToolTip(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._okButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._okButton, "Common.OKButton");
			this._okButton.Location = new System.Drawing.Point(298, 299);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 10;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _L10NSharpExtender
			//
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "LanguageLookup.CannotFindMyLanguage";
			//
			// betterLinkLabel3
			//
			this.betterLinkLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.betterLinkLabel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLinkLabel3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
			this.betterLinkLabel3.ForeColor = System.Drawing.Color.Blue;
			this._L10NSharpExtender.SetLocalizableToolTip(this.betterLinkLabel3, null);
			this._L10NSharpExtender.SetLocalizationComment(this.betterLinkLabel3, null);
			this._L10NSharpExtender.SetLocalizingId(this.betterLinkLabel3, "LanguageLookup.CannotFindMyLanguage.LinkToSILInformationAboutLanguageCodes");
			this.betterLinkLabel3.Location = new System.Drawing.Point(12, 252);
			this.betterLinkLabel3.Multiline = true;
			this.betterLinkLabel3.Name = "betterLinkLabel3";
			this.betterLinkLabel3.ReadOnly = true;
			this.betterLinkLabel3.Size = new System.Drawing.Size(343, 17);
			this.betterLinkLabel3.TabIndex = 9;
			this.betterLinkLabel3.TabStop = false;
			this.betterLinkLabel3.Text = "About Language 639-3 Codes & How To Apply For New Ones";
			this.betterLinkLabel3.URL = "http://www.sil.org/iso639-3/";
			//
			// betterLinkLabel1
			//
			this.betterLinkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.betterLinkLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLinkLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
			this.betterLinkLabel1.ForeColor = System.Drawing.Color.Blue;
			this._L10NSharpExtender.SetLocalizableToolTip(this.betterLinkLabel1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.betterLinkLabel1, "It\'s ok to change what this shows; the underlying destination will remain ethnolo" +
		"gue.com");
			this._L10NSharpExtender.SetLocalizationPriority(this.betterLinkLabel1, L10NSharp.LocalizationPriority.Low);
			this._L10NSharpExtender.SetLocalizingId(this.betterLinkLabel1, "LanguageLookup.CannotFindMyLanguage.LinkToEthnologue");
			this.betterLinkLabel1.Location = new System.Drawing.Point(12, 226);
			this.betterLinkLabel1.Multiline = true;
			this.betterLinkLabel1.Name = "betterLinkLabel1";
			this.betterLinkLabel1.ReadOnly = true;
			this.betterLinkLabel1.Size = new System.Drawing.Size(234, 17);
			this.betterLinkLabel1.TabIndex = 7;
			this.betterLinkLabel1.TabStop = false;
			this.betterLinkLabel1.Text = "Ethnologue.com";
			this.betterLinkLabel1.URL = "http://Ethnologue.com";
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._L10NSharpExtender.SetLocalizableToolTip(this.betterLabel1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.betterLabel1, null);
			this._L10NSharpExtender.SetLocalizingId(this.betterLabel1, "LanguageLookup.CannotFindMyLanguage.LanguageLookup.CannotFindMyLanguageExplanatio" +
		"n2");
			this.betterLabel1.Location = new System.Drawing.Point(21, 16);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(352, 81);
			this.betterLabel1.TabIndex = 6;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = resources.GetString("betterLabel1.Text");
			//
			// CannotFindMyLanguageDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._okButton;
			this.ClientSize = new System.Drawing.Size(393, 334);
			this.ControlBox = false;
			this.Controls.Add(this._okButton);
			this.Controls.Add(this.betterLinkLabel3);
			this.Controls.Add(this.betterLinkLabel1);
			this.Controls.Add(this.betterLabel1);
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "LanguageLookup.CannotFindMyLanguageWindowTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "CannotFindMyLanguageDialog";
			this.Text = "About The ISO Language Registry";
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private BetterLabel betterLabel1;
		private BetterLinkLabel betterLinkLabel1;
		private BetterLinkLabel betterLinkLabel3;
		private System.Windows.Forms.Button _okButton;
		private L10NSharp.Windows.Forms.L10NSharpExtender _L10NSharpExtender;
	}
}