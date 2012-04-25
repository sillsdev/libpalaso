namespace PalasoUIWindowsForms.Tests.SettingsProtection
{
	partial class DialogWithLinkToSettings
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this._customSettingsButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this._settingsLauncherHelper = new Palaso.UI.WindowsForms.SettingProtection.SettingsLauncherHelper(this.components);
			this.settingsLauncherButton1 = new Palaso.UI.WindowsForms.SettingProtection.SettingsLauncherButton();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(27, 39);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(433, 13);
			this.label1.TabIndex = 1;
			this.label1.Text = "This would be some context in the application from which we might want to launch " +
	"settings";
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(200, 114);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(201, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "<--- Standard \"Settings Launcher Button\"";
			//
			// _customSettingsButton
			//
			this._customSettingsButton.Location = new System.Drawing.Point(30, 147);
			this._customSettingsButton.Name = "_customSettingsButton";
			this._customSettingsButton.Size = new System.Drawing.Size(75, 23);
			this._customSettingsButton.TabIndex = 5;
			this._customSettingsButton.Text = "Settings";
			this._customSettingsButton.UseVisualStyleBackColor = true;
			this._customSettingsButton.Click += new System.EventHandler(this._customSettingsButton_Click);
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(27, 86);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(212, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "If you don\'t see controls below, press CTRL";
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(151, 147);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(327, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "<-- Custom Settings launcher which uses a SettingsLauncherHelper ";
			//
			// settingsLauncherButton1
			//
			this.settingsLauncherButton1.LaunchSettingsCallback = null;
			this.settingsLauncherButton1.Location = new System.Drawing.Point(30, 114);
			this.settingsLauncherButton1.Margin = new System.Windows.Forms.Padding(0);
			this.settingsLauncherButton1.Name = "settingsLauncherButton1";
			this.settingsLauncherButton1.Size = new System.Drawing.Size(131, 22);
			this.settingsLauncherButton1.TabIndex = 8;
			//
			// DialogWithLinkToSettings
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(509, 191);
			this.Controls.Add(this.settingsLauncherButton1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._customSettingsButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "DialogWithLinkToSettings";
			this.Text = "DialogWithLinkToSettings";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private Palaso.UI.WindowsForms.SettingProtection.SettingsLauncherHelper _settingsLauncherHelper;
		private System.Windows.Forms.Button _customSettingsButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private Palaso.UI.WindowsForms.SettingProtection.SettingsLauncherButton settingsLauncherButton1;
	}
}