using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Palaso.Progress;
using Palaso.Progress.LogBox;

namespace Palaso.WinForms
{


	public class SimpleStatusProgress : Label, IProgress
	{
		public void WriteStatus(string message, params object[] args)
		{
			string theMessage = GenericProgress.SafeFormat(message, args);
			LastStatus = theMessage;
			if (SyncContext != null)
			{
				SyncContext.Post(UpdateText, theMessage);
			}
			else
			{
				Text = theMessage;
			}
		}

		private void UpdateText(object state)
		{
			Text = state as string;
		}

		public SynchronizationContext SyncContext { get; set; }
		public void WriteMessage(string message, params object[] args) { }
		public void WriteMessageWithColor(string colorName, string message, params object[] args) { }
		public void WriteWarning(string message, params object[] args)
		{
			WarningEncountered = true;
			LastWarning = GenericProgress.SafeFormat(message, args);
			LastStatus = LastWarning;
		}
		public void WriteException(Exception error)
		{
			LastException = error;
			ErrorEncountered = true;
		}
		public void WriteError(string message, params object[] args)
		{
			ErrorEncountered = true;
			LastError = GenericProgress.SafeFormat(message, args);
			LastStatus = LastError;
		}
		public void WriteVerbose(string message, params object[] args) { }
		public bool ShowVerbose { set { } }
		public bool CancelRequested { get; set; }
		public bool WarningEncountered { get; set; }
		public bool ErrorEncountered { get; set; }
		public Exception LastException { get; set; }
		public IProgressIndicator ProgressIndicator { get; set; }
		public string LastStatus { get; private set; }
		public string LastWarning { get; private set; }
		public string LastError { get; private set; }
		public void Reset()
		{
			LastError = "";
			LastWarning = "";
			LastStatus = "";
			CancelRequested = false;
			WarningEncountered = false;
			ErrorEncountered = false;
			WriteStatus("");
		}

	}
}
