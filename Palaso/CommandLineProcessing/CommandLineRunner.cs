using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using ThreadState = System.Threading.ThreadState;

namespace Palaso.CommandLineProcessing
{
	///<summary>
	///
	///</summary>
	public class CommandLineRunner
	{
		public static int TimeoutSecondsOverrideForUnitTests = 10000;

		public static ExecutionResult Run(string exePath, string arguments, string fromDirectory, int secondsBeforeTimeOut, IProgress progress)
		{
			ExecutionResult result = new ExecutionResult();
			Process process = new Process();
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WorkingDirectory = fromDirectory;
			process.StartInfo.FileName = exePath;
			process.StartInfo.Arguments = arguments;

			try
			{
				process.Start();
			}
			catch (Win32Exception error)
			{
				throw;
			}

			var processReader = new ProcessOutputReader();
			if (secondsBeforeTimeOut > TimeoutSecondsOverrideForUnitTests)
				secondsBeforeTimeOut = TimeoutSecondsOverrideForUnitTests;

			bool timedOut = false;
			if (!processReader.Read(ref process, secondsBeforeTimeOut, progress))
			{
				timedOut = !progress.CancelRequested;
				try
				{
					if (process.HasExited)
					{
						progress.WriteWarning("Process exited, cancelRequested was {0}", progress.CancelRequested);
					}
					else
					{
						progress.WriteWarning("Killing Process...");
						process.Kill();
					}
				}
				catch (Exception e)
				{
					progress.WriteWarning("Exception while killing process, as though the process reader failed to notice that the process was over: {0}", e.Message);
					progress.WriteWarning("Process.HasExited={0}", process.HasExited.ToString());
				}
			}
			result.StandardOutput = processReader.StandardOutput;
			result.StandardError = processReader.StandardError;

			if (timedOut)
			{
				result.StandardError += Environment.NewLine + "Timed Out after waiting " + secondsBeforeTimeOut + " seconds.";
				result.ExitCode = ProcessStream.kTimedOut;
			}

			else if (progress.CancelRequested)
			{
				result.StandardError += Environment.NewLine + "User Cancelled.";
				result.ExitCode = ProcessStream.kCancelled;
			}
			else
			{
				result.ExitCode = process.ExitCode;
			}
			return result;
		}

	}

	public class ExecutionResult
	{
		public int ExitCode;
		public string StandardError;
		public string StandardOutput;
		public bool DidTimeOut { get { return ExitCode == ProcessOutputReader.kTimedOut; } }
		public bool UserCancelled { get { return ExitCode == ProcessOutputReader.kCancelled; } }

		public ExecutionResult()
		{

		}
		public ExecutionResult(Process proc)
		{
			ProcessStream ps = new ProcessStream();
			ps.Read(ref proc, 30);
			StandardOutput = ps.StandardOutput;
			StandardError = ps.StandardError;
			ExitCode = proc.ExitCode;
		}
	}

	public class UserCancelledException : ApplicationException
	{
		public UserCancelledException()
			: base("User Cancelled")
		{

		}
	}

	  /// <summary>
	/// This is class originally from  SeemabK (seemabk@yahoo.com).  It has been enhanced for chorus.
	/// </summary>
	internal class ProcessStream
	{
		/*
		* Class to get process stdout/stderr streams
		* Author: SeemabK (seemabk@yahoo.com)
		*/

		private Thread StandardOutputReader;
		private Thread StandardErrorReader;
		private static Process _srunningProcess;

		private string _standardOutput = "";
		public string StandardOutput
		{
			get { return _standardOutput; }
		}
		private string _standardError = "";
		public const int kTimedOut = 99;
		public const int kCancelled = 98;

		public string StandardError
		{
			get { return _standardError; }
		}

		public ProcessStream()
		{
			Init();
		}

