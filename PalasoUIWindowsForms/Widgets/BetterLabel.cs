using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Widgets
{
	/// <summary>
	/// Labels are fairly limitted even in .net, but on mono so far, multi-line
	/// labels are trouble.  This class uses TextBox to essentially be a better
	/// cross-platform label.
	/// </summary>
	public partial class BetterLabel : TextBox
	{
		private Brush _textBrush;
		private Brush _backgroundBrush;
		private int _previousWidth=0;

		public BetterLabel()
		{
			InitializeComponent();
			ReadOnly = true;
			Enabled = false;
			ForeColor = SystemColors.ControlText;
			SetStyle(ControlStyles.UserPaint,true);
			_backgroundBrush = new SolidBrush(BackColor);
			_textBrush = new SolidBrush(ForeColor);
#if __MonoCS__
			// These settings (and the overrides below) don't keep the BetterLabel from getting focus in Mono,
			// but they do keep it from showing highlighted selections within the label.
			Enabled = true;
			// These may not work any better than setting UserPaint true, but ...
			SetStyle(ControlStyles.UserMouse, true);
			SetStyle(ControlStyles.Selectable | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
#endif
		}

#if __MonoCS__
		protected override void OnMouseDown(MouseEventArgs args)
		{
			// ignore the mouse totally.
		}
		protected override void OnMouseMove(MouseEventArgs args)
		{
			// ignore the mouse totally.
		}
		protected override void OnMouseUp(MouseEventArgs args)
		{
			// ignore the mouse totally.
		}
#endif

		/// <summary>
		/// we custom draw so that we can be ReadOnly without being necessarily grey
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>
		/// Mono's TextBox doesn't call OnPaint, so this doesn't work on Linux.
		/// (A Mono comment claims that MS/.Net doesn't call OnPaint, which it does,
		/// and it's unclear how to fix the Mono code reliably.)  Mono also seems to
		/// ignore ControlStyles settings almost totally.
		/// </remarks>
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			e.Graphics.FillRectangle(_backgroundBrush,DisplayRectangle);
			e.Graphics.DrawString(this.Text, this.Font, _textBrush,this.DisplayRectangle);
		}

		//make it transparent
		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			try
			{
				if (DesignMode)
					return;

				if (BackColor != SystemColors.Control && BackColor != Color.White)
					return; //they want a weird background color, so don't track the parent
				Control backgroundColorSource = FindForm();
				if (backgroundColorSource == null)
				{   //if we can't get the form, the next best thing is our container (e.g., a table)
					backgroundColorSource = Parent;
				}
				if (backgroundColorSource != null)
				{
					BackColor = backgroundColorSource.BackColor;
					backgroundColorSource.BackColorChanged += ((x, y) => BackColor = backgroundColorSource.BackColor);
				}
			}
			catch (Exception)
			{
				//trying to harden this against the mysteriously disappearing from a host designer
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			//this is apparently dangerous to do in the constructor
			//Font = new Font(SystemFonts.MessageBoxFont.FontFamily, Font.Size, Font.Style);
			if(Font==SystemFonts.DefaultFont)
				Font = SystemFonts.MessageBoxFont;//sets the default, which can then be customized in the designer

			DetermineHeight();
			base.OnTextChanged(e);
		}

		private void DetermineHeight()
		{
			using (var g = this.CreateGraphics())
			{
				var sz = g.MeasureString(Text, this.Font, Width).ToSize();
				//leave as fixed width
				Height = sz.Height;
			}
		}

		protected override void OnForeColorChanged(EventArgs e)
		{
			if (_textBrush != null)
				_textBrush.Dispose();

			_textBrush = new SolidBrush(ForeColor);
			base.OnForeColorChanged(e);
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			if (_backgroundBrush != null)
				_backgroundBrush.Dispose();

			_backgroundBrush = new SolidBrush(BackColor);
			base.OnBackColorChanged(e);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			if (_previousWidth!=Width)
			{
				DetermineHeight();
				_previousWidth = Width;
			}
			base.OnSizeChanged(e);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			Invalidate(true);
		}
	}
}
