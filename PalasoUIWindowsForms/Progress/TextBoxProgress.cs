using System;
using System.Windows.Forms;
using Palaso.Progress.LogBox;

namespace Palaso.UI.WindowsForms.Progress
{
	public class TextBoxProgress : GenericProgress
	{
		private RichTextBox _box;

		public TextBoxProgress(RichTextBox box)
		{
			_box = box;
			_box.Multiline = true;
		}


		public override void WriteMessage(string message, params object[] args)
		{
			try
			{
				// if (_box.InvokeRequired)
				_box.Invoke(new Action(() =>
				{
					_box.Text += "                          ".Substring(0, indent * 2);
					_box.Text += GenericProgress.SafeFormat(message + Environment.NewLine, args);
				}));
			}
			catch (Exception)
			{

			}
			//            _box.Invoke(new Action<TextBox, int>((box, indentX) =>
			//            {
			//                box.Text += "                          ".Substring(0, indentX * 2);
			//                box.Text += GenericProgress.SafeFormat(message + Environment.NewLine, args);
			//            }), _box, indent);
		}

		public override void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			WriteMessage(message, args);
		}


		public override void WriteException(Exception error)
		{
			WriteError("Exception: ");
			WriteError(error.Message);
			WriteError(error.StackTrace);
			if (error.InnerException != null)
			{
				++indent;
				WriteError("Inner: ");
				WriteException(error.InnerException);
				--indent;
			}
		}


	}
}
