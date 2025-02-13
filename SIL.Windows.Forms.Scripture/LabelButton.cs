// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2025 SIL Global
// <copyright from='2003' to='2025' company='SIL Global'>
//		Copyright (c) 2025 SIL Global
//
//		Distributable under the terms of the MIT License (http://sil.mit-license.org/)
// </copyright>
#endregion
//
// File: LabelButton.cs
// --------------------------------------------------------------------------------------------

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace SIL.Windows.Forms.Scripture
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Implements a simple button-like control derived from the panel control. For the
	/// applications where I use this control, I was using a button control, but that was
	/// giving me some undesired focus problems when the button was the child of other
	/// non-form controls.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class LabelButton : Label
	{
		/// <summary>Event which occurs when the control's background is being painted
		/// (includes the border).</summary>
		public event PaintEventHandler PaintBackground;
		/// <summary>Event which occurs when the control's text is being painted.</summary>
		public event PaintEventHandler PaintText;
		/// <summary>Event which occurs when the control's image is being painted.</summary>
		public event PaintEventHandler PaintImage;

		private bool m_mouseIsOver;
		private bool m_mouseIsDown;
		private bool m_shadeWhenMouseOver = true;
		private int m_textLeadingMargin;
		private StringFormat m_stringFormat;
		private ButtonState m_state = ButtonState.Normal;
		private PaintState m_paintState = PaintState.Normal;

		/// <summary>The various states that cause different painting behavior.</summary>
		protected enum PaintState
		{
			/// <summary>A button control should be painted normally, as though it's
			/// not pushed and the mouse isn't over it.</summary>
			Normal,

			/// <summary>A button control should be painted as though it's pushed and the
			/// mouse is over it.</summary>
			MouseDown,

			/// <summary>A button control should be painted as though it's not pushed and the
			/// mouse is over it.</summary>
			MouseOver,

			/// <summary>A button control should be painted as though it's pushed and the
			/// mouse is not over it (this is for buttons that can toggle)</summary>
			Pushed
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Constructs a PanelButton
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public LabelButton()
		{
			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint |
				ControlStyles.DoubleBuffer, true);

			BackColor = Color.FromArgb(200, SystemColors.Control);
			TextAlign = ContentAlignment.MiddleCenter;
			Name = "LabelButton";
			ResizeRedraw = true;

			SetTextAlignment();
		}

		#region Properties
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the StringFormat object used to draw the text on the PanelButton when
		/// UseCompatibleTextRendering is true.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public StringFormat TextFormat
		{
			get => m_stringFormat;
			set
			{
				if (value != null)
				{
					m_stringFormat = value;
					// Allow this old property to work with new rendering approach.
					FormatFlags = ConvertStringFormatToTextFormatFlags();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the TextFormatFlags enum used to draw the text on the PanelButton when
		/// UseCompatibleTextRendering is false.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public TextFormatFlags FormatFlags { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value for the number of pixels of margin to insert before the text
		/// (when right-to-left is specified, then the margin is on the right side. Otherwise
		/// it's on the left).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public int TextLeadingMargin
		{
			get => m_textLeadingMargin;
			set => m_textLeadingMargin = (value >= 0 ? value : 0);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets a value indicating whether or not the text on the button didn't fit in it's
		/// display rectangle and had to be clipped.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool TextIsClipped { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not a button acts like a toggle button.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool CanToggle { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the state (i.e. normal or pushed) of the button.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public ButtonState State
		{
			get => m_state;
			set
			{
				if (m_state != value)
				{
					m_state = value;
					Invalidate();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the button get's shaded when the
		/// mouse moves over it.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool ShadeWhenMouseOver
		{
			get => m_shadeWhenMouseOver;
			set
			{
				if (m_shadeWhenMouseOver != value)
				{
					m_shadeWhenMouseOver = value;
					Invalidate();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected bool ButtonIsOn => State == ButtonState.Pushed;

		#endregion

		#region Overrides
		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			if (PaintBackground == null)
				OnPaintBackground(e);
			else
				PaintBackground(this, e);

			if (PaintText == null)
				OnPaintText(e);
			else
				PaintText(this, e);

			if (Image != null)
			{
				if (PaintImage == null)
					OnPaintImage(e);
				else
					PaintImage(this, e);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Paints the background and border portion of the button.
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected new virtual void OnPaintBackground(PaintEventArgs e)
		{
			DeterminePaintState();

			// Fill with white first, before filling with partially transparent background color.
			using (var brush = new SolidBrush(Color.White))
			{
				e.Graphics.FillRectangle(brush, ClientRectangle);

				brush.Color = GetBackColorShade(m_paintState, BackColor);
				e.Graphics.FillRectangle(brush, ClientRectangle);

				if (m_paintState != PaintState.Normal)
				{
					Rectangle rc = ClientRectangle;
					rc.Width--;
					rc.Height--;
					e.Graphics.DrawRectangle(new Pen(new SolidBrush(SystemColors.ActiveCaption)), rc);
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Paints the Text on the buttons.
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected virtual void OnPaintText(PaintEventArgs e)
		{
			Rectangle rc = ClientRectangle;
			using (var brush = new SolidBrush(Enabled ? ForeColor : SystemColors.GrayText))
			{
				// If the mouse is over the button then give the text a raised look.
				if (m_paintState == PaintState.MouseOver)
					rc.Offset(-1, -1);
				else
					rc.Height--;

				// Account for any specified leading margin.
				if (TextLeadingMargin > 0)
				{
					rc.Width -= TextLeadingMargin;
					if (RightToLeft == RightToLeft.No)
						rc.X += TextLeadingMargin;
				}

				// Now we'll draw the text.
				e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				Size sz;
				if (UseCompatibleTextRendering)
				{
					e.Graphics.DrawString(Text, Font, brush, rc, TextFormat);
					sz = e.Graphics.MeasureString(Text, Font, new Point(rc.X, rc.Y), TextFormat).ToSize();
				}
				else
				{
					TextRenderer.DrawText(e.Graphics, Text, Font, new Point(rc.X, rc.Y), Enabled ? ForeColor : SystemColors.GrayText, FormatFlags);
					sz = TextRenderer.MeasureText(e.Graphics, Text, Font, rc.Size, FormatFlags);
				}
				// Check if the text was clipped.
				TextIsClipped = (sz.Width > rc.Width || sz.Height > rc.Height);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Convert the information stored in the TextFormat (System.Drawing.StringFormat) object
		/// into a TextFormatFlags bitfield enum as much as possible.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private TextFormatFlags ConvertStringFormatToTextFormatFlags()
		{
			var flags = TextFormatFlags.Default;
			bool rtl = (TextFormat.FormatFlags & StringFormatFlags.DirectionRightToLeft) == StringFormatFlags.DirectionRightToLeft;
			if (!rtl)
				rtl = IsRightToLeft(this);
			if (rtl)
				flags |= TextFormatFlags.RightToLeft;
			switch (TextFormat.Alignment)	// horizontal alignment
			{
			case StringAlignment.Center:	flags |= TextFormatFlags.HorizontalCenter;						break;
			case StringAlignment.Far:		flags |= (rtl ? TextFormatFlags.Left : TextFormatFlags.Right);	break;
			case StringAlignment.Near:		flags |= (rtl ? TextFormatFlags.Right : TextFormatFlags.Left);	break;
			}
			switch (TextFormat.LineAlignment)	// vertical alignment (assume top-to-bottom)
			{
			case StringAlignment.Center:	flags |= TextFormatFlags.VerticalCenter;	break;
			case StringAlignment.Far:		flags |= TextFormatFlags.Bottom;			break;
			case StringAlignment.Near:		flags |= TextFormatFlags.Top;				break;
			}
			switch (TextFormat.Trimming)
			{
			case StringTrimming.Character:			/* default operation? */				break;
			case StringTrimming.Word:				flags |= TextFormatFlags.WordBreak;		break;
			case StringTrimming.EllipsisCharacter:	flags |= TextFormatFlags.EndEllipsis;	break;
			case StringTrimming.EllipsisWord:		flags |= TextFormatFlags.WordEllipsis;	break;
			case StringTrimming.EllipsisPath:		flags |= TextFormatFlags.PathEllipsis;	break;
			case StringTrimming.None:				flags |= TextFormatFlags.NoClipping;	break;
			}
			if ((TextFormat.FormatFlags & StringFormatFlags.FitBlackBox) == StringFormatFlags.FitBlackBox)
				flags |= TextFormatFlags.NoPadding;	/* nearest equivalent? */
			if ((TextFormat.FormatFlags & StringFormatFlags.NoClip) == StringFormatFlags.NoClip)
				flags |= TextFormatFlags.NoClipping;
			if ((TextFormat.FormatFlags & StringFormatFlags.NoWrap) == StringFormatFlags.NoWrap)
			{
				flags |= TextFormatFlags.SingleLine;	/* nearest equivalent? */
				flags &= ~TextFormatFlags.WordBreak;
			}
			// No equivalent for these as far as I can tell:
			// StringFormatFlags.DirectionVertical
			// StringFormatFlags.DisplayFormatControl
			// StringFormatFlags.NoFontFallback
			// StringFormatFlags.MeasureTrailingSpaces
			// StringFormatFlags.LineLimit

			return flags;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Try to figure out if we're supposed to display the text right-to-left
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private static bool IsRightToLeft(Control c)
		{
			if (c.RightToLeft == RightToLeft.Inherit)
				return c.Parent != null && IsRightToLeft(c.Parent);
			return c.RightToLeft == RightToLeft.Yes;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Paints the image on the buttons.
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected virtual void OnPaintImage(PaintEventArgs e)
		{
			Rectangle rc = ClientRectangle;

			// If the mouse is over the button then give the image a raised look.
			if (m_paintState == PaintState.MouseOver)
			{
				rc.Offset(-1, -1);
				rc.Height++;
			}

			DrawImage(e.Graphics, Image, rc, ImageAlign);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			m_mouseIsOver = true;
			Invalidate();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			m_mouseIsOver = false;
			Invalidate();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set's the button's state and invalidates it to force redrawing.
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left)
			{
				m_mouseIsDown = true;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set's the button's state and invalidates it to force redrawing.
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left)
			{
				m_mouseIsDown = false;
				m_mouseIsOver = MouseInBounds(e.X, e.Y);

				if (m_mouseIsOver && CanToggle)
				{
					State =	(State == ButtonState.Normal ?
						ButtonState.Pushed : ButtonState.Normal);
				}

				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="e"></param>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (m_mouseIsDown)
			{
				bool inBounds = MouseInBounds(e.X, e.Y);

				if (inBounds != m_mouseIsOver)
				{
					m_mouseIsOver = inBounds;
					Invalidate();
				}
			}
		}

		#endregion

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="state"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		protected Color GetBackColorShade(PaintState state)
		{
			return GetBackColorShade(state, SystemColors.Control);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="state"></param>
		/// <param name="normalBack"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		protected Color GetBackColorShade(PaintState state, Color normalBack)
		{
			if (Enabled)
			{
				switch (state)
				{
					case PaintState.MouseOver:	return Color.FromArgb(50, SystemColors.ActiveCaption);
					case PaintState.MouseDown: 	return Color.FromArgb(65, SystemColors.ActiveCaption);
					case PaintState.Pushed:		return Color.FromArgb(40, SystemColors.ActiveCaption);
				}
			}

			return normalBack;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private void DeterminePaintState()
		{
			if ((m_mouseIsDown && m_mouseIsOver && ButtonIsOn) ||
				(m_mouseIsDown && m_mouseIsOver) ||
				(m_mouseIsDown && ButtonIsOn) ||
				(m_mouseIsOver && ButtonIsOn))
			{
				m_paintState = PaintState.MouseDown;
			}
			else if (m_mouseIsDown || m_mouseIsOver && ShadeWhenMouseOver)
				m_paintState = PaintState.MouseOver;
			else if (ButtonIsOn)
				m_paintState = PaintState.Pushed;
			else
				m_paintState = PaintState.Normal;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		private bool MouseInBounds(int x, int y)
		{
			return (x >= 0 && y >= 0 && x < Width && y < Height);
		}

		private void SetTextAlignment()
		{
			m_stringFormat = new StringFormat(StringFormat.GenericTypographic);
			m_stringFormat.Alignment = StringAlignment.Center;
			m_stringFormat.LineAlignment = StringAlignment.Center;
			m_stringFormat.Trimming = StringTrimming.EllipsisCharacter;
			m_stringFormat.FormatFlags |= StringFormatFlags.NoWrap;
		}
	}
}
