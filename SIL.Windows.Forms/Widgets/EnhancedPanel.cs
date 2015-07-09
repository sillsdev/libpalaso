using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using SIL.Windows.Forms.Extensions;

namespace SIL.Windows.Forms.Widgets
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Encapsulates a simple panel whose border, by default is 3D if visual styles aren't
	/// enabled and is a single line (painted using visual styles) when visual styles are
	/// enabled. It also supports text, including text containing mnemonic specifiers.
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class EnhancedPanel : Panel
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Allows the subscribers to return text to be drawn in the panel. The panel's Text
		/// property is unaffected by the return value from this delegate.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public delegate string BeforeDrawTextHandler(object sender);

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Allows the subscribers to return text to be drawn in the panel. The panel's Text
		/// property is unaffected by the return value from this delegate.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public event BeforeDrawTextHandler BeforeDrawText;

		public event EventHandler MnemonicInvoked;

		protected TextFormatFlags m_txtFmtFlags = TextFormatFlags.VerticalCenter |
				TextFormatFlags.WordEllipsis | TextFormatFlags.SingleLine |
				TextFormatFlags.LeftAndRightPadding | TextFormatFlags.HidePrefix |
				TextFormatFlags.PreserveGraphicsClipping;

		protected Rectangle m_rcText;
		protected bool m_overrideBorderDrawing;
		private bool m_paintExplorerBarBackground;
		private bool m_drawOnlyBottomBorder;
		private bool m_drawOnlyTopBorder;
		protected Color m_borderColor;
		private bool _autoHeight = false;
		private int _autoHeightPadding = 4;

		/// ------------------------------------------------------------------------------------
		public EnhancedPanel()
		{
			ClipTextForChildControls = true;
			DoubleBuffered = true;
			SetStyle(ControlStyles.UseTextForAccessibility, true);
			base.Font = FontHelper.UIFont;
			m_rcText = ClientRectangle;
			ForeColor = SystemColors.ControlText;

			BorderStyle = (Application.VisualStyleState == VisualStyleState.NoneEnabled ?
				BorderStyle.Fixed3D : BorderStyle.FixedSingle);

			m_borderColor = (PaintingHelper.CanPaintVisualStyle() ?
				VisualStyleInformation.TextControlBorder : SystemColors.ControlDark);
		}

		/// ------------------------------------------------------------------------------------
		public new bool DoubleBuffered
		{
			get { return base.DoubleBuffered; }
			set { base.DoubleBuffered = value; }
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated(e);

			m_borderColor = (PaintingHelper.CanPaintVisualStyle() ?
				VisualStyleInformation.TextControlBorder : SystemColors.ControlDark);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Catch the non client area paint message so we can paint a border that isn't black.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void WndProc(ref Message m)
		{
			base.WndProc(ref m);

			if (m.Msg == PaintingHelper.WM_NCPAINT && m_overrideBorderDrawing)
			{
				PaintingHelper.DrawCustomBorder(this, m_borderColor);
				m.Result = IntPtr.Zero;
				m.Msg = 0;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Make sure the text in the header label acts like a normal label in that it
		/// responds to Alt+letter keys to send focus to the next control in the tab order.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override bool ProcessMnemonic(char charCode)
		{
			if (IsMnemonic(charCode, Text) && Parent != null)
			{
				if (MnemonicGeneratesClick)
				{
					InvokeOnClick(this, EventArgs.Empty);
					return true;
				}

				if (MnemonicInvoked != null)
				{
					MnemonicInvoked(this, EventArgs.Empty);
					return true;
				}

				if (ControlReceivingFocusOnMnemonic != null)
				{
					ControlReceivingFocusOnMnemonic.Focus();
					return true;
				}

				Control ctrl = this;

				do
				{
					ctrl = Parent.GetNextControl(ctrl, true);
				}
				while (ctrl != null && !ctrl.CanSelect);

				if (ctrl != null)
				{
					ctrl.Focus();
					return true;
				}
			}

			return base.ProcessMnemonic(charCode);
		}

		///  ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Browsable(true)]
		public override string Text
		{
			get { return base.Text; }
			set
			{
				base.Text = value;
				CalculateTextRectangle();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the control process the keyboard
		/// mnemonic as a click (like a button) or passes control on to the next control in
		/// the tab order (like a label).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Browsable(true)]
		public bool MnemonicGeneratesClick { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the control that receives focus when the label's text is contains a
		/// mnumonic specifier. When this value is null, then focus is given to the next
		/// control in the tab order.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public Control ControlReceivingFocusOnMnemonic { get; set; }

		/// ------------------------------------------------------------------------------------
		public bool ClipTextForChildControls { get; set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the text format flags used to draw the header label's text.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public TextFormatFlags TextFormatFlags
		{
			get { return m_txtFmtFlags; }
			set
			{
				m_txtFmtFlags = value;
				CalculateTextRectangle();
			}
		}

		/// ------------------------------------------------------------------------------------
		public new BorderStyle BorderStyle
		{
			get {return base.BorderStyle;}
			set
			{
				base.BorderStyle = value;

				m_overrideBorderDrawing = (value == BorderStyle.FixedSingle &&
					(Application.VisualStyleState == VisualStyleState.NonClientAreaEnabled ||
					Application.VisualStyleState == VisualStyleState.ClientAndNonClientAreasEnabled));

				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets the color of the border (only valid when border drawing is
		/// overridden).
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[DefaultValue(typeof(Color), "ControlDark")]
		public Color BorderColor
		{
			get { return m_borderColor; }
			set
			{
				m_borderColor = value;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the background of the panel will
		/// be painted using the visual style's explorer bar element.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool PaintExplorerBarBackground
		{
			get { return m_paintExplorerBarBackground; }
			set
			{
				m_paintExplorerBarBackground = value;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the only border to draw is on the
		/// bottom edge. For this property to work, the BorderStyle property must be set to
		/// None and a border color must be specified.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DrawOnlyBottomBorder
		{
			get { return m_drawOnlyBottomBorder; }
			set
			{
				m_drawOnlyBottomBorder = value;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets or sets a value indicating whether or not the only border to draw is on the
		/// top edge. For this property to work, the BorderStyle property must be set to
		/// None and a border color must be specified.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool DrawOnlyTopBorder
		{
			get { return m_drawOnlyTopBorder; }
			set
			{
				m_drawOnlyTopBorder = value;
				Invalidate();
			}
		}

		/// ------------------------------------------------------------------------------------
		[DefaultValue(4)]
		public int AutoHeightPadding
		{
			get { return _autoHeightPadding; }
			set { _autoHeightPadding = value; }
		}

		/// ------------------------------------------------------------------------------------
		[DefaultValue(false)]
		public bool AutoHeight
		{
			get { return _autoHeight; }
			set { _autoHeight = value; }
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Calculates the rectangle of the text when there are child controls. This method
		/// assumes that controls to the right of the text should clip the text. However, if
		/// the controls are above and below the text, this method will probably screw up
		/// the text drawing.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected void CalculateTextRectangle()
		{
			m_rcText = ClientRectangle;

			if (ClipTextForChildControls)
			{
				int rightExtent = Controls.Cast<Control>()
					.Aggregate(m_rcText.Right, (current, child) => Math.Min(current, child.Left));

				if (rightExtent != m_rcText.Right &&
					m_rcText.Contains(new Point(rightExtent, m_rcText.Top + m_rcText.Height / 2)))
				{
					m_rcText.Width -= (m_rcText.Right - rightExtent);

					// Give a bit more to account for the padding.
					if ((m_txtFmtFlags & TextFormatFlags.LeftAndRightPadding) > 0)
						m_rcText.Width += 8;
				}
			}

			Invalidate();
		}

		/// ------------------------------------------------------------------------------------
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (AutoHeight && (specified & BoundsSpecified.Height) == BoundsSpecified.Height)
			{
				using (var g = CreateGraphics())
					height = TextRenderer.MeasureText(g, "X", Font).Height + _autoHeightPadding;
			}

			base.SetBoundsCore(x, y, width, height, specified);
		}


		/// ------------------------------------------------------------------------------------
		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			CalculateTextRectangle();

			e.Control.Resize += HandleChildControlResize;
			e.Control.LocationChanged += HandleChildControlLocationChanged;
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnControlRemoved(ControlEventArgs e)
		{
			e.Control.Resize -= HandleChildControlResize;
			e.Control.LocationChanged -= HandleChildControlLocationChanged;

			base.OnControlRemoved(e);
			CalculateTextRectangle();
		}

		/// ------------------------------------------------------------------------------------
		protected void HandleChildControlLocationChanged(object sender, EventArgs e)
		{
			CalculateTextRectangle();
		}

		/// ------------------------------------------------------------------------------------
		protected void HandleChildControlResize(object sender, EventArgs e)
		{
			CalculateTextRectangle();
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// After the panel has been resized, force the border to be repainted. I found that
		/// often, after resizing the panel at runtime (e.g. when it's docked inside a
		/// splitter panel and the splitter moved), the portion of the border that was newly
		/// repainted didn't show the overriden border color handled by the WndProc above.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnClientSizeChanged(EventArgs e)
		{
			base.OnClientSizeChanged(e);

			if (m_overrideBorderDrawing)
				this.SendMessage(PaintingHelper.WM_NCPAINT, 1, 0);

			CalculateTextRectangle();
		}

		/// ------------------------------------------------------------------------------------
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			//if (Site != null && !Site.DesignMode && m_paintExplorerBarBackground)
			if (PaintExplorerBarBackground)
			{
				VisualStyleElement element = VisualStyleElement.ExplorerBar.NormalGroupBackground.Normal;
				if (PaintingHelper.CanPaintVisualStyle(element))
				{
					VisualStyleRenderer renderer = new VisualStyleRenderer(element);
					renderer.DrawBackground(e.Graphics, ClientRectangle);
					return;
				}
			}

			base.OnPaintBackground(e);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Paint the text on the panel, if there is any.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected override void OnPaint(PaintEventArgs e)
		{
			DrawBottomBorder(e);
			DrawTopBorder(e);

			var text = (BeforeDrawText != null ? BeforeDrawText(this) : Text);

			if (!string.IsNullOrEmpty(text))
				TextRenderer.DrawText(e.Graphics, text, Font, m_rcText, ForeColor, m_txtFmtFlags);

			base.OnPaint(e);
		}

		/// ------------------------------------------------------------------------------------
		public void DrawBottomBorder(PaintEventArgs e)
		{
			if (m_drawOnlyBottomBorder)
			{
				var rc = ClientRectangle;
				using (var pen = new Pen(m_borderColor))
					e.Graphics.DrawLine(pen, 0, rc.Height - 1, rc.Width, rc.Height - 1);
			}
		}

		/// ------------------------------------------------------------------------------------
		public void DrawTopBorder(PaintEventArgs e)
		{
			if (m_drawOnlyTopBorder)
			{
				var rc = ClientRectangle;
				using (var pen = new Pen(m_borderColor))
					e.Graphics.DrawLine(pen, 0, 0, rc.Width, 0);
			}
		}
	}
}
