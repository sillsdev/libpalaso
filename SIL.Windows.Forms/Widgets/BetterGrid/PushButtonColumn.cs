using System;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Widgets.BetterGrid
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Extends the DataGridViewColumn to include a button on the right side of the cells
	/// in the column. Sort of like a combo box cell, only the button can be used to drop-down
	/// a custom control.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class PushButtonColumn : DataGridViewColumn
	{
		/// ------------------------------------------------------------------------------------
		public enum ButtonType
		{
			/// <summary>Draws just a simple drop-down arrow with no button border or background.</summary>
			MinimalistCombo,
			/// <summary>Draws a plain combo box button, not using visual styles.</summary>
			PlainCombo,
			/// <summary>Draws a plain push button, not using visual styles.</summary>
			PlainPush,
			/// <summary>Draws a combo box button using current visual style.</summary>
			VisualStyleCombo,
			/// <summary>Draws a normal button using current visual style.</summary>
			VisualStylePush
		}

		/// ------------------------------------------------------------------------------------
		public event DataGridViewCellMouseEventHandler ButtonClicked;

		private bool _showButton = true;
		private string _buttonToolTip;
		private ToolTip _toolTip;
		private bool _showCellToolTips = true;

		/// ------------------------------------------------------------------------------------
		public PushButtonColumn() : base(new PushButtonCell())
		{
			DrawDefaultComboButtonWidth = true;
			ButtonWidth = SystemInformation.VerticalScrollBarWidth;
			base.DefaultCellStyle.Font = SystemInformation.MenuFont;
			ButtonFont = SystemInformation.MenuFont;
			Width = 100 + ButtonWidth;
			HeaderText = string.Empty;
		}

		/// ------------------------------------------------------------------------------------
		public PushButtonColumn(string name) : this()
		{
			Name = name;
		}

		/// ------------------------------------------------------------------------------------
		public PushButtonColumn(string name, bool showButton) : this()
		{
			Name = name;
			_showButton = showButton;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Make sure the template is always a radion button cell.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public override DataGridViewCell CellTemplate
		{
			get { return base.CellTemplate; }
			set
			{
				if (value != null && !value.GetType().IsAssignableFrom(typeof(PushButtonCell)))
					throw new InvalidCastException(string.Format("Cell template must be based on {0}", typeof(PushButtonCell).Name));

				base.CellTemplate = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Need to save the value of the owning grid's ShowCellToolTips value because we may
		/// change it once in a while.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnDataGridViewChanged()
		{
			base.OnDataGridViewChanged();
			if (DataGridView != null)
				_showCellToolTips = DataGridView.ShowCellToolTips;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the text of the button cells in this column.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ButtonText { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the text of the button cells in this column.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Font ButtonFont { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the width of the button within the column's cells.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public int ButtonWidth { get; set; }

		/// ------------------------------------------------------------------------------------
		public ButtonType ButtonStyle { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the combo button's width is
		/// calculated automatically by the system (based on the theme). This value is only
		/// relevant when UseComboButtonStyle is true and visual styles in the OS are
		/// turned on.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DrawDefaultComboButtonWidth { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not to show the button.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ShowButton
		{
			get { return _showButton; }
			set
			{
				_showButton = value;
				if (DataGridView != null)
					DataGridView.InvalidateColumn(Index);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not text in the button cells in the
		/// column will be drawn using ellipsis path string formatting.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DrawTextWithEllipsisPath { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the tooltip text for the buttons in the button cells in this column.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public string ButtonToolTip
		{
			get { return _buttonToolTip; }
			set
			{
				if (_toolTip != null)
				{
					_toolTip.Dispose();
					_toolTip = null;
				}

				_buttonToolTip = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		internal void ShowToolTip()
		{
			if ((_toolTip != null && _toolTip.Active) || string.IsNullOrEmpty(_buttonToolTip) ||
				DataGridView == null || DataGridView.FindForm() == null)
			{
				return;
			}

			if (_toolTip == null)
				_toolTip = new ToolTip();

			DataGridView.ShowCellToolTips = false;
			Size sz = SystemInformation.CursorSize;
			Point pt = DataGridView.FindForm().PointToClient(Control.MousePosition);
			pt.X += (int)(sz.Width * 0.6);
			pt.Y += sz.Height;
			_toolTip.Active = true;
			_toolTip.Show(_buttonToolTip, DataGridView.FindForm(), pt);
		}

		/// ------------------------------------------------------------------------------------
		internal void HideToolTip()
		{
			DataGridView.ShowCellToolTips = _showCellToolTips;

			if (_toolTip != null)
			{
				_toolTip.Hide(DataGridView);
				_toolTip.Active = false;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Provides a way for an owned cell to fire the button clicked event on the column.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		internal void InvokeButtonClick(DataGridViewCellMouseEventArgs e)
		{
			if (ButtonClicked != null)
				ButtonClicked(DataGridView, e);
		}
	}
}
