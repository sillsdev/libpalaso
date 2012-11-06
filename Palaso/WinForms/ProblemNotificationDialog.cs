using System;
using System.Drawing;
using System.Windows.Forms;
using Palaso.Properties;

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

		public Image Icon
		{
			get { return _icon.Image; }
			set { _icon.Image = value; }
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
			_message.BackColor = BackColor;
			_message.ForeColor = ForeColor;
			_reoccurenceMessage.Font = SystemFonts.MessageBoxFont;
			_icon.Image = SystemIcons.Warning.ToBitmap();
			base.Icon = SystemIcons.Warning;
		}

		/// <summary>
		/// Use this one if you need to customize the dialog, e.g. to setup an alternate button
		/// </summary>
		/// <param name="message"></param>
		/// <param name="dialogTitle"></param>
		public ProblemNotificationDialog(string message, string dialogTitle) : this(message, dialogTitle, null)
		{
		}

		/// <summary>
		/// Use this one if you need to customize the dialog, e.g. to setup an alternate button
		/// </summary>
		public ProblemNotificationDialog(string message, string dialogTitle, Image icon) : this()
		{
			if (icon != null)
				_icon.Image = icon;

		   Text = dialogTitle;
			_message.Text = message;


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

		private void HandleMessageTextChanged(object sender, EventArgs e)
		{
			AdjustHeights();
		}

		private void AdjustHeights()
		{
			//hack: I don't know why this is needed, but it was chopping off the last line in the case of the following message:
			// "There was a problem connecting to the Internet.\r\nWarning: This machine does not have a live network connection.\r\nConnection attempt failed."
			const int kFudge = 50;
			_message.Height = GetDesiredTextBoxHeight()+kFudge;


			var desiredWindowHeight = tableLayout.Height + Padding.Top +
				Padding.Bottom + (Height - ClientSize.Height);

			var scn = Screen.FromControl(this);
			int maxWindowHeight = scn.WorkingArea.Height - 25;

			if (desiredWindowHeight > maxWindowHeight)
			{
				_message.Height -= (desiredWindowHeight - maxWindowHeight);
				_message.ScrollBars = ScrollBars.Vertical;
			}

			Height = Math.Min(desiredWindowHeight, maxWindowHeight);
		}

		private int GetDesiredTextBoxHeight()
		{
			if (!IsHandleCreated)
				CreateHandle();

			using (var g = _message.CreateGraphics())
			{
				const TextFormatFlags flags = TextFormatFlags.NoClipping | TextFormatFlags.NoPadding |
					TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak;

				return TextRenderer.MeasureText(g, _message.Text, _message.Font,
					new Size(_message.ClientSize.Width, 0), flags).Height;
			}
		}

		private void ProblemNotificationDialog_Load(object sender, EventArgs e)
		{
			TaskBarFlasher.FlashTaskBarButtonUntilFocussed(this);
		}
	}
}