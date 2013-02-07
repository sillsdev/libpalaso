using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Palaso.Code;
using Palaso.Progress;

namespace Palaso.CommandLineProcessing
{
	/// <summary>
	/// Read from the process asynchronously, so as to fuel some UI progres downstream
	///
	/// NOTE: THIS WILL NOT WORK as expected with some programs. They look fine in a console app, but
	/// in this context, then give all the output at the end. The going guess is that they
	/// are buffering their output.
	/// I've had this problem with wkhtml2pdf,
	/// but it seems that others have had it with other programs:
	/// http://stackoverflow.com/questions/3626537/asynchronous-capture-from-a-process-output-not-working-properly?rq=1
	/// http://stackoverflow.com/questions/6597800/capture-output-of-process-synchronously-i-e-when-it-happens?lq=1
	/// http://stackoverflow.com/questions/5362121/reading-other-process-console-output
	/// http://stackoverflow.com/questions/4501511/c-sharp-realtime-console-output-redirection
	/// http://stackoverflow.com/questions/3844267/how-to-disable-output-buffering-in-process-standardoutput?lq=1
	///
	/// In the case that data is available on the stream, but just no linefeed, it looks like maybe this would be
	/// worth trying:
	/// http://stackoverflow.com/questions/4501511/c-sharp-realtime-console-output-redirection
	/// that is, there may be bytes available to read before the OutputDataReceived would be called.
	/// </summary>
	public class AsyncProcessOutputReader : ProcessOutputReaderBase
	{
		private readonly Action<string> _actionForReportingProgress;
		private StringBuilder _outputBuilder;
		private StringBuilder _errorBuilder;
		private bool _stillReading;

		public AsyncProcessOutputReader(Process process, IProgress progress, Action<string> actionForReportingProgress)
		{
			_actionForReportingProgress = actionForReportingProgress;
			//not allowed to even ask: Guard.Against(process.StartTime!=default(DateTime),"Must initialize this reader before starting the process");
			_process = process;
			_progress = progress;
			_outputBuilder = new StringBuilder();
			_errorBuilder = new StringBuilder();

			_process.OutputDataReceived += (p, data) => HandleNewData(data, _outputBuilder, "StdOutput");
			_process.ErrorDataReceived += (p, data) => HandleNewData(data, _errorBuilder, "StdError");
		}

		/// <param name="secondsBeforeTimeOut"></param>
		/// <returns>true if the process completed before the timeout or cancellation</returns>
		public override bool Read(int secondsBeforeTimeOut)
		{
			_stillReading = true;
			_process.BeginOutputReadLine();
			_process.BeginErrorReadLine();

			var end = DateTime.Now.AddSeconds(secondsBeforeTimeOut);
			try
			{
				while (!_process.HasExited)
				{
					if (_progress.CancelRequested)
						return false;

					Thread.Sleep(100);
					if (secondsBeforeTimeOut>0 && DateTime.Now > end)
					{
						return false;
					}
				}

				StandardOutput = _outputBuilder.ToString();
				StandardError = _errorBuilder.ToString();

				return true;
			}
			finally
			{
				_stillReading = false; //probably the following cancells, which I learned of later, would remove the need for this _stillReading flag
				_process.CancelOutputRead();
				_process.CancelErrorRead();
			}
		}


		private void HandleNewData(DataReceivedEventArgs data, StringBuilder builder, string label)
		{
			//Debug.WriteLine("AsyncProcessOutputReader HandleNewData at " + DateTime.Now.ToString("HH:mm:ss.ffff"));
			if (!_stillReading || data.Data == null)
				return;
			var s = data.Data.Replace("\r\n", "\n");
			//for some lost reason, some client wanted this... maybe easier string matching?
			builder.AppendLine(s.Trim());

		//	Debug.WriteLine("AsyncProcessOutputReader " + label + ": " + s.Trim());
			_actionForReportingProgress(s);
		}
	}
}