namespace WeSay.LexicalTools.AddPictures
{
	partial class PictureChooser
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
			this._searchTermsBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this._searchButton = new System.Windows.Forms.Button();
			this._okButton = new System.Windows.Forms.Button();
			this._thumbnailViewer = new WeSay.LexicalTools.AddPictures.ThumbnailViewer();
			this.SuspendLayout();
			//
			// _searchTermsBox
			//
			this._searchTermsBox.Location = new System.Drawing.Point(62, 18);
			this._searchTermsBox.Name = "_searchTermsBox";
			this._searchTermsBox.Size = new System.Drawing.Size(249, 20);
			this._searchTermsBox.TabIndex = 1;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(38, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Words";
			//
			// _searchButton
			//
			this._searchButton.Location = new System.Drawing.Point(317, 15);
			this._searchButton.Name = "_searchButton";
			this._searchButton.Size = new System.Drawing.Size(75, 23);
			this._searchButton.TabIndex = 3;
			this._searchButton.Text = "Search";
			this._searchButton.UseVisualStyleBackColor = true;
			this._searchButton.Click += new System.EventHandler(this._searchButton_Click);
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Location = new System.Drawing.Point(552, 15);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(75, 23);
			this._okButton.TabIndex = 4;
			this._okButton.Text = "&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _thumbnailViewer
			//
			this._thumbnailViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._thumbnailViewer.CanLoad = false;
			this._thumbnailViewer.CaptionMethod = null;
			this._thumbnailViewer.Location = new System.Drawing.Point(0, 56);
			this._thumbnailViewer.Name = "_thumbnailViewer";
			this._thumbnailViewer.Size = new System.Drawing.Size(627, 544);
			this._thumbnailViewer.TabIndex = 0;
			this._thumbnailViewer.ThumbBorderColor = System.Drawing.Color.Wheat;
			this._thumbnailViewer.ThumbNailSize = 95;
			this._thumbnailViewer.UseCompatibleStateImageBehavior = false;
			this._thumbnailViewer.DoubleClick += new System.EventHandler(this._thumbnailViewer_DoubleClick);
			//
			// PictureChooser
			//
			this.AcceptButton = this._searchButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(639, 612);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._searchButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._searchTermsBox);
			this.Controls.Add(this._thumbnailViewer);
			this.MaximizeBox = false;
			this.Name = "PictureChooser";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "Search Image Gallery";
			this.Load += new System.EventHandler(this.PictureChooser_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ThumbnailViewer _thumbnailViewer;
		private System.Windows.Forms.TextBox _searchTermsBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button _searchButton;
		private System.Windows.Forms.Button _okButton;

	}
}
