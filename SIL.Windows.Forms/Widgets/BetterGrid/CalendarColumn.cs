using System;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	public class CalendarColumn : DataGridViewColumn
	{
		/// ------------------------------------------------------------------------------------
		public CalendarColumn() : this(null, CalendarCell.UserAction.BeginEdit)
		{
		}

		/// ------------------------------------------------------------------------------------
		public CalendarColumn(Func<DateTime> getDefaultValue, CalendarCell.UserAction whenToUseDefault)
			: base(new CalendarCell(getDefaultValue, whenToUseDefault))
		{
			base.DefaultCellStyle.ForeColor = SystemColors.WindowText;
			base.DefaultCellStyle.BackColor = SystemColors.Window;
			base.DefaultCellStyle.Font = SystemFonts.MenuFont;
			base.CellTemplate.Style = DefaultCellStyle;
		}

		/// ------------------------------------------------------------------------------------
		public override DataGridViewCell CellTemplate
		{
			get { return base.CellTemplate; }
			set
			{
				// Ensure that the cell used for the template is a CalendarCell.
				if (value != null && !value.GetType().IsAssignableFrom(typeof(CalendarCell)))
				{
					throw new InvalidCastException(
						string.Format("CellTemplate cannot be set to an object of type {0}. Cell template must be assignable from a {1}",
						value.GetType().Name, typeof(CalendarCell).Name));
				}
				base.CellTemplate = value;
			}
		}
	}
}
