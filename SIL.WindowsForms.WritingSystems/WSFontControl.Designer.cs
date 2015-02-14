using SIL.WindowsForms.Widgets;

namespace SIL.WindowsForms.WritingSystems
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.splitContainer2 = new System.Windows.Forms.SplitContainer();
			this._rightToLeftCheckBox = new System.Windows.Forms.CheckBox();
			this._promptForFontTestArea = new Prompt();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
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
			this._fontComboBox.Location = new System.Drawing.Point(0, 16);
			this._fontComboBox.Name = "_fontComboBox";
			this._fontComboBox.Size = new System.Drawing.Size(299, 143);
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
			this._fontSizeComboBox.Location = new System.Drawing.Point(0, 16);
			this._fontSizeComboBox.Name = "_fontSizeComboBox";
			this._fontSizeComboBox.Size = new System.Drawing.Size(173, 143);
			this._fontSizeComboBox.TabIndex = 1;
			this._fontSizeComboBox.TextChanged += new System.EventHandler(this.FontSizeComboBox_TextChanged);
			//
			// label1
			//
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(3, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(302, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "&Font:";
			//
			// label2
			//
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label2.Location = new System.Drawing.Point(3, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(170, 14);
			this.label2.TabIndex = 0;
			this.label2.Text = "&Size:";
			//
			// label3
			//
			this.label3.Location = new System.Drawing.Point(3, 30);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(478, 14);
			this.label3.TabIndex = 1;
			this.label3.Text = "&Test Area:";
			//
			// _testArea
			//
			this._testArea.AcceptsReturn = true;
			this._testArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this._testArea.Location = new System.Drawing.Point(0, 46);
			this._testArea.Multiline = true;
			this._testArea.Name = "_testArea";
			this._testArea.Size = new System.Drawing.Size(478, 95);
			this._testArea.TabIndex = 2;
			this._testArea.Enter += new System.EventHandler(this._testArea_Enter);
			this._testArea.Leave += new System.EventHandler(this._testArea_Leave);
			//
			// splitContainer1
			//
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.IsSplitterFixed = true;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			//
			// splitContainer1.Panel1
			//
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
			//
			// splitContainer1.Panel2
			//
			this.splitContainer1.Panel2.Controls.Add(this._rightToLeftCheckBox);
			this.splitContainer1.Panel2.Controls.Add(this._testArea);
			this.splitContainer1.Panel2.Controls.Add(this.label3);
			this.splitContainer1.Size = new System.Drawing.Size(478, 303);
			this.splitContainer1.SplitterDistance = 158;
			this.splitContainer1.TabIndex = 6;
			this.splitContainer1.TabStop = false;
			//
			// splitContainer2
			//
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.IsSplitterFixed = true;
			this.splitContainer2.Location = new System.Drawing.Point(0, 0);
			this.splitContainer2.Name = "splitContainer2";
			//
			// splitContainer2.Panel1
			//
			this.splitContainer2.Panel1.Controls.Add(this.label1);
			this.splitContainer2.Panel1.Controls.Add(this._fontComboBox);
			//
			// splitContainer2.Panel2
			//
			this.splitContainer2.Panel2.Controls.Add(this._fontSizeComboBox);
			this.splitContainer2.Panel2.Controls.Add(this.label2);
			this.splitContainer2.Size = new System.Drawing.Size(478, 158);
			this.splitContainer2.SplitterDistance = 298;
			this.splitContainer2.TabIndex = 0;
			this.splitContainer2.TabStop = false;
			//
			// _rightToLeftCheckBox
			//
			this._rightToLeftCheckBox.Location = new System.Drawing.Point(3, 3);
			this._rightToLeftCheckBox.Name = "_rightToLeftCheckBox";
			this._rightToLeftCheckBox.Size = new System.Drawing.Size(472, 24);
			this._rightToLeftCheckBox.TabIndex = 0;
			this._rightToLeftCheckBox.Text = "This is a &right to left writing system.";
			this._rightToLeftCheckBox.UseVisualStyleBackColor = false;
			this._rightToLeftCheckBox.CheckedChanged += new System.EventHandler(this.RightToLeftCheckBox_CheckedChanged);
			//
			// WSFontControl
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.splitContainer1);
			this.Name = "WSFontControl";
			this.Size = new System.Drawing.Size(478, 303);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox _fontComboBox;
		private System.Windows.Forms.ComboBox _fontSizeComboBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox _testArea;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.SplitContainer splitContainer2;
		private System.Windows.Forms.CheckBox _rightToLeftCheckBox;
		private Prompt _promptForFontTestArea;
	}
}
