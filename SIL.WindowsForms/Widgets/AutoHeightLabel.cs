using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SIL.WindowsForms.Widgets
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// This class extends the label control to provide 2 extra features. 1) Allows left and
	/// right margins to be set so when the label is docked, text can still be indented.
	/// 2) Automatically adjusts the height of the label to accomodate all the text in the
	/// control. Then it can be added to a stacked group of controls in a flow layout panel
	/// and the controls below the label will automatically get pushed down as the label grows.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class AutoHeightLabel : Label
	{
		private const int GapBetweenImageAndText = 10;

		private bool m_autoSizingInProgress;
		private int m_leftMargin;
		private int m_rightMargin;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoHeightLabel"/> class.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public AutoHeightLabel()
		{
			AutoSize = false;
			AutoEllipsis = true;
		}

		/// ------------------------------------------------------------------------------------
		public override string Text
		{
			get { return base.Text; }
			set
			{
				base.Text = value;
				AdjustHeight();
			}
		}

		/// ------------------------------------------------------------------------------------
		public override Font Font
		{
			get { return base.Font; }
			set
			{
				base.Font = value;
				AdjustHeight();
			}
		}

		/// ------------------------------------------------------------------------------------
		public new Image Image
		{
			get { return base.Image; }
			set
			{
				base.Image = value;
				AdjustHeight();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the left margin in pixels.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[DefaultValue(0)]
		public int LeftMargin
		{
			get { return m_leftMargin; }
			set
			{
				m_leftMargin = value;
				AdjustHeight();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the right margin.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[DefaultValue(0)]
		public int RightMargin
		{
			get { return m_rightMargin; }
			set
			{
				m_rightMargin = value;
				AdjustHeight();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the X offset where the labels text will be drawn within the control's client
		/// area.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public int TextsXOffset
		{
			get { return LeftMargin + (Image == null ? 0 : Image.Width + GapBetweenImageAndText); }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Adjusts the height of the label to accomodate all the text within the current
		/// width of the control, less the left and right margins.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		private void AdjustHeight()
		{
			m_autoSizingInProgress = true;

			var rc = ClientRectangle;
			rc.Width -= (LeftMargin + RightMargin);

			if (Image != null)
				rc.Width -= (Image.Width + GapBetweenImageAndText);

			using (var g = CreateGraphics())
			{
				int height = TextRenderer.MeasureText(g, Text, Font,
					new Size(rc.Width, int.MaxValue), TextFormatFlags.WordBreak).Height;

				if (Image != null && height < Image.Height)
					height = Image.Height;

				Height = (DesignMode && height == 0 ? 20 : height);
			}

			m_autoSizingInProgress = false;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.SizeChanged"/> event.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			if (!m_autoSizingInProgress)
				AdjustHeight();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			if (Image != null)
				e.Graphics.DrawImageUnscaled(Image, LeftMargin, 0);

			var rc = ClientRectangle;
			rc.Width -= (LeftMargin + RightMargin);
			rc.X += LeftMargin;

			if (Image != null)
			{
				rc.Width -= (Image.Width + GapBetweenImageAndText);
				rc.X += (Image.Width + GapBetweenImageAndText);
			}

			TextRenderer.DrawText(e.Graphics, Text, Font, rc, ForeColor,
				TextFormatFlags.WordBreak);
		}
	}
}
