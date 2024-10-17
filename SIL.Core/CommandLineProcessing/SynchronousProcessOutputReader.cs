using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using SIL.Progress;
using ThreadState = System.Threading.ThreadState;

namespace SIL.CommandLineProcessing
{
	public abstract class ProcessOutputReaderBase
	{
		public string StandardOutput { get; set; }
		public string StandardError { get; set; }

		protected Process Process { get; set; }

		protected IProgress Progress { get; set; }

	    /// <summary>
		/// Safely read the streams of the process
		/// </summary>
		/// <param name="secondsBeforeTimeOut"></param>
		/// <returns>true if the process completed before the timeout or cancellation</returns>
		public abstract bool Read(int secondsBeforeTimeOut);
	}


	/// <summary>
	/// Read from the process at the end; this one won't keep UI up to date on long-running processes,
	/// but this old approach is relied on by many existing clients of CommandLineRunner,
	/// so I didn't want to shake things up needlessly when I introduced the AsyncProcessOutputReader.
	/// </summary>
	public class SynchronousProcessOutputReader : ProcessOutputReaderBase
	{
		private Thread _outputReader;
		private Thread _errorReader;

		public SynchronousProcessOutputReader(Process process, IProgress progress)
		{
			Process = process;
			Progress = progress;
		}

		/// <summary>
		/// Safely read the streams of the process
		/// </summary>
		/// <returns>true if the process completed before the timeout or cancellation</returns>
		public override bool Read(int secondsBeforeTimeOut)
		{
			var outputReaderArgs = new ReaderArgs() {Proc = Process, Reader = Process.StandardOutput, Progress = Progress};
			if (Process.StartInfo.RedirectStandardOutput)
			{
				_outputReader = new Thread(new ParameterizedThreadStart(ReadStream));
				_outputReader.Start(outputReaderArgs);
			}
			var errorReaderArgs = new ReaderArgs() {Proc = Process, Reader = Process.StandardError, Progress = Progress};
			if (Process.StartInfo.RedirectStandardError)
			{
				_errorReader = new Thread(new ParameterizedThreadStart(ReadStream));
				_errorReader.Start(errorReaderArgs);
			}

			var end = DateTime.Now.AddSeconds(secondsBeforeTimeOut);


			//nb: at one point I (jh) tried adding !_process.HasExited, but that made things less stable.
			while (MoreToRead())
			{
				if (Progress.CancelRequested)
					return false;

				Thread.Sleep(100);
				if (secondsBeforeTimeOut>0 && DateTime.Now > end)
				{
					if (_outputReader != null)
						_outputReader.Interrupt();
					if (_errorReader != null)
						_errorReader.Interrupt();
					return false;
				}
			}

			// See http://www.wesay.org/issues/browse/WS-14948
			// The output reader threads may exit slightly prior to the application closing.
			// So we wait for the exit to be confirmed.
			Process.WaitForExit(1000);
			StandardOutput = outputReaderArgs.Results;
			StandardError = errorReaderArgs.Results;

			return true;
		}

		private bool MoreToRead()
		{
			var moreOutputToRead = !(_outputReader.ThreadState == ThreadState.AbortRequested ||
							 _outputReader.ThreadState == ThreadState.Aborted ||
							 _outputReader.ThreadState == ThreadState.Stopped);

			bool moreStdErrorToRead = false;
			if(_errorReader!=null)
			{
				moreStdErrorToRead = !(_errorReader.ThreadState == ThreadState.AbortRequested ||
							 _errorReader.ThreadState == ThreadState.Aborted ||
							 _errorReader.ThreadState == ThreadState.Stopped);
			}

			return moreOutputToRead || moreStdErrorToRead;
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
					Debug.WriteLine("CommandLineRunner: " + s);
					result.AppendLine(s.Trim());
					if (readerArgs.Progress != null)
						readerArgs.Progress.WriteVerbose(s);
				}
			} while (!reader.EndOfStream);

			readerArgs.Results = result.ToString().Replace("\r\n", "\n");
		}
	}
}