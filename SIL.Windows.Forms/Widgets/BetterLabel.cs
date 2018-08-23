using System;
using System.Drawing;
using System.Windows.Forms;
using SIL.PlatformUtilities;

namespace SIL.Windows.Forms.Widgets
{
	/// <summary>
	/// Labels are fairly limited even in .NET, but on mono so far, multi-line
	/// labels are trouble.  This class uses TextBox to essentially be a better
	/// cross-platform label.
	/// </summary>
	public partial class BetterLabel : TextBox
	{
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

		protected override void OnMouseDown(MouseEventArgs args)
		{
			if (Platform.IsWindows)
				base.OnMouseDown(args);
			else
			{
				if (IsTextSelectable)
					base.OnMouseDown(args);
				// Or ignore the mouse totally.
			}
		}

		protected override void OnMouseMove(MouseEventArgs args)
		{
			if (Platform.IsWindows)
				base.OnMouseMove(args);
			else
			{
				if (IsTextSelectable)
					base.OnMouseMove(args);
				// Or ignore the mouse totally.
			}
		}

		protected override void OnMouseUp(MouseEventArgs args)
		{
			if (Platform.IsWindows)
				base.OnMouseUp(args);
			else
			{
				if (IsTextSelectable)
					base.OnMouseUp(args);
				// Or ignore the mouse totally.
			}
		}

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
			TextRenderer.DrawText(e.Graphics, Text, Font, DisplayRectangle, ForeColor);
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
				// Use this rather than MeasureString, which uses the obsolete GDI+ and can crash on some
				// non-Roman strings (e.g., ones containing ZWJ).
				Height = TextRenderer.MeasureText(g, Text, this.Font, new Size(Width, int.MaxValue)).Height;
			}
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
