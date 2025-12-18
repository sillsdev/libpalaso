using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.Windows.Forms.Extensions;

namespace SIL.Windows.Forms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	internal class CalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
	{
		private DataGridView _parentGrid;

		/// ------------------------------------------------------------------------------------
		public CalendarEditingControl()
		{
			Format = DateTimePickerFormat.Short;
		}

		#region Private properties
		/// ------------------------------------------------------------------------------------
		private DataGridView Grid
		{
			get
			{
				if (_parentGrid == null)
				{
					Control parent = Parent;
					while (parent != null && !(parent is DataGridView))
						parent = parent.Parent;
					_parentGrid = parent as DataGridView;
				}
				return _parentGrid;
			}
		}

		/// ------------------------------------------------------------------------------------
		private Color GridForeColor
		{
			get
			{
				return (Grid != null) ? Grid.ForeColor : Color.Black;
			}
		}

		/// ------------------------------------------------------------------------------------
		private Color GridBackColor
		{
			get
			{
				return (Grid != null) ? Grid.BackColor : Color.White;
			}
		}
		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Notify the DataGridView that the contents of the cell have changed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnValueChanged(EventArgs eventargs)
		{
			EditingControlValueChanged = true;
			EditingControlDataGridView.NotifyCurrentCellDirty(true);
			base.OnValueChanged(eventargs);
		}

		#region IDataGridViewEditingControl implementations

		public int EditingControlRowIndex { get; set; }
		public bool EditingControlValueChanged { get; set; }
		public DataGridView EditingControlDataGridView { get; set; }

		/// ------------------------------------------------------------------------------------
		public Cursor EditingPanelCursor
		{
			get { return base.Cursor; }
		}

		/// ------------------------------------------------------------------------------------
		public object EditingControlFormattedValue
		{
			get { return Value.ToShortDateString(); }
			set
			{
				if (value is string && ((string)value).Trim() != string.Empty)
					Value = DateTime.Parse((string)value);
			}
		}

		/// ------------------------------------------------------------------------------------
		public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
		{
			return EditingControlFormattedValue;
		}

		/// ------------------------------------------------------------------------------------
		public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
		{
			Font = dataGridViewCellStyle.Font;
			if (!Application.RenderWithVisualStyles)
			{
				var foreColor = dataGridViewCellStyle.ForeColor;
				if (foreColor.IsEmpty)
					foreColor = GridForeColor;
				var backColor = dataGridViewCellStyle.BackColor;
				if (backColor.IsEmpty)
					backColor = GridBackColor;
				if (!foreColor.IsEmpty && foreColor != backColor)
					CalendarForeColor = foreColor;
				if (!backColor.IsEmpty && backColor != CalendarForeColor)
					CalendarMonthBackground = backColor;
			}
		}

		/// ------------------------------------------------------------------------------------
		public bool EditingControlWantsInputKey(Keys key, bool dataGridViewWantsInputKey)
		{
			// Let the DateTimePicker handle navigation keys.
			return key.IsNavigationKey() || !dataGridViewWantsInputKey;
		}

		/// ------------------------------------------------------------------------------------
		public void PrepareEditingControlForEdit(bool selectAll)
		{
			// No preparation needs to be done.
		}

		/// ------------------------------------------------------------------------------------
		public bool RepositionEditingControlOnValueChange => false;

		#endregion
	}
}
