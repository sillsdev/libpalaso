using SIL.Windows.Forms.Widgets;

namespace SIL.Windows.Forms.WritingSystems
{
	partial class WSFontControl
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
			this._fontComboBox = new System.Windows.Forms.ComboBox();
			this._fontSizeComboBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this._testArea = new System.Windows.Forms.TextBox();
			this._rightToLeftCheckBox = new System.Windows.Forms.CheckBox();
			this._tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this._fontNotAvailableLabel = new System.Windows.Forms.Label();
			this._promptForFontTestArea = new SIL.Windows.Forms.Widgets.Prompt();
			this._tableLayoutPanel.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// _fontComboBox
			// 
			this._fontComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._fontComboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
			this._fontComboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this._fontComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
			this._fontComboBox.FormattingEnabled = true;
			this._fontComboBox.IntegralHeight = false;
			this._fontComboBox.Location = new System.Drawing.Point(3, 19);
			this._fontComboBox.Name = "_fontComboBox";
			this._fontComboBox.Size = new System.Drawing.Size(292, 143);
			this._fontComboBox.TabIndex = 1;
			this._fontComboBox.TextChanged += new System.EventHandler(this.FontComboBox_TextChanged);
			// 
			// _fontSizeComboBox
			// 
			this._fontSizeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._fontSizeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
			this._fontSizeComboBox.FormatString = "N2";
			this._fontSizeComboBox.FormattingEnabled = true;
			this._fontSizeComboBox.IntegralHeight = false;
			this._fontSizeComboBox.Items.AddRange(new object[] {
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24"});
			this._fontSizeComboBox.Location = new System.Drawing.Point(301, 19);
			this._fontSizeComboBox.Name = "_fontSizeComboBox";
			this._fontSizeComboBox.Size = new System.Drawing.Size(174, 143);
			this._fontSizeComboBox.TabIndex = 2;
			this._fontSizeComboBox.TextChanged += new System.EventHandler(this.FontSizeComboBox_TextChanged);
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(292, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Font:";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(301, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(170, 14);
			this.label2.TabIndex = 0;
			this.label2.Text = "&Size:";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(56, 13);
			this.label3.TabIndex = 1;
			this.label3.Text = "&Test Area:";
			// 
			// _testArea
			// 
			this._testArea.AcceptsReturn = true;
			this._testArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanel.SetColumnSpan(this._testArea, 2);
			this._testArea.Location = new System.Drawing.Point(3, 218);
			this._testArea.Multiline = true;
			this._testArea.Name = "_testArea";
			this._testArea.Size = new System.Drawing.Size(472, 82);
			this._testArea.TabIndex = 4;
			this._testArea.Enter += new System.EventHandler(this._testArea_Enter);
			this._testArea.Leave += new System.EventHandler(this._testArea_Leave);
			// 
			// _rightToLeftCheckBox
			// 
			this._tableLayoutPanel.SetColumnSpan(this._rightToLeftCheckBox, 2);
			this._rightToLeftCheckBox.Location = new System.Drawing.Point(3, 168);
			this._rightToLeftCheckBox.Name = "_rightToLeftCheckBox";
			this._rightToLeftCheckBox.Size = new System.Drawing.Size(472, 24);
			this._rightToLeftCheckBox.TabIndex = 3;
			this._rightToLeftCheckBox.Text = "This is a &right to left writing system.";
			this._rightToLeftCheckBox.UseVisualStyleBackColor = false;
			this._rightToLeftCheckBox.CheckedChanged += new System.EventHandler(this.RightToLeftCheckBox_CheckedChanged);
			// 
			// _tableLayoutPanel
			// 
			this._tableLayoutPanel.ColumnCount = 2;
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 298F));
			this._tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanel.Controls.Add(this._testArea, 0, 5);
			this._tableLayoutPanel.Controls.Add(this._rightToLeftCheckBox, 0, 2);
			this._tableLayoutPanel.Controls.Add(this._fontSizeComboBox, 1, 1);
			this._tableLayoutPanel.Controls.Add(this._fontComboBox, 0, 1);
			this._tableLayoutPanel.Controls.Add(this.label1, 0, 0);
			this._tableLayoutPanel.Controls.Add(this.label2, 1, 0);
			this._tableLayoutPanel.Controls.Add(this.tableLayoutPanel1, 0, 4);
			this._tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this._tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
			this._tableLayoutPanel.Name = "_tableLayoutPanel";
			this._tableLayoutPanel.RowCount = 6;
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
			this._tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanel.Size = new System.Drawing.Size(478, 303);
			this._tableLayoutPanel.TabIndex = 7;
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 2;
			this._tableLayoutPanel.SetColumnSpan(this.tableLayoutPanel1, 2);
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this._fontNotAvailableLabel, 1, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 198);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(472, 14);
			this.tableLayoutPanel1.TabIndex = 3;
			// 
			// _fontNotAvailableLabel
			// 
			this._fontNotAvailableLabel.AutoSize = true;
			this._fontNotAvailableLabel.ForeColor = System.Drawing.Color.Red;
			this._fontNotAvailableLabel.Location = new System.Drawing.Point(65, 0);
			this._fontNotAvailableLabel.Name = "_fontNotAvailableLabel";
			this._fontNotAvailableLabel.Size = new System.Drawing.Size(317, 13);
			this._fontNotAvailableLabel.TabIndex = 2;
			this._fontNotAvailableLabel.Text = "(The selected font is not available on this machine. Using default.)";
			this._fontNotAvailableLabel.Visible = false;
			// 
			// WSFontControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._tableLayoutPanel);
			this.Name = "WSFontControl";
			this.Size = new System.Drawing.Size(478, 303);
			this._tableLayoutPanel.ResumeLayout(false);
			this._tableLayoutPanel.PerformLayout();
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox _fontComboBox;
		private System.Windows.Forms.ComboBox _fontSizeComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _testArea;
		private System.Windows.Forms.CheckBox _rightToLeftCheckBox;
		private Prompt _promptForFontTestArea;
		private System.Windows.Forms.TableLayoutPanel _tableLayoutPanel;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label _fontNotAvailableLabel;
	}
}
