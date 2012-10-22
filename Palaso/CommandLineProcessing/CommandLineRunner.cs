using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Palaso.Progress;

namespace Palaso.CommandLineProcessing
{
	///<summary>
	/// Used to run external processes, with support for things like timeout,
	/// async progress reporting, and user UI cancellability.
	///</summary>
	public class CommandLineRunner
	{
		public static int TimeoutSecondsOverrideForUnitTests = 10000;
		private ProcessOutputReaderBase _processReader;
		private Process _process;

		/// <summary>
		/// This one doesn't attemtp to influence the encoding used
		/// </summary>
		public static ExecutionResult Run(string exePath, string arguments, string fromDirectory, int secondsBeforeTimeOut, IProgress progress)
		{
			return Run(exePath, arguments, null, fromDirectory, secondsBeforeTimeOut, progress,null);
		}

		public static ExecutionResult Run(string exePath, string arguments, Encoding encoding, string fromDirectory, int secondsBeforeTimeOut, IProgress progress)
		{
			return Run(exePath, arguments, encoding, fromDirectory, secondsBeforeTimeOut, progress, null);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="exePath"></param>
		/// <param name="arguments"></param>
		/// <param name="encoding"></param>
		/// <param name="fromDirectory"></param>
		/// <param name="secondsBeforeTimeOut"></param>
		/// <param name="progress"></param>
		/// <param name="actionForReportingProgress"> Normally a simple thing like this: (s)=>progress.WriteVerbose(s). If you pass null, then the old synchronous reader will be used instead, with no feedback to the user as the process runs.
		/// </param>
		/// <returns></returns>
		public static ExecutionResult Run(string exePath, string arguments, Encoding encoding, string fromDirectory, int secondsBeforeTimeOut, IProgress progress, Action<string> actionForReportingProgress)
		{
			return new CommandLineRunner().Start(exePath, arguments, encoding, fromDirectory, secondsBeforeTimeOut, progress,
										actionForReportingProgress);
		}

		public bool Abort(int secondsBeforeTimeout)
		{
			_process.Kill();
			return _process.WaitForExit(secondsBeforeTimeout*1000);
		}

		/// <summary>
		/// use this one if you're doing a long running task that you'll have running in a thread, so that you need a way to abort it
		/// </summary>
		public ExecutionResult Start(string exePath, string arguments, Encoding encoding, string fromDirectory, int secondsBeforeTimeOut, IProgress progress, Action<string> actionForReportingProgress)
		{
			progress.WriteVerbose("running '{0} {1}' from '{2}'", exePath, arguments, fromDirectory);
			ExecutionResult result = new ExecutionResult();
			_process = new Process();
			_process.StartInfo.RedirectStandardError = true;
			_process.StartInfo.RedirectStandardOutput = true;
			_process.StartInfo.UseShellExecute = false;
			_process.StartInfo.CreateNoWindow = true;
			_process.StartInfo.WorkingDirectory = fromDirectory;
			_process.StartInfo.FileName = exePath;
			_process.StartInfo.Arguments = arguments;
			if(encoding!=null)
			{
				_process.StartInfo.StandardOutputEncoding = encoding;
			}
			if (actionForReportingProgress!=null)
				_processReader = new AsyncProcessOutputReader(_process, progress, actionForReportingProgress);
			else
				_processReader = new SynchronousProcessOutputReader(_process, progress);

			try
			{
				Debug.WriteLine("CommandLineRunner Starting at "+DateTime.Now.ToString());
				_process.Start();
			}
			catch (Win32Exception error)
			{
				throw;
			}

			if (secondsBeforeTimeOut > TimeoutSecondsOverrideForUnitTests)
				secondsBeforeTimeOut = TimeoutSecondsOverrideForUnitTests;

			bool timedOut = false;
			Debug.WriteLine("CommandLineRunner Reading at " + DateTime.Now.ToString("HH:mm:ss.ffff"));
			if (!_processReader.Read(secondsBeforeTimeOut))
			{
				timedOut = !progress.CancelRequested;
				try
				{
					if (_process.HasExited)
					{
						progress.WriteWarning("Process exited, cancelRequested was {0}", progress.CancelRequested);
					}
					else
					{
						if(timedOut)
							progress.WriteWarning("({0}) Timed Out...", exePath);
						progress.WriteWarning("Killing Process ({0})", exePath);
						_process.Kill();
					}
				}
				catch (Exception e)
				{
					progress.WriteWarning("Exception while killing process, as though the process reader failed to notice that the process was over: {0}", e.Message);
					progress.WriteWarning("Process.HasExited={0}", _process.HasExited.ToString());
				}
			}
			result.StandardOutput = _processReader.StandardOutput;
			result.StandardError = _processReader.StandardError;

			if (timedOut)
			{
				result.StandardError += Environment.NewLine + "Timed Out after waiting " + secondsBeforeTimeOut + " seconds.";
				result.ExitCode = ExecutionResult.kTimedOut;
			}

			else if (progress.CancelRequested)
			{
				result.StandardError += Environment.NewLine + "User Cancelled.";
				result.ExitCode = ExecutionResult.kCancelled;
			}
			else
			{
				result.ExitCode = _process.ExitCode;
			}
			return result;
		}

	}

	public class ExecutionResult
	{
		public const int kCancelled = 98;
		public const int kTimedOut = 99;

		public int ExitCode;
		public string StandardError;
		public string StandardOutput;
		public bool DidTimeOut { get { return ExitCode ==  kTimedOut; } }
		public bool UserCancelled { get { return ExitCode == kCancelled; } }
	}

	public class UserCancelledException : ApplicationException
	{
		public UserCancelledException()
			: base("User Cancelled")
		{

		}
	}



	class ReaderArgs
	{
		public StreamReader Reader;
		public Process Proc;
		public string Results;
		public IProgress Progress;
	}
}
