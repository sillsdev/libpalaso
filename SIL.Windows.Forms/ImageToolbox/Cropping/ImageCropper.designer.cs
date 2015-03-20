namespace SIL.Windows.Forms.ImageToolbox.Cropping
{
	partial class ImageCropper
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			//
			// ImageCropper
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.White;
			this.DoubleBuffered = true;
			this.Name = "ImageCropper";
			this.Size = new System.Drawing.Size(362, 293);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ImageCropper_MouseDown);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ImageCropper_MouseUp);
			this.Resize += new System.EventHandler(this.ImageCropper_Resize);
			this.ResumeLayout(false);

		}

		#endregion


	}
}
