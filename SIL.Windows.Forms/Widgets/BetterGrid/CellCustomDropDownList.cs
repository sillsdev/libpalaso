using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	public class CellCustomDropDownList : Panel
	{
		private readonly ListBox _listBox;
		private readonly CustomDropDown _dropDown;

		/// ------------------------------------------------------------------------------------
		public CellCustomDropDownList()
		{
			DoubleBuffered = true;
			_dropDown = new CustomDropDown();
			_dropDown.AutoCloseWhenMouseLeaves = false;
			_dropDown.AddControl(this);
			_dropDown.Closed += delegate { AssociatedCell = null; };

			_listBox = new ListBox();
			_listBox.BorderStyle = BorderStyle.None;
			_listBox.Dock = DockStyle.Fill;
			_listBox.KeyDown += HandleListBoxKeyDown;
			_listBox.MouseClick += HandleListBoxMouseClick;
			_listBox.MouseMove += HandleListBoxMouseMove;

			BorderStyle = BorderStyle.FixedSingle;
			Padding = new Padding(1);
			Controls.Add(_listBox);

			Font = SystemFonts.IconTitleFont;
		}

		/// ------------------------------------------------------------------------------------
		public override Font Font
		{
			get { return base.Font; }
			set
			{
				_listBox.Font = value;
				base.Font = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		public ListBox.ObjectCollection Items
		{
			get { return _listBox.Items; }
		}

		/// ------------------------------------------------------------------------------------
		public object SelectedItem
		{
			get { return _listBox.SelectedItem; }
			set { _listBox.SelectedItem = value; }
		}

		/// ------------------------------------------------------------------------------------
		public int SelectedIndex
		{
			get { return _listBox.SelectedIndex; }
			set { _listBox.SelectedIndex = value; }
		}

		/// ------------------------------------------------------------------------------------
		public bool IsDroppedDown
		{
			get { return AssociatedCell != null; }
		}

		protected DataGridViewCell AssociatedCell { get; set; }

		protected ListBox Box
		{
			get { return _listBox; }
		}

		protected CustomDropDown DropDown
		{
			get { return _dropDown; }
		}

	    /// ------------------------------------------------------------------------------------
		public void Close()
		{
			_dropDown.Close();
		}

		/// ------------------------------------------------------------------------------------
		public void Show(DataGridViewCell cell, IEnumerable<string> items)
		{
			// This is sort of a kludge, but right before the first time the list is
			// displayed, it's handle hasn't been created therefore the preferred
			// size cannot be accurately determined and the preferred width is needed
			// below. So to ensure the handle gets created, show then hide the drop-down.
			if (!IsHandleCreated)
			{
				Size = new Size(0, 0);
				_dropDown.Show(0, 0);
				_dropDown.Close();
			}

			Items.Clear();
			Items.AddRange(items.ToArray());
			SelectedItem = cell.Value as string;

			if (SelectedIndex < 0 && Items.Count > 0)
				SelectedIndex = 0;

			AssociatedCell = cell;
			int col = cell.ColumnIndex;
			int row = cell.RowIndex;
			Width = Math.Max(cell.DataGridView.Columns[col].Width, _listBox.PreferredSize.Width);
			Height = (Math.Min(Items.Count, 15) * _listBox.ItemHeight) + Padding.Vertical + 2;
			var rc = cell.DataGridView.GetCellDisplayRectangle(col, row, false);
			rc.Y += rc.Height;
			_dropDown.Show(cell.DataGridView.PointToScreen(rc.Location));
			_listBox.Focus();
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void HandleListBoxMouseMove(object sender, MouseEventArgs e)
		{
			int i = _listBox.IndexFromPoint(e.Location);
			if (i >= 0 && i != SelectedIndex)
				SelectedIndex = i;
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void HandleListBoxMouseClick(object sender, MouseEventArgs e)
		{
			int i = _listBox.IndexFromPoint(e.Location);
			if (i >= 0)
				AssociatedCell.Value = Items[i] as string;

			_dropDown.Close();
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void HandleListBoxKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Escape)
				_dropDown.Close();
			else if (e.KeyCode == Keys.Return && SelectedItem != null)
			{
				AssociatedCell.Value = SelectedItem as string;
				_dropDown.Close();
			}
		}
	}
}
