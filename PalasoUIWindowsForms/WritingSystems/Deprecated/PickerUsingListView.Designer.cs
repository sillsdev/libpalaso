namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class PickerUsingListView
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
			System.Windows.Forms.ColumnHeader _descriptionColumnHeader;
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Don\'t remove this");
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("");
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PickerUsingListView));
			this._setupWritingSystemsLink = new System.Windows.Forms.LinkLabel();
			this.listView1 = new System.Windows.Forms.ListView();
			this._labelColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			_descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			//
			// _descriptionColumnHeader
			//
			_descriptionColumnHeader.Text = "Description";
			_descriptionColumnHeader.Width = 133;
			//
			// _setupWritingSystemsLink
			//
			this._setupWritingSystemsLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._setupWritingSystemsLink.AutoSize = true;
			this._setupWritingSystemsLink.Location = new System.Drawing.Point(3, 138);
			this._setupWritingSystemsLink.Name = "_setupWritingSystemsLink";
			this._setupWritingSystemsLink.Size = new System.Drawing.Size(40, 13);
			this._setupWritingSystemsLink.TabIndex = 1;
			this._setupWritingSystemsLink.TabStop = true;
			this._setupWritingSystemsLink.Text = "More...";
			this._setupWritingSystemsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._editListLink_LinkClicked);
			//
			// listView1
			//
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.CheckBoxes = true;
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this._labelColumnHeader,
			_descriptionColumnHeader});
			this.listView1.FullRowSelect = true;
			listViewItem1.StateImageIndex = 0;
			listViewItem2.Checked = true;
			listViewItem2.StateImageIndex = 1;
			this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
			listViewItem1,
			listViewItem2});
			this.listView1.Location = new System.Drawing.Point(6, 3);
			this.listView1.Name = "listView1";
			this.listView1.ShowItemToolTips = true;
			this.listView1.Size = new System.Drawing.Size(260, 132);
			this.listView1.StateImageList = this.imageList1;
			this.listView1.TabIndex = 2;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
			this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			//
			// _labelColumnHeader
			//
			this._labelColumnHeader.Text = "Label";
			this._labelColumnHeader.Width = 108;
			//
			// imageList1
			//
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			this.imageList1.Images.SetKeyName(0, "wsIcon.png");
			this.imageList1.Images.SetKeyName(1, "wsIconChecked16.png");
			//
			// PickerUsingListView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.listView1);
			this.Controls.Add(this._setupWritingSystemsLink);
			this.Name = "PickerUsingListView";
			this.Size = new System.Drawing.Size(278, 158);
			this.Load += new System.EventHandler(this.PickerUsingListView_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.LinkLabel _setupWritingSystemsLink;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader _labelColumnHeader;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.ImageList imageList1;
	}
}
