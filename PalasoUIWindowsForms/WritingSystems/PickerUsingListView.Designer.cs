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
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this._indentifierColumn = new System.Windows.Forms.ColumnHeader();
			this._editListLink = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			//
			// listView1
			//
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
			this.columnHeader2,
			this._indentifierColumn});
			this.listView1.FullRowSelect = true;
			this.listView1.HideSelection = false;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(278, 135);
			this.listView1.Sorting = System.Windows.Forms.SortOrder.Descending;
			this.listView1.TabIndex = 0;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
			//
			// columnHeader2
			//
			this.columnHeader2.Text = "Name";
			this.columnHeader2.Width = 120;
			//
			// _indentifierColumn
			//
			this._indentifierColumn.Text = "Identifier";
			this._indentifierColumn.Width = 140;
			//
			// _editListLink
			//
			this._editListLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._editListLink.AutoSize = true;
			this._editListLink.Location = new System.Drawing.Point(3, 138);
			this._editListLink.Name = "_editListLink";
			this._editListLink.Size = new System.Drawing.Size(40, 13);
			this._editListLink.TabIndex = 1;
			this._editListLink.TabStop = true;
			this._editListLink.Text = "More...";
			this._editListLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._editListLink_LinkClicked);
			//
			// PickerUsingListView
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._editListLink);
			this.Controls.Add(this.listView1);
			this.Name = "PickerUsingListView";
			this.Size = new System.Drawing.Size(278, 158);
			this.Load += new System.EventHandler(this.PickerUsingListView_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader _indentifierColumn;
		private System.Windows.Forms.LinkLabel _editListLink;
	}
}
