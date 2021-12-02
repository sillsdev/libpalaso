using SIL.Windows.Forms.SettingProtection;

namespace SIL.Windows.Forms.Tests.SettingsProtection
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
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.settingsLauncherButton1 = new SIL.Windows.Forms.SettingProtection.SettingsLauncherButton();
			this._settingsProtectionHelper = new SIL.Windows.Forms.SettingProtection.SettingsProtectionHelper(this.components);
			this._toolStripButtonMaybe = new System.Windows.Forms.ToolStripButton();
			this._toolStripButtonToHide = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
			this.toolStrip1.SuspendLayout();
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
			this.label3.Size = new System.Drawing.Size(246, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "If you don\'t see controls below, press CTRL-SHIFT";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(151, 147);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(330, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "<-- Custom Settings launcher which uses a SettingsProtectionHelper ";
			// 
			// toolStrip1
			// 
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._toolStripButtonToHide,
            this.toolStripButton2,
            this._toolStripButtonMaybe});
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(509, 25);
			this.toolStrip1.TabIndex = 9;
			this.toolStrip1.Text = "toolStrip1";
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
			// _toolStripButtonMaybe
			// 
			this._toolStripButtonMaybe.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this._toolStripButtonMaybe.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._toolStripButtonMaybe.Name = "_toolStripButtonMaybe";
			this._toolStripButtonMaybe.Size = new System.Drawing.Size(47, 22);
			this._toolStripButtonMaybe.Text = "Maybe";
			// 
			// _toolStripButtonToHide
			// 
			this._toolStripButtonToHide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this._toolStripButtonToHide.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._toolStripButtonToHide.Name = "_toolStripButtonToHide";
			this._toolStripButtonToHide.Size = new System.Drawing.Size(56, 22);
			this._toolStripButtonToHide.Text = "Hide Me";
			// 
			// toolStripButton2
			// 
			this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.toolStripButton2.Name = "toolStripButton2";
			this.toolStripButton2.Size = new System.Drawing.Size(51, 22);
			this.toolStripButton2.Text = "Not me";
			// 
			// DialogWithLinkToSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(509, 191);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this.settingsLauncherButton1);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._customSettingsButton);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Name = "DialogWithLinkToSettings";
			this.Text = "DialogWithLinkToSettings";
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private SettingsProtectionHelper _settingsProtectionHelper;
		private System.Windows.Forms.Button _customSettingsButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private SettingsLauncherButton settingsLauncherButton1;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton _toolStripButtonToHide;
		private System.Windows.Forms.ToolStripButton toolStripButton2;
		private System.Windows.Forms.ToolStripButton _toolStripButtonMaybe;
	}
}