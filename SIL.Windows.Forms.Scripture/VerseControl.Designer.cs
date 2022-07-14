namespace SIL.Windows.Forms.Scripture
{
	sealed partial class VerseControl
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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.uiVerseSpinner = new SIL.Windows.Forms.Widgets.HorizontalSpinner();
			this.uiToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.uiChapter = new VCEnterTextBox();
			this.uiChapter.CopyEvent += HandleCopy;
			this.uiChapter.PasteEvent += HandlePaste;
			this.uiChapter.PopUpEvent += HandlePopUpContextMenu;
			this.uiChapter.CollapseEvent += HandleCollapseContextMenu;
			this.uiVerse = new VCEnterTextBox();
			this.uiVerse.CopyEvent += HandleCopy;
			this.uiVerse.PasteEvent += HandlePaste;
			this.uiVerse.PopUpEvent += HandlePopUpContextMenu;
			this.uiVerse.CollapseEvent += HandleCollapseContextMenu;
			this.uiChapterSpinner = new SIL.Windows.Forms.Widgets.HorizontalSpinner();
			this.uiBook = new VCSafeComboBox();
			this.uiBook.CopyEvent += HandleCopy;
			this.uiBook.PasteEvent += HandlePaste;
			this.uiBook.PopUpEvent += HandlePopUpContextMenu;
			this.uiBook.CollapseEvent += HandleCollapseContextMenu;
			this.tableLayoutPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoSize = true;
			this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.tableLayoutPanel1.BackColor = System.Drawing.SystemColors.Control;
			this.tableLayoutPanel1.ColumnCount = 5;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.uiVerseSpinner, 4, 0);
			this.tableLayoutPanel1.Controls.Add(this.uiChapter, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.uiVerse, 3, 0);
			this.tableLayoutPanel1.Controls.Add(this.uiChapterSpinner, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.uiBook, 0, 0);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 1;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this.tableLayoutPanel1.Size = new System.Drawing.Size(191, 22);
			this.tableLayoutPanel1.TabIndex = 10;
			// 
			// uiVerseSpinner
			// 
			this.uiVerseSpinner.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.uiVerseSpinner.Location = new System.Drawing.Point(166, 0);
			this.uiVerseSpinner.Margin = new System.Windows.Forms.Padding(0);
			this.uiVerseSpinner.Name = "uiVerseSpinner";
			this.uiVerseSpinner.Size = new System.Drawing.Size(25, 22);
			this.uiVerseSpinner.TabIndex = 9;
			this.uiVerseSpinner.TabStop = false;
			this.uiVerseSpinner.Text = "horizontalSpinner1";
			this.uiVerseSpinner.Incremented += new System.EventHandler(this.uiVerseNext_Click);
			this.uiVerseSpinner.Decremented += new System.EventHandler(this.uiVersePrev_Click);
			// 
			// uiChapter
			// 
			this.uiChapter.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.uiChapter.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.uiChapter.Location = new System.Drawing.Point(61, 0);
			this.uiChapter.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.uiChapter.Name = "uiChapter";
			this.uiChapter.Size = new System.Drawing.Size(36, 21);
			this.uiChapter.TabIndex = 6;
			this.uiChapter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.uiChapter_KeyDown);
			this.uiChapter.MouseDown += new System.Windows.Forms.MouseEventHandler(this.uiChapter_MouseDown);
			this.uiChapter.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.uiChapter_KeyPress);
			this.uiChapter.Enter += new System.EventHandler(this.uiChapter_Enter);
			// 
			// uiVerse
			// 
			this.uiVerse.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.uiVerse.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.uiVerse.Location = new System.Drawing.Point(130, 0);
			this.uiVerse.Margin = new System.Windows.Forms.Padding(8, 0, 0, 0);
			this.uiVerse.Name = "uiVerse";
			this.uiVerse.Size = new System.Drawing.Size(36, 21);
			this.uiVerse.TabIndex = 7;
			this.uiVerse.KeyDown += new System.Windows.Forms.KeyEventHandler(this.uiVerse_KeyDown);
			this.uiVerse.MouseDown += new System.Windows.Forms.MouseEventHandler(this.uiVerse_MouseDown);
			this.uiVerse.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.uiVerse_KeyPress);
			this.uiVerse.Enter += new System.EventHandler(this.uiVerse_Enter);
			// 
			// uiChapterSpinner
			// 
			this.uiChapterSpinner.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.uiChapterSpinner.Location = new System.Drawing.Point(97, 0);
			this.uiChapterSpinner.Margin = new System.Windows.Forms.Padding(0);
			this.uiChapterSpinner.Name = "uiChapterSpinner";
			this.uiChapterSpinner.Size = new System.Drawing.Size(25, 22);
			this.uiChapterSpinner.TabIndex = 8;
			this.uiChapterSpinner.TabStop = false;
			this.uiChapterSpinner.Text = "horizontalSpinner1";
			this.uiChapterSpinner.Incremented += new System.EventHandler(this.uiChapterNext_Click);
			this.uiChapterSpinner.Decremented += new System.EventHandler(this.uiChapterPrev_Click);
			// 
			// uiBook
			// 
			this.uiBook.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.uiBook.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this.uiBook.DropDownHeight = 400;
			this.uiBook.DropDownWidth = 200;
			this.uiBook.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.uiBook.IntegralHeight = false;
			this.uiBook.Location = new System.Drawing.Point(0, 0);
			this.uiBook.Margin = new System.Windows.Forms.Padding(0);
			this.uiBook.MaxLength = 3;
			this.uiBook.Name = "uiBook";
			this.uiBook.Size = new System.Drawing.Size(53, 22);
			this.uiBook.TabIndex = 5;
			this.uiBook.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.uiBook_DrawItem);
			this.uiBook.SelectionChangeCommitted += new System.EventHandler(this.uiBook_SelectionChangeCommitted);
			this.uiBook.FontChanged += new System.EventHandler(this.uiBook_FontChanged);
			this.uiBook.Leave += new System.EventHandler(this.uiBook_Leave);
			this.uiBook.Enter += new System.EventHandler(this.uiBook_Enter);
			this.uiBook.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.uiBook_KeyPress);
			this.uiBook.KeyDown += new System.Windows.Forms.KeyEventHandler(this.uiBook_KeyDown);
			this.uiBook.TextUpdate += new System.EventHandler(this.uiBook_TextUpdate);
			this.uiBook.DropDown += new System.EventHandler(this.uiBook_DropDown);
			// 
			// VerseControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSize = true;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.tableLayoutPanel1);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "VerseControl";
			this.Size = new System.Drawing.Size(191, 22);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private SIL.Windows.Forms.Widgets.HorizontalSpinner uiVerseSpinner;
		private SIL.Windows.Forms.Widgets.HorizontalSpinner uiChapterSpinner;
		private VCEnterTextBox uiChapter;
		private VCEnterTextBox uiVerse;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.ToolTip uiToolTip;
		private VCSafeComboBox uiBook;
	}
}
