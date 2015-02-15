using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.Windows.Forms.HotSpot
{
	public class RetrieveHotSpotsEventArgs:EventArgs
	{
		private readonly TextBoxBase _control;
		private readonly List<HotSpot> _hotSpots;
		private Color _color;
		public RetrieveHotSpotsEventArgs(TextBoxBase control)
		{
			if (control == null)
			{
				throw new ArgumentNullException();
			}
			_color = Color.Red;
			_control = control;
			_hotSpots = new List<HotSpot>();
		}

		public string Text
		{
			get { return _control.Text; }
		}

		public Color Color
		{
			get { return _color; }
			set
			{
				if(value == Color.Empty)
				{
					throw new ArgumentOutOfRangeException();
				}
				_color = value;
			}
		}
		public TextBoxBase Control
		{
			get
			{
				return _control;
			}
		}
		public IEnumerable<HotSpot> HotSpots
		{
			get { return _hotSpots; }
		}

		public void AddHotSpot(HotSpot hotSpot)
		{
			_hotSpots.Add(hotSpot);
		}
	}
}