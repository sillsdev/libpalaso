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
			this._addButton = new System.Windows.Forms.Button();
			this._duplicateButton = new System.Windows.Forms.Button();
			this._moreButton = new System.Windows.Forms.Button();
			this._contextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
			this._deleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._importMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._contextMenu.SuspendLayout();
			this.SuspendLayout();
			//
			// _addButton
			//
			this._addButton.Location = new System.Drawing.Point(3, 3);
			this._addButton.Name = "_addButton";
			this._addButton.Size = new System.Drawing.Size(75, 23);
			this._addButton.TabIndex = 0;
			this._addButton.Text = "Add";
			this._addButton.UseVisualStyleBackColor = true;
			this._addButton.Click += new System.EventHandler(this.AddButtonClick);
			//
			// _duplicateButton
			//
			this._duplicateButton.Location = new System.Drawing.Point(84, 3);
			this._duplicateButton.Name = "_duplicateButton";
			this._duplicateButton.Size = new System.Drawing.Size(75, 23);
			this._duplicateButton.TabIndex = 1;
			this._duplicateButton.Text = "Duplicate";
			this._duplicateButton.UseVisualStyleBackColor = true;
			this._duplicateButton.Click += new System.EventHandler(this.DuplicateButtonClick);
			//
			// _moreButton
			//
			this._moreButton.Cursor = System.Windows.Forms.Cursors.Arrow;
			this._moreButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this._moreButton.Location = new System.Drawing.Point(165, 3);
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
			this._deleteMenuItem,
			this._importMenuItem});
			this._contextMenu.Name = "_contextMenu";
			this._contextMenu.Size = new System.Drawing.Size(153, 70);
			//
			// _deleteMenuItem
			//
			this._deleteMenuItem.Name = "_deleteMenuItem";
			this._deleteMenuItem.Size = new System.Drawing.Size(152, 22);
			this._deleteMenuItem.Text = "&Delete";
			this._deleteMenuItem.Click += new System.EventHandler(this.DeleteMenuClick);
			//
			// _importMenuItem
			//
			this._importMenuItem.Name = "_importMenuItem";
			this._importMenuItem.Size = new System.Drawing.Size(152, 22);
			this._importMenuItem.Text = "&Import";
			this._importMenuItem.Click += new System.EventHandler(this.ImportMenuClick);
			//
			// WSAddDuplicateMoreButtonBar
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._addButton);
			this.Controls.Add(this._moreButton);
			this.Controls.Add(this._duplicateButton);
			this.Name = "WSAddDuplicateMoreButtonBar";
			this.Size = new System.Drawing.Size(245, 31);
			this._contextMenu.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button _addButton;
		private System.Windows.Forms.Button _duplicateButton;
		private System.Windows.Forms.Button _moreButton;
		private System.Windows.Forms.ContextMenuStrip _contextMenu;
		private System.Windows.Forms.ToolStripMenuItem _deleteMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _importMenuItem;
	}
}
