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
			this._licenseDescription = new System.Windows.Forms.TextBox();
			this._lockedCheckbox = new System.Windows.Forms.CheckBox();
			((System.ComponentModel.ISupportInitialize)(this._licenseImage)).BeginInit();
			this.SuspendLayout();
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(0, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(49, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Illustrator";
			//
			// _illustrator
			//
			this._illustrator.Location = new System.Drawing.Point(55, 8);
			this._illustrator.Name = "_illustrator";
			this._illustrator.Size = new System.Drawing.Size(146, 20);
			this._illustrator.TabIndex = 1;
			this._illustrator.Text = "Artist Annie";
			this._illustrator.TextChanged += new System.EventHandler(this._illustrator_TextChanged);
			//
			// _copyright
			//
			this._copyright.Location = new System.Drawing.Point(55, 35);
			this._copyright.Name = "_copyright";
			this._copyright.Size = new System.Drawing.Size(146, 20);
			this._copyright.TabIndex = 3;
			this._copyright.Text = "2011 Sago Land";
			this._copyright.TextChanged += new System.EventHandler(this._copyright_TextChanged);
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(0, 38);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(51, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Copyright";
			//
			// label3
			//
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(1, 65);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(44, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "License";
			//
			// _licenseImage
			//
			this._licenseImage.Image = ((System.Drawing.Image)(resources.GetObject("_licenseImage.Image")));
			this._licenseImage.Location = new System.Drawing.Point(55, 65);
			this._licenseImage.Name = "_licenseImage";
			this._licenseImage.Size = new System.Drawing.Size(163, 35);
			this._licenseImage.TabIndex = 5;
			this._licenseImage.TabStop = false;
			//
			// _licenseDescription
			//
			this._licenseDescription.Enabled = false;
			this._licenseDescription.Location = new System.Drawing.Point(4, 106);
			this._licenseDescription.Multiline = true;
			this._licenseDescription.Name = "_licenseDescription";
			this._licenseDescription.Size = new System.Drawing.Size(214, 87);
			this._licenseDescription.TabIndex = 6;
			this._licenseDescription.TextChanged += new System.EventHandler(this._licenseDescription_TextChanged);
			//
			// _lockedCheckbox
			//
			this._lockedCheckbox.AutoSize = true;
			this._lockedCheckbox.Location = new System.Drawing.Point(4, 199);
			this._lockedCheckbox.Name = "_lockedCheckbox";
			this._lockedCheckbox.Size = new System.Drawing.Size(62, 17);
			this._lockedCheckbox.TabIndex = 7;
			this._lockedCheckbox.Text = "Locked";
			this._lockedCheckbox.UseVisualStyleBackColor = true;
			this._lockedCheckbox.CheckedChanged += new System.EventHandler(this._lockedCheckbox_CheckedChanged);
			//
			// ImageMetadataControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._lockedCheckbox);
			this.Controls.Add(this._licenseDescription);
			this.Controls.Add(this._licenseImage);
			this.Controls.Add(this.label3);
			this.Controls.Add(this._copyright);
			this.Controls.Add(this.label2);
			this.Controls.Add(this._illustrator);
			this.Controls.Add(this.label1);
			this.Name = "ImageMetadataControl";
			this.Size = new System.Drawing.Size(269, 228);
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
		private System.Windows.Forms.TextBox _licenseDescription;
		private System.Windows.Forms.CheckBox _lockedCheckbox;


	}
}
