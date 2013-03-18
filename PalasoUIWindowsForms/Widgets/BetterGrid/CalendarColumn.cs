using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	public class CalendarColumn : DataGridViewColumn
	{
		/// ------------------------------------------------------------------------------------
		public CalendarColumn() : base(new CalendarCell())
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
					throw new InvalidCastException(string.Format("Cell template must be a {0}", typeof(CalendarCell).Name));

				base.CellTemplate = value;
			}
		}
	}
}
