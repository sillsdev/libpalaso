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
			this.components = new System.ComponentModel.Container();
			this._fontComboBox = new System.Windows.Forms.ComboBox();
			this._fontSizeComboBox = new System.Windows.Forms.ComboBox();
			this._fontLabel = new System.Windows.Forms.Label();
			this._sizeLabel = new System.Windows.Forms.Label();
			this._rightToLeftCheckBox = new System.Windows.Forms.CheckBox();
			this._tableLayoutPanelMain = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanelTestArea = new System.Windows.Forms.TableLayoutPanel();
			this._testArea = new System.Windows.Forms.TextBox();
			this._testAreaLabel = new System.Windows.Forms.Label();
			this._fontNotAvailableLabel = new System.Windows.Forms.Label();
			this._promptForFontTestArea = new SIL.Windows.Forms.Widgets.Prompt();
			this._l10NSharpExtender = new L10NSharp.UI.L10NSharpExtender(this.components);
			this._tableLayoutPanelMain.SuspendLayout();
			this.tableLayoutPanelTestArea.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this._l10NSharpExtender)).BeginInit();
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
			this._l10NSharpExtender.SetLocalizableToolTip(this._fontComboBox, null);
			this._l10NSharpExtender.SetLocalizationComment(this._fontComboBox, null);
			this._l10NSharpExtender.SetLocalizationPriority(this._fontComboBox, L10NSharp.LocalizationPriority.NotLocalizable);
			this._l10NSharpExtender.SetLocalizingId(this._fontComboBox, "WSFontControl._fontComboBox");
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
			this._l10NSharpExtender.SetLocalizableToolTip(this._fontSizeComboBox, null);
			this._l10NSharpExtender.SetLocalizationComment(this._fontSizeComboBox, null);
			this._l10NSharpExtender.SetLocalizationPriority(this._fontSizeComboBox, L10NSharp.LocalizationPriority.NotLocalizable);
			this._l10NSharpExtender.SetLocalizingId(this._fontSizeComboBox, "WSFontControl._fontSizeComboBox");
			this._fontSizeComboBox.Location = new System.Drawing.Point(301, 19);
			this._fontSizeComboBox.Name = "_fontSizeComboBox";
			this._fontSizeComboBox.Size = new System.Drawing.Size(174, 143);
			this._fontSizeComboBox.TabIndex = 3;
			this._fontSizeComboBox.TextChanged += new System.EventHandler(this.FontSizeComboBox_TextChanged);
			// 
			// _fontLabel
			// 
			this._fontLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._l10NSharpExtender.SetLocalizableToolTip(this._fontLabel, null);
			this._l10NSharpExtender.SetLocalizationComment(this._fontLabel, null);
			this._l10NSharpExtender.SetLocalizingId(this._fontLabel, "WSFontControl.Font");
			this._fontLabel.Location = new System.Drawing.Point(3, 0);
			this._fontLabel.Name = "_fontLabel";
			this._fontLabel.Size = new System.Drawing.Size(292, 16);
			this._fontLabel.TabIndex = 0;
			this._fontLabel.Text = "&Font:";
			// 
			// _sizeLabel
			// 
			this._sizeLabel.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this._l10NSharpExtender.SetLocalizableToolTip(this._sizeLabel, null);
			this._l10NSharpExtender.SetLocalizationComment(this._sizeLabel, null);
			this._l10NSharpExtender.SetLocalizingId(this._sizeLabel, "WSFontControl.Size");
			this._sizeLabel.Location = new System.Drawing.Point(301, 0);
			this._sizeLabel.Name = "_sizeLabel";
			this._sizeLabel.Size = new System.Drawing.Size(170, 14);
			this._sizeLabel.TabIndex = 2;
			this._sizeLabel.Text = "&Size:";
			// 
			// _rightToLeftCheckBox
			// 
			this._tableLayoutPanelMain.SetColumnSpan(this._rightToLeftCheckBox, 2);
			this._l10NSharpExtender.SetLocalizableToolTip(this._rightToLeftCheckBox, null);
			this._l10NSharpExtender.SetLocalizationComment(this._rightToLeftCheckBox, null);
			this._l10NSharpExtender.SetLocalizingId(this._rightToLeftCheckBox, "WSFontControl.RightToLeftWS");
			this._rightToLeftCheckBox.Location = new System.Drawing.Point(3, 168);
			this._rightToLeftCheckBox.Name = "_rightToLeftCheckBox";
			this._rightToLeftCheckBox.Size = new System.Drawing.Size(472, 24);
			this._rightToLeftCheckBox.TabIndex = 3;
			this._rightToLeftCheckBox.Text = "This is a &right to left writing system.";
			this._rightToLeftCheckBox.UseVisualStyleBackColor = false;
			this._rightToLeftCheckBox.CheckedChanged += new System.EventHandler(this.RightToLeftCheckBox_CheckedChanged);
			// 
			// _tableLayoutPanelMain
			// 
			this._tableLayoutPanelMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._tableLayoutPanelMain.ColumnCount = 2;
			this._tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 298F));
			this._tableLayoutPanelMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._tableLayoutPanelMain.Controls.Add(this._rightToLeftCheckBox, 0, 2);
			this._tableLayoutPanelMain.Controls.Add(this._fontSizeComboBox, 1, 1);
			this._tableLayoutPanelMain.Controls.Add(this._fontComboBox, 0, 1);
			this._tableLayoutPanelMain.Controls.Add(this._fontLabel, 0, 0);
			this._tableLayoutPanelMain.Controls.Add(this._sizeLabel, 1, 0);
			this._tableLayoutPanelMain.Controls.Add(this.tableLayoutPanelTestArea, 0, 3);
			this._tableLayoutPanelMain.Location = new System.Drawing.Point(0, 0);
			this._tableLayoutPanelMain.Name = "_tableLayoutPanelMain";
			this._tableLayoutPanelMain.RowCount = 4;
			this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanelMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._tableLayoutPanelMain.Size = new System.Drawing.Size(478, 303);
			this._tableLayoutPanelMain.TabIndex = 0;
			// 
			// tableLayoutPanelTestArea
			// 
			this.tableLayoutPanelTestArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanelTestArea.AutoSize = true;
			this.tableLayoutPanelTestArea.ColumnCount = 2;
			this._tableLayoutPanelMain.SetColumnSpan(this.tableLayoutPanelTestArea, 2);
			this.tableLayoutPanelTestArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanelTestArea.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelTestArea.Controls.Add(this._testAreaLabel, 0, 0);
			this.tableLayoutPanelTestArea.Controls.Add(this._fontNotAvailableLabel, 1, 0);
			this.tableLayoutPanelTestArea.Controls.Add(this._testArea, 0, 1);
			this.tableLayoutPanelTestArea.Location = new System.Drawing.Point(3, 198);
			this.tableLayoutPanelTestArea.Name = "tableLayoutPanelTestArea";
			this.tableLayoutPanelTestArea.RowCount = 2;
			this.tableLayoutPanelTestArea.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanelTestArea.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanelTestArea.Size = new System.Drawing.Size(472, 102);
			this.tableLayoutPanelTestArea.TabIndex = 4;
			// 
			// _testArea
			// 
			this._testArea.AcceptsReturn = true;
			this._testArea.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanelTestArea.SetColumnSpan(this._testArea, 2);
			this._l10NSharpExtender.SetLocalizableToolTip(this._testArea, null);
			this._l10NSharpExtender.SetLocalizationComment(this._testArea, null);
			this._l10NSharpExtender.SetLocalizationPriority(this._testArea, L10NSharp.LocalizationPriority.NotLocalizable);
			this._l10NSharpExtender.SetLocalizingId(this._testArea, "WSFontControl._testArea");
			this._testArea.Location = new System.Drawing.Point(3, 16);
			this._testArea.Multiline = true;
			this._testArea.Name = "_testArea";
			this._testArea.Size = new System.Drawing.Size(466, 83);
			this._testArea.TabIndex = 5;
			this._testArea.Enter += new System.EventHandler(this._testArea_Enter);
			this._testArea.Leave += new System.EventHandler(this._testArea_Leave);
			// 
			// _testAreaLabel
			// 
			this._testAreaLabel.AutoSize = true;
			this._l10NSharpExtender.SetLocalizableToolTip(this._testAreaLabel, null);
			this._l10NSharpExtender.SetLocalizationComment(this._testAreaLabel, null);
			this._l10NSharpExtender.SetLocalizingId(this._testAreaLabel, "WSFontControl.TestArea");
			this._testAreaLabel.Location = new System.Drawing.Point(3, 0);
			this._testAreaLabel.Name = "_testAreaLabel";
			this._testAreaLabel.Size = new System.Drawing.Size(56, 13);
			this._testAreaLabel.TabIndex = 0;
			this._testAreaLabel.Text = "&Test Area:";
			// 
			// _fontNotAvailableLabel
			// 
			this._fontNotAvailableLabel.AutoSize = true;
			this._fontNotAvailableLabel.ForeColor = System.Drawing.Color.Red;
			this._l10NSharpExtender.SetLocalizableToolTip(this._fontNotAvailableLabel, null);
			this._l10NSharpExtender.SetLocalizationComment(this._fontNotAvailableLabel, null);
			this._l10NSharpExtender.SetLocalizingId(this._fontNotAvailableLabel, "WSFontControl.FontNotAvailable");
			this._fontNotAvailableLabel.Location = new System.Drawing.Point(65, 0);
			this._fontNotAvailableLabel.Name = "_fontNotAvailableLabel";
			this._fontNotAvailableLabel.Size = new System.Drawing.Size(317, 13);
			this._fontNotAvailableLabel.TabIndex = 1;
			this._fontNotAvailableLabel.Text = "(The selected font is not available on this machine. Using default.)";
			this._fontNotAvailableLabel.Visible = false;
			// 
			// _l10NSharpExtender
			// 
			this._l10NSharpExtender.LocalizationManagerId = "Palaso";
			this._l10NSharpExtender.PrefixForNewItems = "WSFontControl";
			// 
			// WSFontControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this._tableLayoutPanelMain);
			this._l10NSharpExtender.SetLocalizableToolTip(this, null);
			this._l10NSharpExtender.SetLocalizationComment(this, null);
			this._l10NSharpExtender.SetLocalizationPriority(this, L10NSharp.LocalizationPriority.NotLocalizable);
			this._l10NSharpExtender.SetLocalizingId(this, "WSFontControl.WSFontControl");
			this.Name = "WSFontControl";
			this.Size = new System.Drawing.Size(478, 303);
			this._tableLayoutPanelMain.ResumeLayout(false);
			this._tableLayoutPanelMain.PerformLayout();
			this.tableLayoutPanelTestArea.ResumeLayout(false);
			this.tableLayoutPanelTestArea.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this._l10NSharpExtender)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox _fontComboBox;
		private System.Windows.Forms.ComboBox _fontSizeComboBox;
		private System.Windows.Forms.Label _fontLabel;
		private System.Windows.Forms.Label _sizeLabel;
		private System.Windows.Forms.CheckBox _rightToLeftCheckBox;
		private Prompt _promptForFontTestArea;
		private System.Windows.Forms.TableLayoutPanel _tableLayoutPanelMain;
		private L10NSharp.UI.L10NSharpExtender _l10NSharpExtender;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanelTestArea;
		private System.Windows.Forms.Label _testAreaLabel;
		private System.Windows.Forms.Label _fontNotAvailableLabel;
		private System.Windows.Forms.TextBox _testArea;
	}
}
