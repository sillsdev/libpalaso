using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public partial class ProblemNotificationDialog : Form
	{
		public static void Show(string message)
		{
			Show(message, "Problem", "&OK", string.Empty);
		}
		public static void Show(string message, string dialogTitle, string buttonLabel, string reocurrenceMessage)
		{
			ProblemNotificationDialog d = new ProblemNotificationDialog();
			d.Text = dialogTitle;
			d._message.Text = message;
			d._reoccurenceMessage.Text = reocurrenceMessage;
			d.ShowDialog();
		}

		private ProblemNotificationDialog()
		{
			InitializeComponent();
		}

		private void NonFatalErrorDialog_Load(object sender, EventArgs e)
		{
			_icon.Image = SystemIcons.Warning.ToBitmap();
		}

		private void _acceptButton_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void _message_TextChanged(object sender, EventArgs e)
		{
			using(var g = this.CreateGraphics())
			{
				int textHeight = (int) Math.Ceiling(g.MeasureString(_message.Text, _message.Font, _message.Width).Height);
				if(textHeight > _message.Height)
				{
					this.Height += (textHeight - _message.Height) + _message.Font.Height*2/*fudge*/;

					//hack... I would just like to get it to not grow larger than the screen
					if (this.Height > 600)
						this.Height = 600;
				}
			}
		}
	}
}