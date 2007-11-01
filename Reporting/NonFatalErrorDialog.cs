using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public partial class NonFatalErrorDialog : Form
	{
		public static void Show(string message)
		{
			Show(message, "Problem", "&OK");
		}
		public static void Show(string message, string dialogTitle, string buttonLabel)
		{
			NonFatalErrorDialog d = new NonFatalErrorDialog();
			d.Text = dialogTitle;
			d._message.Text = message;
			d.ShowDialog();
		}

		private NonFatalErrorDialog()
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