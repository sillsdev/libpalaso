using System;

namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class GetImageFromFileSystemControl
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
			this.button1 = new System.Windows.Forms.Button();
			this._pictureBox = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this._pictureBox)).BeginInit();
			this.SuspendLayout();
			//
			// button1
			//
			this.button1.Location = new System.Drawing.Point(84, 43);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(207, 23);
			this.button1.TabIndex = 4;
			this.button1.Text = "Get image from disk or USB drive...";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			//
			// _pictureBox
			//
			this._pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._pictureBox.Location = new System.Drawing.Point(84, 125);
			this._pictureBox.Name = "_pictureBox";
			this._pictureBox.Size = new System.Drawing.Size(383, 278);
			this._pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._pictureBox.TabIndex = 5;
			this._pictureBox.TabStop = false;
			//
			// GetImageFromFileSystemControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._pictureBox);
			this.Controls.Add(this.button1);
			this.Name = "GetImageFromFileSystemControl";
			this.Size = new System.Drawing.Size(550, 445);
			((System.ComponentModel.ISupportInitialize)(this._pictureBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PictureBox _pictureBox;
		private System.Windows.Forms.Button button1;

	}
}
