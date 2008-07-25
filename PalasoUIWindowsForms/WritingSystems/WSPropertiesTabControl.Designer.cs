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
			this._tabControl = new System.Windows.Forms.TabControl();
			this._aboutPage = new System.Windows.Forms.TabPage();
			this._aboutControl = new Palaso.UI.WindowsForms.WritingSystems.WSAboutControl();
			this._spellingPage = new System.Windows.Forms.TabPage();
			this._autoReplacePage = new System.Windows.Forms.TabPage();
			this._fontsPage = new System.Windows.Forms.TabPage();
			this._fontControl = new Palaso.UI.WindowsForms.WritingSystems.WSFontControl();
			this._keyboardsPage = new System.Windows.Forms.TabPage();
			this._keyboardControl = new Palaso.UI.WindowsForms.WritingSystems.WSKeyboardControl();
			this._sortingPage = new System.Windows.Forms.TabPage();
			this._otherPage = new System.Windows.Forms.TabPage();
			this._tabControl.SuspendLayout();
			this._aboutPage.SuspendLayout();
			this._fontsPage.SuspendLayout();
			this._keyboardsPage.SuspendLayout();
			this.SuspendLayout();
			//
			// _tabControl
			//
			this._tabControl.Controls.Add(this._aboutPage);
			this._tabControl.Controls.Add(this._spellingPage);
			this._tabControl.Controls.Add(this._autoReplacePage);
			this._tabControl.Controls.Add(this._fontsPage);
			this._tabControl.Controls.Add(this._keyboardsPage);
			this._tabControl.Controls.Add(this._sortingPage);
			this._tabControl.Controls.Add(this._otherPage);
			this._tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tabControl.Location = new System.Drawing.Point(0, 0);
			this._tabControl.Name = "_tabControl";
			this._tabControl.SelectedIndex = 0;
			this._tabControl.Size = new System.Drawing.Size(678, 424);
			this._tabControl.TabIndex = 0;
			//
			// _aboutPage
			//
			this._aboutPage.Controls.Add(this._aboutControl);
			this._aboutPage.Location = new System.Drawing.Point(4, 22);
			this._aboutPage.Name = "_aboutPage";
			this._aboutPage.Padding = new System.Windows.Forms.Padding(3);
			this._aboutPage.Size = new System.Drawing.Size(670, 398);
			this._aboutPage.TabIndex = 0;
			this._aboutPage.Text = "About";
			this._aboutPage.UseVisualStyleBackColor = true;
			//
			// _aboutControl
			//
			this._aboutControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._aboutControl.Location = new System.Drawing.Point(3, 3);
			this._aboutControl.Name = "_aboutControl";
			this._aboutControl.Size = new System.Drawing.Size(664, 392);
			this._aboutControl.TabIndex = 0;
			//
			// _spellingPage
			//
			this._spellingPage.Location = new System.Drawing.Point(4, 22);
			this._spellingPage.Name = "_spellingPage";
			this._spellingPage.Padding = new System.Windows.Forms.Padding(3);
			this._spellingPage.Size = new System.Drawing.Size(670, 398);
			this._spellingPage.TabIndex = 1;
			this._spellingPage.Text = "Spelling";
			this._spellingPage.UseVisualStyleBackColor = true;
			//
			// _autoReplacePage
			//
			this._autoReplacePage.Location = new System.Drawing.Point(4, 22);
			this._autoReplacePage.Name = "_autoReplacePage";
			this._autoReplacePage.Size = new System.Drawing.Size(670, 398);
			this._autoReplacePage.TabIndex = 2;
			this._autoReplacePage.Text = "Auto Replace";
			this._autoReplacePage.UseVisualStyleBackColor = true;
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
			this._fontControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._fontControl.Location = new System.Drawing.Point(0, 0);
			this._fontControl.Name = "_fontControl";
			this._fontControl.Size = new System.Drawing.Size(670, 398);
			this._fontControl.TabIndex = 0;
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
			this._keyboardControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this._keyboardControl.Location = new System.Drawing.Point(0, 0);
			this._keyboardControl.Name = "_keyboardControl";
			this._keyboardControl.Size = new System.Drawing.Size(670, 398);
			this._keyboardControl.TabIndex = 0;
			//
			// _sortingPage
			//
			this._sortingPage.Location = new System.Drawing.Point(4, 22);
			this._sortingPage.Name = "_sortingPage";
			this._sortingPage.Size = new System.Drawing.Size(670, 398);
			this._sortingPage.TabIndex = 5;
			this._sortingPage.Text = "Sorting";
			this._sortingPage.UseVisualStyleBackColor = true;
			//
			// _otherPage
			//
			this._otherPage.Location = new System.Drawing.Point(4, 22);
			this._otherPage.Name = "_otherPage";
			this._otherPage.Size = new System.Drawing.Size(670, 398);
			this._otherPage.TabIndex = 6;
			this._otherPage.Text = "Other";
			this._otherPage.UseVisualStyleBackColor = true;
			//
			// WSPropertiesTabControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._tabControl);
			this.Name = "WSPropertiesTabControl";
			this.Size = new System.Drawing.Size(678, 424);
			this._tabControl.ResumeLayout(false);
			this._aboutPage.ResumeLayout(false);
			this._fontsPage.ResumeLayout(false);
			this._keyboardsPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TabControl _tabControl;
		private System.Windows.Forms.TabPage _aboutPage;
		private System.Windows.Forms.TabPage _spellingPage;
		private WSAboutControl _aboutControl;
		private System.Windows.Forms.TabPage _autoReplacePage;
		private System.Windows.Forms.TabPage _fontsPage;
		private System.Windows.Forms.TabPage _keyboardsPage;
		private System.Windows.Forms.TabPage _sortingPage;
		private System.Windows.Forms.TabPage _otherPage;
		private WSFontControl _fontControl;
		private WSKeyboardControl _keyboardControl;
	}
}
