namespace Palaso.UI.WindowsForms.SettingProtection
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
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			((System.ComponentModel.ISupportInitialize)(this._image)).BeginInit();
			this.SuspendLayout();
			//
			// _image
			//
			this._image.Image = global::Palaso.UI.WindowsForms.Properties.Resources.settings16x16;
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
			// linkLabel1
			//
			this.linkLabel1.AutoSize = true;
			this.linkLabel1.Font = new System.Drawing.Font("Segoe UI", 9F);
			this.linkLabel1.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
			this.linkLabel1.LinkColor = System.Drawing.Color.Black;
			this.linkLabel1.Location = new System.Drawing.Point(23, 0);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(58, 15);
			this.linkLabel1.TabIndex = 3;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Settings...";
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkClicked);
			//
			// SettingsLauncherButton
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.linkLabel1);
			this.Controls.Add(this._image);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "SettingsLauncherButton";
			this.Size = new System.Drawing.Size(131, 22);
			((System.ComponentModel.ISupportInitialize)(this._image)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.PictureBox _image;
		private System.Windows.Forms.Timer _checkVisibilityTmer;
		private System.Windows.Forms.LinkLabel linkLabel1;

	}
}
