namespace Palaso.UI.WindowsForms.SettingProtection
{
	partial class SettingProtectionDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingProtectionDialog));
			this._okButton = new System.Windows.Forms.Button();
			this._normallyHiddenCheckbox = new System.Windows.Forms.CheckBox();
			this._requirePasswordCheckBox = new System.Windows.Forms.CheckBox();
			this._image = new System.Windows.Forms.PictureBox();
			this._passwordNotice = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel1 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel2 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.betterLabel3 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.l10NSharpExtender1 = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._image)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).BeginInit();
			this.SuspendLayout();
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._okButton, null);
			this.l10NSharpExtender1.SetLocalizingId(this._okButton, "Common.OKButton");
			this._okButton.Location = new System.Drawing.Point(403, 274);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(88, 30);
			this._okButton.TabIndex = 0;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _normallyHiddenCheckbox
			//
			this._normallyHiddenCheckbox.AutoSize = true;
			this._normallyHiddenCheckbox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._normallyHiddenCheckbox, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._normallyHiddenCheckbox, null);
			this.l10NSharpExtender1.SetLocalizingId(this._normallyHiddenCheckbox, "SettingProtection.NormallyHiddenCheckbox");
			this._normallyHiddenCheckbox.Location = new System.Drawing.Point(12, 88);
			this._normallyHiddenCheckbox.Name = "_normallyHiddenCheckbox";
			this._normallyHiddenCheckbox.Size = new System.Drawing.Size(216, 19);
			this._normallyHiddenCheckbox.TabIndex = 1;
			this._normallyHiddenCheckbox.Text = "Hide the button that opens settings.";
			this._normallyHiddenCheckbox.UseVisualStyleBackColor = true;
			this._normallyHiddenCheckbox.CheckedChanged += new System.EventHandler(this.OnNormallHidden_CheckedChanged);
			//
			// _requirePasswordCheckBox
			//
			this._requirePasswordCheckBox.AutoSize = true;
			this._requirePasswordCheckBox.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.l10NSharpExtender1.SetLocalizableToolTip(this._requirePasswordCheckBox, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._requirePasswordCheckBox, null);
			this.l10NSharpExtender1.SetLocalizingId(this._requirePasswordCheckBox, "SettingProtection.RequirePasswordCheckBox");
			this._requirePasswordCheckBox.Location = new System.Drawing.Point(12, 189);
			this._requirePasswordCheckBox.Name = "_requirePasswordCheckBox";
			this._requirePasswordCheckBox.Size = new System.Drawing.Size(284, 19);
			this._requirePasswordCheckBox.TabIndex = 2;
			this._requirePasswordCheckBox.Text = "Require the factory password to get into settings.";
			this._requirePasswordCheckBox.UseVisualStyleBackColor = true;
			this._requirePasswordCheckBox.CheckedChanged += new System.EventHandler(this.OnRequirePasswordCheckBox_CheckedChanged);
			//
			// _image
			//
			this._image.Image = global::Palaso.UI.WindowsForms.Properties.Resources.lockOpen48x48;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._image, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._image, null);
			this.l10NSharpExtender1.SetLocalizingId(this._image, "SettingProtectionDialog._image");
			this._image.Location = new System.Drawing.Point(12, 20);
			this._image.Name = "_image";
			this._image.Size = new System.Drawing.Size(48, 48);
			this._image.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this._image.TabIndex = 3;
			this._image.TabStop = false;
			//
			// _passwordNotice
			//
			this._passwordNotice.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._passwordNotice.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._passwordNotice.Enabled = false;
			this._passwordNotice.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._passwordNotice.ForeColor = System.Drawing.Color.DimGray;
			this.l10NSharpExtender1.SetLocalizableToolTip(this._passwordNotice, null);
			this.l10NSharpExtender1.SetLocalizationComment(this._passwordNotice, "Don\'t forget to have the {0} and {1} in your translated string!");
			this.l10NSharpExtender1.SetLocalizingId(this._passwordNotice, "SettingProtection.PasswordNotice");
			this._passwordNotice.Location = new System.Drawing.Point(29, 212);
			this._passwordNotice.Multiline = true;
			this._passwordNotice.Name = "_passwordNotice";
			this._passwordNotice.ReadOnly = true;
			this._passwordNotice.Size = new System.Drawing.Size(462, 33);
			this._passwordNotice.TabIndex = 5;
			this._passwordNotice.TabStop = false;
			this._passwordNotice.Text = "Factory password for these settings is \"{0}\".  If you forget it, you can always g" +
	"oogle for \"{1}\" and \"factory password\"";
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Enabled = false;
			this.betterLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.betterLabel1, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.betterLabel1, null);
			this.l10NSharpExtender1.SetLocalizingId(this.betterLabel1, "SettingProtectionDialog.betterLabel1");
			this.betterLabel1.Location = new System.Drawing.Point(0, 0);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(166, 0);
			this.betterLabel1.TabIndex = 4;
			this.betterLabel1.TabStop = false;
			//
			// betterLabel2
			//
			this.betterLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel2.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel2.Enabled = false;
			this.betterLabel2.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.betterLabel2.ForeColor = System.Drawing.Color.DimGray;
			this.l10NSharpExtender1.SetLocalizableToolTip(this.betterLabel2, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.betterLabel2, null);
			this.l10NSharpExtender1.SetLocalizingId(this.betterLabel2, "SettingProtection.CtrlShiftHint");
			this.betterLabel2.Location = new System.Drawing.Point(29, 113);
			this.betterLabel2.Multiline = true;
			this.betterLabel2.Name = "betterLabel2";
			this.betterLabel2.ReadOnly = true;
			this.betterLabel2.Size = new System.Drawing.Size(462, 17);
			this.betterLabel2.TabIndex = 6;
			this.betterLabel2.TabStop = false;
			this.betterLabel2.Text = "The button will show up when you hold down the Ctrl and Shift keys together.";
			//
			// betterLabel3
			//
			this.betterLabel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel3.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel3.Enabled = false;
			this.betterLabel3.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.l10NSharpExtender1.SetLocalizableToolTip(this.betterLabel3, null);
			this.l10NSharpExtender1.SetLocalizationComment(this.betterLabel3, null);
			this.l10NSharpExtender1.SetLocalizingId(this.betterLabel3, "SettingProtection.AlsoHideOthersNotice");
			this.betterLabel3.Location = new System.Drawing.Point(29, 136);
			this.betterLabel3.Multiline = true;
			this.betterLabel3.Name = "betterLabel3";
			this.betterLabel3.ReadOnly = true;
			this.betterLabel3.Size = new System.Drawing.Size(431, 33);
			this.betterLabel3.TabIndex = 7;
			this.betterLabel3.TabStop = false;
			this.betterLabel3.Text = "This may also hide other buttons which are not needed by the non-advanced user.";
			//
			// l10NSharpExtender1
			//
			this.l10NSharpExtender1.LocalizationManagerId = "Palaso";
			this.l10NSharpExtender1.PrefixForNewItems = null;
			//
			// SettingProtectionDialog
			//
			this.AcceptButton = this._okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(503, 316);
			this.Controls.Add(this.betterLabel3);
			this.Controls.Add(this.betterLabel2);
			this.Controls.Add(this._passwordNotice);
			this.Controls.Add(this.betterLabel1);
			this.Controls.Add(this._image);
			this.Controls.Add(this._requirePasswordCheckBox);
			this.Controls.Add(this._normallyHiddenCheckbox);
			this.Controls.Add(this._okButton);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.l10NSharpExtender1.SetLocalizableToolTip(this, null);
			this.l10NSharpExtender1.SetLocalizationComment(this, null);
			this.l10NSharpExtender1.SetLocalizingId(this, "SettingProtection.SettingProtectionDialogTitle");
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingProtectionDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Settings Protection";
			((System.ComponentModel.ISupportInitialize)(this._image)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.l10NSharpExtender1)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.CheckBox _normallyHiddenCheckbox;
		private System.Windows.Forms.CheckBox _requirePasswordCheckBox;
		private System.Windows.Forms.PictureBox _image;
		private Widgets.BetterLabel betterLabel1;
		private Widgets.BetterLabel _passwordNotice;
		private Widgets.BetterLabel betterLabel2;
		private Widgets.BetterLabel betterLabel3;
		private L10NSharp.UI.L10NSharpExtender l10NSharpExtender1;
	}
}