		public int Read(ref Process process, int secondsBeforeTimeOut)
		{
//            try
//            {
				Init();
				_srunningProcess = process;

				if (_srunningProcess.StartInfo.RedirectStandardOutput)
				{
					StandardOutputReader = new Thread(new ThreadStart(ReadStandardOutput));
					StandardOutputReader.Start();
				}
				if (_srunningProcess.StartInfo.RedirectStandardError)
				{
					StandardErrorReader = new Thread(new ThreadStart(ReadStandardError));
					StandardErrorReader.Start();
				}

				//_srunningProcess.WaitForExit();
				if (StandardOutputReader != null)
				{
					if (!StandardOutputReader.Join(new TimeSpan(0, 0, 0, secondsBeforeTimeOut)))
					{
						return kTimedOut;
					}
				}
				if (StandardErrorReader != null)
				{
					if (!StandardErrorReader.Join(new TimeSpan(0, 0, 0, secondsBeforeTimeOut)))
					{
						return kTimedOut;
					}
				}
//            }
//            catch
//            { }

			return 1;
		}

		private void ReadStandardOutput()
		{
			if (_srunningProcess != null)
				_standardOutput = _srunningProcess.StandardOutput.ReadToEnd();
		}

		private void ReadStandardError()
		{
			if (_srunningProcess != null)
				_standardError = _srunningProcess.StandardError.ReadToEnd();
		}

		private int Init()
		{
			_standardError = "";
			_standardOutput = "";
			_srunningProcess = null;
			Stop();
			return 1;
		}

		[System.Diagnostics.DebuggerStepThrough]
		public int Stop()
		{
			try { StandardOutputReader.Abort(); }
			catch { }
			try { StandardErrorReader.Abort(); }
			catch { }
			StandardOutputReader = null;
			StandardErrorReader = null;
			return 1;
		}
	}

	public class ProcessOutputReader
	{
		private Thread _outputReader;
		private Thread _errorReader;

		private string _standardOutput = "";
		public string StandardOutput
		{
			get { return _standardOutput; }
		}
		private string _standardError = "";
		public const int kCancelled = 98;
		public const int kTimedOut = 99;

		public string StandardError
		{
			get { return _standardError; }
		}

		/// <summary>
		/// Safely read the streams of the process
		/// </summary>
		/// <param name="process"></param>
		/// <param name="secondsBeforeTimeOut"></param>
		/// <returns>true if the process completed before the timeout or cancellation</returns>
		public bool Read(ref Process process, int secondsBeforeTimeOut, IProgress progress)
		{
			var outputReaderArgs = new ReaderArgs() {Proc = process, Reader = process.StandardOutput};
			if (process.StartInfo.RedirectStandardOutput)
			{
				_outputReader = new Thread(new ParameterizedThreadStart(ReadStream));
				_outputReader.Start(outputReaderArgs);
			}
		   var errorReaderArgs = new ReaderArgs() { Proc = process, Reader = process.StandardError };
		   if (process.StartInfo.RedirectStandardError)
			{
				_errorReader = new Thread(new ParameterizedThreadStart(ReadStream));
				_errorReader.Start(errorReaderArgs);
			}

			var end = DateTime.Now.AddSeconds(secondsBeforeTimeOut);

			//nb: at one point I (jh) tried adding !process.HasExited, but that made things less stable.
			while (/*!process.HasExited &&*/ (_outputReader.ThreadState == ThreadState.Running || (_errorReader != null && _errorReader.ThreadState == ThreadState.Running)))
			{
				if(progress.CancelRequested)
					return false;

				Thread.Sleep(100);
				if (DateTime.Now > end)
				{
					if (_outputReader != null)
						_outputReader.Abort();
					if (_errorReader != null)
						_errorReader.Abort();
					return false;
				}
			}
			// See http://www.wesay.org/issues/browse/WS-14948
			// The output reader threads may exit slightly prior to the application closing.
			// So we wait for the exit to be confirmed.
			process.WaitForExit(1000);
			_standardOutput = outputReaderArgs.Results;
			_standardError = errorReaderArgs.Results;

			return true;

		}


		private static void ReadStream(object args)
		{
			StringBuilder result = new StringBuilder();
			var readerArgs = args as ReaderArgs;

			var reader = readerArgs.Reader;
			do
			{
				var s = reader.ReadLine();
				if (s != null)
				{
					result.AppendLine(s.Trim());
				}
			} while (!reader.EndOfStream);

			readerArgs.Results = result.ToString().Replace("\r\n", "\n");
		}
	}

	class ReaderArgs
	{
		public StreamReader Reader;
		public Process Proc;
		public string Results;
	}
}
