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
	}
}