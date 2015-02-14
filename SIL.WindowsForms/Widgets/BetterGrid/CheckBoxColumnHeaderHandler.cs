using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SIL.WindowsForms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class draws a checkbox in a column header and lets the user click/unclick the
	/// check box, firing an event when they do so. IMPORTANT: This class must be instantiated
	/// after the column has been added to a DataGridView control.
	///
	/// This is good for columns in which all the cells are checkbox cells and you want the
	/// user to be able to check or uncheck all the cells by just clicking in the checkbox
	/// in the column heading.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class CheckBoxColumnHeaderHandler
	{
		/// ------------------------------------------------------------------------------------
		public delegate bool CheckChangeHandler(CheckBoxColumnHeaderHandler sender,
			CheckState oldState);

		/// ------------------------------------------------------------------------------------
		public event CheckChangeHandler CheckChanged;

		private readonly DataGridViewColumn _col;
		private readonly DataGridView _owningGrid;
		private readonly Size _szCheckBox = Size.Empty;
		private CheckState _state = CheckState.Checked;

		/// ------------------------------------------------------------------------------------
		public CheckBoxColumnHeaderHandler(DataGridViewColumn col)
		{
			Debug.Assert(col != null);
			Debug.Assert(col is DataGridViewCheckBoxColumn);
			Debug.Assert(col.DataGridView != null);

			_col = col;
			_owningGrid = col.DataGridView;
			_owningGrid.HandleDestroyed += HandleHandleDestroyed;
			_owningGrid.CellPainting += HandleHeaderCellPainting;
			_owningGrid.CellMouseMove += HandleHeaderCellMouseMove;
			_owningGrid.CellMouseClick += HandleHeaderCellMouseClick;
			_owningGrid.CellContentClick += HandleDataCellCellContentClick;
			_owningGrid.Scroll += HandleGridScroll;
			_owningGrid.RowsAdded += HandleGridRowsAdded;
			_owningGrid.RowsRemoved += HandleGridRowsRemoved;

			if (!BetterGrid.CanPaintVisualStyle())
				_szCheckBox = new Size(13, 13);
			else
			{
				var element = VisualStyleElement.Button.CheckBox.CheckedNormal;
				var renderer = new VisualStyleRenderer(element);
				using (var g = _owningGrid.CreateGraphics())
					_szCheckBox = renderer.GetPartSize(g, ThemeSizeType.True);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the state of the column header's check box.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public CheckState HeadersCheckState
		{
			get { return _state; }
			set
			{
				_state = value;
				_owningGrid.InvalidateCell(_col.HeaderCell);
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleHandleDestroyed(object sender, EventArgs e)
		{
			_owningGrid.HandleDestroyed -= HandleHandleDestroyed;
			_owningGrid.CellPainting -= HandleHeaderCellPainting;
			_owningGrid.CellMouseMove -= HandleHeaderCellMouseMove;
			_owningGrid.CellMouseClick -= HandleHeaderCellMouseClick;
			_owningGrid.CellContentClick -= HandleDataCellCellContentClick;
			_owningGrid.Scroll -= HandleGridScroll;
			_owningGrid.RowsAdded -= HandleGridRowsAdded;
			_owningGrid.RowsRemoved -= HandleGridRowsRemoved;
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
		{
			UpdateHeadersCheckStateFromColumnsValues();
		}

		/// ------------------------------------------------------------------------------------
		private void HandleGridRowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
		{
			UpdateHeadersCheckStateFromColumnsValues();
		}

		/// ------------------------------------------------------------------------------------
		void HandleGridScroll(object sender, ScrollEventArgs e)
		{
			if (e.ScrollOrientation == ScrollOrientation.HorizontalScroll)
			{
				var rc = _owningGrid.ClientRectangle;
				rc.Height = _owningGrid.ColumnHeadersHeight;
				_owningGrid.Invalidate(rc);
			}
		}

		/// ------------------------------------------------------------------------------------
		private void UpdateHeadersCheckStateFromColumnsValues()
		{
			bool foundOneChecked = false;
			bool foundOneUnChecked = false;

			foreach (DataGridViewRow row in _owningGrid.Rows)
			{
				object cellValue = row.Cells[_col.Index].Value;
				if (!(cellValue is bool))
					continue;

				bool chked = (bool)cellValue;
				if (!foundOneChecked && chked)
					foundOneChecked = true;
				else if (!foundOneUnChecked && !chked)
					foundOneUnChecked = true;

				if (foundOneChecked && foundOneUnChecked)
				{
					HeadersCheckState = CheckState.Indeterminate;
					return;
				}
			}

			HeadersCheckState = (foundOneChecked ? CheckState.Checked : CheckState.Unchecked);
		}

		/// ------------------------------------------------------------------------------------
		private void UpdateColumnsDataValuesFromHeadersCheckState()
		{
			foreach (DataGridViewRow row in _owningGrid.Rows)
			{
				if (row.Cells[_col.Index] == _owningGrid.CurrentCell && _owningGrid.IsCurrentCellInEditMode)
					_owningGrid.EndEdit();

				row.Cells[_col.Index].Value = (_state == CheckState.Checked);
			}
		}

		#region Mouse move and click handlers
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Handles toggling the selected state of a file in the file list.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void HandleDataCellCellContentClick(object sender, DataGridViewCellEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex == _col.Index)
			{
				bool currCellValue = (bool)_owningGrid[e.ColumnIndex, e.RowIndex].Value;
				_owningGrid[e.ColumnIndex, e.RowIndex].Value = !currCellValue;
				UpdateHeadersCheckStateFromColumnsValues();
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleHeaderCellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.RowIndex >= 0 || e.ColumnIndex != _col.Index)
				return;

			var oldState = HeadersCheckState;

			HeadersCheckState = (HeadersCheckState == CheckState.Checked ?
				CheckState.Unchecked : CheckState.Checked);

			_owningGrid.InvalidateCell(_col.HeaderCell);

			bool updateValues = true;
			if (CheckChanged != null)
				updateValues = CheckChanged(this, oldState);

			if (updateValues)
				UpdateColumnsDataValuesFromHeadersCheckState();
		}

		/// ------------------------------------------------------------------------------------
		private void HandleHeaderCellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
		{
			if (e.ColumnIndex == _col.Index && e.RowIndex < 0)
				_owningGrid.InvalidateCell(_col.HeaderCell);
		}

		#endregion

		#region Painting methods
		/// ------------------------------------------------------------------------------------
		private void HandleHeaderCellPainting(object sender, DataGridViewCellPaintingEventArgs e)
		{
			if (e.RowIndex >= 0 && e.ColumnIndex != _col.Index)
				return;

			var rcCell = _owningGrid.GetCellDisplayRectangle(_col.Index, -1, false);
			if (rcCell.IsEmpty)
				return;

			// At this point, we know at least part of the header cell is visible, therefore,
			// force the rectangle's width to that of the column's.
			rcCell.X = rcCell.Right - _col.Width;

			// Subtract one so as not to include the left border in the width.
			rcCell.Width = _col.Width - 1;

			int dx = (int)Math.Floor((rcCell.Width - _szCheckBox.Width) / 2f);
			int dy = (int)Math.Floor((rcCell.Height - _szCheckBox.Height) / 2f);
			var rc = new Rectangle(rcCell.X + dx, rcCell.Y + dy, _szCheckBox.Width, _szCheckBox.Height);

			if (BetterGrid.CanPaintVisualStyle())
				DrawVisualStyleCheckBox(e.Graphics, rc);
			else
			{
				var state = ButtonState.Checked;
				if (HeadersCheckState == CheckState.Unchecked)
					state = ButtonState.Normal;
				else if (HeadersCheckState == CheckState.Indeterminate)
					state |= ButtonState.Inactive;

				ControlPaint.DrawCheckBox(e.Graphics, rc, state | ButtonState.Flat);
			}
		}

		/// ------------------------------------------------------------------------------------
		private void DrawVisualStyleCheckBox(IDeviceContext g, Rectangle rc)
		{
			var isHot = rc.Contains(_owningGrid.PointToClient(Control.MousePosition));
			var element = VisualStyleElement.Button.CheckBox.CheckedNormal;

			if (HeadersCheckState == CheckState.Unchecked)
			{
				element = (isHot ? VisualStyleElement.Button.CheckBox.UncheckedHot :
					VisualStyleElement.Button.CheckBox.UncheckedNormal);
			}
			else if (HeadersCheckState == CheckState.Indeterminate)
			{
				element = (isHot ? VisualStyleElement.Button.CheckBox.MixedHot :
					VisualStyleElement.Button.CheckBox.MixedNormal);
			}
			else if (isHot)
				element = VisualStyleElement.Button.CheckBox.CheckedHot;

			var renderer = new VisualStyleRenderer(element);
			renderer.DrawBackground(g, rc);
		}

		#endregion
	}
}
