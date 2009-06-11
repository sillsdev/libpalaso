using System;
using System.Drawing;
using System.Windows.Forms;

namespace Palaso.Reporting
{
	public partial class ProblemNotificationDialog : Form
	{
		private DialogResult _alternateButton1DialogResult;

		public string DialogTitle
		{
			get { return Text; }
			set { Text = value; }
		}

		public string Message
		{
			get { return _message.Text; }
			set { _message.Text = value; }
		}

		public string ReoccurenceMessage
		{
			get { return _reoccurenceMessage.Text; }
			set { _reoccurenceMessage.Text = value;}
		}


		public static void Show(string message)
		{
			using (var d = new ProblemNotificationDialog())
			{
				d.DialogTitle = "Problem";
				d.Message = message;
				d.ShowDialog();
			}
		}

//        public static void Show(string message, string dialogTitle, string buttonLabel, string reocurrenceMessage)
//        {
//            ProblemNotificationDialog d = new ProblemNotificationDialog();
//
//            d.Text = dialogTitle;
//            d._message.Text = message;
//            d._reoccurenceMessage.Text = reocurrenceMessage;
//
//            d.ShowDialog();
//        }

		/// <summary>
		/// Use this one if you need to customize the dialog, e.g. to setup an alternate button
		/// </summary>
		/// <param name="message"></param>
		/// <param name="dialogTitle"></param>
		public ProblemNotificationDialog(string message, string dialogTitle):this()
		{
			Text = dialogTitle;
			_message.Text = message;
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
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.Close();
		}

		private void OnAlternateButton1_Click(object sender, EventArgs e)
		{
			this.DialogResult = _alternateButton1DialogResult;
			this.Close();
		}


		public void EnableAlternateButton1(string label, DialogResult resultToReturn)
		{
			_alternateButton1DialogResult = resultToReturn;
			_alternateButton1.Text = label;
			_alternateButton1.Visible = true;
			_alternateButton1.Enabled = true;
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