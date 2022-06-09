using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SIL.Windows.Forms.SuperToolTip
{
	[ToolboxItem(false)]
	public partial class SuperToolTipWindowData : UserControl
	{
		#region Private Members

		int bodyPicWidth = 100;

		private SuperToolTipInfo _superToolTipInfo;

		#endregion

		#region Public Properties

		public SuperToolTipInfo SuperInfo
		{
			get { return _superToolTipInfo; }
			set
			{
				//palaso additions

				_superToolTipInfo.OffsetForWhereToDisplay = value.OffsetForWhereToDisplay;

				bool redrawBackground = false;
				// if (!_superToolTipInfo.Equals(value))
				{
					if (_superToolTipInfo.BackgroundGradientBegin != value.BackgroundGradientBegin)
					{
						_superToolTipInfo.BackgroundGradientBegin = value.BackgroundGradientBegin;
						redrawBackground = true;
					}
					if (_superToolTipInfo.BackgroundGradientMiddle != value.BackgroundGradientMiddle)
					{
						_superToolTipInfo.BackgroundGradientMiddle = value.BackgroundGradientMiddle;
						redrawBackground = true;
					}
					if (_superToolTipInfo.BackgroundGradientEnd != value.BackgroundGradientEnd)
					{
						_superToolTipInfo.BackgroundGradientEnd = value.BackgroundGradientEnd;
						redrawBackground = true;
					}
					if (_superToolTipInfo.BodyImage != value.BodyImage || _superToolTipInfo.BodyForeColor != value.BodyForeColor ||
						_superToolTipInfo.BodyFont != value.BodyFont || _superToolTipInfo.BodyText != value.BodyText)
					{
						_superToolTipInfo.BodyImage = value.BodyImage;
						_superToolTipInfo.BodyForeColor = value.BodyForeColor;
						_superToolTipInfo.BodyText = value.BodyText;
						_superToolTipInfo.BodyFont = value.BodyFont;

						picBody.Visible = _superToolTipInfo.BodyImage == null ? false : true;
						picBody.Image = _superToolTipInfo.BodyImage;
						SetBodyData();
					}
					if (value.ShowHeader)
					{
						_superToolTipInfo.ShowHeader = value.ShowHeader;

						if (_superToolTipInfo.HeaderFont != value.HeaderFont || _superToolTipInfo.HeaderText != value.HeaderText
							|| _superToolTipInfo.HeaderForeColor != value.HeaderForeColor || _superToolTipInfo.ShowHeaderSeparator != value.ShowHeaderSeparator)
						{
							_superToolTipInfo.HeaderText = value.HeaderText;
							_superToolTipInfo.HeaderForeColor = value.HeaderForeColor;
							_superToolTipInfo.HeaderFont = value.HeaderFont;
							_superToolTipInfo.ShowHeaderSeparator = value.ShowHeaderSeparator;

							SetHeaderData();
						}
					}
					lblHeader.Visible = value.ShowHeader;

					if (value.ShowFooter)
					{
						_superToolTipInfo.ShowFooter = value.ShowFooter;

						if (_superToolTipInfo.FooterFont != value.FooterFont || _superToolTipInfo.FooterText != value.FooterText
							|| _superToolTipInfo.FooterForeColor != value.FooterForeColor || _superToolTipInfo.FooterImage != value.FooterImage || _superToolTipInfo.ShowFooterSeparator != value.ShowFooterSeparator)
						{
							_superToolTipInfo.FooterText = value.FooterText;
							_superToolTipInfo.FooterForeColor = value.FooterForeColor;
							_superToolTipInfo.FooterFont = value.FooterFont;
							_superToolTipInfo.FooterImage = value.FooterImage;
							_superToolTipInfo.ShowFooterSeparator = value.ShowFooterSeparator;
							picFooter.Visible = _superToolTipInfo.FooterImage == null ? false : true;

							SetFooterData();
						}
					}
					flwPnlFooter.Visible = value.ShowFooter;

					if (redrawBackground) RedrawBackground();
				}
			}
		}

		#endregion

		#region Constructors

		public SuperToolTipWindowData()
		{
			InitializeComponent();
			_superToolTipInfo = new SuperToolTipInfo();
		}

		#endregion

		#region Painting

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			Rectangle rect1 = new Rectangle(0, Height / 2, Width, Height / 2);
			Rectangle rect2 = new Rectangle(0, 0, Width, Height / 2);
			using (LinearGradientBrush b2 = new LinearGradientBrush(new Rectangle(0, Height / 2 - 1, Width, Height / 2), _superToolTipInfo.BackgroundGradientMiddle, _superToolTipInfo.BackgroundGradientEnd, LinearGradientMode.Vertical))
			using (LinearGradientBrush b1 = new LinearGradientBrush(new Rectangle(0, 0, Width, Height / 2), _superToolTipInfo.BackgroundGradientBegin, _superToolTipInfo.BackgroundGradientMiddle, LinearGradientMode.Vertical))
			{
				e.Graphics.FillRectangle(b2, rect1);
				e.Graphics.FillRectangle(b1, rect2);
			}

		}

		#endregion

		#region Helpers
		private void RedrawBackground()
		{

		}
		private void SetHeaderData()
		{
			lblHeader.Text = _superToolTipInfo.HeaderText;
			lblHeader.ForeColor = _superToolTipInfo.HeaderForeColor;
			lblHeader.Font = _superToolTipInfo.HeaderFont;
			lblHeader.Visible = _superToolTipInfo.ShowHeader;
			pnlHeaderSeparator.Invalidate();
			pnlHeaderSeparator.Visible = _superToolTipInfo.ShowHeader;
		}
		private void SetFooterData()
		{
			lblFooter.Text = _superToolTipInfo.FooterText;
			lblFooter.Font = _superToolTipInfo.FooterFont;
			lblFooter.ForeColor = _superToolTipInfo.FooterForeColor;
			if (_superToolTipInfo.FooterImage != null)
			{
				picFooter.Image = _superToolTipInfo.FooterImage;
			}
			pnlFooterSeparator.Invalidate();

			flwPnlFooter.Visible = _superToolTipInfo.ShowFooter;
			pnlFooterSeparator.Visible = _superToolTipInfo.ShowFooter;
		}
		private void SetBodyData()
		{
			// Do this before setting the Text so lblBody's parents
			// can properly adjust to this max size. BL-11297
			lblBody.MaximumSize = new Size(200, 1000);

			lblBody.Text = _superToolTipInfo.BodyText;
			lblBody.Font = _superToolTipInfo.BodyFont;
			lblBody.ForeColor = _superToolTipInfo.BodyForeColor;
			if (_superToolTipInfo.BodyImage != null)
			{
				picBody.Image = _superToolTipInfo.BodyImage;
				int height = _superToolTipInfo.BodyImage.Height * bodyPicWidth / _superToolTipInfo.BodyImage.Width;
				picBody.Size = new Size(picBody.Size.Width, height);
			}

		}
		#endregion

		private void pnlHeaderSeparator_Paint(object sender, PaintEventArgs e)
		{
			if (_superToolTipInfo.ShowHeader && _superToolTipInfo.ShowHeaderSeparator)
			{
				DrawSeparator(e.Graphics, pnlHeaderSeparator);
			}
		}

		private void pnlFooterSeparator_Paint(object sender, PaintEventArgs e)
		{
			if (_superToolTipInfo.ShowFooter && _superToolTipInfo.ShowFooterSeparator)
			{
				DrawSeparator(e.Graphics, pnlFooterSeparator);
			}
		}

		private void DrawSeparator(Graphics graphics, Panel p)
		{
			graphics.DrawLine(Pens.White, new Point(0, p.Height / 2), new Point(p.Width, p.Height / 2));
			graphics.DrawLine(Pens.Black, new Point(0, p.Height / 2 + 1), new Point(p.Width, p.Height / 2 + 1));
		}
		protected override void OnSizeChanged(EventArgs e)
		{
			pnlHeaderSeparator.Width = this.Width * 95 / 100;
			pnlFooterSeparator.Width = this.Width * 95 / 100;
			base.OnSizeChanged(e);
		}
	}
}
