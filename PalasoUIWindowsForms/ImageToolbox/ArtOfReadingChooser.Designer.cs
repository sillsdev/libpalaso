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
			this._notFoundLabel = new System.Windows.Forms.Label();
			this._searchButton = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this._searchTermsBox = new System.Windows.Forms.TextBox();
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
			// _notFoundLabel
			//
			this._notFoundLabel.AutoSize = true;
			this._notFoundLabel.BackColor = System.Drawing.SystemColors.Window;
			this._notFoundLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
			this._notFoundLabel.ForeColor = System.Drawing.Color.Gray;
			this._notFoundLabel.Location = new System.Drawing.Point(29, 71);
			this._notFoundLabel.Name = "_notFoundLabel";
			this._notFoundLabel.Size = new System.Drawing.Size(196, 25);
			this._notFoundLabel.TabIndex = 9;
			this._notFoundLabel.Text = "~No matching images";
			this._notFoundLabel.Visible = false;
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
			//
			// ArtOfReadingChooser
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._notFoundLabel);
			this.Controls.Add(this._searchButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._searchTermsBox);
			this.Controls.Add(this._thumbnailViewer);
			this.Name = "ArtOfReadingChooser";
			this.Size = new System.Drawing.Size(408, 325);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ImageGallery.ThumbnailViewer _thumbnailViewer;
		private System.Windows.Forms.Label _notFoundLabel;
		private System.Windows.Forms.Button _searchButton;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _searchTermsBox;
	}
}
