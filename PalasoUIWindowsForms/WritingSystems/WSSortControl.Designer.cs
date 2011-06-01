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
			this.components = new System.ComponentModel.Container();
			this._sortUsingComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this._testSortText = new System.Windows.Forms.TextBox();
			this._testSortResult = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this._rulesValidationTimer = new System.Windows.Forms.Timer(this.components);
			this._testSortButton = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
			this.switcher_panel = new System.Windows.Forms.Panel();
			this._languagecombo_panel = new System.Windows.Forms.TableLayoutPanel();
			this.label2 = new System.Windows.Forms.Label();
			this._languageComboBox = new System.Windows.Forms.ComboBox();
			this._sortrules_panel = new System.Windows.Forms.Panel();
			this._sortRulesTextBox = new System.Windows.Forms.TextBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.tableLayoutPanel3.SuspendLayout();
			this.switcher_panel.SuspendLayout();
			this._languagecombo_panel.SuspendLayout();
			this._sortrules_panel.SuspendLayout();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			//
			// _sortUsingComboBox
			//
			this._sortUsingComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this._sortUsingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._sortUsingComboBox.FormattingEnabled = true;
			this._sortUsingComboBox.Location = new System.Drawing.Point(38, 3);
			this._sortUsingComboBox.Name = "_sortUsingComboBox";
			this._sortUsingComboBox.Size = new System.Drawing.Size(195, 21);
			this._sortUsingComboBox.TabIndex = 1;
			this._sortUsingComboBox.SelectedIndexChanged += new System.EventHandler(this._sortUsingComboBox_SelectedIndexChanged);
			//
			// label1
			//
			this.label1.AutoSize = true;
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(29, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Sort:";
			this.label1.Click += new System.EventHandler(this.label1_Click);
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
			this._testSortText.Size = new System.Drawing.Size(250, 158);
			this._testSortText.TabIndex = 1;
			this._testSortText.Enter += new System.EventHandler(this.TextControl_Enter);
			this._testSortText.Leave += new System.EventHandler(this.TextControl_Leave);
			//
			// _testSortResult
			//
			this._testSortResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._testSortResult.BackColor = System.Drawing.SystemColors.Window;
			this._testSortResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._testSortResult.Location = new System.Drawing.Point(3, 222);
			this._testSortResult.Multiline = true;
			this._testSortResult.Name = "_testSortResult";
			this._testSortResult.ReadOnly = true;
			this._testSortResult.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this._testSortResult.Size = new System.Drawing.Size(250, 159);
			this._testSortResult.TabIndex = 1;
			//
			// label4
			//
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 206);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(62, 13);
			this.label4.TabIndex = 0;
			this.label4.Text = "Sort Result:";
			//
			// _rulesValidationTimer
			//
			this._rulesValidationTimer.Interval = 500;
			this._rulesValidationTimer.Tick += new System.EventHandler(this._rulesValidationTimer_Tick);
			//
			// _testSortButton
			//
			this._testSortButton.AutoSize = true;
			this._testSortButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._testSortButton.Dock = System.Windows.Forms.DockStyle.Top;
			this._testSortButton.Location = new System.Drawing.Point(3, 3);
			this._testSortButton.Name = "_testSortButton";
			this._testSortButton.Size = new System.Drawing.Size(250, 23);
			this._testSortButton.TabIndex = 0;
			this._testSortButton.Text = "&Test Sort";
			this._testSortButton.UseVisualStyleBackColor = true;
			this._testSortButton.Click += new System.EventHandler(this._testSortButton_Click);
			//
			// splitContainer1
			//
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			//
			// splitContainer1.Panel1
			//
			this.splitContainer1.Panel1.Controls.Add(this.tableLayoutPanel1);
			//
			// splitContainer1.Panel2
			//
			this.splitContainer1.Panel2.Controls.Add(this.tableLayoutPanel2);
			this.splitContainer1.Size = new System.Drawing.Size(502, 384);
			this.splitContainer1.SplitterDistance = 242;
			this.splitContainer1.TabIndex = 2;
			//
			// tableLayoutPanel1
			//
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.switcher_panel, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 2;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15.73604F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 84.26396F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(242, 384);
			this.tableLayoutPanel1.TabIndex = 0;
			//
			// tableLayoutPanel3
			//
			this.tableLayoutPanel3.ColumnCount = 2;
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Controls.Add(this._sortUsingComboBox, 1, 0);
			this.tableLayoutPanel3.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
			this.tableLayoutPanel3.Name = "tableLayoutPanel3";
			this.tableLayoutPanel3.RowCount = 1;
			this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel3.Size = new System.Drawing.Size(236, 54);
			this.tableLayoutPanel3.TabIndex = 0;
			//
			// switcher_panel
			//
			this.switcher_panel.Controls.Add(this._languagecombo_panel);
			this.switcher_panel.Controls.Add(this._sortrules_panel);
			this.switcher_panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.switcher_panel.Location = new System.Drawing.Point(3, 63);
			this.switcher_panel.Name = "switcher_panel";
			this.switcher_panel.Size = new System.Drawing.Size(236, 318);
			this.switcher_panel.TabIndex = 1;
			//
			// _languagecombo_panel
			//
			this._languagecombo_panel.AutoSize = true;
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
			this._languagecombo_panel.Size = new System.Drawing.Size(236, 318);
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
			this._languageComboBox.Size = new System.Drawing.Size(230, 21);
			this._languageComboBox.TabIndex = 1;
			//
			// _sortrules_panel
			//
			this._sortrules_panel.Controls.Add(this._sortRulesTextBox);
			this._sortrules_panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._sortrules_panel.Location = new System.Drawing.Point(0, 0);
			this._sortrules_panel.Name = "_sortrules_panel";
			this._sortrules_panel.Size = new System.Drawing.Size(236, 318);
			this._sortrules_panel.TabIndex = 3;
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
			this._sortRulesTextBox.Size = new System.Drawing.Size(236, 318);
			this._sortRulesTextBox.TabIndex = 0;
			this._sortRulesTextBox.Leave += new System.EventHandler(this._sortRulesTextBox_TextChanged);
			//
			// tableLayoutPanel2
			//
			this.tableLayoutPanel2.AutoSize = true;
			this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel2.ColumnCount = 1;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Controls.Add(this._testSortButton, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this._testSortResult, 0, 4);
			this.tableLayoutPanel2.Controls.Add(this.label4, 0, 3);
			this.tableLayoutPanel2.Controls.Add(this.label3, 0, 1);
			this.tableLayoutPanel2.Controls.Add(this._testSortText, 0, 2);
			this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 5;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(256, 384);
			this.tableLayoutPanel2.TabIndex = 0;
			//
			// WSSortControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "WSSortControl";
			this.Size = new System.Drawing.Size(502, 384);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel3.ResumeLayout(false);
			this.tableLayoutPanel3.PerformLayout();
			this.switcher_panel.ResumeLayout(false);
			this.switcher_panel.PerformLayout();
			this._languagecombo_panel.ResumeLayout(false);
			this._languagecombo_panel.PerformLayout();
			this._sortrules_panel.ResumeLayout(false);
			this._sortrules_panel.PerformLayout();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.tableLayoutPanel2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox _sortUsingComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox _testSortText;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _testSortResult;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Timer _rulesValidationTimer;
		private System.Windows.Forms.Button _testSortButton;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
		private System.Windows.Forms.Panel switcher_panel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.Panel _sortrules_panel;
		private System.Windows.Forms.TableLayoutPanel _languagecombo_panel;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox _languageComboBox;
		private System.Windows.Forms.TextBox _sortRulesTextBox;
	}
}
