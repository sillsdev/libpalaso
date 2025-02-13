// Copyright (c) 2010-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Threading;
using SIL.Extensions;

namespace SIL.Progress
{
	public class ConsoleProgress : IProgress, IDisposable
	{
		public static int  indent = 0;
		private bool _verbose;

		public ConsoleProgress()
		{
		}

		public ConsoleProgress(string message, params string[] args)
		{
			WriteStatus(message, args);
			indent++;
		}
		public bool ErrorEncountered { get; set; }

		public IProgressIndicator ProgressIndicator { get; set; }
		public SynchronizationContext SyncContext { get; set; }
		public void WriteStatus(string message, params object[] args)
		{
			Console.Write("                          ".Substring(0, indent * 2));
			Console.WriteLine(message.FormatWithErrorStringInsteadOfException(args));
		}

		public void WriteMessage(string message, params object[] args)
		{
			WriteStatus(message, args);

		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			WriteStatus(message, args);
		}


		public void WriteWarning(string message, params object[] args)
		{
			WriteStatus("Warning: "+ message, args);
		}

		public void WriteException(Exception error)
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
			ErrorEncountered = true;
		}


		public void WriteError(string message, params object[] args)
		{
			WriteStatus("Error: "+ message, args);
			ErrorEncountered = true;
		}

		public void WriteVerbose(string message, params object[] args)
		{
			if(!_verbose)
				return;
			var lines = message.FormatWithErrorStringInsteadOfException(args);
			foreach (var line in lines.Split('\n'))
			{
				WriteStatus("    " + line);
			}
		}

		public bool ShowVerbose
		{
			set => _verbose = value;
		}

		public bool CancelRequested { get; set; }

		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
			if(indent>0)
				indent--;
		}
	}
}