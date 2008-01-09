namespace Palaso.DictionaryService.SampleClient
{
	partial class MainWindow
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this._tabControl = new System.Windows.Forms.TabControl();
			this._lookupTabPage = new System.Windows.Forms.TabPage();
			this._addEntryTabPage = new System.Windows.Forms.TabPage();
			this._logText = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this._chooseDictionaryButton = new System.Windows.Forms.Button();
			this._dictionaryPathLabel = new System.Windows.Forms.Label();
			this._settingsTabPage = new System.Windows.Forms.TabPage();
			this.lookupControl1 = new Palaso.DictionaryService.SampleClient.LookupControl();
			this.addEntry1 = new Palaso.DictionaryService.SampleClient.AddEntry();
			this.settingsControl1 = new Palaso.DictionaryService.SampleClient.SettingsControl();
			this._tabControl.SuspendLayout();
			this._lookupTabPage.SuspendLayout();
			this._addEntryTabPage.SuspendLayout();
			this._settingsTabPage.SuspendLayout();
			this.SuspendLayout();
			//
			// _tabControl
			//
			this._tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._tabControl.Controls.Add(this._lookupTabPage);
			this._tabControl.Controls.Add(this._addEntryTabPage);
			this._tabControl.Controls.Add(this._settingsTabPage);
			this._tabControl.Location = new System.Drawing.Point(12, 49);
			this._tabControl.Name = "_tabControl";
			this._tabControl.SelectedIndex = 0;
			this._tabControl.Size = new System.Drawing.Size(474, 245);
			this._tabControl.TabIndex = 0;
			//
			// _lookupTabPage
			//
			this._lookupTabPage.Controls.Add(this.lookupControl1);
			this._lookupTabPage.Location = new System.Drawing.Point(4, 22);
			this._lookupTabPage.Name = "_lookupTabPage";
			this._lookupTabPage.Padding = new System.Windows.Forms.Padding(3);
			this._lookupTabPage.Size = new System.Drawing.Size(466, 219);
			this._lookupTabPage.TabIndex = 0;
			this._lookupTabPage.Text = "Lookup";
			this._lookupTabPage.UseVisualStyleBackColor = true;
			//
			// _addEntryTabPage
			//
			this._addEntryTabPage.Controls.Add(this.addEntry1);
			this._addEntryTabPage.Location = new System.Drawing.Point(4, 22);
			this._addEntryTabPage.Name = "_addEntryTabPage";
			this._addEntryTabPage.Padding = new System.Windows.Forms.Padding(3);
			this._addEntryTabPage.Size = new System.Drawing.Size(466, 219);
			this._addEntryTabPage.TabIndex = 1;
			this._addEntryTabPage.Text = "Add Entry";
			this._addEntryTabPage.UseVisualStyleBackColor = true;
			//
			// _logText
			//
			this._logText.Location = new System.Drawing.Point(9, 325);
			this._logText.Multiline = true;
			this._logText.Name = "_logText";
			this._logText.ReadOnly = true;
			this._logText.Size = new System.Drawing.Size(477, 71);
			this._logText.TabIndex = 1;
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(9, 306);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(25, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "Log";
			//
			// _chooseDictionaryButton
			//
			this._chooseDictionaryButton.Enabled = false;
			this._chooseDictionaryButton.Location = new System.Drawing.Point(12, 13);
			this._chooseDictionaryButton.Name = "_chooseDictionaryButton";
			this._chooseDictionaryButton.Size = new System.Drawing.Size(75, 23);
			this._chooseDictionaryButton.TabIndex = 3;
			this._chooseDictionaryButton.Text = "Dictionary...";
			this._chooseDictionaryButton.UseVisualStyleBackColor = true;
			//
			// _dictionaryPathLabel
			//
			this._dictionaryPathLabel.AutoSize = true;
			this._dictionaryPathLabel.Location = new System.Drawing.Point(93, 18);
			this._dictionaryPathLabel.Name = "_dictionaryPathLabel";
			this._dictionaryPathLabel.Size = new System.Drawing.Size(285, 13);
			this._dictionaryPathLabel.TabIndex = 4;
			this._dictionaryPathLabel.Text = "E:\\Users\\John\\Documents\\WeSay\\NooSupu\\noosupu.lift";
			//
			// _settingsTabPage
			//
			this._settingsTabPage.Controls.Add(this.settingsControl1);
			this._settingsTabPage.Location = new System.Drawing.Point(4, 22);
			this._settingsTabPage.Name = "_settingsTabPage";
			this._settingsTabPage.Padding = new System.Windows.Forms.Padding(3);
			this._settingsTabPage.Size = new System.Drawing.Size(466, 219);
			this._settingsTabPage.TabIndex = 2;
			this._settingsTabPage.Text = "Settings";
			this._settingsTabPage.UseVisualStyleBackColor = true;
			//
			// lookupControl1
			//
			this.lookupControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lookupControl1.Location = new System.Drawing.Point(3, 3);
			this.lookupControl1.Name = "lookupControl1";
			this.lookupControl1.Size = new System.Drawing.Size(460, 213);
			this.lookupControl1.TabIndex = 0;
			//
			// addEntry1
			//
			this.addEntry1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.addEntry1.Location = new System.Drawing.Point(3, 3);
			this.addEntry1.Name = "addEntry1";
			this.addEntry1.Size = new System.Drawing.Size(460, 213);
			this.addEntry1.TabIndex = 0;
			//
			// settingsControl1
			//
			this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.settingsControl1.Location = new System.Drawing.Point(3, 3);
			this.settingsControl1.Name = "settingsControl1";
			this.settingsControl1.Size = new System.Drawing.Size(460, 213);
			this.settingsControl1.TabIndex = 0;
			//
			// MainWindow
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(498, 408);
			this.Controls.Add(this._dictionaryPathLabel);
			this.Controls.Add(this._chooseDictionaryButton);
			this.Controls.Add(this.label1);
			this.Controls.Add(this._logText);
			this.Controls.Add(this._tabControl);
			this.Name = "MainWindow";
			this.Text = "Sample Dictionary Services Client";
			this.Load += new System.EventHandler(this.Main_Load);
			this._tabControl.ResumeLayout(false);
			this._lookupTabPage.ResumeLayout(false);
			this._addEntryTabPage.ResumeLayout(false);
			this._settingsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TabControl _tabControl;
		private System.Windows.Forms.TabPage _lookupTabPage;
		private System.Windows.Forms.TabPage _addEntryTabPage;
		private AddEntry addEntry1;
		private System.Windows.Forms.TextBox _logText;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button _chooseDictionaryButton;
		private System.Windows.Forms.Label _dictionaryPathLabel;
		private LookupControl lookupControl1;
		private System.Windows.Forms.TabPage _settingsTabPage;
		private SettingsControl settingsControl1;
	}
}