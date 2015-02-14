using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SIL.WindowsForms.Widgets
{
	public partial class ControlListBox : UserControl
	{
		public interface ISelectableControl
		{
			event EventHandler Selecting;

			bool Selected
			{
				get;
				set;
			}
		}
		private bool _firstOne = true;//a hack to cover my lack of understanding

		public ControlListBox()
		{
			InitializeComponent();
			_table.Controls.Clear();
			_table.RowCount = 0;
		}

		public void AddControlToBottom(Control control)
		{
			AddControl(control, -1);
		}

		void OnItemSelecting(object sender, EventArgs e)
		{
			foreach (ISelectableControl item in Items)
			{
				if (item != sender && item.Selected)
				{
					item.Selected = false;
					break;
				}
			}
			LayoutRows();
		}

		/// <summary>
		/// Callers: consider control.AutoSizeMode = AutoSizeMode.GrowAndShrink if the control supports it.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="insertAtRow"></param>
		public void AddControl(Control control, int insertAtRow)
		{
			_table.SuspendLayout();
			if (_firstOne)
			{
				_firstOne = false;
				_table.ColumnCount = 1;
				_table.Controls.Clear();
				_table.RowCount = 0;
				_table.RowStyles.Clear();
			}
			if (insertAtRow < 0)
			{
				insertAtRow = _table.RowCount;
			}
			_table.Controls.Add(control);
			_table.SetCellPosition(control, new TableLayoutPanelCellPosition(0, insertAtRow));
			_table.RowCount++;
			_table.Controls.SetChildIndex(control, insertAtRow);


			LayoutRows();
			_table.ResumeLayout();
			_table.Invalidate();
			_table.Visible = false;
			_table.Visible = true;
			if (control is ISelectableControl)
			{
				((ISelectableControl) control).Selecting +=OnItemSelecting;
			}
		}


		public void RemoveControl(Control control)
		{
			this.SuspendLayout();
			if (control is ISelectableControl)
			{
				((ISelectableControl)control).Selecting -= OnItemSelecting;
			}
			int nowEmptyRow = _table.Controls.GetChildIndex(control);
			_table.Controls.Remove(control);
			for (int r = nowEmptyRow; r < _table.RowCount-1; r++)
			{
				_table.SetCellPosition(_table.GetControlFromPosition(0,r+1),new TableLayoutPanelCellPosition(0,r));
			}
			_table.RowCount--;
			LayoutRows();
			this.ResumeLayout(true);
		}

		public void LayoutRows()
		{
			foreach (Control c in _table.Controls)
			{
				c.TabIndex = _table.Controls.GetChildIndex(c);
			}

			float h = 0;
			_table.RowStyles.Clear();
			for (int r = 0; r < _table.RowCount; r++)
			{
				Control c = _table.GetControlFromPosition(0, r);
				if (c != null)// null happens at design time
				{
					RowStyle style = new RowStyle(SizeType.Absolute, c.Height + _table.Margin.Vertical);
					_table.RowStyles.Add(style);
					h += style.Height;
				}
			}
			_table.Height = (int)h;
			//_table.Invalidate();
			_table.Refresh();
		}

		public IEnumerable<Control> Items
		{
			get
			{
				for (int r = 0; r < _table.RowCount; r++)
				{
					yield return  _table.GetControlFromPosition(0, r);
				}
			}
		}


		public void Clear()
		{
			_table.Controls.Clear();
			_table.RowStyles.Clear();
			_firstOne = true;
		}

	}
}