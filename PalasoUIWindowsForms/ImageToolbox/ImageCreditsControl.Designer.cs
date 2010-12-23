namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class ImageCreditsControl
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
			this.betterLabel1 = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.SuspendLayout();
			//
			// betterLabel1
			//
			this.betterLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.betterLabel1.Location = new System.Drawing.Point(127, 80);
			this.betterLabel1.Multiline = true;
			this.betterLabel1.Name = "betterLabel1";
			this.betterLabel1.ReadOnly = true;
			this.betterLabel1.Size = new System.Drawing.Size(290, 38);
			this.betterLabel1.TabIndex = 1;
			this.betterLabel1.TabStop = false;
			this.betterLabel1.Text = "This tool is has not been built yet.";
			//
			// ImageCreditsControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.betterLabel1);
			this.Name = "ImageCreditsControl";
			this.Size = new System.Drawing.Size(551, 358);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Widgets.BetterLabel betterLabel1;
	}
}
