namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSAddDuplicateMoreButtonBar
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
			this._moreButton = new System.Windows.Forms.Button();
			this._contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this._duplicateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._exportMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._importMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._contextMenu.SuspendLayout();
			this.SuspendLayout();
			//
			// _moreButton
			//
			this._moreButton.Cursor = System.Windows.Forms.Cursors.Arrow;
			this._moreButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this._moreButton.Location = new System.Drawing.Point(3, 3);
			this._moreButton.Name = "_moreButton";
			this._moreButton.Size = new System.Drawing.Size(75, 23);
			this._moreButton.TabIndex = 2;
			this._moreButton.Text = "More";
			this._moreButton.UseVisualStyleBackColor = true;
			this._moreButton.Click += new System.EventHandler(this.MoreButtonClick);
			//
			// _contextMenu
			//
			this._contextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this._duplicateMenuItem,
			this._deleteMenuItem,
			this._exportMenuItem,
			this._importMenuItem});
			this._contextMenu.Name = "_contextMenu";
			this._contextMenu.Size = new System.Drawing.Size(260, 92);
			//
			// _duplicateMenuItem
			//
			this._duplicateMenuItem.Name = "_duplicateMenuItem";
			this._duplicateMenuItem.Size = new System.Drawing.Size(259, 22);
			this._duplicateMenuItem.Text = "Add New Language by Copying {0}";
			this._duplicateMenuItem.Click += new System.EventHandler(this.DuplicateButtonClick);
			//
			// _deleteMenuItem
			//
			this._deleteMenuItem.Name = "_deleteMenuItem";
			this._deleteMenuItem.Size = new System.Drawing.Size(259, 22);
			this._deleteMenuItem.Text = "&Delete {0}...";
			this._deleteMenuItem.Click += new System.EventHandler(this.DeleteMenuClick);
			//
			// _exportMenuItem
			//
			this._exportMenuItem.Name = "_exportMenuItem";
			this._exportMenuItem.Size = new System.Drawing.Size(259, 22);
			this._exportMenuItem.Text = "&Save a Copy of the {0} LDML File...";
			this._exportMenuItem.Click += new System.EventHandler(this.ExportMenuClick);
			//
			// _importMenuItem
			//
			this._importMenuItem.Name = "_importMenuItem";
			this._importMenuItem.Size = new System.Drawing.Size(259, 22);
			this._importMenuItem.Text = "&Import from LDML File...";
			this._importMenuItem.Click += new System.EventHandler(this.ImportMenuClick);
			//
			// WSAddDuplicateMoreButtonBar
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._moreButton);
			this.Name = "WSAddDuplicateMoreButtonBar";
			this.Size = new System.Drawing.Size(87, 31);
			this._contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _moreButton;
		private System.Windows.Forms.ContextMenuStrip _contextMenu;
		private System.Windows.Forms.ToolStripMenuItem _deleteMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _importMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _exportMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _duplicateMenuItem;
	}
}
