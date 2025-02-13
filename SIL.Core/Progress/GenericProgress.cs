// Copyright (c) 2010-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Threading;
using SIL.Extensions;

namespace SIL.Progress
{
	public abstract class GenericProgress : IProgress
	{
		public  int  indent = 0;
		private bool _verbose;

		public GenericProgress()
		{
		}

		public static string SafeFormat(string format, params object[] args)
		{
			return format.FormatWithErrorStringInsteadOfException(args);
		}

		public bool CancelRequested { get; set; }
		public abstract void WriteMessage(string          message,   params object[] args);
		public abstract void WriteMessageWithColor(string colorName, string          message, params object[] args);
		public SynchronizationContext SyncContext { get; set; }
		public bool ErrorEncountered { get; set; }

		public IProgressIndicator ProgressIndicator { get; set; }

		public void WriteStatus(string message, params object[] args)
		{
			WriteMessage(message, args);
		}

		public void WriteWarning(string message, params object[] args)
		{
			WriteMessage("Warning: " + message, args);
		}


		public virtual void WriteException(Exception error)
		{
			WriteError(error.Message);
			ErrorEncountered = true;
		}

		public void WriteError(string message, params object[] args)
		{
			WriteMessage("Error:" + message, args);
			ErrorEncountered = true;
		}

		public void WriteVerbose(string message, params object[] args)
		{
			if(!_verbose)
				return;
			var lines = message.FormatWithErrorStringInsteadOfException(args);
			foreach (var line in lines.Split('\n'))
			{
				WriteMessage("   " + line);
			}

		}

		public bool ShowVerbose
		{
			set => _verbose = value;
		}
	}
}