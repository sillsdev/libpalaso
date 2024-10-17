// Copyright (c) 2010-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Threading;

namespace SIL.Progress
{
	public interface IProgress
	{
		void WriteStatus(string message, params object[] args);
		void WriteMessage(string message, params object[] args);
		void WriteMessageWithColor(string colorName, string message, params object[] args);
		void WriteWarning(string message, params object[] args);
		void WriteException(Exception error);
		void WriteError(string message, params object[] args);
		void WriteVerbose(string message, params object[] args);
		bool ShowVerbose {set; }
		bool CancelRequested { get; set; }
		bool ErrorEncountered { get; set; }
		IProgressIndicator ProgressIndicator { get; set; }
		SynchronizationContext SyncContext { get; set; }
	}
}