using SIL.Windows.Forms.SettingProtection;

namespace SIL.Windows.Forms.Tests.SettingsProtection
{
	partial class DialogWithSomeSettings
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
			this._settingsProtectionLauncherButton = new SettingsProtectionLauncherButton();
			this.checkBox3 = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			//
			// _settingsProtectionLauncherButton
			//
			this._settingsProtectionLauncherButton.Location = new System.Drawing.Point(9, 216);
			this._settingsProtectionLauncherButton.Margin = new System.Windows.Forms.Padding(0);
			this._settingsProtectionLauncherButton.Name = "_settingsProtectionLauncherButton";
			this._settingsProtectionLauncherButton.Size = new System.Drawing.Size(257, 37);
			this._settingsProtectionLauncherButton.TabIndex = 0;
			//
			// checkBox3
			//
			this.checkBox3.AutoSize = true;
			this.checkBox3.Location = new System.Drawing.Point(21, 38);
			this.checkBox3.Name = "checkBox3";
			this.checkBox3.Size = new System.Drawing.Size(126, 17);
			this.checkBox3.TabIndex = 3;
			this.checkBox3.Text = "Some pretend setting";
			this.checkBox3.UseVisualStyleBackColor = true;
			//
			// DialogWithSomeSettings
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 262);
			this.Controls.Add(this.checkBox3);
			this.Controls.Add(this._settingsProtectionLauncherButton);
			this.Name = "DialogWithSomeSettings";
			this.Text = "DialogWithSomeSettings";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SettingsProtectionLauncherButton _settingsProtectionLauncherButton;
		private System.Windows.Forms.CheckBox checkBox3;
	}
}