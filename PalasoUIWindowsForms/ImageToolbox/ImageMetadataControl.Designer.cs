namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class ImageMetadataControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImageMetadataControl));
			this.label1 = new System.Windows.Forms.Label();
			this._illustrator = new System.Windows.Forms.TextBox();
			this._copyright = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this._licenseImage = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this._licenseImage)).BeginInit();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(0, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Illustrator";
			//
			// _illustrator
			//
			this._illustrator.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._illustrator.Location = new System.Drawing.Point(62, 10);
			this._illustrator.Name = "_illustrator";
			this._illustrator.Size = new System.Drawing.Size(163, 23);
			this._illustrator.TabIndex = 1;
			this._illustrator.Text = "Artist Annie";
			this._illustrator.TextChanged += new System.EventHandler(this._illustrator_TextChanged);
			//
			// _copyright
			//
			this._copyright.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._copyright.Location = new System.Drawing.Point(62, 37);
			this._copyright.Name = "_copyright";
			this._copyright.Size = new System.Drawing.Size(163, 23);
			this._copyright.TabIndex = 3;
			this._copyright.Text = "2011 Sago Land";
			this._copyright.TextChanged += new System.EventHandler(this._copyright_TextChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(0, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(60, 15);
			this.label2.TabIndex = 2;
			this.label2.Text = "Copyright";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label3.Location = new System.Drawing.Point(1, 65);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(46, 15);
			this.label3.TabIndex = 4;
			this.label3.Text = "License";
			//
			// _licenseImage
			//
			this._licenseImage.Image = ((System.Drawing.Image)(resources.GetObject("_licenseImage.Image")));
			this._licenseImage.Location = new System.Drawing.Point(62, 65);
			this._licenseImage.Name = "_licenseImage";
			this._licenseImage.Size = new System.Drawing.Size(163, 35);
			this._licenseImage.TabIndex = 5;
			this._licenseImage.TabStop = false;
			//
			// ImageMetadataControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._licenseImage);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._copyright);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._illustrator);
			this.Controls.Add(this.label1);
			this.Name = "ImageMetadataControl";
			this.Size = new System.Drawing.Size(269, 228);
			this.Load += new System.EventHandler(this.ImageMetadataControl_Load);
			this.Validating += new System.ComponentModel.CancelEventHandler(this.ImageMetadataControl_Validating);
			((System.ComponentModel.ISupportInitialize)(this._licenseImage)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _illustrator;
		private System.Windows.Forms.TextBox _copyright;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.PictureBox _licenseImage;


	}
}
