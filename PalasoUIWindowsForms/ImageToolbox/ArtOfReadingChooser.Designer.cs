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
			this._thumbnailViewer = new Palaso.UI.WindowsForms.ImageGallery.ThumbnailViewer();
			this._searchButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this._searchTermsBox = new System.Windows.Forms.TextBox();
			this._messageLabel = new Palaso.UI.WindowsForms.Widgets.BetterLabel();
			this.SuspendLayout();
			//
			// _thumbnailViewer
			//
			this._thumbnailViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._thumbnailViewer.CanLoad = false;
			this._thumbnailViewer.CaptionMethod = null;
			this._thumbnailViewer.Location = new System.Drawing.Point(12, 39);
			this._thumbnailViewer.MultiSelect = false;
			this._thumbnailViewer.Name = "_thumbnailViewer";
			this._thumbnailViewer.Size = new System.Drawing.Size(372, 273);
			this._thumbnailViewer.TabIndex = 0;
			this._thumbnailViewer.ThumbBorderColor = System.Drawing.Color.Wheat;
			this._thumbnailViewer.ThumbNailSize = 95;
			this._thumbnailViewer.UseCompatibleStateImageBehavior = false;
			//
			// _searchButton
			//
			this._searchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this._searchButton.Location = new System.Drawing.Point(278, 3);
			this._searchButton.Name = "_searchButton";
			this._searchButton.Size = new System.Drawing.Size(106, 26);
			this._searchButton.TabIndex = 8;
			this._searchButton.Text = "~&Search";
			this._searchButton.UseVisualStyleBackColor = true;
			this._searchButton.Click += new System.EventHandler(this._searchButton_Click);
			//
			// label1
			//
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.label1.Location = new System.Drawing.Point(9, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 28);
			this.label1.TabIndex = 7;
			this.label1.Text = "~Search Words";
			//
			// _searchTermsBox
			//
			this._searchTermsBox.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._searchTermsBox.Location = new System.Drawing.Point(106, 5);
			this._searchTermsBox.Name = "_searchTermsBox";
			this._searchTermsBox.Size = new System.Drawing.Size(157, 24);
			this._searchTermsBox.TabIndex = 6;
			this._searchTermsBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this._searchTermsBox_KeyDown);
			//
			// _messageLabel
			//
			this._messageLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._messageLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._messageLabel.Font = new System.Drawing.Font("Segoe UI", 15F);
			this._messageLabel.ForeColor = System.Drawing.Color.Gray;
			this._messageLabel.Location = new System.Drawing.Point(34, 99);
			this._messageLabel.Multiline = true;
			this._messageLabel.Name = "_messageLabel";
			this._messageLabel.ReadOnly = true;
			this._messageLabel.Size = new System.Drawing.Size(333, 150);
			this._messageLabel.TabIndex = 10;
			this._messageLabel.TabStop = false;
			this._messageLabel.Text = "~No matching images";
			//
			// ArtOfReadingChooser
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._messageLabel);
			this.Controls.Add(this._searchButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._searchTermsBox);
			this.Controls.Add(this._thumbnailViewer);
			this.Name = "ArtOfReadingChooser";
			this.Size = new System.Drawing.Size(408, 325);
			this.Load += new System.EventHandler(this.ArtOfReadingChooser_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ImageGallery.ThumbnailViewer _thumbnailViewer;
		private System.Windows.Forms.Button _searchButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _searchTermsBox;
		private Widgets.BetterLabel _messageLabel;
	}
}
