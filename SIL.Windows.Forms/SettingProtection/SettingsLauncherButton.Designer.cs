using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.SettingProtection
{
	partial class SettingsLauncherButton
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
			this._image = new System.Windows.Forms.PictureBox();
			this._checkVisibilityTmer = new System.Windows.Forms.Timer(this.components);
			this.l10NSharpExtender1 = new L10NSharp.Windows.Forms.L10NSharpExtender(this.components);
			this._linkLabel = new BetterLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this._image)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _image
			//
			this._image.Image = global::SIL.Windows.Forms.Properties.Resources.settings16x16;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._image, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._image, null);
			this.l10NSharpExtender1.SetLocalizingId(this._image, "SettingsLauncherButton._image");
			this._image.Location = new System.Drawing.Point(1, 0);
			this._image.Name = "_image";
			this._image.Size = new System.Drawing.Size(16, 16);
			this._image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this._image.TabIndex = 2;
			this._image.TabStop = false;
			//
			// _checkVisibilityTmer
			//
			this._checkVisibilityTmer.Enabled = true;
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Palaso";
			this.l10NSharpExtender1.PrefixForNewItems = null;
			//
			// betterLinkLabel1
			//
			this._linkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._linkLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._linkLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
			this._linkLabel.ForeColor = System.Drawing.Color.Blue;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._linkLabel, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._linkLabel, null);
			this.l10NSharpExtender1.SetLocalizingId(this._linkLabel, "SettingsLauncherButton.LauncherButtonLabel");
			this._linkLabel.Location = new System.Drawing.Point(23, 3);
			this._linkLabel.Multiline = true;
			this._linkLabel.Name = "betterLinkLabel1";
			this._linkLabel.ReadOnly = true;
			this._linkLabel.Size = new System.Drawing.Size(171, 17);
			this._linkLabel.TabIndex = 4;
			this._linkLabel.TabStop = false;
			this._linkLabel.Text = "Settings...";
			this._linkLabel.URL = null;
			//
			// SettingsLauncherButton
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._linkLabel);
			this.Controls.Add(this._image);
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "SettingsLauncherButton");
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SettingsLauncherButton";
			this.Size = new System.Drawing.Size(194, 22);
			((System.ComponentModel.ISupportInitialize)(this._image)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox _image;
		private System.Windows.Forms.Timer _checkVisibilityTmer;
		private L10NSharp.Windows.Forms.L10NSharpExtender l10NSharpExtender1;
		private Widgets.BetterLinkLabel _linkLabel;

	}
}
