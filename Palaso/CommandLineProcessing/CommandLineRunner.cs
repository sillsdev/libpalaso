using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Palaso.Progress.LogBox;

namespace Palaso.CommandLineProcessing
{
	///<summary>
	/// Used to run external processes, with support for things like timeout,
	/// async progress reporting, and user UI cancellability.
	///</summary>
	public class CommandLineRunner
	{
		public static int TimeoutSecondsOverrideForUnitTests = 10000;

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
			ExecutionResult result = new ExecutionResult();
			Process process = new Process();
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.UseShellExecute = false;
			process.StartInfo.CreateNoWindow = true;
			process.StartInfo.WorkingDirectory = fromDirectory;
			process.StartInfo.FileName = exePath;
			process.StartInfo.Arguments = arguments;
			if(encoding!=null)
			{
				process.StartInfo.StandardOutputEncoding = encoding;
			}
			ProcessOutputReaderBase processReader;
			if (actionForReportingProgress!=null)
				processReader = new AsyncProcessOutputReader(process, progress, actionForReportingProgress);
			else
				processReader = new SynchronousProcessOutputReader(process, progress);

			try
			{
				Debug.WriteLine("CommandLineRunner Starting at "+DateTime.Now.ToString());
				process.Start();
			}
			catch (Win32Exception error)
			{
				throw;
			}

			if (secondsBeforeTimeOut > TimeoutSecondsOverrideForUnitTests)
				secondsBeforeTimeOut = TimeoutSecondsOverrideForUnitTests;

			bool timedOut = false;
			Debug.WriteLine("CommandLineRunner Reading at " + DateTime.Now.ToString("HH:mm:ss.ffff"));
			if (!processReader.Read(secondsBeforeTimeOut))
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
				result.ExitCode = ExecutionResult.kTimedOut;
			}

			else if (progress.CancelRequested)
			{
				result.StandardError += Environment.NewLine + "User Cancelled.";
				result.ExitCode = ExecutionResult.kCancelled;
			}
			else
			{
				result.ExitCode = process.ExitCode;
			}
			return result;
		}

		/// <summary>
		/// On Windows, We can't get unicode over the command-line barrier, so
		/// instead create 8.3 filename, which, happily, will have no non-english characters
		/// for any part of the path. This is safe to call from Linux, too
		/// </summary>
		public static string MakePathToFileSafeFromEncodingProblems(string path)
		{
			if(Directory.Exists(path))
				throw new ArgumentException(string.Format("MakePathToFileSafeFromEncodingProblems() is only for files, but {0} is a directory.",path));

			var safe = "";

			//if the filename doesn't exist yet, we can't get the 8.3 name. So we make it, get the name, then delete it.
			//NB: this will not yet deal with the problem of creating a directory
			if(!File.Exists(path))
			{
				File.WriteAllText(path,"");
				safe = Palaso.IO.FileUtils.MakePathSafeFromEncodingProblems(path);
				File.Delete(path);
			}
			else
			{
				safe = Palaso.IO.FileUtils.MakePathSafeFromEncodingProblems(path);
			}

			return safe;
		}

		/// <summary>
		/// On Windows, We can't get unicode over the command-line barrier, so
		/// instead create 8.3 filename, which, happily, will have no non-english characters
		/// for any part of the path. This is safe to call from Linux, too
		/// </summary>
		public static string MakePathToDirectorySafeFromEncodingProblems(string path)
		{
			if (File.Exists(path))
				throw new ArgumentException(string.Format("MakePathToDirectorySafeFromEncodingProblems() is only for directories, but {0} is a file.", path));

			var safe = "";

			//if the filename doesn't exist yet, we can't get the 8.3 name. So we make it, get the name, then delete it.
			//NB: this will not yet deal with the problem of creating a directory
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
				safe = Palaso.IO.FileUtils.MakePathSafeFromEncodingProblems(path);
				Directory.Delete(path);
			}
			else
			{
				safe = Palaso.IO.FileUtils.MakePathSafeFromEncodingProblems(path);
			}

			return safe;
		}

		/// <summary>
		/// Some command line applications (e.g. exiftool) can handle html-encoded characters in their command arguments
		/// </summary>
		/// <remarks>From Rick Strahl at http://www.west-wind.com/weblog/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb</remarks>
		public static string HtmlEncodeNonAsciiCharacters(string text)
		{
			if (text == null)
				return null;

			StringBuilder sb = new StringBuilder(text.Length);

			int len = text.Length;
			for (int i = 0; i < len; i++)
			{
				if (text[i] > 159)
				{
					// decimal numeric entity
					sb.Append("&#");
					sb.Append(((int)text[i]).ToString(CultureInfo.InvariantCulture));
					sb.Append(";");
				}
				else
					sb.Append(text[i]);
			}
			return sb.ToString();
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
