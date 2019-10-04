namespace SIL.Windows.Forms.ImageToolbox.ImageGallery
{
	partial class ImageGalleryControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private bool disposed = false;
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposed)
				return;
			disposed = true;
			if (disposing)
			{
				if (components != null)
					components.Dispose();
				if (_thumbnailViewer != null)
				{
					_thumbnailViewer.Dispose();
					_thumbnailViewer = null;
				}
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
			this._searchButton = new System.Windows.Forms.Button();
			this._searchResultStats = new System.Windows.Forms.Label();
			this._labelSearch = new System.Windows.Forms.Label();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this._searchLanguageMenu = new System.Windows.Forms.ToolStripDropDownButton();
			this._downloadInstallerLink = new SIL.Windows.Forms.Widgets.BetterLinkLabel();
			this._messageLabel = new SIL.Windows.Forms.Widgets.BetterLabel();
			this._searchTermsBox = new SIL.Windows.Forms.Widgets.TextInputBox();
			this._thumbnailViewer = new ThumbnailViewer();
			this._localizationHelper = new SIL.Windows.Forms.i18n.LocalizationHelper(this.components);
			this._collectionToolStrip = new System.Windows.Forms.ToolStrip();
			this._collectionDropDown = new System.Windows.Forms.ToolStripDropDownButton();
			this.toolStrip1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).BeginInit();
			this._collectionToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// _searchButton
			// 
			this._searchButton.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._searchButton.Image = global::SIL.Windows.Forms.Properties.Resources.search18x18;
			this._searchButton.Location = new System.Drawing.Point(175, 35);
			this._searchButton.Name = "_searchButton";
			this._searchButton.Size = new System.Drawing.Size(48, 28);
			this._searchButton.TabIndex = 1;
			this._searchButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._searchButton.UseVisualStyleBackColor = true;
			this._searchButton.Click += new System.EventHandler(this._searchButton_Click);
			// 
			// _searchResultStats
			// 
			this._searchResultStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this._searchResultStats.Font = new System.Drawing.Font("Segoe UI", 10F);
			this._searchResultStats.Location = new System.Drawing.Point(9, 70);
			this._searchResultStats.Name = "_searchResultStats";
			this._searchResultStats.Size = new System.Drawing.Size(375, 22);
			this._searchResultStats.TabIndex = 12;
			this._searchResultStats.Text = "~Search Result Stats";
			// 
			// _labelSearch
			// 
			this._labelSearch.AutoSize = true;
			this._labelSearch.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._labelSearch.Location = new System.Drawing.Point(8, 8);
			this._labelSearch.Name = "_labelSearch";
			this._labelSearch.Size = new System.Drawing.Size(120, 20);
			this._labelSearch.TabIndex = 14;
			this._labelSearch.Text = "Image Galleries";
			// 
			// toolStrip1
			// 
			this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.toolStrip1.AutoSize = false;
			this.toolStrip1.BackColor = System.Drawing.Color.Transparent;
			this.toolStrip1.CanOverflow = false;
			this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._searchLanguageMenu});
			this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this.toolStrip1.Location = new System.Drawing.Point(228, 35);
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.Size = new System.Drawing.Size(200, 28);
			this.toolStrip1.TabIndex = 15;
			this.toolStrip1.Text = "toolStrip1";
			// 
			// _searchLanguageMenu
			// 
			this._searchLanguageMenu.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this._searchLanguageMenu.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._searchLanguageMenu.Name = "_searchLanguageMenu";
			this._searchLanguageMenu.Size = new System.Drawing.Size(107, 19);
			this._searchLanguageMenu.Text = "Language Name";
			this._searchLanguageMenu.Visible = false;
			// 
			// _downloadInstallerLink
			// 
			this._downloadInstallerLink.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._downloadInstallerLink.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._downloadInstallerLink.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
			this._downloadInstallerLink.ForeColor = System.Drawing.Color.Blue;
			this._downloadInstallerLink.IsTextSelectable = true;
			this._downloadInstallerLink.Location = new System.Drawing.Point(24, 269);
			this._downloadInstallerLink.Multiline = true;
			this._downloadInstallerLink.Name = "_downloadInstallerLink";
			this._downloadInstallerLink.ReadOnly = true;
			this._downloadInstallerLink.Size = new System.Drawing.Size(470, 17);
			this._downloadInstallerLink.TabIndex = 11;
			this._downloadInstallerLink.TabStop = false;
			this._downloadInstallerLink.Text = "Download Art Of Reading Installer";
			this._downloadInstallerLink.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this._downloadInstallerLink.URL = "http://bloomlibrary.org/#/artofreading";
			this._downloadInstallerLink.Visible = false;
			// 
			// _messageLabel
			// 
			this._messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._messageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._messageLabel.Enabled = false;
			this._messageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._messageLabel.ForeColor = System.Drawing.Color.Gray;
			this._messageLabel.IsTextSelectable = false;
			this._messageLabel.Location = new System.Drawing.Point(24, 124);
			this._messageLabel.Multiline = true;
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.ReadOnly = true;
			this._messageLabel.Size = new System.Drawing.Size(470, 17);
			this._messageLabel.TabIndex = 10;
			this._messageLabel.TabStop = false;
			this._messageLabel.Text = "~No matching images";
			this._messageLabel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			// 
			// _searchTermsBox
			// 
			this._searchTermsBox.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._searchTermsBox.Location = new System.Drawing.Point(12, 35);
			this._searchTermsBox.Name = "_searchTermsBox";
			this._searchTermsBox.Size = new System.Drawing.Size(157, 28);
			this._searchTermsBox.TabIndex = 0;
			this._searchTermsBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this._searchTermsBox_KeyDown);
			// 
			// _thumbnailViewer
			// 
			this._thumbnailViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._thumbnailViewer.CaptionMethod = null;
			this._thumbnailViewer.Location = new System.Drawing.Point(12, 95);
			this._thumbnailViewer.Name = "_thumbnailViewer";
			this._thumbnailViewer.Size = new System.Drawing.Size(494, 228);
			this._thumbnailViewer.TabIndex = 2;
			this._thumbnailViewer.ThumbBorderColor = System.Drawing.Color.Wheat;
			this._thumbnailViewer.ThumbNailSize = 95;
			this._thumbnailViewer.DoubleClick += new System.EventHandler(this._thumbnailViewer_DoubleClick);
			// 
			// _localizationHelper
			// 
			this._localizationHelper.Parent = this;
			// 
			// _collectionToolStrip
			// 
			this._collectionToolStrip.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._collectionToolStrip.AutoSize = false;
			this._collectionToolStrip.BackColor = System.Drawing.Color.Transparent;
			this._collectionToolStrip.CanOverflow = false;
			this._collectionToolStrip.Dock = System.Windows.Forms.DockStyle.None;
			this._collectionToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this._collectionToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._collectionDropDown});
			this._collectionToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
			this._collectionToolStrip.Location = new System.Drawing.Point(373, 35);
			this._collectionToolStrip.Name = "_collectionToolStrip";
			this._collectionToolStrip.Size = new System.Drawing.Size(133, 28);
			this._collectionToolStrip.TabIndex = 18;
			this._collectionToolStrip.Text = "_collectionToolStrip";
			// 
			// _collectionDropDown
			// 
			this._collectionDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
			this._collectionDropDown.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._collectionDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
			this._collectionDropDown.Name = "_collectionDropDown";
			this._collectionDropDown.Size = new System.Drawing.Size(64, 19);
			this._collectionDropDown.Text = "Galleries";
			this._collectionDropDown.Visible = false;
			// 
			// ArtOfReadingChooser
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._labelSearch);
			this.Controls.Add(this._collectionToolStrip);
			this.Controls.Add(this.toolStrip1);
			this.Controls.Add(this._searchResultStats);
			this.Controls.Add(this._downloadInstallerLink);
			this.Controls.Add(this._messageLabel);
			this.Controls.Add(this._searchButton);
			this.Controls.Add(this._searchTermsBox);
			this.Controls.Add(this._thumbnailViewer);
			this.Name = "ImageGalleryControl";
			this.Size = new System.Drawing.Size(530, 325);
			this.Load += new System.EventHandler(this.ArtOfReadingChooser_Load);
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).EndInit();
			this._collectionToolStrip.ResumeLayout(false);
			this._collectionToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ImageGallery.ThumbnailViewer _thumbnailViewer;
		private System.Windows.Forms.Button _searchButton;
		private SIL.Windows.Forms.Widgets.TextInputBox _searchTermsBox;
		private Widgets.BetterLabel _messageLabel;
		private i18n.LocalizationHelper _localizationHelper;
		private Widgets.BetterLinkLabel _downloadInstallerLink;
		private System.Windows.Forms.Label _searchResultStats;
		private System.Windows.Forms.Label _labelSearch;
		private System.Windows.Forms.ToolStrip toolStrip1;
		private System.Windows.Forms.ToolStripDropDownButton _searchLanguageMenu;
		private System.Windows.Forms.ToolStrip _collectionToolStrip;
		private System.Windows.Forms.ToolStripDropDownButton _collectionDropDown;
	}
}
