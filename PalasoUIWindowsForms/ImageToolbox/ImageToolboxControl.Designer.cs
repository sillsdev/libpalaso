namespace Palaso.UI.WindowsForms.ImageToolbox
{
	partial class ImageToolboxControl
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
			if (disposing)
			{
				_toolImages.Dispose();
				if (components != null)
				{

					components.Dispose();
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
			System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("test", System.Windows.Forms.HorizontalAlignment.Left);
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("testing");
			this._toolListView = new System.Windows.Forms.ListView();
			this._panelForControls = new System.Windows.Forms.Panel();
			this._currentImageBox = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).BeginInit();
			this.SuspendLayout();
			//
			// _toolListView
			//
			this._toolListView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			listViewGroup2.Header = "test";
			listViewGroup2.Name = "listViewGroup1";
			this._toolListView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
			listViewGroup2});
			listViewItem2.Group = listViewGroup2;
			this._toolListView.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem2});
			this._toolListView.Location = new System.Drawing.Point(0, 0);
			this._toolListView.Name = "_toolListView";
			this._toolListView.Size = new System.Drawing.Size(175, 371);
			this._toolListView.TabIndex = 0;
			this._toolListView.UseCompatibleStateImageBehavior = false;
			this._toolListView.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			//
			// _panelForControls
			//
			this._panelForControls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._panelForControls.BackColor = System.Drawing.Color.MistyRose;
			this._panelForControls.Location = new System.Drawing.Point(181, 0);
			this._panelForControls.Name = "_panelForControls";
			this._panelForControls.Size = new System.Drawing.Size(376, 502);
			this._panelForControls.TabIndex = 3;
			//
			// _currentImageBox
			//
			this._currentImageBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._currentImageBox.Location = new System.Drawing.Point(3, 377);
			this._currentImageBox.Name = "_currentImageBox";
			this._currentImageBox.Size = new System.Drawing.Size(172, 125);
			this._currentImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this._currentImageBox.TabIndex = 0;
			this._currentImageBox.TabStop = false;
			//
			// ImageToolboxControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._currentImageBox);
			this.Controls.Add(this._panelForControls);
			this.Controls.Add(this._toolListView);
			this.Name = "ImageToolboxControl";
			this.Size = new System.Drawing.Size(560, 505);
			((System.ComponentModel.ISupportInitialize)(this._currentImageBox)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView _toolListView;
		private System.Windows.Forms.Panel _panelForControls;
		private System.Windows.Forms.PictureBox _currentImageBox;
	}
}
