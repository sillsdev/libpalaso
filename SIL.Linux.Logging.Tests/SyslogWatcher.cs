using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SIL.Linux.Logging.Tests
{
	public class SyslogWatcher
	{
		public string StringToWatchFor { get; set; }
		private string WatchedFile { get; set; }
		private FileStream WatchingStream { get; set; }
		private StreamReader WatchingReader { get; set; }
		private AutoResetEvent DataReady { get; set; }
		private Int64 _startPos = Int64.MinValue;
		private List<string> _collectedStrings = new List<string>();
		private Timer _pollTimer;
		private double _pollInterval;
		private int _timeout;

		public SyslogWatcher(string stringToWatchFor = null, string logfile = "/var/log/syslog", double pollInterval = 50.0, int timeout = 10000)
		{
			StringToWatchFor = stringToWatchFor ?? String.Empty;
			WatchedFile = logfile;
			_pollInterval = pollInterval;
			_timeout = timeout;
		}

		public void StartWatching()
		{
			// Once we start using .NET 4.5, this can be rewritten much more simply using async/await
			var info = new FileInfo(WatchedFile);
			_startPos = info.Length;
			SafelyCloseStreamAndReader();
			WatchingStream = new FileStream(WatchedFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			WatchingStream.Seek(_startPos, SeekOrigin.Begin);
			WatchingReader = new StreamReader(WatchingStream, Encoding.UTF8);
			DataReady = new AutoResetEvent(false);
			StartTimer();
		}

		public void StopWatching()
		{
			StopTimer();
			SafelyCloseStreamAndReader();
		}

		public IEnumerable<string> WaitForData(int numLinesExpected = 1)
		{
			string[] result = new string[numLinesExpected];
			int linesFound = 0;
			while (true) { 
				if (DataReady.WaitOne(_timeout))
				{
					StopTimer();
					_collectedStrings.CopyTo(0, result, linesFound, (numLinesExpected - linesFound));
					linesFound += _collectedStrings.Count;
					_collectedStrings.Clear();
					if (linesFound >= numLinesExpected)
						return result;
					StartTimer();
				}
				else
				{
					// If timeout reached without finding any new data, return what we have so far
					return result;
				}
			}
		}

		private void StartTimer()
		{
			_collectedStrings.Clear();
			_pollTimer = new Timer(_pollInterval);
			_pollTimer.Elapsed += CheckForData;
			_pollTimer.Start();
		}

		private void StopTimer()
		{
			if (_pollTimer != null)
			{
				_pollTimer.Stop();
				_pollTimer.Elapsed -= CheckForData;
			}
		}

		private void CheckForData(object sender, ElapsedEventArgs elapsedEventArgs)
		{
			while (true)
			{
				string s = WatchingReader.ReadLine();
				if (s == null)
					break;
				if (String.IsNullOrEmpty(StringToWatchFor) || s.Contains(StringToWatchFor))
				{
					_collectedStrings.Add(s);
				} 
			}
			if (_collectedStrings.Count > 0)
				DataReady.Set();
		}

		private void SafelyCloseStreamAndReader()
		{
			if (WatchingReader != null)
			{
				WatchingReader.Close();
				WatchingReader = StreamReader.Null;
			}
			if (WatchingStream != null)
			{
				WatchingStream.Close();
				WatchingStream = null;
			}
		}
	}
}