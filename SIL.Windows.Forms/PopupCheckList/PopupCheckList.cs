using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static System.String;

namespace SIL.Windows.Forms.PopupCheckList
{
	public class PopupCheckList : UserControl
	{
		private readonly int _verticalHeightAllowance;
		private TextBox _textBox;
		private ComboBox _comboArrow;
		private TableLayoutPanel _panel;
		private readonly List<CheckedListItem> _items = new List<CheckedListItem>();

		public PopupCheckList()
		{
			InitializeComponent();
			_verticalHeightAllowance = Height - _textBox.Height;
			SetMinAndMaxSize();
		}

		public IReadOnlyList<CheckedListItem> Items => _items;

		public string Separator { get; set; } = ", ";

		public override Font Font
		{
			get => _textBox.Font;
			set
			{
				_textBox.Font = value;
				SetMinAndMaxSize();
			}
		}

		public override Color BackColor
		{
			get => _textBox.BackColor;
			set => _textBox.BackColor = _panel.BackColor = value;
		}

		public override Color ForeColor
		{
			get => _textBox.ForeColor;
			set => _textBox.ForeColor = value;
		}

		public override Size MinimumSize
		{
			get => base.MinimumSize;
			set => MinimumSize = new Size(Math.Max(value.Width, _comboArrow.Width + 2), base.MinimumSize.Height);
		}

		public void AddItem(CheckedListItem item)
		{
			InsertItem_Internal(_items.Count, item, true);
		}

		public void InsertItem(int position, CheckedListItem item)
		{
			InsertItem_Internal(position, item, true);
		}

		public void AddRange(IEnumerable<CheckedListItem> items)
		{
			var position = _items.Count;
			bool anyItemChecked = false;
			foreach (var item in items)
			{
				anyItemChecked |= item.Checked;
				InsertItem_Internal(position++, item, false);
			}

			if (anyItemChecked)
				SetEditText();
		}

		public void InsertItem_Internal(int position, CheckedListItem item, bool setEditText)
		{
			item.ItemCheckStateChanged += ItemCheckStateChanged;
			_items.Add(item);
			if (item.Checked)
				SetEditText();
		}

		private void ItemCheckStateChanged(CheckedListItem sender, bool checkState)
		{
			SetEditText();
		}

		private void SetMinAndMaxSize()
		{
			var newHeight = _textBox.Height + _verticalHeightAllowance;
			MinimumSize = new Size(MinimumSize.Width, newHeight);
			MaximumSize = new Size(MaximumSize.Width, newHeight);
		}

		private void SetEditText()
		{
			_textBox.Text = Join(Separator,
				Items.Where(i => i.Checked).Select(i => i.EditSummaryDisplay));
		}

		private void InitializeComponent()
		{
			this._textBox = new System.Windows.Forms.TextBox();
			this._comboArrow = new System.Windows.Forms.ComboBox();
			this._panel = new System.Windows.Forms.TableLayoutPanel();
			this._panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// _textBox
			// 
			this._textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this._textBox.BackColor = System.Drawing.SystemColors.Window;
			this._textBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this._textBox.Location = new System.Drawing.Point(1, 6);
			this._textBox.Margin = new System.Windows.Forms.Padding(1, 0, 1, 3);
			this._textBox.Name = "_textBox";
			this._textBox.ReadOnly = true;
			this._textBox.ShortcutsEnabled = false;
			this._textBox.Size = new System.Drawing.Size(108, 15);
			this._textBox.TabIndex = 0;
			// 
			// _comboArrow
			// 
			this._comboArrow.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this._comboArrow.Cursor = System.Windows.Forms.Cursors.Default;
			this._comboArrow.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			this._comboArrow.DropDownHeight = 1;
			this._comboArrow.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this._comboArrow.DropDownWidth = 1;
			this._comboArrow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this._comboArrow.FormattingEnabled = true;
			this._comboArrow.IntegralHeight = false;
			this._comboArrow.ItemHeight = 18;
			this._comboArrow.Location = new System.Drawing.Point(112, 0);
			this._comboArrow.Margin = new System.Windows.Forms.Padding(0);
			this._comboArrow.MaxDropDownItems = 1;
			this._comboArrow.Name = "_comboArrow";
			this._comboArrow.Size = new System.Drawing.Size(19, 24);
			this._comboArrow.TabIndex = 1;
			// 
			// _panel
			// 
			this._panel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this._panel.AutoSize = true;
			this._panel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this._panel.BackColor = System.Drawing.SystemColors.Window;
			this._panel.ColumnCount = 2;
			this._panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this._panel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this._panel.Controls.Add(this._textBox, 0, 0);
			this._panel.Controls.Add(this._comboArrow, 1, 0);
			this._panel.Location = new System.Drawing.Point(0, 0);
			this._panel.Name = "_panel";
			this._panel.RowCount = 1;
			this._panel.RowStyles.Add(new System.Windows.Forms.RowStyle());
			this._panel.Size = new System.Drawing.Size(131, 24);
			this._panel.TabIndex = 2;
			// 
			// PopupCheckList
			// 
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Controls.Add(this._panel);
			this.Margin = new System.Windows.Forms.Padding(0);
			this.Name = "PopupCheckList";
			this.Size = new System.Drawing.Size(131, 24);
			this._panel.ResumeLayout(false);
			this._panel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
