using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SIL.Windows.Forms.Extensions;
using Timer = System.Windows.Forms.Timer;

namespace SIL.Windows.Forms.ClearShare.WinFormsUI
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

		protected Timer Timer { get; set; }
		protected Point MsgPoint { get; set; }
		protected TimerState State { get; set; }

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
			MsgPoint = pt;
		}

		/// ------------------------------------------------------------------------------------
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				Timer?.Dispose();
			}

			Timer = null;

			base.Dispose(disposing);
		}

		/// ------------------------------------------------------------------------------------
		protected override bool ShowWithoutActivation => true;

		/// ------------------------------------------------------------------------------------
		public override string Text
		{
			get => base.Text;
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

			Location = new Point(MsgPoint.X - (rc.Width / 2), MsgPoint.Y - Height);

			// start the fading/waiting timer.
			Timer = new Timer();
			Timer.Interval = 30;
			Timer.Tick += HandleTimerTick;
			Timer.Start();
			State = TimerState.FadingIn;
		}

		/// ------------------------------------------------------------------------------------
		protected virtual void HandleTimerTick(object sender, EventArgs e)
		{
			try
			{
				double currentOpacity = Opacity;

				if (currentOpacity.Equals(0.001))
					Refresh();

				switch (State)
				{
					case TimerState.FadingIn:
						Opacity = currentOpacity + 0.05;
						if (currentOpacity.Equals(1.0))
							State = TimerState.FadeInComplete;
						break;

					case TimerState.FadeInComplete:
						Timer.Interval = 3000;
						State = TimerState.WaitingToFadeOut;
						break;

					case TimerState.WaitingToFadeOut:
						Timer.Interval = 50;
						if (!AdjustedClientRectangle().Contains(PointToClient(MousePosition)))
							State = TimerState.FadingOut;
						break;

					case TimerState.FadingOut:
						if (AdjustedClientRectangle().Contains(PointToClient(MousePosition)))
						{
							Opacity = 1;
							State = TimerState.WaitingToFadeOut;
						}
						else
						{
							Opacity = currentOpacity - 0.05;
							if (Opacity.Equals(0))
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
			Timer.Stop();
			Timer = null;
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
				e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
				e.Graphics.FillPath(SystemBrushes.Info, path);
				e.Graphics.DrawPath(Pens.Gray, path);
			}
		}
	}

	/// ----------------------------------------------------------------------------------------
	public class FadingMessageWindow
	{
		private FadingMessageForm m_msgForm;
		private readonly object m_syncLock = new object();

		/// <summary>
		/// Shows a fading message with the specified text at the specified screen location.
		/// </summary>
		/// <remarks>Caller is responsible for ensuring this is called on the UI thread.</remarks>
		public void Show(string text, Point pt)
		{
			lock (m_syncLock)
			{
				Close();
				m_msgForm = new FadingMessageForm(pt)
				{
					Text = text
				};
				m_msgForm.FormClosed += (s, e) =>
				{
					lock (m_syncLock)
					{
						m_msgForm.Dispose();
						m_msgForm = null; // Clear the reference when the form is closed
					}
				};
				m_msgForm.Show();
			}
		}

		public void Close()
		{
			lock (m_syncLock)
			{
				if (m_msgForm == null || m_msgForm.IsDisposed)
					return;

				// Close the form on the UI thread
				m_msgForm.SafeInvoke(() =>
					{
						// Technically, since this is being invoked synchronously, we don't need to
						// re-obtain the lock here, but it makes the IDE (or Resharper) happy.
						lock (m_syncLock)
							m_msgForm?.Close();
					}, "closing fading message",
					ControlExtensions.ErrorHandlingAction.IgnoreIfDisposed, true);
			}
		}
	}
}
