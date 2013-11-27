using System;
using System.Threading;
using System.Windows.Forms;
using Palaso.Progress;

namespace Palaso.UI.WindowsForms.Progress
{
	/// <summary>
	/// Just conveys status, not all messages
	/// </summary>
	public class LabelStatus : IProgress
	{
		private Label _box;

		public LabelStatus(Label box)
		{
			_box = box;
		}

		public SynchronizationContext SyncContext { get; set; }

		public bool ShowVerbose
		{
			set { }
		}

		public bool ErrorEncountered { get; set; }

		public IProgressIndicator ProgressIndicator { get; set; }

		public bool CancelRequested { get; set; }


		public void WriteStatus(string message, params object[] args)
		{
			try
			{
				_box.Invoke(new Action(() =>
										   {
											   _box.Text = GenericProgress.SafeFormat(message + Environment.NewLine,
																					  args);
										   }));
			}
			catch (Exception)
			{

			}
		}

		public void WriteMessage(string message, params object[] args)
		{

		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{

		}

		public void WriteWarning(string message, params object[] args)
		{
		}

		public void WriteException(Exception error)
		{
			WriteError("Error");
			ErrorEncountered = true;
		}

		public void WriteError(string message, params object[] args)
		{
			WriteStatus(message, args);
			ErrorEncountered = true;
		}

		public void WriteVerbose(string message, params object[] args)
		{

		}

	}
}