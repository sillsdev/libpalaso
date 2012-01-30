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
		public BetterLabel()
		{
			InitializeComponent();
		}

		//make it transparent
		private void BetterLabel_ParentChanged(object sender, System.EventArgs e)
		{
			try
			{
				if (DesignMode)
					return;
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
			Font =  SystemFonts.MessageBoxFont;

		}
	}

	public class BetterLinkLabel : BetterLabel
	{
		public BetterLinkLabel()
		{
			ReadOnly = true;
			this.MouseEnter += new EventHandler(BetterLinkLabel_MouseEnter);
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
				System.Diagnostics.Process.Start(URL);
			}
		}
	}
}