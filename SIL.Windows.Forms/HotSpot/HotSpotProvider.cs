using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Security.Permissions;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.HotSpot
{
	[ProvideProperty("EnableHotSpots", typeof (TextBoxBase))]
	public class HotSpotProvider: IExtenderProvider, IComponent
	{
		private readonly Dictionary<Control, HotSpotPainter> _extendees;
		private ISite site;

		public HotSpotProvider()
		{
			_extendees = new Dictionary<Control, HotSpotPainter>();
		}

		#region IComponent Members

		public event EventHandler Disposed = delegate
												 { };

		public ISite Site
		{
			get
			{
				VerifyNotDisposed();
				return site;
			}
			set
			{
				VerifyNotDisposed();
				site = value;
			}
		}

		private bool _isDisposed = false;
		public void Dispose()
		{
			if (!_isDisposed)
			{
				_isDisposed = true;
				foreach (KeyValuePair<Control, HotSpotPainter> pair in _extendees)
				{
					pair.Value.Dispose();
				}
				_extendees.Clear();
				Disposed(this, new EventArgs());
			}
		}

		private void VerifyNotDisposed()
		{
			if (_isDisposed)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion

		#region IExtenderProvider Members

		public bool CanExtend(object extendee)
		{
			VerifyNotDisposed();
			return extendee is TextBoxBase;
		}

		#endregion

		private EventHandler<RetrieveHotSpotsEventArgs> _retrieveHotSpots;
		public event EventHandler<RetrieveHotSpotsEventArgs> RetrieveHotSpots
		{
			add
			{
				_retrieveHotSpots += value;
				RefreshAll();
			}
			remove
			{
				_retrieveHotSpots -= value;
				RefreshAll();
			}
		}

		[DefaultValue(false)]
		public bool GetEnableHotSpots(Control c)
		{
			if (c == null)
			{
				throw new ArgumentNullException();
			}
			if (!CanExtend(c))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}
			return _extendees.ContainsKey(c);
		}

		internal IEnumerable<HotSpot> GetHotSpotsFromPosition(Control c, Point position)
		{
			if (c == null)
			{
				throw new ArgumentNullException();
			}
			if (!CanExtend(c))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}
			HotSpotPainter hotSpotPainter;
			if(_extendees.TryGetValue(c, out hotSpotPainter))
			{
				ICollection<HotSpotInternal> hotSpots = hotSpotPainter.GetHotSpotsFromPosition((TextBoxBase)c, position);
				foreach (HotSpotInternal hotSpotInternal in hotSpots)
				{
					yield return hotSpotInternal.HotSpot;
				}
			}
		}

		public void SetEnableHotSpots(Control c, bool value)
		{
			if (c == null)
			{
				throw new ArgumentNullException();
			}
			if (!CanExtend(c))
			{
				throw new ArgumentException("Control must be derived from TextBoxBase");
			}
			if (!value)
			{
				HotSpotPainter hotSpotPainter;
				if (_extendees.TryGetValue(c, out hotSpotPainter))
				{
					_extendees.Remove(c);
					hotSpotPainter.Dispose();
				}
			}
			else if (!_extendees.ContainsKey(c))
			{
				_extendees[c] = new HotSpotPainter(this, (TextBoxBase) c);
			}
		}

		private void AddHotSpots(ICollection<HotSpotInternal> hotSpots, TextBoxBase textBox)
		{
			if (_retrieveHotSpots != null)
			{
				RetrieveHotSpotsEventArgs retrieveHotSpots = new RetrieveHotSpotsEventArgs(textBox);
				_retrieveHotSpots.Invoke(this, retrieveHotSpots);

				foreach (HotSpot spot in retrieveHotSpots.HotSpots)
				{
					hotSpots.Add(new HotSpotInternal(spot, retrieveHotSpots.Color));
				}
			}
		}

		#region Nested type: HotSpotInternal

		private class HotSpotInternal
		{
			public readonly HotSpot HotSpot;
			public readonly Color UnderlineColor;

			public HotSpotInternal(HotSpot hotspot, Color underlineColor)
			{
				HotSpot = hotspot;
				UnderlineColor = underlineColor;
			}
		}

		#endregion

		#region Nested type: HotSpotPainter

		private class HotSpotPainter: NativeWindow, IDisposable
		{
			private readonly TextBoxBase _control;
			private readonly HotSpotProvider _parent;

			public readonly List<HotSpotInternal> HotSpots;

			private ICollection<HotSpotInternal> _currentHotSpots;
			private bool _disposed = false;

			public HotSpotPainter(HotSpotProvider parent, TextBoxBase c)
			{
				if (c.IsHandleCreated)
				{
					AssignHandle(c.Handle);
				}
				c.HandleCreated += ControlHandleCreated;
				c.HandleDestroyed += ControlHandleDestroyed;

				c.TextChanged += OnTextChanged;
				c.MouseMove += OnMouseMove;
				c.MouseDown += OnMouseDown;
				c.MouseClick += OnMouseClick;
				c.MouseUp += OnMouseUp;
				c.MouseHover += OnMouseHover;
				c.MouseLeave += OnMouseLeave;

				_control = c;
				HotSpots = new List<HotSpotInternal>();
				_currentHotSpots = new List<HotSpotInternal>();
				_parent = parent;
			}

			#region IDisposable Members

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion

			private void ControlHandleCreated(object sender, EventArgs e)
			{
				AssignHandle(((Control) sender).Handle);
			}

			private void ControlHandleDestroyed(object sender, EventArgs e)
			{
				ReleaseHandle();
			}

			protected override void WndProc(ref Message m)
			{
				const int WM_PAINT = 0xF;
				const int WM_SETFOCUS = 0x7;
				const int WM_KILLFOCUS = 0x8;

				const int WM_MOUSEMOVE = 0x200;
				const int WM_LBUTTONDOWN = 0x201;
				const int WM_RBUTTONDOWN = 0x204;
				const int WM_MBUTTONDOWN = 0x207;
				const int WM_LBUTTONUP = 0x202;
				const int WM_RBUTTONUP = 0x205;
				const int WM_MBUTTONUP = 0x208;
				const int WM_LBUTTONDBLCLK = 0x203;
				const int WM_RBUTTONDBLCLK = 0x206;
				const int WM_MBUTTONDBLCLK = 0x209;

				const int WM_KEYDOWN = 0x0100;
				const int WM_KEYUP = 0x0101;
				const int WM_CHAR = 0x0102;

				switch (m.Msg)
				{
					case WM_PAINT:
						base.WndProc(ref m);
						OnWmPaint();
						break;
					case WM_SETFOCUS:
					case WM_KILLFOCUS:
					case WM_LBUTTONDOWN:
					case WM_RBUTTONDOWN:
					case WM_MBUTTONDOWN:
					case WM_LBUTTONUP:
					case WM_RBUTTONUP:
					case WM_MBUTTONUP:
					case WM_LBUTTONDBLCLK:
					case WM_RBUTTONDBLCLK:
					case WM_MBUTTONDBLCLK:
					case WM_KEYDOWN:
					case WM_CHAR:
					case WM_KEYUP:
						base.WndProc(ref m);
						_control.Invalidate();
						// if we don't invalidate, then we end up with artifacts from the wave when it is removed
						break;
					case WM_MOUSEMOVE: // Only need to process if a mouse button is down:
						base.WndProc(ref m);
						if (!m.WParam.Equals(IntPtr.Zero))
						{
							_control.Invalidate();
							// if we don't invalidate, then we end up with artifacts from the wave when it is removed
						}
						break;
					case 0x286:	//IME_CHAR
					case 0x10F:	//IME_COMPOSITION
						//These two messages cause an extra character to be drawn by mono
						//At this time (Oct 9 2009) we are not sure wether this is a bug with mono
						//or with our code and we will need to do further testing (also under windows)
						//For the time being it fixes an annoying bug (Ws-15007) where 4 characters
						//were being displayed for each keystroke.
						//See WS-15008 for a full description of the problem and a roadmap for fixing it

						// On Linux, ignore these messages, on Windows handle them
						if (Platform.IsWindows)
							base.WndProc(ref m);
						break;
					default:
						base.WndProc(ref m);
						break;
				}
			}

			// OnPaint will only be called when ControlStyles.UserPaint is true
			// however, that adds other problems
			// see http://www.pcreview.co.uk/forums/thread-1317347.php
			private void OnWmPaint()
			{
				using (Graphics g = Graphics.FromHwnd(Handle))
				{
					foreach (HotSpotInternal hotSpot in HotSpots)
					{
						Rectangle bounds = GetHotAreaBounds(g, hotSpot.HotSpot);

						if (bounds.Top < 0)
						{
							// we don't want to underline if text is off screen
							continue;
						}
						DrawHorizontalWaveLine(g,
											   hotSpot.UnderlineColor,
											   bounds.Left,
											   bounds.Right,
											   bounds.Bottom + 1);
					}
				}
			}

			private Rectangle GetHotAreaBounds(IDeviceContext g, HotSpot area)
			{
				//GetPositionFromCharIndex give us the point at the top
				// left corner of this word. (see documentation on EM_POSFROMCHAR)
				Point start = _control.GetPositionFromCharIndex(area.Offset);
				// the following is necessary to work around a .Net bug
				int startX = (Int16) start.X;
				int startY = (Int16) start.Y;

				Size textSize =
					TextRenderer.MeasureText(g,
											 area.Text,
											 _control.Font,
											 _control.ClientSize,
											 TextFormatFlags.TextBoxControl |
											 TextFormatFlags.NoPadding |
											 TextFormatFlags.WordBreak);
				return new Rectangle(startX, startY, textSize.Width, textSize.Height);
			}

			// Drawn more like peaks than waves /\/\/\/\/\/\
			// we also won't worry about any remainder since it isn't big enough to be noticeable
			private static void DrawHorizontalWaveLine(Graphics g,
													   Color color,
													   int x1,
													   int x2,
													   int y)
			{
				using (Pen pen = new Pen(color))
				{
					const float height = 1.5F;
					if (x1 == x2) // no distance to draw
					{
						return;
					}
					if (x1 > x2)
					{
						// swap x1 and x2 so x1 is always less than x2
						int t = x2;
						x2 = x1;
						x1 = t;
					}

					List<PointF> points = new List<PointF>();

					float yOffset = height / 2; // y is our midpoint
					points.Add(new PointF(x1, y - yOffset)); // add our starting point

					float x = x1;
					for (int i = 1;x + height < x2;++i, yOffset *= -1)
					{
						x += height; // advance by height so slope is 1
						points.Add(new PointF(x, y + yOffset));
					}

					g.DrawLines(pen, points.ToArray());
				}
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!_disposed)
				{
					if (disposing)
					{
						// dispose-only, i.e. non-finalizable logic
						_control.HandleCreated -= ControlHandleCreated;
						_control.HandleDestroyed -= ControlHandleDestroyed;
						_control.TextChanged -= OnTextChanged;
						_control.MouseMove -= OnMouseMove;
						_control.MouseDown -= OnMouseDown;
						_control.MouseClick -= OnMouseClick;
						_control.MouseUp -= OnMouseUp;
						_control.MouseHover -= OnMouseHover;
						_control.MouseLeave -= OnMouseLeave;

						if (Handle != IntPtr.Zero)
						{
							ReleaseHandle();
						}
					}

					// shared (dispose and finalizable) cleanup logic
					_disposed = true;
				}
			}

			/// <summary>
			/// Find the first HotArea that contains the offset
			/// </summary>
			/// <returns>null when no HotArea contains the offset</returns>
			private ICollection<HotSpotInternal> GetHotSpotsFromOffset(int offset)
			{
				List<HotSpotInternal> result = new List<HotSpotInternal>();
				foreach (HotSpotInternal hotSpot in HotSpots)
				{
					int hotSpotOffset = hotSpot.HotSpot.Offset;
					if (offset == hotSpotOffset)
					{
						result.Add(hotSpot);
					}
					else if (offset > hotSpotOffset &&
							 offset < hotSpotOffset + hotSpot.HotSpot.Text.Length)
					{
						result.Add(hotSpot);
					}
				}
				return result;
			}

			private void OnMouseLeave(object sender, EventArgs e)
			{
				foreach (HotSpotInternal hotSpot in _currentHotSpots)
				{
					hotSpot.HotSpot.OnMouseLeave(e);
				}
				_currentHotSpots = new List<HotSpotInternal>();
			}

			private void OnMouseHover(object sender, EventArgs e)
			{
				foreach (HotSpotInternal hotSpot in _currentHotSpots)
				{
					hotSpot.HotSpot.OnMouseHover(e);
				}
			}

			private void OnMouseUp(object sender, MouseEventArgs e)
			{
				HandleMouseEvent((TextBoxBase) sender,
								 e.Location,
								 delegate(HotSpot hotSpot) { hotSpot.OnMouseUp(e); });
			}

			private void OnMouseClick(object sender, MouseEventArgs e)
			{
				HandleMouseEvent((TextBoxBase) sender,
								 e.Location,
								 delegate(HotSpot hotSpot) { hotSpot.OnMouseClick(e); });
			}

			private void OnMouseDown(object sender, MouseEventArgs e)
			{
				HandleMouseEvent((TextBoxBase) sender,
								 e.Location,
								 delegate(HotSpot hotSpot) { hotSpot.OnMouseDown(e); });
			}

			private void OnMouseMove(object sender, MouseEventArgs e)
			{
				HandleMouseEvent((TextBoxBase) sender,
								 e.Location,
								 delegate(HotSpot hotSpot) { hotSpot.OnMouseMove(e); });
			}

			private void HandleMouseEvent(TextBoxBase textBox,
										  Point position,
										  HotSpotMouseEventDelegate d)
			{
				ICollection<HotSpotInternal> hotSpots = GetHotSpotsFromPosition(textBox, position);
				UpdateCurrentHotSpots(hotSpots);
				foreach (HotSpotInternal hotSpot in hotSpots)
				{
					d(hotSpot.HotSpot);
				}
			}

			public ICollection<HotSpotInternal> GetHotSpotsFromPosition(TextBoxBase textBox, Point position) {
				int offset = textBox.GetCharIndexFromPosition(position);

				return GetHotSpotsFromOffset(offset);
			}

			private void UpdateCurrentHotSpots(ICollection<HotSpotInternal> hotSpots)
			{
				ICollection<HotSpotInternal> previousHotSpots = _currentHotSpots;
				_currentHotSpots = hotSpots;

				SendMouseEnterForHotSpotsNewlyUnderMouse(_currentHotSpots, previousHotSpots);
				SendMouseLeaveForHotSpotsNoLongerUnderMouse(_currentHotSpots, previousHotSpots);
			}

			static private void SendMouseEnterForHotSpotsNewlyUnderMouse(
				IEnumerable<HotSpotInternal> hotSpotsUnderMouse,
				ICollection<HotSpotInternal> hotSpotsPreviouslyUnderMouse)
			{
				foreach (HotSpotInternal hotSpotInternal in hotSpotsUnderMouse)
				{
					bool isPreviouslyUnderMouse = hotSpotsPreviouslyUnderMouse.Contains(hotSpotInternal);
					if (!isPreviouslyUnderMouse)
					{
						hotSpotInternal.HotSpot.OnMouseEnter(new EventArgs());
					}
				}
			}

			static private void SendMouseLeaveForHotSpotsNoLongerUnderMouse(
				ICollection<HotSpotInternal> hotSpotsUnderMouse,
				IEnumerable<HotSpotInternal> hotSpotsPreviouslyUnderMouse)
			{
				foreach (HotSpotInternal hotSpotInternal in hotSpotsPreviouslyUnderMouse)
				{
					bool isUnderMouse = hotSpotsUnderMouse.Contains(hotSpotInternal);
					if (!isUnderMouse)
					{
						hotSpotInternal.HotSpot.OnMouseLeave(new EventArgs());
					}
				}
			}

			private void OnTextChanged(object sender, EventArgs e)
			{
				Refresh();
			}

			public void Refresh()
			{
				_currentHotSpots.Clear();
				HotSpots.Clear();
				_parent.AddHotSpots(HotSpots, _control);
				_control.Refresh();
			}

			#region Nested type: HotSpotMouseEventDelegate

			private delegate void HotSpotMouseEventDelegate(HotSpot hotspot);

			#endregion
		}

		#endregion

		public void Refresh(Control control)
		{
			HotSpotPainter hotSpotPainter;
			if (_extendees.TryGetValue(control, out hotSpotPainter))
			{
				hotSpotPainter.Refresh();
			}
		}

		public void RefreshAll()
		{
			foreach (Control control in _extendees.Keys)
			{
				Refresh(control);
			}
		}
	}
}