// Copyright (c) 2010-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Threading;
using JetBrains.Annotations;
using SIL.Extensions;

namespace SIL.Progress
{
	public class StatusProgress : IProgress
	{
		public SynchronizationContext SyncContext { get; set; }
		public string LastStatus { get; private set; }
		public string LastWarning { get; private set; }
		public string LastError { get; private set; }
		public bool CancelRequested { get; set; }
		public bool WarningEncountered { get => !string.IsNullOrEmpty(LastWarning); }
		public bool ErrorEncountered { get => !string.IsNullOrEmpty(LastError);
			set { }
		}

		public IProgressIndicator ProgressIndicator { get; set; }


		public  void WriteStatus(string message, params object[] args)
		{
			LastStatus = message.FormatWithErrorStringInsteadOfException(args);
		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{

		}

		public void WriteWarning(string message, params object[] args)
		{
			LastWarning = message.FormatWithErrorStringInsteadOfException(args);
			LastStatus = LastWarning;
		}

		public void WriteException(Exception error)
		{
			WriteError(error.Message);
		}

		public void WriteError(string message, params object[] args)
		{
			LastError = message.FormatWithErrorStringInsteadOfException(args);
			LastStatus = LastError;
		}

		public void WriteMessage(string message, params object[] args)
		{
		}

		public void WriteVerbose(string message, params object[] args)
		{
		}

		public bool ShowVerbose
		{
			set {  }
		}

		[PublicAPI]
		public bool WasCancelled
		{
			get
			{
				if(LastWarning!=null)
					return LastWarning.ToLower().Contains("cancelled");
				return false;
			} //improve: this is pretty flimsy
		}

		public void Clear()
		{
			LastError = LastWarning = LastStatus = string.Empty;
		}
	}
}