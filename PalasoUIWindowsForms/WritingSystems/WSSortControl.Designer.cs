namespace Palaso.UI.WindowsForms.WritingSystems
{
	partial class WSSortControl
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
			this._sortUsingComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this._sortPanelOtherLanguage = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this._languageComboBox = new System.Windows.Forms.ComboBox();
			this._sortPanelCustomText = new System.Windows.Forms.Panel();
			this._sortRulesTextBox = new System.Windows.Forms.TextBox();
			this._testSortText = new System.Windows.Forms.TextBox();
			this._testSortButton = new System.Windows.Forms.Button();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this._sortPanelOtherLanguage.SuspendLayout();
			this._sortPanelCustomText.SuspendLayout();
			this.SuspendLayout();
			//
			// _sortUsingComboBox
			//
			this._sortUsingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._sortUsingComboBox.FormattingEnabled = true;
			this._sortUsingComboBox.Location = new System.Drawing.Point(100, 0);
			this._sortUsingComboBox.Name = "_sortUsingComboBox";
			this._sortUsingComboBox.Size = new System.Drawing.Size(290, 21);
			this._sortUsingComboBox.TabIndex = 1;
			this._sortUsingComboBox.SelectedIndexChanged += new System.EventHandler(this._sortUsingComboBox_SelectedIndexChanged);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(91, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Sorting approach:";
			//
			// splitContainer1
			//
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			//
			// splitContainer1.Panel1
			//
			this.splitContainer1.Panel1.Controls.Add(this._sortPanelOtherLanguage);
			this.splitContainer1.Panel1.Controls.Add(this._sortPanelCustomText);
			this.splitContainer1.Panel1.Controls.Add(this._sortUsingComboBox);
			this.splitContainer1.Panel1.Controls.Add(this.label1);
			//
			// splitContainer1.Panel2
			//
			this.splitContainer1.Panel2.Controls.Add(this._testSortText);
			this.splitContainer1.Panel2.Controls.Add(this._testSortButton);
			this.splitContainer1.Size = new System.Drawing.Size(610, 373);
			this.splitContainer1.SplitterDistance = 396;
			this.splitContainer1.TabIndex = 2;
			//
			// _sortPanelOtherLanguage
			//
			this._sortPanelOtherLanguage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._sortPanelOtherLanguage.Controls.Add(this.label2);
			this._sortPanelOtherLanguage.Controls.Add(this._languageComboBox);
			this._sortPanelOtherLanguage.Location = new System.Drawing.Point(0, 34);
			this._sortPanelOtherLanguage.Name = "_sortPanelOtherLanguage";
			this._sortPanelOtherLanguage.Size = new System.Drawing.Size(393, 336);
			this._sortPanelOtherLanguage.TabIndex = 3;
			//
			// label2
			//
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(36, 3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(58, 13);
			this.label2.TabIndex = 0;
			this.label2.Text = "&Language:";
			//
			// _languageComboBox
			//
			this._languageComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._languageComboBox.FormattingEnabled = true;
			this._languageComboBox.Location = new System.Drawing.Point(100, 0);
			this._languageComboBox.Name = "_languageComboBox";
			this._languageComboBox.Size = new System.Drawing.Size(290, 21);
			this._languageComboBox.TabIndex = 1;
			this._languageComboBox.SelectedIndexChanged += new System.EventHandler(this._languageComboBox_SelectedIndexChanged);
			//
			// _sortPanelCustomText
			//
			this._sortPanelCustomText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._sortPanelCustomText.Controls.Add(this._sortRulesTextBox);
			this._sortPanelCustomText.Location = new System.Drawing.Point(3, 34);
			this._sortPanelCustomText.Name = "_sortPanelCustomText";
			this._sortPanelCustomText.Size = new System.Drawing.Size(390, 336);
			this._sortPanelCustomText.TabIndex = 2;
			//
			// _sortRulesTextBox
			//
			this._sortRulesTextBox.AcceptsReturn = true;
			this._sortRulesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._sortRulesTextBox.Location = new System.Drawing.Point(0, 0);
			this._sortRulesTextBox.Multiline = true;
			this._sortRulesTextBox.Name = "_sortRulesTextBox";
			this._sortRulesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._sortRulesTextBox.Size = new System.Drawing.Size(390, 336);
			this._sortRulesTextBox.TabIndex = 0;
			this._sortRulesTextBox.TextChanged += new System.EventHandler(this._sortRulesTextBox_TextChanged);
			//
			// _testSortText
			//
			this._testSortText.AcceptsReturn = true;
			this._testSortText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._testSortText.Location = new System.Drawing.Point(3, 40);
			this._testSortText.Multiline = true;
			this._testSortText.Name = "_testSortText";
			this._testSortText.Size = new System.Drawing.Size(204, 330);
			this._testSortText.TabIndex = 1;
			//
			// _testSortButton
			//
			this._testSortButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._testSortButton.Location = new System.Drawing.Point(44, 0);
			this._testSortButton.Name = "_testSortButton";
			this._testSortButton.Size = new System.Drawing.Size(122, 34);
			this._testSortButton.TabIndex = 0;
			this._testSortButton.Text = "&Test Sort";
			this._testSortButton.UseVisualStyleBackColor = true;
			this._testSortButton.Click += new System.EventHandler(this._testSortButton_Click);
			//
			// WSSortControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "WSSortControl";
			this.Size = new System.Drawing.Size(610, 373);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this._sortPanelOtherLanguage.ResumeLayout(false);
			this._sortPanelOtherLanguage.PerformLayout();
			this._sortPanelCustomText.ResumeLayout(false);
			this._sortPanelCustomText.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox _sortUsingComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TextBox _testSortText;
		private System.Windows.Forms.Button _testSortButton;
		private System.Windows.Forms.Panel _sortPanelCustomText;
		private System.Windows.Forms.TextBox _sortRulesTextBox;
		private System.Windows.Forms.Panel _sortPanelOtherLanguage;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox _languageComboBox;
	}
}
