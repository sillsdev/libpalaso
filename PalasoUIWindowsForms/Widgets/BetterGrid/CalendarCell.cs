using System;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	public class CalendarCell : DataGridViewTextBoxCell
	{
		public enum UserAction
		{
			BeginEdit,
			CellMouseClick,
			CellEnter,
		}

		private UserAction _whenToUseDefault;
		private Func<DateTime> _getDefaultValue;

		/// ------------------------------------------------------------------------------------
		public CalendarCell() : this(null, UserAction.BeginEdit)
		{
		}

		/// ------------------------------------------------------------------------------------
		public CalendarCell(Func<DateTime> getDefaultValue, UserAction whenToUseDefault)
		{
			_getDefaultValue = getDefaultValue ?? (() => DateTime.Now.Date);
			_whenToUseDefault = whenToUseDefault;
			// Use the short date format.
			Style.Format = "d";
		}

		/// ------------------------------------------------------------------------------------
		public override void InitializeEditingControl(int rowIndex, object
			initialFormattedValue, DataGridViewCellStyle dataGridViewCellStyle)
		{
			// Set the value of the editing control to the current cell value.
			base.InitializeEditingControl(rowIndex, initialFormattedValue, dataGridViewCellStyle);
			var ctrl = DataGridView.EditingControl as CalendarEditingControl;

			if (Value != null && Value.GetType() == typeof(DateTime))
				ctrl.Value = ((DateTime)Value < ctrl.MinDate ? _getDefaultValue() : (DateTime)Value);
		}

		/// ------------------------------------------------------------------------------------
		public override Type EditType
		{
			get { return typeof(CalendarEditingControl); }
		}

		/// ------------------------------------------------------------------------------------
		public override Type ValueType
		{
			get { return typeof(DateTime); }
		}

		/// ------------------------------------------------------------------------------------
		public override object DefaultNewRowValue
		{
			get { return null; }
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnEnter(int rowIndex, bool throughMouseClick)
		{
			base.OnEnter(rowIndex, throughMouseClick);
			if (Value == null &&
				(_whenToUseDefault == UserAction.CellEnter ||
				throughMouseClick && _whenToUseDefault == UserAction.CellMouseClick))
			{
				Value = _getDefaultValue();
			}
		}

		/// ------------------------------------------------------------------------------------
		public override object Clone()
		{
			var copy = (CalendarCell)base.Clone();
			copy._getDefaultValue = _getDefaultValue;
			copy._whenToUseDefault = _whenToUseDefault;
			return copy;
		}
	}
}
