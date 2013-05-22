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
			SetStyle(ControlStyles.UserPaint,true);
			_backgroundBrush = new SolidBrush(BackColor);
			_textBrush = new SolidBrush(ForeColor);
		}

		/// <summary>
		/// we custom draw so that we can be ReadOnly without being necessarily grey
		/// </summary>
		/// <param name="e"></param>
		protected override void OnPaint(PaintEventArgs e)
		{
			e.Graphics.FillRectangle(_backgroundBrush,DisplayRectangle);
			e.Graphics.DrawString(this.Text, this.Font, _textBrush,this.DisplayRectangle);
		}

		//make it transparent
		private void BetterLabel_ParentChanged(object sender, System.EventArgs e)
		{
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
			catch (Exception error)
			{
				//trying to harden this against the mysteriously disappearing from a host designer
			}
		}


		private void BetterLabel_TextChanged(object sender, System.EventArgs e)
		{
			//this is apparently dangerous to do in the constructor
			//Font = new Font(SystemFonts.MessageBoxFont.FontFamily, Font.Size, Font.Style);
			if(Font==SystemFonts.DefaultFont)
				Font = SystemFonts.MessageBoxFont;//sets the default, which can then be customized in the designer

			DetermineHeight();
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

		private void BetterLabel_ForeColorChanged(object sender, EventArgs e)
		{
			if (_textBrush != null)
				_textBrush.Dispose();

			_textBrush = new SolidBrush(ForeColor);
		}

		private void BetterLabel_BackColorChanged(object sender, EventArgs e)
		{
			if (_backgroundBrush != null)
				_backgroundBrush.Dispose();

			_backgroundBrush = new SolidBrush(BackColor);
		}

		private void BetterLabel_SizeChanged(object sender, EventArgs e)
		{
			if (_previousWidth!=Width)
			{
				DetermineHeight();
				_previousWidth = Width;
			}
		}
	}

	public class BetterLinkLabel : BetterLabel
	{
		public BetterLinkLabel()
		{
			ReadOnly = false;
			Enabled = true;
			this.MouseEnter += new EventHandler(BetterLinkLabel_MouseEnter);
			SetStyle(ControlStyles.UserPaint, false);
			EnabledChanged+=new EventHandler(BetterLinkLabel_EnabledChanged);
		}

		private void BetterLinkLabel_EnabledChanged(object sender, EventArgs e)
		{

		}

		void BetterLinkLabel_MouseEnter(object sender, EventArgs e)
		{
			Cursor = Cursors.Hand;
		}
		protected override void OnLayout(LayoutEventArgs levent)
		{
			base.OnLayout(levent);
			ForeColor = Color.Blue;//TODO
			Font = new Font(Font, FontStyle.Underline);
		}

		/// <summary>
		/// The url to launch
		/// </summary>
		/// <param name="e"></param>
		public string URL { get; set; }

		protected override void OnClick(EventArgs e)
		{
			base.OnClick(e);
			if(!string.IsNullOrEmpty(URL))
			{
				try
				{
					System.Diagnostics.Process.Start(URL);
				}
				catch(Exception)
				{
					Palaso.Reporting.ErrorReport.NotifyUserOfProblem(string.Format("Could not follow that link to {0}. Your computer is not set up to follow links of that kind, but you can try typing it into your web browser.",URL));
				}
			}
		}
	}
}