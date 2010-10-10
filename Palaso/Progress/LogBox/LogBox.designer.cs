namespace Palaso.Progress.LogBox
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
			this._box = new System.Windows.Forms.RichTextBox();
			this._verboseBox = new System.Windows.Forms.RichTextBox();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this._menu = new System.Windows.Forms.ToolStripMenuItem();
			this._showDetailsMenu = new System.Windows.Forms.ToolStripMenuItem();
			this.copyToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this._runDiagnostics = new System.Windows.Forms.ToolStripMenuItem();
			this.panel1 = new System.Windows.Forms.Panel();
			this._reportProblemLink = new System.Windows.Forms.LinkLabel();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			//
			// _box
			//
			this._box.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._box.BackColor = System.Drawing.SystemColors.Window;
			this._box.Location = new System.Drawing.Point(3, 3);
			this._box.Name = "_box";
			this._box.ReadOnly = true;
			this._box.Size = new System.Drawing.Size(483, 155);
			this._box.TabIndex = 0;
			this._box.TabStop = false;
			this._box.Text = "";
			//
			// _verboseBox
			//
			this._verboseBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._verboseBox.Location = new System.Drawing.Point(3, 3);
			this._verboseBox.Name = "_verboseBox";
			this._verboseBox.Size = new System.Drawing.Size(483, 155);
			this._verboseBox.TabIndex = 3;
			this._verboseBox.TabStop = false;
			this._verboseBox.Text = "";
			this._verboseBox.Visible = false;
			//
			// menuStrip1
			//
			this.menuStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.menuStrip1.BackColor = System.Drawing.Color.Transparent;
			this.menuStrip1.Dock = System.Windows.Forms.DockStyle.None;
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this._menu});
			this.menuStrip1.Location = new System.Drawing.Point(-2, 155);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
			this.menuStrip1.Size = new System.Drawing.Size(30, 24);
			this.menuStrip1.TabIndex = 4;
			this.menuStrip1.Text = "menuStrip1";
			this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
			//
			// _menu
			//
			this._menu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
			this._showDetailsMenu,
			this.copyToClipboardToolStripMenuItem,
			this._runDiagnostics});
			this._menu.Image = LogBoxResources.menuButton;
			this._menu.Name = "_menu";
			this._menu.Size = new System.Drawing.Size(28, 20);
			//
			// _showDetailsMenu
			//
			this._showDetailsMenu.CheckOnClick = true;
			this._showDetailsMenu.Name = "_showDetailsMenu";
			this._showDetailsMenu.Size = new System.Drawing.Size(171, 22);
			this._showDetailsMenu.Text = "Show Details";
			this._showDetailsMenu.CheckStateChanged += new System.EventHandler(this._showDetails_CheckedChanged);
			//
			// copyToClipboardToolStripMenuItem
			//
			this.copyToClipboardToolStripMenuItem.Name = "copyToClipboardToolStripMenuItem";
			this.copyToClipboardToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
			this.copyToClipboardToolStripMenuItem.Text = "CopyTo Clipboard";
			this.copyToClipboardToolStripMenuItem.Click += new System.EventHandler(this.copyToClipboardToolStripMenuItem_Click);
			//
			// _runDiagnostics
			//
			this._runDiagnostics.Name = "_runDiagnostics";
			this._runDiagnostics.Size = new System.Drawing.Size(171, 22);
			this._runDiagnostics.Text = "Run diagnostics";
			this._runDiagnostics.Click += new System.EventHandler(this.OnRunDiagnosticsClick);
			//
			// panel1
			//
			this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panel1.BackColor = System.Drawing.SystemColors.Window;
			this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.panel1.Location = new System.Drawing.Point(4, 4);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(482, 154);
			this.panel1.TabIndex = 5;
			//
			// _reportProblemLink
			//
			this._reportProblemLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._reportProblemLink.AutoSize = true;
			this._reportProblemLink.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._reportProblemLink.LinkColor = System.Drawing.Color.Red;
			this._reportProblemLink.Location = new System.Drawing.Point(22, 157);
			this._reportProblemLink.Name = "_reportProblemLink";
			this._reportProblemLink.Size = new System.Drawing.Size(246, 17);
			this._reportProblemLink.TabIndex = 6;
			this._reportProblemLink.TabStop = true;
			this._reportProblemLink.Text = "Report this problem to the developers";
			this._reportProblemLink.Visible = false;
			this._reportProblemLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this._reportProblemLink_LinkClicked);
			//
			// LogBox
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this._reportProblemLink);
			this.Controls.Add(this._verboseBox);
			this.Controls.Add(this._box);
			this.Controls.Add(this.menuStrip1);
			this.Controls.Add(this.panel1);
			this.Name = "LogBox";
			this.Size = new System.Drawing.Size(489, 184);
			this.BackColorChanged += new System.EventHandler(this.LogBox_BackColorChanged);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox _box;
		private System.Windows.Forms.RichTextBox _verboseBox;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem _menu;
		private System.Windows.Forms.ToolStripMenuItem _showDetailsMenu;
		private System.Windows.Forms.ToolStripMenuItem copyToClipboardToolStripMenuItem;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.LinkLabel _reportProblemLink;
		private System.Windows.Forms.ToolStripMenuItem _runDiagnostics;
	}
}
