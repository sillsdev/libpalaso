namespace ArchivingTestApp
{
	partial class MainForm
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			m_btnIMDI = new Button();
			m_tableLayoutPanelMain = new TableLayoutPanel();
			m_btnRamp = new Button();
			m_lblTitle = new SIL.Windows.Forms.Widgets.BetterLabel();
			m_txtTitle = new TextBox();
			m_btnAddFiles = new Button();
			m_listFiles = new ListView();
			m_tableLayoutPanelMain.SuspendLayout();
			SuspendLayout();
			// 
			// m_btnIMDI
			// 
			m_btnIMDI.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			m_btnIMDI.AutoSize = true;
			m_btnIMDI.Enabled = false;
			m_btnIMDI.Location = new Point(812, 469);
			m_btnIMDI.Margin = new Padding(4, 3, 4, 3);
			m_btnIMDI.Name = "m_btnIMDI";
			m_btnIMDI.Size = new Size(99, 29);
			m_btnIMDI.TabIndex = 0;
			m_btnIMDI.Text = "IMDI Archive";
			m_btnIMDI.UseVisualStyleBackColor = true;
			// 
			// m_tableLayoutPanelMain
			// 
			m_tableLayoutPanelMain.ColumnCount = 3;
			m_tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle());
			m_tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
			m_tableLayoutPanelMain.ColumnStyles.Add(new ColumnStyle());
			m_tableLayoutPanelMain.Controls.Add(m_btnIMDI, 2, 2);
			m_tableLayoutPanelMain.Controls.Add(m_btnRamp, 1, 2);
			m_tableLayoutPanelMain.Controls.Add(m_lblTitle, 0, 0);
			m_tableLayoutPanelMain.Controls.Add(m_txtTitle, 1, 0);
			m_tableLayoutPanelMain.Controls.Add(m_btnAddFiles, 0, 1);
			m_tableLayoutPanelMain.Controls.Add(m_listFiles, 1, 1);
			m_tableLayoutPanelMain.Dock = DockStyle.Fill;
			m_tableLayoutPanelMain.Location = new Point(9, 9);
			m_tableLayoutPanelMain.Margin = new Padding(4, 3, 4, 3);
			m_tableLayoutPanelMain.Name = "m_tableLayoutPanelMain";
			m_tableLayoutPanelMain.RowCount = 3;
			m_tableLayoutPanelMain.RowStyles.Add(new RowStyle());
			m_tableLayoutPanelMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
			m_tableLayoutPanelMain.RowStyles.Add(new RowStyle());
			m_tableLayoutPanelMain.Size = new Size(915, 501);
			m_tableLayoutPanelMain.TabIndex = 1;
			// 
			// m_btnRamp
			// 
			m_btnRamp.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			m_btnRamp.AutoSize = true;
			m_btnRamp.Enabled = false;
			m_btnRamp.Location = new Point(702, 471);
			m_btnRamp.Margin = new Padding(4, 3, 4, 3);
			m_btnRamp.Name = "m_btnRamp";
			m_btnRamp.Padding = new Padding(0, 0, 9, 0);
			m_btnRamp.Size = new Size(102, 27);
			m_btnRamp.TabIndex = 1;
			m_btnRamp.Text = "RAMP Archive";
			m_btnRamp.UseVisualStyleBackColor = true;
			m_btnRamp.Click += m_btnRamp_Click;
			// 
			// m_lblTitle
			// 
			m_lblTitle.BorderStyle = BorderStyle.None;
			m_lblTitle.Enabled = false;
			m_lblTitle.ForeColor = SystemColors.ControlText;
			m_lblTitle.IsTextSelectable = false;
			m_lblTitle.Location = new Point(4, 3);
			m_lblTitle.Margin = new Padding(4, 3, 4, 3);
			m_lblTitle.Multiline = true;
			m_lblTitle.Name = "m_lblTitle";
			m_lblTitle.ReadOnly = true;
			m_lblTitle.Size = new Size(117, 15);
			m_lblTitle.TabIndex = 2;
			m_lblTitle.TabStop = false;
			m_lblTitle.Text = "Title of Submission:";
			// 
			// m_txtTitle
			// 
			m_txtTitle.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			m_txtTitle.Location = new Point(129, 3);
			m_txtTitle.Margin = new Padding(4, 3, 4, 3);
			m_txtTitle.Name = "m_txtTitle";
			m_txtTitle.Size = new Size(675, 23);
			m_txtTitle.TabIndex = 3;
			// 
			// m_btnAddFiles
			// 
			m_btnAddFiles.Anchor = AnchorStyles.Top;
			m_btnAddFiles.AutoSize = true;
			m_btnAddFiles.Location = new Point(25, 32);
			m_btnAddFiles.Name = "m_btnAddFiles";
			m_btnAddFiles.Size = new Size(75, 25);
			m_btnAddFiles.TabIndex = 4;
			m_btnAddFiles.Text = "Add Files";
			m_btnAddFiles.UseVisualStyleBackColor = true;
			m_btnAddFiles.Click += HandleAddFilesClick;
			// 
			// m_listFiles
			// 
			m_listFiles.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
			m_listFiles.HeaderStyle = ColumnHeaderStyle.Nonclickable;
			m_listFiles.HideSelection = true;
			m_listFiles.LabelWrap = false;
			m_listFiles.Location = new Point(128, 32);
			m_listFiles.MultiSelect = false;
			m_listFiles.Name = "m_listFiles";
			m_listFiles.Size = new Size(677, 431);
			m_listFiles.TabIndex = 5;
			m_listFiles.UseCompatibleStateImageBehavior = false;
			m_listFiles.View = View.List;
			// 
			// MainForm
			// 
			AutoScaleDimensions = new SizeF(7F, 15F);
			AutoScaleMode = AutoScaleMode.Font;
			ClientSize = new Size(933, 519);
			Controls.Add(m_tableLayoutPanelMain);
			Margin = new Padding(4, 3, 4, 3);
			Name = "MainForm";
			Padding = new Padding(9);
			Text = "Archiving Dialog Test app";
			m_tableLayoutPanelMain.ResumeLayout(false);
			m_tableLayoutPanelMain.PerformLayout();
			ResumeLayout(false);
		}

		#endregion

		private Button m_btnIMDI;
		private TableLayoutPanel m_tableLayoutPanelMain;
		private Button m_btnRamp;
		private SIL.Windows.Forms.Widgets.BetterLabel m_lblTitle;
		private TextBox m_txtTitle;
		private Button m_btnAddFiles;
		private ListView m_listFiles;
	}
}
