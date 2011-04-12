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
			using (var d = new ProblemNotificationDialog(message, "Problem"))
				d.ShowDialog();
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

		private ProblemNotificationDialog()
		{
			InitializeComponent();
			_message.Font = SystemFonts.MessageBoxFont;
			_reoccurenceMessage.Font = SystemFonts.MessageBoxFont;
			_icon.Image = SystemIcons.Warning.ToBitmap();
		}

		/// <summary>
		/// Use this one if you need to customize the dialog, e.g. to setup an alternate button
		/// </summary>
		/// <param name="message"></param>
		/// <param name="dialogTitle"></param>
		public ProblemNotificationDialog(string message, string dialogTitle) : this()
		{
			Text = dialogTitle;
			_message.Text = message;

			// Sometimes, setting the text in the previous line will force the table layout control
			// to resize itself accordingly, which will fire its SizeChanged event. However,
			// sometimes the text is not long enough to force the table layout to be resized,
			// therefore, we need to call it manually, just to be sure the form gets sized correctly.
			HandleTableLayoutSizeChanged(null, null);
		}

		private void HandleTableLayoutSizeChanged(object sender, EventArgs e)
		{
			if (!IsHandleCreated)
				CreateHandle();

			var desiredHeight = tableLayout.Height + Padding.Top + Padding.Bottom + (Height - ClientSize.Height);
			var scn = Screen.FromControl(this);
			Height = Math.Min(desiredHeight, scn.WorkingArea.Height - 20);
			AutoScroll = (desiredHeight > scn.WorkingArea.Height - 20);
		}

		private void _acceptButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		private void OnAlternateButton1_Click(object sender, EventArgs e)
		{
			DialogResult = _alternateButton1DialogResult;
			Close();
		}

		public void EnableAlternateButton1(string label, DialogResult resultToReturn)
		{
			_alternateButton1DialogResult = resultToReturn;
			_alternateButton1.Text = label;
			_alternateButton1.Visible = true;
			_alternateButton1.Enabled = true;
		}

		//private void _message_TextChanged(object sender, EventArgs e)
		//{
		//    using(var g = this.CreateGraphics())
		//    {
		//        int textHeight = (int) Math.Ceiling(g.MeasureString(_message.Text, _message.Font, _message.Width).Height);
		//        if (textHeight > _message.Height)
		//        {
		//            Height += (textHeight - _message.Height) + _message.Font.Height*2/*fudge*/;

		//            //hack... I would just like to get it to not grow larger than the screen
		//            if (Height > 600)
		//                Height = 600;
		//        }
		//    }
		//}
	}
}