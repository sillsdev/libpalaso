using SIL.Windows.Forms.ImageToolbox.ImageGallery;
using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.ImageToolbox
{
	partial class AcquireImageControl
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
			_galleryControl?.Dispose();	// Can be null if error when initializing.
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AcquireImageControl));
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this._galleryButton = new System.Windows.Forms.ToolStripButton();
			this._scannerButton = new System.Windows.Forms.ToolStripButton();
			this._cameraButton = new System.Windows.Forms.ToolStripButton();
			this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
			this._pictureBox = new System.Windows.Forms.PictureBox();
			this._galleryControl = new ImageGalleryControl();
			this._messageLabel = new BetterLabel();
			this._focusTimer = new System.Windows.Forms.Timer(this.components);
			this._L10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this.toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._pictureBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).BeginInit();
			this.SuspendLayout();
			//
			// toolStrip1
			//
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this._galleryButton,
			this._scannerButton,
			this._cameraButton,
			this.toolStripButton1});
			this._L10NSharpExtender.SetLocalizableToolTip(this.toolStrip1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.toolStrip1, null);
			this._L10NSharpExtender.SetLocalizationPriority(this.toolStrip1, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this.toolStrip1, "ImageToolbox.AcquireImageControl.toolStrip1");
			this.toolStrip1.Location = new System.Drawing.Point(0, 0);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip1.Size = new System.Drawing.Size(556, 54);
			this.toolStrip1.TabIndex = 0;
			this.toolStrip1.Text = "toolStrip1";
			//
			// _galleryButton
			//
			this._galleryButton.Image = ((System.Drawing.Image)(resources.GetObject("_galleryButton.Image")));
			this._galleryButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._L10NSharpExtender.SetLocalizableToolTip(this._galleryButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._galleryButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._galleryButton, "ImageToolbox.ImageGalleries");
			this._galleryButton.Margin = new System.Windows.Forms.Padding(20, 1, 0, 2);
			this._galleryButton.Name = "_galleryButton";
			this._galleryButton.Size = new System.Drawing.Size(89, 51);
			this._galleryButton.Text = "Image Galleries";
			this._galleryButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this._galleryButton.Click += new System.EventHandler(this.OnGalleryClick);
			//
			// _scannerButton
			//
			this._scannerButton.Image = ((System.Drawing.Image)(resources.GetObject("_scannerButton.Image")));
			this._scannerButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._L10NSharpExtender.SetLocalizableToolTip(this._scannerButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._scannerButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._scannerButton, "ImageToolbox.Scanner");
			this._scannerButton.Margin = new System.Windows.Forms.Padding(20, 1, 0, 2);
			this._scannerButton.Name = "_scannerButton";
			this._scannerButton.Size = new System.Drawing.Size(53, 51);
			this._scannerButton.Text = "Scanner";
			this._scannerButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this._scannerButton.Click += new System.EventHandler(this.OnScannerClick);
			//
			// _cameraButton
			//
			this._cameraButton.Image = ((System.Drawing.Image)(resources.GetObject("_cameraButton.Image")));
			this._cameraButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._L10NSharpExtender.SetLocalizableToolTip(this._cameraButton, null);
			this._L10NSharpExtender.SetLocalizationComment(this._cameraButton, null);
			this._L10NSharpExtender.SetLocalizingId(this._cameraButton, "ImageToolbox.Camera");
			this._cameraButton.Margin = new System.Windows.Forms.Padding(20, 1, 0, 2);
			this._cameraButton.Name = "_cameraButton";
			this._cameraButton.Size = new System.Drawing.Size(52, 51);
			this._cameraButton.Text = "Camera";
			this._cameraButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this._cameraButton.Click += new System.EventHandler(this.OnCameraClick);
			//
			// toolStripButton1
			//
			this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
			this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._L10NSharpExtender.SetLocalizableToolTip(this.toolStripButton1, null);
			this._L10NSharpExtender.SetLocalizationComment(this.toolStripButton1, null);
			this._L10NSharpExtender.SetLocalizingId(this.toolStripButton1, "ImageToolbox.FileButton");
			this.toolStripButton1.Margin = new System.Windows.Forms.Padding(20, 1, 0, 2);
			this.toolStripButton1.Name = "toolStripButton1";
			this.toolStripButton1.Size = new System.Drawing.Size(36, 51);
			this.toolStripButton1.Text = "File";
			this.toolStripButton1.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
			this.toolStripButton1.Click += new System.EventHandler(this.OnGetFromFileSystemClick);
			//
			// _pictureBox
			//
			this._pictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._pictureBox, null);
			this._L10NSharpExtender.SetLocalizationComment(this._pictureBox, null);
			this._L10NSharpExtender.SetLocalizingId(this._pictureBox, "ImageToolbox.AcquireImageControl._pictureBox");
			this._pictureBox.Location = new System.Drawing.Point(0, 57);
			this._pictureBox.Name = "_pictureBox";
			this._pictureBox.Size = new System.Drawing.Size(556, 349);
			this._pictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._pictureBox.TabIndex = 6;
			this._pictureBox.TabStop = false;
			//
			// _galleryControl
			//
			this._galleryControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._L10NSharpExtender.SetLocalizableToolTip(this._galleryControl, null);
			this._L10NSharpExtender.SetLocalizationComment(this._galleryControl, null);
			this._L10NSharpExtender.SetLocalizingId(this._galleryControl, "ImageToolbox.AcquireImageControl.ArtOfReadingChooser");
			this._galleryControl.Location = new System.Drawing.Point(3, 57);
			this._galleryControl.Name = "_galleryControl";
			this._galleryControl.Size = new System.Drawing.Size(551, 349);
			this._galleryControl.TabIndex = 7;
			//
			// _messageLabel
			//
			this._messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._messageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._messageLabel.Enabled = false;
			this._messageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._messageLabel.ForeColor = System.Drawing.Color.Gray;
			this._L10NSharpExtender.SetLocalizableToolTip(this._messageLabel, null);
			this._L10NSharpExtender.SetLocalizationComment(this._messageLabel, null);
			this._L10NSharpExtender.SetLocalizationPriority(this._messageLabel, L10NSharp.LocalizationPriority.NotLocalizable);
			this._L10NSharpExtender.SetLocalizingId(this._messageLabel, "ImageToolbox.AcquireImageControl._messageLabel");
			this._messageLabel.Location = new System.Drawing.Point(97, 171);
			this._messageLabel.Multiline = true;
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.ReadOnly = true;
			this._messageLabel.Size = new System.Drawing.Size(333, 17);
			this._messageLabel.TabIndex = 11;
			this._messageLabel.TabStop = false;
			this._messageLabel.Text = "This will notify you of problems";
			this._messageLabel.Visible = false;
			//
			// _focusTimer
			//
			this._focusTimer.Tick += new System.EventHandler(this._focusTimer_Tick);
			//
			// _L10NSharpExtender
			//
			this._L10NSharpExtender.LocalizationManagerId = "Palaso";
			this._L10NSharpExtender.PrefixForNewItems = "ImageToolbox";
			//
			// AcquireImageControl
			//
			this.AllowDrop = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._messageLabel);
			this.Controls.Add(this._galleryControl);
			this.Controls.Add(this._pictureBox);
			this.Controls.Add(this.toolStrip1);
			this._L10NSharpExtender.SetLocalizableToolTip(this, null);
			this._L10NSharpExtender.SetLocalizationComment(this, null);
			this._L10NSharpExtender.SetLocalizingId(this, "ImageToolbox.AcquireImageControl.AcquireImageControl");
			this.Name = "AcquireImageControl";
			this.Size = new System.Drawing.Size(556, 409);
			this.Load += new System.EventHandler(this.AcquireImageControl_Load);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.OnDragDrop);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.AcquireImageControl_DragEnter);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._pictureBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this._L10NSharpExtender)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripButton toolStripButton1;
		private System.Windows.Forms.ToolStripButton _cameraButton;
		private System.Windows.Forms.ToolStripButton _scannerButton;
		private System.Windows.Forms.ToolStripButton _galleryButton;
		private System.Windows.Forms.PictureBox _pictureBox;
		private ImageGalleryControl _galleryControl;
		private Widgets.BetterLabel _messageLabel;
		private System.Windows.Forms.Timer _focusTimer;
		private L10NSharp.UI.L10NSharpExtender _L10NSharpExtender;
	}
}
