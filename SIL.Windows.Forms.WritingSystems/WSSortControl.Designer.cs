namespace SIL.Windows.Forms.WritingSystems
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
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this._testSortText = new System.Windows.Forms.TextBox();
            this._testSortResult = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this._testSortButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this._sortUsingComboBox = new System.Windows.Forms.ComboBox();
            this.switcher_panel = new System.Windows.Forms.Panel();
            this._sortrules_panel = new System.Windows.Forms.Panel();
            this._languagecombo_panel = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this._languageComboBox = new System.Windows.Forms.ComboBox();
            this._sortRulesTextBox = new System.Windows.Forms.TextBox();
            this._helpLabel = new System.Windows.Forms.LinkLabel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.switcher_panel.SuspendLayout();
            this._sortrules_panel.SuspendLayout();
            this._languagecombo_panel.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "&Sort:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Te&xt to Sort:";
            // 
            // _testSortText
            // 
            this._testSortText.AcceptsReturn = true;
            this._testSortText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._testSortText.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._testSortText.Location = new System.Drawing.Point(3, 45);
            this._testSortText.Multiline = true;
            this._testSortText.Name = "_testSortText";
            this._testSortText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._testSortText.Size = new System.Drawing.Size(426, 161);
            this._testSortText.TabIndex = 1;
            this._testSortText.Enter += new System.EventHandler(this.TextControl_Enter);
            this._testSortText.Leave += new System.EventHandler(this.TextControl_Leave);
            // 
            // _testSortResult
            // 
            this._testSortResult.BackColor = System.Drawing.SystemColors.Window;
            this._testSortResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this._testSortResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._testSortResult.Location = new System.Drawing.Point(3, 225);
            this._testSortResult.Multiline = true;
            this._testSortResult.Name = "_testSortResult";
            this._testSortResult.ReadOnly = true;
            this._testSortResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._testSortResult.Size = new System.Drawing.Size(426, 161);
            this._testSortResult.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 209);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Sort Result:";
            // 
            // _testSortButton
            // 
            this._testSortButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this._testSortButton.AutoSize = true;
            this._testSortButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._testSortButton.Location = new System.Drawing.Point(186, 3);
            this._testSortButton.Name = "_testSortButton";
            this._testSortButton.Size = new System.Drawing.Size(60, 23);
            this._testSortButton.TabIndex = 0;
            this._testSortButton.Text = "&Test Sort";
            this._testSortButton.UseVisualStyleBackColor = true;
            this._testSortButton.Click += new System.EventHandler(this._testSortButton_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel4, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.switcher_panel, 1, 1);
            this.tableLayoutPanel3.Controls.Add(this._helpLabel, 1, 2);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 3;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Size = new System.Drawing.Size(187, 389);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.AutoSize = true;
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this._sortUsingComboBox, 1, 1);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.Size = new System.Drawing.Size(181, 27);
            this.tableLayoutPanel4.TabIndex = 1;
            // 
            // _sortUsingComboBox
            // 
            this._sortUsingComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._sortUsingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._sortUsingComboBox.FormattingEnabled = true;
            this._sortUsingComboBox.Location = new System.Drawing.Point(38, 3);
            this._sortUsingComboBox.Name = "_sortUsingComboBox";
            this._sortUsingComboBox.Size = new System.Drawing.Size(140, 21);
            this._sortUsingComboBox.TabIndex = 2;
            this._sortUsingComboBox.SelectedIndexChanged += new System.EventHandler(this._sortUsingComboBox_SelectedIndexChanged);
            // 
            // switcher_panel
            // 
            this.switcher_panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.switcher_panel.Controls.Add(this._sortrules_panel);
            this.switcher_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.switcher_panel.Location = new System.Drawing.Point(3, 36);
            this.switcher_panel.Name = "switcher_panel";
            this.switcher_panel.Size = new System.Drawing.Size(181, 337);
            this.switcher_panel.TabIndex = 1;
            // 
            // _sortrules_panel
            // 
            this._sortrules_panel.Controls.Add(this._languagecombo_panel);
            this._sortrules_panel.Controls.Add(this._sortRulesTextBox);
            this._sortrules_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sortrules_panel.Location = new System.Drawing.Point(0, 0);
            this._sortrules_panel.Name = "_sortrules_panel";
            this._sortrules_panel.Size = new System.Drawing.Size(181, 337);
            this._sortrules_panel.TabIndex = 4;
            // 
            // _languagecombo_panel
            // 
            this._languagecombo_panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this._languagecombo_panel.ColumnCount = 1;
            this._languagecombo_panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this._languagecombo_panel.Controls.Add(this.label2, 0, 0);
            this._languagecombo_panel.Controls.Add(this._languageComboBox, 0, 1);
            this._languagecombo_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._languagecombo_panel.Location = new System.Drawing.Point(0, 0);
            this._languagecombo_panel.Name = "_languagecombo_panel";
            this._languagecombo_panel.RowCount = 2;
            this._languagecombo_panel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._languagecombo_panel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this._languagecombo_panel.Size = new System.Drawing.Size(181, 337);
            this._languagecombo_panel.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "&Language:";
            // 
            // _languageComboBox
            // 
            this._languageComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._languageComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._languageComboBox.DropDownWidth = 156;
            this._languageComboBox.FormattingEnabled = true;
            this._languageComboBox.Location = new System.Drawing.Point(3, 16);
            this._languageComboBox.Name = "_languageComboBox";
            this._languageComboBox.Size = new System.Drawing.Size(175, 21);
            this._languageComboBox.TabIndex = 1;
            this._languageComboBox.SelectedIndexChanged += new System.EventHandler(this._languageComboBox_SelectedIndexChanged);
            // 
            // _sortRulesTextBox
            // 
            this._sortRulesTextBox.AcceptsReturn = true;
            this._sortRulesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._sortRulesTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this._sortRulesTextBox.Location = new System.Drawing.Point(0, 0);
            this._sortRulesTextBox.Multiline = true;
            this._sortRulesTextBox.Name = "_sortRulesTextBox";
            this._sortRulesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this._sortRulesTextBox.Size = new System.Drawing.Size(181, 337);
            this._sortRulesTextBox.TabIndex = 0;
            this._sortRulesTextBox.TextChanged += new System.EventHandler(this._sortRulesTextBox_TextChanged);
            this._sortRulesTextBox.Leave += new System.EventHandler(this._sortRulesTextBox_TextChanged);
            // 
            // _helpLabel
            // 
            this._helpLabel.AutoSize = true;
            this._helpLabel.Location = new System.Drawing.Point(3, 376);
            this._helpLabel.Name = "_helpLabel";
            this._helpLabel.Size = new System.Drawing.Size(29, 13);
            this._helpLabel.TabIndex = 3;
            this._helpLabel.TabStop = true;
            this._helpLabel.Text = "Help";
            this._helpLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnHelpLabelClicked);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 1;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Controls.Add(this._testSortResult, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.label4, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this._testSortText, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this._testSortButton, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(196, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 5;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(432, 389);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 2;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel3, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel2, 1, 0);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 1;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(631, 395);
            this.tableLayoutPanel5.TabIndex = 3;
            // 
            // WSSortControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel5);
            this.Name = "WSSortControl";
            this.Size = new System.Drawing.Size(631, 395);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.switcher_panel.ResumeLayout(false);
            this._sortrules_panel.ResumeLayout(false);
            this._sortrules_panel.PerformLayout();
            this._languagecombo_panel.ResumeLayout(false);
            this._languagecombo_panel.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _testSortText;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _testSortResult;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button _testSortButton;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.Panel switcher_panel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TableLayoutPanel _languagecombo_panel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox _languageComboBox;
		private System.Windows.Forms.TextBox _sortRulesTextBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
		private System.Windows.Forms.ComboBox _sortUsingComboBox;
		private System.Windows.Forms.LinkLabel _helpLabel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
		private System.Windows.Forms.Panel _sortrules_panel;
	}
}
