using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Palaso.UI.WindowsForms.ClearShare.WinFormsUI
{
	public partial class FadingMessageForm : Form
	{
		protected enum TimerState
		{
			FadingIn,
			FadeInComplete,
			WaitingToFadeOut,
			FadingOut,
		}

		protected const int kCornerRadius = 8;
		protected const int kPointerSize = 8;
		protected const int kMessageMargin = 7;

		protected Timer _timer;
		protected Point _point;
		protected TimerState _state;

		/// ------------------------------------------------------------------------------------
		private FadingMessageForm()
		{
			InitializeComponent();
			Opacity = .001;

			_labelMessage.Font =
				SystemFonts.IconTitleFont.FontFamily.IsStyleAvailable(FontStyle.Bold) ?
				new Font(SystemFonts.IconTitleFont, FontStyle.Bold) : SystemFonts.IconTitleFont;
		}

		/// ------------------------------------------------------------------------------------
		public FadingMessageForm(Point pt) : this()
		{
			_point = pt;
		}

		/// ------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
					components.Dispose();

				if (_timer != null)
					_timer.Dispose();
			}

			_timer = null;

			base.Dispose(disposing);
		}

		/// ------------------------------------------------------------------------------------
		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}

		/// ------------------------------------------------------------------------------------
		public override string Text
		{
			get { return base.Text; }
			set
			{
				base.Text = value;
				_labelMessage.Text = value;
			}
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			if (DesignMode)
				return;

			Size = new Size(_tableLayout.Width + (kMessageMargin * 2),
				_tableLayout.Height + (kMessageMargin * 2) + kPointerSize);

			var rc = AdjustedClientRectangle();

			_tableLayout.Location = new Point(
				rc.X + (rc.Width - _tableLayout.Width) / 2,
				rc.Y + (rc.Height - kPointerSize - _tableLayout.Height) / 2);

			Location = new Point(_point.X - (rc.Width / 2), _point.Y - Height);

			// start the fading/waiting timer.
			_timer = new Timer();
			_timer.Interval = 30;
			_timer.Tick += HandleTimerTick;
			_timer.Start();
			_state = TimerState.FadingIn;
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void HandleTimerTick(object sender, EventArgs e)
		{
			try
			{
				double currentOpacity = Opacity;

				if (currentOpacity == 0.001)
					Refresh();

				switch (_state)
				{
					case TimerState.FadingIn:
						Opacity = currentOpacity + 0.05;
						if (currentOpacity == 1.0)
							_state = TimerState.FadeInComplete;
						break;

					case TimerState.FadeInComplete:
						_timer.Interval = 3000;
						_state = TimerState.WaitingToFadeOut;
						break;

					case TimerState.WaitingToFadeOut:
						_timer.Interval = 50;
						if (!AdjustedClientRectangle().Contains(PointToClient(MousePosition)))
							_state = TimerState.FadingOut;
						break;

					case TimerState.FadingOut:
						if (AdjustedClientRectangle().Contains(PointToClient(MousePosition)))
						{
							Opacity = 1;
							_state = TimerState.WaitingToFadeOut;
						}
						else
						{
							Opacity = currentOpacity - 0.05;
							if (Opacity == 0)
								Close();
						}
						break;
				}
			}
			catch { }
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);
			_timer.Stop();
			_timer = null;
		}

		/// ------------------------------------------------------------------------------------
		protected Rectangle AdjustedClientRectangle()
		{
			var rc = ClientRectangle;
			rc.Width = _tableLayout.Width + (kMessageMargin * 2);
			rc.Height = _tableLayout.Height + (kMessageMargin * 2) + kPointerSize;

			if (ClientSize.Height > rc.Height)
				rc.Y = ClientSize.Height - rc.Height;

			return rc;
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			var rc = AdjustedClientRectangle();

			int x = rc.X;
			int y = rc.Y;
			int width = rc.Width;
			int height = rc.Height - kPointerSize;
			int right = x + width - 1;
			int bottom = y + height - 1;

			// Draw and fill call-out bubble
			using (var path = new GraphicsPath())
			{
				// Top Line
				path.AddLine(x + kCornerRadius, y, right - (kCornerRadius * 2), y);

				// Top, Right Corner
				path.AddArc(right - (kCornerRadius * 2), y, kCornerRadius * 2, kCornerRadius * 2, 270, 90);

				// Right vertical Line
				path.AddLine(right, y + kCornerRadius, right, bottom - (kCornerRadius * 2));

				// Bottom, Right Corner
				path.AddArc(right - (kCornerRadius * 2), bottom - (kCornerRadius * 2), kCornerRadius * 2, kCornerRadius * 2, 0, 90);

				// Bottom Line from right corner to intersection with pointer part of call-out.
				path.AddLine(right - (kCornerRadius * 2), bottom, width / 2 + kCornerRadius, bottom);

				// Right-side, slanting part of call-out pointer (line runs southwest).
				path.AddLine(width / 2 + kPointerSize, bottom, width / 2, ClientSize.Height - 1);

				// Left-side, slanting part of call-out pointer (line runs northwest).
				path.AddLine(width / 2, ClientSize.Height - 1, width / 2 - kPointerSize, bottom);

				// Bottom Line from intersection of pointer part of call-out to left right corner.
				path.AddLine(width / 2 - kCornerRadius, bottom, x + kCornerRadius, bottom);

				// Bottom, left Corner
				path.AddArc(x, bottom - (kCornerRadius * 2), kCornerRadius * 2, kCornerRadius * 2, 90, 90);

				// Left vertical Line
				path.AddLine(x, bottom - (kCornerRadius * 2), x, y + kCornerRadius);

				// Top, left Corner
				path.AddArc(x, y, kCornerRadius * 2, kCornerRadius * 2, 180, 90);

				path.CloseFigure();
				e.Graphics.FillPath(SystemBrushes.Info, path);
				e.Graphics.DrawPath(Pens.Gray, path);
			}
		}
	}

	/// ----------------------------------------------------------------------------------------
	public class FadingMessageWindow
	{
		protected Thread _thread;
		protected FadingMessageForm _form;
		protected string _text;
		protected Point _point;

		/// ------------------------------------------------------------------------------------
		public void Show(string text, Point pt)
		{
			if (_thread != null)
				return;

			_text = text;
			_point = pt;

			// For some reason we have to specify a stack size, otherwise we get a stack overflow.
			// The default stack size of 1MB works on WinXP. Needs to be 2MB on Win2K.
			// Don't know what value it's using if we don't specify it.
			_thread = new Thread(ShowForm, 0x200000);
			_thread.CurrentUICulture = Thread.CurrentThread.CurrentUICulture;
			_thread.IsBackground = true;
			_thread.SetApartmentState(ApartmentState.STA);
			_thread.Name = "FadingWindow";
			_thread.Start();
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void ShowForm()
		{
			_form = new FadingMessageForm(_point);
			_form.Text = _text;
			_form.FormClosed += HandleFormClosed;
			_form.ShowDialog();
		}

		/// ------------------------------------------------------------------------------------
		public void Close()
		{
			if (_form != null)
			{
				lock (_form)
				{
					_form.Invoke(new MethodInvoker(_form.Close));
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		private void HandleFormClosed(object sender, FormClosedEventArgs e)
		{
			_form.FormClosed -= HandleFormClosed;
			_form.Dispose();
			_form = null;
			try
			{
				_thread.Abort();
			}
			catch (ThreadAbortException)
			{
				_thread = null;
			}
		}
	}
}
