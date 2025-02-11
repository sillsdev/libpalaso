using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace SIL.Windows.Forms.Widgets
{
	/// ----------------------------------------------------------------------------------------
	public class XButton : Label
	{
		public delegate bool DrawBackgroundHandler(XButton button, PaintEventArgs e, PaintState state);
		public event DrawBackgroundHandler DrawBackground;

		private bool m_drawLeftArrowButton;
		private bool m_drawRightArrowButton;
		private bool m_checked;
		private bool m_mouseDown;
		private bool m_mouseOver;
		private PaintState m_state = PaintState.Normal;

		/// ------------------------------------------------------------------------------------
		public XButton()
		{
			base.AutoSize = false;
			base.BackColor = SystemColors.Control;
			base.Font = new Font("Marlett", 9, GraphicsUnit.Point);
			Size = new Size(16, 16);

			SetStyle(ControlStyles.UserPaint |
				ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer, true);

			FormatFlags = TextFormatFlags.NoPadding |
			   TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPrefix |
			   TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine |
			   TextFormatFlags.PreserveGraphicsClipping;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the button's checked state changes
		/// when clicked.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool CanBeChecked { get; set; }

		/// ------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TextFormatFlags FormatFlags { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the button is checked.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool Checked
		{
			get { return m_checked; }
			set
			{
				if (CanBeChecked)
				{
					m_checked = value;
					Invalidate();
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not an X is drawn on the button when no
		/// image, text or arrow direction is specified. By default, when no image, text or
		/// arrow direction is specified, the button is drawn with an X (like a close window-
		/// type of X). However, when DrawEmpty is true, nothing will be drawn except the
		/// highlighted look given when the mouse is over or down or when the button is checked.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DrawEmpty { get; set; }

		/// ------------------------------------------------------------------------------------
		public new Image Image
		{
			get { return base.Image; }
			set
			{
				base.Image = value;

				if (value != null)
				{
					m_drawLeftArrowButton = false;
					m_drawRightArrowButton = false;
				}

				OnSystemColorsChanged(null);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the button should be drawn with a
		/// left pointing arrow (like the left button of a horizontal scrollbar).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DrawLeftArrowButton
		{
			get { return m_drawLeftArrowButton; }
			set
			{
				m_drawLeftArrowButton = value;
				if (value)
				{
					m_drawRightArrowButton = false;
					base.Image = null;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the button should be drawn with a
		/// right pointing arrow (like the right button of a horizontal scrollbar).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DrawRightArrowButton
		{
			get { return m_drawRightArrowButton; }
			set
			{
				m_drawRightArrowButton = value;
				if (value)
				{
					m_drawLeftArrowButton = false;
					base.Image = null;
				}
			}
		}

		/// ------------------------------------------------------------------------------------
		public void PerformClick()
		{
			InvokeOnClick(this, EventArgs.Empty);
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged(e);
			if (Image != null)
				BackColor = Color.Transparent;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Repaint the button when the mouse isn't over it.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			m_mouseOver = false;
			Invalidate();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Change appearance when mouse is pressed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (e.Button == MouseButtons.Left)
			{
				m_mouseDown = true;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Change appearance when the mouse button is released.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (e.Button == MouseButtons.Left)
			{
				m_mouseDown = false;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Track when the mouse leaves the control when a mouse button is pressed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			m_mouseOver = ClientRectangle.Contains(e.Location);
			PaintState newState = (m_mouseOver ? PaintState.Hot : PaintState.Normal);

			if (m_mouseOver && m_mouseDown)
				newState = PaintState.HotDown;

			if (newState != m_state)
			{
				m_state = newState;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);

			if (m_mouseOver || Checked)
				m_state = (m_mouseDown ? PaintState.HotDown : PaintState.Hot);
			else
				m_state = PaintState.Normal;

			if (DrawBackground != null && DrawBackground(this, e, m_state))
				return;

			Rectangle rc = ClientRectangle;

			using (SolidBrush br = new SolidBrush(BackColor))
				e.Graphics.FillRectangle(br, rc);

			if (m_state != PaintState.Normal)
				PaintingHelper.DrawHotBackground(e.Graphics, rc, m_state);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			if (Image != null)
				DrawWithImage(e);
			else if (m_drawLeftArrowButton || m_drawRightArrowButton)
				DrawArrow(e);
			else if (!string.IsNullOrEmpty(Text))
				DrawText(e);
			else if (!DrawEmpty)
				DrawWithX(e);
			else
				base.OnPaint(e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws the button's text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void DrawText(PaintEventArgs e)
		{
			Color clr = (Enabled ? ForeColor : SystemColors.GrayText);
			TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle, clr, FormatFlags);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draw the button with text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void DrawWithX(PaintEventArgs e)
		{
			Rectangle rc = ClientRectangle;

			Color clr = (m_state == PaintState.Normal ? SystemColors.ControlDarkDark :
				SystemColors.ControlText);

			// The 'r' in the Marlette font is the close button symbol 'X'
			TextRenderer.DrawText(e.Graphics, "r", Font, rc, clr, FormatFlags);

			// Draw the border around the button.
			rc.Width--;
			rc.Height--;
			using (Pen pen = new Pen(clr))
				e.Graphics.DrawRectangle(pen, rc);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws the button with an image.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void DrawWithImage(PaintEventArgs e)
		{
			if (Image == null)
				return;

			int x = (Width - Image.Width) / 2;
			int y = (Height - Image.Height) / 2;
			Rectangle rc = new Rectangle(x, y, Image.Width, Image.Height);

			if (Enabled)
				e.Graphics.DrawImageUnscaledAndClipped(Image, rc);
			else
				ControlPaint.DrawImageDisabled(e.Graphics, Image, x, y, SystemColors.Control);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Draws the button with an image.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void DrawArrow(PaintEventArgs e)
		{
			Rectangle rc = ClientRectangle;

			// If possible, render the button with visual styles. Otherwise,
			// paint the plain Windows 2000 push button.
			var element = GetCorrectVisualStyleArrowElement();
			if (PaintingHelper.CanPaintVisualStyle(element))
			{
				VisualStyleRenderer renderer = new VisualStyleRenderer(element);
				renderer.DrawParentBackground(e.Graphics, rc, this);
				renderer.DrawBackground(e.Graphics, rc);
				return;
			}

			if (Font.SizeInPoints != 12)
				Font = new Font(Font.FontFamily, 12, GraphicsUnit.Point);

			ControlPaint.DrawButton(e.Graphics, rc,
				(m_state == PaintState.HotDown ? ButtonState.Pushed : ButtonState.Normal));

			// In the Marlette font, '3' is the left arrow and '4' is the right.
			string arrowGlyph = (m_drawLeftArrowButton ? "3" : "4");

			Color clr = (Enabled ? SystemColors.ControlText : SystemColors.GrayText);

			// The 'r' in the Marlette font is the close button symbol 'X'
			TextRenderer.DrawText(e.Graphics, arrowGlyph, Font, rc, clr, FormatFlags);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the correct visual style arrow button and in the correct state.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private VisualStyleElement GetCorrectVisualStyleArrowElement()
		{
			if (m_drawLeftArrowButton)
			{
				if (!Enabled)
					return VisualStyleElement.Spin.DownHorizontal.Disabled;

				if (m_state == PaintState.Normal)
					return VisualStyleElement.Spin.DownHorizontal.Normal;

				return (m_state == PaintState.Hot ?
						VisualStyleElement.Spin.DownHorizontal.Hot :
						VisualStyleElement.Spin.DownHorizontal.Pressed);
			}

			if (!Enabled)
				return VisualStyleElement.Spin.UpHorizontal.Disabled;

			if (m_state == PaintState.Normal)
				return VisualStyleElement.Spin.UpHorizontal.Normal;

			return (m_state == PaintState.Hot ?
					VisualStyleElement.Spin.UpHorizontal.Hot :
					VisualStyleElement.Spin.UpHorizontal.Pressed);
		}
	}
}
