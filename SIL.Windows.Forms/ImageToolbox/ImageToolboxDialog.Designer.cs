using SIL.Core.ClearShare;
using SIL.Windows.Forms.ClearShare;

namespace SIL.Windows.Forms.ImageToolbox
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
			PalasoImage palasoImage1 = new PalasoImage();
			Metadata metadata1 = new Metadata();
			this._cancelButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._imageToolboxControl = new ImageToolboxControl();
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			//
			// _cancelButton
			//
			this._cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this._cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this._L10NSharpExtender.SetLocalizableToolTip(this._cancelButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._cancelButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._cancelButton, "Common.CancelButton");
			this._cancelButton.Location = new System.Drawing.Point(799, 457);
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
			this._L10NSharpExtender.SetLocalizableToolTip(this._okButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._okButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._okButton, "Common.OKButton");
			this._okButton.Location = new System.Drawing.Point(718, 457);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 2;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// imageToolboxControl1
			//
			this._imageToolboxControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._imageToolboxControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._imageToolboxControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			palasoImage1.Image = null;
			metadata1.AttributionUrl = null;
			metadata1.CollectionName = null;
			metadata1.CollectionUri = null;
			metadata1.CopyrightNotice = "";
			metadata1.Creator = null;
			metadata1.HasChanges = true;
			metadata1.License = null;
			palasoImage1.Metadata = metadata1;
			palasoImage1.MetadataLocked = false;
			this._imageToolboxControl.ImageInfo = palasoImage1;
			this._imageToolboxControl.InitialSearchString = null;
			this._L10NSharpExtender.SetLocalizableToolTip(this._imageToolboxControl, null);
			this._L10NSharpExtender.SetLocalizationComment(this._imageToolboxControl, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._imageToolboxControl, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._imageToolboxControl, "ImageToolbox.ImageToolboxDialog.ImageToolboxControl");
			this._imageToolboxControl.Location = new System.Drawing.Point(1, 1);
			this._imageToolboxControl.Name = "_imageToolboxControl";
			this._imageToolboxControl.Size = new System.Drawing.Size(873, 450);
			this._imageToolboxControl.TabIndex = 3;
			//
			// _L10NSharpExtender
			//
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "ImageToolbox";
			//
			// ImageToolboxDialog
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.CancelButton = this._cancelButton;
			this.ClientSize = new System.Drawing.Size(886, 485);
			this.Controls.Add(this._imageToolboxControl);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._cancelButton);
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "ImageToolbox.ImageToolboxWindowTitle");
			this.MinimumSize = new System.Drawing.Size(732, 432);
			this.Name = "ImageToolboxDialog";
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Image Toolbox";
			this.Load += new System.EventHandler(this.ImageToolboxDialog_Load);
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _cancelButton;
		private System.Windows.Forms.Button _okButton;
		private ImageToolboxControl _imageToolboxControl;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
	}
}