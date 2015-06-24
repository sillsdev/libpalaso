using System;
using System.Windows.Forms;

namespace SIL.Windows.Forms.HotSpot
{
	public class HotSpot
	{
		private readonly int _offset;
		private readonly string _text;
		private readonly TextBoxBase _control;

		public HotSpot(TextBoxBase control, int offset, int length)
		{
			if(control == null)
			{
				throw  new ArgumentNullException("control");
			}
			if(offset < 0)
			{
				throw  new ArgumentOutOfRangeException("offset", offset, "offset cannot be less than zero");
			}
			if (offset >= control.Text.Length)
			{
				throw new ArgumentOutOfRangeException("offset", offset, "offset must be shorter than text.");
			}
			if(length <= 0)
			{
				throw new ArgumentOutOfRangeException("length", offset, "length must be greater than zero");
			}
			if (offset + length > control.Text.Length)
			{
				throw new ArgumentOutOfRangeException("length", length, "length plus offset must be shorter than text.");
			}
			_control = control;
			_offset = offset;
			_text = _control.Text.Substring(_offset, length);
		}

		public event EventHandler MouseEnter;

		public int Offset
		{
			get { return _offset; }
		}

		public string Text
		{
			get
			{
				return _text;
			}
		}

		public TextBoxBase Control
		{
			get { return _control; }
		}

		internal void OnMouseEnter(EventArgs e)
		{
			if (MouseEnter != null)
			{
				MouseEnter.Invoke(this, e);
			}
		}

		public event MouseEventHandler MouseMove;

		internal void OnMouseMove(MouseEventArgs e)
		{
			if (MouseMove != null)
			{
				MouseMove.Invoke(this, e);
			}
		}

		public event EventHandler MouseHover;

		internal void OnMouseHover(EventArgs e)
		{
			if (MouseHover != null)
			{
				MouseHover.Invoke(this, e);
			}
		}

		public event MouseEventHandler MouseDown;

		internal void OnMouseDown(MouseEventArgs e)
		{
			if (MouseDown != null)
			{
				MouseDown.Invoke(this, e);
			}
		}

		public event MouseEventHandler MouseClick;

		internal void OnMouseClick(MouseEventArgs e)
		{
			if (MouseClick != null)
			{
				MouseClick.Invoke(this, e);
			}
		}

		public event MouseEventHandler MouseUp;

		internal void OnMouseUp(MouseEventArgs e)
		{
			if (MouseUp != null)
			{
				MouseUp.Invoke(this, e);
			}
		}

		public event EventHandler MouseLeave;

		internal void OnMouseLeave(EventArgs e)
		{
			if (MouseLeave != null)
			{
				MouseLeave.Invoke(this, e);
			}
		}
	}
}