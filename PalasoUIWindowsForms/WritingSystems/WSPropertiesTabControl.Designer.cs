namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSPropertiesTabControl
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
			this._sortingPage = new System.Windows.Forms.TabPage();
			this._sortControl = new Palaso.UI.WindowsForms.WritingSystems.WSSortControl();
			this._keyboardsPage = new System.Windows.Forms.TabPage();
			this._keyboardControl = new Palaso.UI.WindowsForms.WritingSystems.WSKeyboardControl();
			this._fontsPage = new System.Windows.Forms.TabPage();
			this._fontControl = new Palaso.UI.WindowsForms.WritingSystems.WSFontControl();
			this._spellingPage = new System.Windows.Forms.TabPage();
			this._spellingControl = new Palaso.UI.WindowsForms.WritingSystems.WSSpellingControl();
			this._identifiersPage = new System.Windows.Forms.TabPage();
			this._identifiersControl = new Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers.WSIdentifierView();
			this._tabControl = new System.Windows.Forms.TabControl();
			this._sortingPage.SuspendLayout();
			this._keyboardsPage.SuspendLayout();
			this._fontsPage.SuspendLayout();
			this._spellingPage.SuspendLayout();
			this._identifiersPage.SuspendLayout();
			this._tabControl.SuspendLayout();
			this.SuspendLayout();
			//
			// _sortingPage
			//
			this._sortingPage.Controls.Add(this._sortControl);
			this._sortingPage.Location = new System.Drawing.Point(4, 22);
			this._sortingPage.Name = "_sortingPage";
			this._sortingPage.Size = new System.Drawing.Size(670, 398);
			this._sortingPage.TabIndex = 5;
			this._sortingPage.Text = "Sorting";
			this._sortingPage.UseVisualStyleBackColor = true;
			//
			// _sortControl
			//
			this._sortControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._sortControl.Location = new System.Drawing.Point(0, 14);
			this._sortControl.Name = "_sortControl";
			this._sortControl.Size = new System.Drawing.Size(670, 384);
			this._sortControl.TabIndex = 0;
			//
			// _keyboardsPage
			//
			this._keyboardsPage.Controls.Add(this._keyboardControl);
			this._keyboardsPage.Location = new System.Drawing.Point(4, 22);
			this._keyboardsPage.Name = "_keyboardsPage";
			this._keyboardsPage.Size = new System.Drawing.Size(670, 398);
			this._keyboardsPage.TabIndex = 4;
			this._keyboardsPage.Text = "Keyboards";
			this._keyboardsPage.UseVisualStyleBackColor = true;
			//
			// _keyboardControl
			//
			this._keyboardControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._keyboardControl.Location = new System.Drawing.Point(0, 13);
			this._keyboardControl.Name = "_keyboardControl";
			this._keyboardControl.Size = new System.Drawing.Size(670, 385);
			this._keyboardControl.TabIndex = 0;
			//
			// _fontsPage
			//
			this._fontsPage.Controls.Add(this._fontControl);
			this._fontsPage.Location = new System.Drawing.Point(4, 22);
			this._fontsPage.Name = "_fontsPage";
			this._fontsPage.Size = new System.Drawing.Size(670, 398);
			this._fontsPage.TabIndex = 3;
			this._fontsPage.Text = "Fonts";
			this._fontsPage.UseVisualStyleBackColor = true;
			//
			// _fontControl
			//
			this._fontControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._fontControl.Location = new System.Drawing.Point(3, 13);
			this._fontControl.Name = "_fontControl";
			this._fontControl.Size = new System.Drawing.Size(667, 385);
			this._fontControl.TabIndex = 0;
			//
			// _spellingPage
			//
			this._spellingPage.Controls.Add(this._spellingControl);
			this._spellingPage.Location = new System.Drawing.Point(3, 13);
			this._spellingPage.Name = "_spellingPage";
			this._spellingPage.Padding = new System.Windows.Forms.Padding(3);
			this._spellingPage.Size = new System.Drawing.Size(670, 398);
			this._spellingPage.TabIndex = 1;
			this._spellingPage.Text = "Spelling";
			this._spellingPage.UseVisualStyleBackColor = true;
			//
			// _spellingControl
			//
			this._spellingControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._spellingControl.Location = new System.Drawing.Point(3, 13);
			this._spellingControl.Name = "_spellingControl";
			this._spellingControl.Size = new System.Drawing.Size(661, 380);
			this._spellingControl.TabIndex = 0;
			//
			// _identifiersPage
			//
			this._identifiersPage.Controls.Add(this._identifiersControl);
			this._identifiersPage.Location = new System.Drawing.Point(4, 22);
			this._identifiersPage.Name = "_identifiersPage";
			this._identifiersPage.Padding = new System.Windows.Forms.Padding(3);
			this._identifiersPage.Size = new System.Drawing.Size(670, 398);
			this._identifiersPage.TabIndex = 6;
			this._identifiersPage.Text = "Identifiers";
			//
			// _identifiersControl
			//
			this._identifiersControl.Location = new System.Drawing.Point(3, 13);
			this._identifiersControl.Name = "_identifiersControl";
			this._identifiersControl.Size = new System.Drawing.Size(577, 317);
			this._identifiersControl.TabIndex = 0;
			//
			// _tabControl
			//
			this._tabControl.Controls.Add(this._identifiersPage);
			this._tabControl.Controls.Add(this._spellingPage);
			this._tabControl.Controls.Add(this._fontsPage);
			this._tabControl.Controls.Add(this._keyboardsPage);
			this._tabControl.Controls.Add(this._sortingPage);
			this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tabControl.Location = new System.Drawing.Point(0, 0);
			this._tabControl.Name = "_tabControl";
			this._tabControl.SelectedIndex = 0;
			this._tabControl.Size = new System.Drawing.Size(678, 424);
			this._tabControl.TabIndex = 0;
			//
			// WSPropertiesTabControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._tabControl);
			this.Name = "WSPropertiesTabControl";
			this.Size = new System.Drawing.Size(678, 424);
			this._sortingPage.ResumeLayout(false);
			this._keyboardsPage.ResumeLayout(false);
			this._fontsPage.ResumeLayout(false);
			this._spellingPage.ResumeLayout(false);
			this._identifiersPage.ResumeLayout(false);
			this._tabControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabPage _sortingPage;
		private WSSortControl _sortControl;
		private System.Windows.Forms.TabPage _keyboardsPage;
		private WSKeyboardControl _keyboardControl;
		private System.Windows.Forms.TabPage _fontsPage;
		private WSFontControl _fontControl;
		private System.Windows.Forms.TabPage _spellingPage;
		private WSSpellingControl _spellingControl;
		private System.Windows.Forms.TabPage _identifiersPage;
		private Palaso.UI.WindowsForms.WritingSystems.WSIdentifiers.WSIdentifierView _identifiersControl;
		private System.Windows.Forms.TabControl _tabControl;


	}
}
