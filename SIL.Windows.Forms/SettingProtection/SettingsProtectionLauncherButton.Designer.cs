using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.SettingProtection
{
	partial class SettingsProtectionLauncherButton
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
			this.betterLinkLabel1 = new BetterLinkLabel();
			this._image = new System.Windows.Forms.PictureBox();
			this.l10NSharpExtender1 = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._image)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			// 
			// betterLinkLabel1
			// 
			this.betterLinkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.betterLinkLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLinkLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
			this.betterLinkLabel1.ForeColor = System.Drawing.Color.Blue;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.betterLinkLabel1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.betterLinkLabel1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.betterLinkLabel1, "SettingsProtection.LauncherButtonLabel");
			this.betterLinkLabel1.Location = new System.Drawing.Point(36, 12);
			this.betterLinkLabel1.Multiline = true;
			this.betterLinkLabel1.Name = "betterLinkLabel1";
			this.betterLinkLabel1.ReadOnly = true;
			this.betterLinkLabel1.Size = new System.Drawing.Size(118, 17);
			this.betterLinkLabel1.TabIndex = 1;
			this.betterLinkLabel1.TabStop = false;
			this.betterLinkLabel1.Text = "Settings Protection...";
			this.betterLinkLabel1.URL = null;
			this.betterLinkLabel1.Click += new System.EventHandler(this.betterLinkLabel1_Click);
			// 
			// _image
			// 
			this._image.Image = global::SIL.Windows.Forms.Properties.Resources.lockClosed48x48;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._image, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._image, null);
			this.l10NSharpExtender1.SetLocalizingId(this._image, "SettingsProtection.SettingsProtectionLauncherButton._image");
			this._image.Location = new System.Drawing.Point(1, 0);
			this._image.Name = "_image";
			this._image.Size = new System.Drawing.Size(32, 32);
			this._image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._image.TabIndex = 2;
			this._image.TabStop = false;
			// 
			// l10NSharpExtender1
			// 
			this.l10NSharpExtender1.LocalizationManagerId = "Palaso";
			this.l10NSharpExtender1.PrefixForNewItems = "SettingsProtection";
			// 
			// SettingsProtectionLauncherButton
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.betterLinkLabel1);
			this.Controls.Add(this._image);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "SettingsProtection.SettingsProtectionLauncherButton.SettingsProtectionLauncherBut" +
        "ton");
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SettingsProtectionLauncherButton";
			this.Size = new System.Drawing.Size(163, 37);
			((System.ComponentModel.ISupportInitialize)(this._image)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Widgets.BetterLinkLabel betterLinkLabel1;
		private System.Windows.Forms.PictureBox _image;
		private L10NSharp.Windows.Forms.L10NSharpExtender l10NSharpExtender1;

	}
}
