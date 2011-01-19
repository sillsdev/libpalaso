namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class ImageToolboxDialog
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
			Palaso.UI.WindowsForms.ImageToolbox.PalasoImage palasoImage1 = new Palaso.UI.WindowsForms.ImageToolbox.PalasoImage();
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this.imageToolboxControl1 = new Palaso.UI.WindowsForms.ImageToolbox.ImageToolboxControl();
			this.localizationHelper1 = new Palaso.UI.WindowsForms.i18n.LocalizationHelper(this.components);
			((System.ComponentModel.ISupportInitialize)(this.localizationHelper1)).BeginInit();
			this.SuspendLayout();
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._cancelButton.Location = new System.Drawing.Point(667, 459);
			this._cancelButton.Name = "_cancelButton";
			this._cancelButton.Size = new System.Drawing.Size(75, 23);
			this._cancelButton.TabIndex = 1;
			this._cancelButton.Text = "&Cancel";
			this._cancelButton.UseVisualStyleBackColor = true;
			this._cancelButton.Click += new System.EventHandler(this._cancelButton_Click);
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(586, 459);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// imageToolboxControl1
			//
			this.imageToolboxControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.imageToolboxControl1.ImageInfo = palasoImage1;
			this.imageToolboxControl1.Location = new System.Drawing.Point(1, 1);
			this.imageToolboxControl1.Name = "imageToolboxControl1";
			this.imageToolboxControl1.Size = new System.Drawing.Size(696, 452);
			this.imageToolboxControl1.TabIndex = 3;
			//
			// localizationHelper1
			//
			this.localizationHelper1.Parent = this;
			//
			// ImageToolboxDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(754, 487);
			this.Controls.Add(this.imageToolboxControl1);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._cancelButton);
			this.Name = "ImageToolboxDialog";
			this.Text = "ImageToolboxDialog";
			((System.ComponentModel.ISupportInitialize)(this.localizationHelper1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private ImageToolboxControl imageToolboxControl1;
		private i18n.LocalizationHelper localizationHelper1;
	}
}