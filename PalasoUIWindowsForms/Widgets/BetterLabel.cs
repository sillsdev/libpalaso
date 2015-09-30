using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.UI.WindowsForms.Widgets
{
	/// <summary>
	/// Labels are fairly limited even in .NET, but on mono so far, multi-line
	/// labels are trouble.  This class uses TextBox to essentially be a better
	/// cross-platform label.
	/// </summary>
	public partial class BetterLabel : TextBox
	{
		private Brush _textBrush;
		private Brush _backgroundBrush;
		private bool _isTextSelectable;

		public BetterLabel()
		{
			InitializeComponent();
			ReadOnly = true;
			IsTextSelectable = false;
			ForeColor = SystemColors.ControlText;
			SetStyle(ControlStyles.UserPaint,true);
			_backgroundBrush = new SolidBrush(BackColor);
			_textBrush = new SolidBrush(ForeColor);
		}

		/// <summary>
		/// Should the label allow a user to select and copy the text, such as from an error message?
		/// </summary>
		public bool IsTextSelectable
		{
			get
			{
				return _isTextSelectable;
			}
			set
			{
				_isTextSelectable = value;

				// Always Enabled on Mono so text is black not grey.
				if (Environment.OSVersion.Platform == PlatformID.Unix)
				{
					Enabled = true;
					return;
				}

				Enabled = value;
			}
		}

#if __MonoCS__
		protected override void OnMouseDown(MouseEventArgs args)
		{
			if (IsTextSelectable)
				base.OnMouseDown(args);
			// Or ignore the mouse totally.
		}
		protected override void OnMouseMove(MouseEventArgs args)
		{
			if (IsTextSelectable)
				base.OnMouseMove(args);
			// Or ignore the mouse totally.
		}
		protected override void OnMouseUp(MouseEventArgs args)
		{
			if (IsTextSelectable)
				base.OnMouseUp(args);
			// Or ignore the mouse totally.
		}
#endif

		/// <summary>
		/// Custom draw to be ReadOnly without being necessarily grey.
		/// </summary>
		/// <param name="e"></param>
		/// <remarks>
		/// Mono's TextBox doesn't call OnPaint, so this doesn't work on Linux.
		/// (A Mono comment claims that MS/.Net doesn't call OnPaint, which it does,
		/// and it's unclear how to fix the Mono code reliably.)  Mono also seems to
		/// ignore ControlStyles settings almost totally.
		/// The text is black in Mono if Enabled is true.
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
				// This may require the use of mono-sil which fixes MeasureString to calculate the correct value to be tall enough.
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
			DetermineHeight();
			base.OnSizeChanged(e);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			Invalidate(true);
		}
	}
}
