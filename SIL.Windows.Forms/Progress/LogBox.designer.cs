namespace SIL.Windows.Forms.Progress
{
	partial class LogBox
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
			this._box = new System.Windows.Forms.RichTextBox();
			this._verboseBox = new System.Windows.Forms.RichTextBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this._menu = new System.Windows.Forms.ToolStripMenuItem();
			this._showDetailsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this._copyToClipboardMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._runDiagnostics = new System.Windows.Forms.ToolStripMenuItem();
			this._chooseFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._tableLayout = new System.Windows.Forms.TableLayoutPanel();
			this._panelBox = new System.Windows.Forms.Panel();
			this._scrollToEndTimer = new System.Windows.Forms.Timer(this.components);
			this.menuStrip1.SuspendLayout();
			this._tableLayout.SuspendLayout();
			this._panelBox.SuspendLayout();
			this.SuspendLayout();
			//
			// _box
			//
			this._box.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._box.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
			this._box.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._box.BulletIndent = 5;
			this._box.Location = new System.Drawing.Point(17, 20);
			this._box.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this._box.Name = "_box";
			this._box.ReadOnly = true;
			this._box.Size = new System.Drawing.Size(121, 276);
			this._box.TabIndex = 0;
			this._box.TabStop = false;
			this._box.Text = "";
			//
			// _verboseBox
			//
			this._verboseBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._verboseBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
			this._verboseBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._verboseBox.BulletIndent = 5;
			this._verboseBox.Location = new System.Drawing.Point(141, 27);
			this._verboseBox.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
			this._verboseBox.Name = "_verboseBox";
			this._verboseBox.Size = new System.Drawing.Size(113, 269);
			this._verboseBox.TabIndex = 3;
			this._verboseBox.TabStop = false;
			this._verboseBox.Text = "";
			this._verboseBox.Visible = false;
			//
			// menuStrip1
			//
			this.menuStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.menuStrip1.BackColor = System.Drawing.Color.Silver;
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this._menu});
			this.menuStrip1.Location = new System.Drawing.Point(0, 329);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(293, 24);
			this.menuStrip1.TabIndex = 4;
			this.menuStrip1.Text = "menuStrip1";
			this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
			//
			// _menu
			//
			this._menu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this._showDetailsMenu,
			this._copyToClipboardMenuItem,
			this._runDiagnostics,
			this._chooseFontMenuItem});
			this._menu.Image = global::SIL.Windows.Forms.Progress.LogBoxResources.MenuButtonArrow;
			this._menu.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this._menu.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
			this._menu.Name = "_menu";
			this._menu.Size = new System.Drawing.Size(27, 20);
			//
			// _showDetailsMenu
			//
			this._showDetailsMenu.CheckOnClick = true;
			this._showDetailsMenu.Name = "_showDetailsMenu";
			this._showDetailsMenu.Size = new System.Drawing.Size(171, 22);
			this._showDetailsMenu.Text = "Show Details";
			this._showDetailsMenu.CheckStateChanged += new System.EventHandler(this._showDetails_CheckedChanged);
			//
			// _copyToClipboardMenuItem
			//
			this._copyToClipboardMenuItem.Name = "_copyToClipboardMenuItem";
			this._copyToClipboardMenuItem.Size = new System.Drawing.Size(171, 22);
			this._copyToClipboardMenuItem.Text = "Copy to Clipboard";
			this._copyToClipboardMenuItem.Click += new System.EventHandler(this.copyToClipboardToolStripMenuItem_Click);
			//
			// _runDiagnostics
			//
			this._runDiagnostics.Name = "_runDiagnostics";
			this._runDiagnostics.Size = new System.Drawing.Size(171, 22);
			this._runDiagnostics.Text = "Run Diagnostics";
			this._runDiagnostics.Click += new System.EventHandler(this.OnRunDiagnosticsClick);
			//
			// _chooseFontMenuItem
			//
			this._chooseFontMenuItem.Name = "_chooseFontMenuItem";
			this._chooseFontMenuItem.Size = new System.Drawing.Size(171, 22);
			this._chooseFontMenuItem.Text = "Choose Font...";
			this._chooseFontMenuItem.Click += new System.EventHandler(this.OnChooseFontClick);
			//
			// _tableLayout
			//
			this._tableLayout.BackColor = System.Drawing.Color.Transparent;
			this._tableLayout.ColumnCount = 1;
			this._tableLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayout.Controls.Add(this._panelBox, 0, 0);
			this._tableLayout.Controls.Add(this.menuStrip1, 0, 1);
			this._tableLayout.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tableLayout.Location = new System.Drawing.Point(0, 0);
			this._tableLayout.Name = "_tableLayout";
			this._tableLayout.RowCount = 2;
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayout.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayout.Size = new System.Drawing.Size(293, 353);
			this._tableLayout.TabIndex = 7;
			this._tableLayout.Paint += new System.Windows.Forms.PaintEventHandler(this.HandleTableLayoutPaint);
			//
			// _panelBox
			//
			this._panelBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this._panelBox.BackColor = System.Drawing.Color.Transparent;
			this._panelBox.Controls.Add(this._box);
			this._panelBox.Controls.Add(this._verboseBox);
			this._panelBox.Location = new System.Drawing.Point(5, 5);
			this._panelBox.Margin = new System.Windows.Forms.Padding(5, 5, 0, 3);
			this._panelBox.Name = "_panelBox";
			this._panelBox.Size = new System.Drawing.Size(288, 321);
			this._panelBox.TabIndex = 8;
			// 
			// _scrollToEndTimer
			// 
			this._scrollToEndTimer.Tick += new System.EventHandler(this.ScrollVisibleBoxToEnd);
			// 
			// LogBox
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
			this.Controls.Add(this._tableLayout);
			this.Name = "LogBox";
			this.Size = new System.Drawing.Size(293, 353);
			this.Load += new System.EventHandler(this.LogBox_Load);
			this.BackColorChanged += new System.EventHandler(this.LogBox_BackColorChanged);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this._tableLayout.ResumeLayout(false);
			this._tableLayout.PerformLayout();
			this._panelBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.RichTextBox _box;
		private System.Windows.Forms.RichTextBox _verboseBox;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem _menu;
		private System.Windows.Forms.ToolStripMenuItem _showDetailsMenu;
		private System.Windows.Forms.ToolStripMenuItem _copyToClipboardMenuItem;
		private System.Windows.Forms.ToolStripMenuItem _runDiagnostics;
		private System.Windows.Forms.ToolStripMenuItem _chooseFontMenuItem;
		private System.Windows.Forms.TableLayoutPanel _tableLayout;
		private System.Windows.Forms.Panel _panelBox;
		private System.Windows.Forms.Timer _scrollToEndTimer;
	}
}
