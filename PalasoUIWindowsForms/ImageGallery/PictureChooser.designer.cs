using Palaso.UI.WindowsForms.ImageGallery;

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
			this._notFoundLabel = new System.Windows.Forms.Label();
			this._thumbnailViewer = new Palaso.UI.WindowsForms.ImageGallery.ThumbnailViewer();
			this._localizationHelper = new Palaso.UI.WindowsForms.i8n.LocalizationHelper(this.components);
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).BeginInit();
			this.SuspendLayout();
			//
			// _searchTermsBox
			//
			this._searchTermsBox.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._searchTermsBox.Location = new System.Drawing.Point(109, 18);
			this._searchTermsBox.Name = "_searchTermsBox";
			this._searchTermsBox.Size = new System.Drawing.Size(232, 24);
			this._searchTermsBox.TabIndex = 1;
			//
			// label1
			//
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this.label1.Location = new System.Drawing.Point(12, 21);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 28);
			this.label1.TabIndex = 2;
			this.label1.Text = "~Search Words";
			//
			// _searchButton
			//
			this._searchButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this._searchButton.Location = new System.Drawing.Point(358, 15);
			this._searchButton.Name = "_searchButton";
			this._searchButton.Size = new System.Drawing.Size(106, 35);
			this._searchButton.TabIndex = 3;
			this._searchButton.Text = "~&Search";
			this._searchButton.UseVisualStyleBackColor = true;
			this._searchButton.Click += new System.EventHandler(this._searchButton_Click);
			//
			// _okButton
			//
			this._okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this._okButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
			this._okButton.Location = new System.Drawing.Point(523, 15);
			this._okButton.Name = "_okButton";
			this._okButton.Size = new System.Drawing.Size(104, 35);
			this._okButton.TabIndex = 4;
			this._okButton.Text = "~&OK";
			this._okButton.UseVisualStyleBackColor = true;
			this._okButton.Click += new System.EventHandler(this._okButton_Click);
			//
			// _notFoundLabel
			//
			this._notFoundLabel.AutoSize = true;
			this._notFoundLabel.BackColor = System.Drawing.SystemColors.Window;
			this._notFoundLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F);
			this._notFoundLabel.ForeColor = System.Drawing.Color.Gray;
			this._notFoundLabel.Location = new System.Drawing.Point(58, 92);
			this._notFoundLabel.Name = "_notFoundLabel";
			this._notFoundLabel.Size = new System.Drawing.Size(196, 25);
			this._notFoundLabel.TabIndex = 5;
			this._notFoundLabel.Text = "~No matching images";
			this._notFoundLabel.Visible = false;
			//
			// _thumbnailViewer
			//
			this._thumbnailViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._thumbnailViewer.CanLoad = false;
			this._thumbnailViewer.CaptionMethod = null;
			this._thumbnailViewer.Location = new System.Drawing.Point(0, 66);
			this._thumbnailViewer.MultiSelect = false;
			this._thumbnailViewer.Name = "_thumbnailViewer";
			this._thumbnailViewer.Size = new System.Drawing.Size(627, 534);
			this._thumbnailViewer.TabIndex = 0;
			this._thumbnailViewer.ThumbBorderColor = System.Drawing.Color.Wheat;
			this._thumbnailViewer.ThumbNailSize = 95;
			this._thumbnailViewer.UseCompatibleStateImageBehavior = false;
			this._thumbnailViewer.SelectedIndexChanged += new System.EventHandler(this._thumbnailViewer_SelectedIndexChanged);
			this._thumbnailViewer.DoubleClick += new System.EventHandler(this._thumbnailViewer_DoubleClick);
			//
			// _localizationHelper
			//
			this._localizationHelper.Parent = this;
			//
			// PictureChooser
			//
			this.AcceptButton = this._searchButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(639, 612);
			this.Controls.Add(this._notFoundLabel);
			this.Controls.Add(this._okButton);
			this.Controls.Add(this._searchButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._searchTermsBox);
			this.Controls.Add(this._thumbnailViewer);
			this.MaximizeBox = false;
			this.Name = "PictureChooser";
			this.ShowIcon = false;
			this.ShowInTaskbar = false;
			this.Text = "~Search Image Gallery";
			this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
			this.Load += new System.EventHandler(this.PictureChooser_Load);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PictureChooser_FormClosing);
			((System.ComponentModel.ISupportInitialize)(this._localizationHelper)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ThumbnailViewer _thumbnailViewer;
		private System.Windows.Forms.TextBox _searchTermsBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button _searchButton;
		private System.Windows.Forms.Button _okButton;
		private System.Windows.Forms.Label _notFoundLabel;
		private Palaso.UI.WindowsForms.i8n.LocalizationHelper _localizationHelper;

	}
}
