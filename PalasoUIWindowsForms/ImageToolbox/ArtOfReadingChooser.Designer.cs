namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class ArtOfReadingChooser
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
			this._searchButton = new System.Windows.Forms.Button();
			this._searchTermsBox = new System.Windows.Forms.TextBox();
			this.betterLinkLabel1 = new Palaso.UI.WindowsForms.Widgets.BetterLinkLabel();
			this._messageLabel = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this._thumbnailViewer = new Palaso.UI.WindowsForms.ImageGallery.ThumbnailViewer();
			this._localizationHelper = new Palaso.UI.WindowsForms.i18n.LocalizationHelper(this.components);
			this._searchResultStats = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).BeginInit();
			this.SuspendLayout();
			//
			// _searchButton
			//
			this._searchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this._searchButton.Image = global::Palaso.UI.WindowsForms.Properties.Resources.search18x18;
			this._searchButton.Location = new System.Drawing.Point(175, 25);
			this._searchButton.Name = "_searchButton";
			this._searchButton.Size = new System.Drawing.Size(48, 28);
			this._searchButton.TabIndex = 1;
			this._searchButton.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
			this._searchButton.UseVisualStyleBackColor = true;
			this._searchButton.Click += new System.EventHandler(this._searchButton_Click);
			//
			// _searchTermsBox
			//
			this._searchTermsBox.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._searchTermsBox.Location = new System.Drawing.Point(12, 25);
			this._searchTermsBox.Name = "_searchTermsBox";
			this._searchTermsBox.Size = new System.Drawing.Size(157, 24);
			this._searchTermsBox.TabIndex = 0;
			this._searchTermsBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this._searchTermsBox_KeyDown);
			//
			// betterLinkLabel1
			//
			this.betterLinkLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.betterLinkLabel1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.betterLinkLabel1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Underline);
			this.betterLinkLabel1.ForeColor = System.Drawing.Color.Blue;
			this.betterLinkLabel1.Location = new System.Drawing.Point(24, 244);
			this.betterLinkLabel1.Multiline = true;
			this.betterLinkLabel1.Name = "betterLinkLabel1";
			this.betterLinkLabel1.ReadOnly = true;
			this.betterLinkLabel1.Size = new System.Drawing.Size(348, 17);
			this.betterLinkLabel1.TabIndex = 11;
			this.betterLinkLabel1.TabStop = false;
			this.betterLinkLabel1.Text = "Download Art Of Reading Installer";
			this.betterLinkLabel1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.betterLinkLabel1.URL = "http://bloom.palaso.org/art-of-reading/";
			this.betterLinkLabel1.Visible = false;
			//
			// _messageLabel
			//
			this._messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._messageLabel.BackColor = System.Drawing.Color.White;
			this._messageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._messageLabel.Enabled = false;
			this._messageLabel.Font = new System.Drawing.Font("Segoe UI", 9F);
			this._messageLabel.ForeColor = System.Drawing.Color.Gray;
			this._messageLabel.Location = new System.Drawing.Point(24, 99);
			this._messageLabel.Multiline = true;
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.ReadOnly = true;
			this._messageLabel.Size = new System.Drawing.Size(348, 17);
			this._messageLabel.TabIndex = 10;
			this._messageLabel.TabStop = false;
			this._messageLabel.Text = "~No matching images";
			this._messageLabel.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			//
			// _thumbnailViewer
			//
			this._thumbnailViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._thumbnailViewer.CanLoad = false;
			this._thumbnailViewer.CaptionMethod = null;
			this._thumbnailViewer.Location = new System.Drawing.Point(12, 55);
			this._thumbnailViewer.MultiSelect = false;
			this._thumbnailViewer.Name = "_thumbnailViewer";
			this._thumbnailViewer.Size = new System.Drawing.Size(372, 245);
			this._thumbnailViewer.TabIndex = 2;
			this._thumbnailViewer.ThumbBorderColor = System.Drawing.Color.Wheat;
			this._thumbnailViewer.ThumbNailSize = 95;
			this._thumbnailViewer.UseCompatibleStateImageBehavior = false;
			this._thumbnailViewer.DoubleClick += new System.EventHandler(this._thumbnailViewer_DoubleClick);
			//
			// _localizationHelper
			//
			this._localizationHelper.Parent = this;
			//
			// _searchResultStats
			//
			this._searchResultStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._searchResultStats.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this._searchResultStats.Location = new System.Drawing.Point(9, 303);
			this._searchResultStats.Name = "_searchResultStats";
			this._searchResultStats.Size = new System.Drawing.Size(375, 22);
			this._searchResultStats.TabIndex = 12;
			this._searchResultStats.Text = "~Search Result Stats";
			//
			// ArtOfReadingChooser
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._searchResultStats);
			this.Controls.Add(this.betterLinkLabel1);
			this.Controls.Add(this._messageLabel);
			this.Controls.Add(this._searchButton);
			this.Controls.Add(this._searchTermsBox);
			this.Controls.Add(this._thumbnailViewer);
			this.Name = "ArtOfReadingChooser";
			this.Size = new System.Drawing.Size(408, 325);
			this.Load += new System.EventHandler(this.ArtOfReadingChooser_Load);
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ImageGallery.ThumbnailViewer _thumbnailViewer;
		private System.Windows.Forms.Button _searchButton;
		private System.Windows.Forms.TextBox _searchTermsBox;
		private Widgets.BetterLabel _messageLabel;
		private i18n.LocalizationHelper _localizationHelper;
		private Widgets.BetterLinkLabel betterLinkLabel1;
		private System.Windows.Forms.Label _searchResultStats;
	}
}
