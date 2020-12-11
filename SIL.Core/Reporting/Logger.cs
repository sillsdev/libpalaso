using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using SIL.IO;

namespace SIL.Reporting
{

	public interface ILogger
	{
		/// <summary>
		/// This is something that should be listed in the source control checkin
		/// </summary>
		void WriteConciseHistoricalEvent(string message, params object[] args);
	}
	public class MultiLogger: ILogger
	{
		private readonly List<ILogger> _loggers= new List<ILogger>();

//          this just lead to problems.  Better to say "this doesn't own anything", and let the DI container handle the lifetimes
//        public void Dispose()
//        {
//            foreach (ILogger logger in _loggers)
//            {
//
//                IDisposable d = logger as IDisposable;
//                if(d!=null)
//                    d.Dispose();
//            }
//            _loggers.Clear();
//        }

		/// <summary>
		/// NB: you must handle disposal of the logger yourself (easy with a DI container)
		/// </summary>
		/// <param name="logger"></param>
		public void Add(ILogger logger)
		{
			_loggers.Add(logger);
		}

		public void WriteConciseHistoricalEvent(string message, params object[] args)
		{
			foreach (ILogger logger in _loggers)
			{
				logger.WriteConciseHistoricalEvent(message,args);
			}
		}
	}

	public class StringLogger :  ILogger
	{
		StringBuilder _builder = new StringBuilder();
		public void WriteConciseHistoricalEvent(string message, params object[] args)
		{
			_builder.Append(Logger.FormatMessage(message, args));
		}
		public string GetLogText()
		{
			return _builder.ToString();
		}
	}

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Logs stuff to a file created in
	/// c:\Documents and Settings\Username\Local Settings\Temp\Companyname\Productname\Log.txt
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class Logger: IDisposable, ILogger
	{
		private static Logger _singleton;
		private static string _actualLogPath;
		private static string _logfilePrefix;

		protected StreamWriter m_out;
		private StringBuilder m_minorEvents;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Creates the logger. The logging functions can't be used until this method is
		/// called.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void Init()
		{
			if(Singleton == null)
				Init(null);
		}

		/// <summary>
		/// Creates the logger. The logging functions can't be used until this method is called.
		/// Initializes the logger by creating a new log file, prepending the specified
		/// <paramref name="logfilePrefix"/>. If Init has been called before, the previous
		/// Logger gets shutdown first.
		/// </summary>
		/// <remarks>
		/// This method is useful when an application wants to write different logging files
		/// while it is running. For example, FieldWorks writes to a different log file after
		/// loading the project. This is also necessary when an application can run multiple
		/// instances simultaneously.
		/// </remarks>
		public static void Init(string logfilePrefix)
		{
			Init(logfilePrefix, true);
		}

		/// <summary>
		/// Creates the logger. The logging functions can't be used until this method is called.
		/// Initializes the logger by creating a new log file, prepending the specified
		/// <paramref name="logfilePrefix"/>. If Init has been called before, the previous
		/// Logger gets shutdown first.
		/// </summary>
		public static void Init(string logfilePrefix, bool startWithNewFile)
		{
			ShutDown();

			if (Singleton == null)
				_singleton = new Logger(logfilePrefix, startWithNewFile);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Shut down the logger. The logging functions can't be used after this method is
		/// called.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void ShutDown()
		{
			if (Singleton != null)
			{
				Singleton.Dispose();
				_singleton = null;
			}
		}

		public Logger(): this(null, true)
		{
		}

		private Logger(string logfilePrefix, bool startWithNewFile)
		{
			if(_singleton != null)
			{
				throw new ApplicationException("Sadly, only one instance of Logger is currently allowed, per instance of the application.");
			}
			try
			{
				_logfilePrefix = logfilePrefix;
				if (!string.IsNullOrEmpty(logfilePrefix))
					_logfilePrefix += "_";

				m_out = null;
				if (startWithNewFile || !File.Exists(LogPath))
				{
					try
					{
						m_out = File.CreateText(LogPath);
					}
					catch (Exception)
					{
						//try again with a different file.  We loose the history, but oh well.
						SetActualLogPath(_logfilePrefix + "Log-"+Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".txt");
						m_out = File.CreateText(LogPath);
					}

					m_out.WriteLine(DateTime.Now.ToLongDateString());
				}
				else
					StartAppendingToLog();

				RestartMinorEvents();

				_singleton = this;

				this.WriteEventCore("App Launched with [" + System.Environment.CommandLine + "]");
			}
			catch
			{
				// If the output file can not be created then just disable logging.
				_singleton = null;
			}
		}

		private void StartAppendingToLog()
		{
			m_out = File.AppendText(LogPath);
		}

		private void RestartMinorEvents()
		{
			m_minorEvents = new StringBuilder();
		}

		#region IDisposable & Co. implementation
		// Region last reviewed: never

		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		/// True, if the object has been disposed.
		/// </summary>
		private bool m_isDisposed = false;

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return m_isDisposed; }
		}

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~Logger()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// <summary>
		///
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Executes in two distinct scenarios.
		///
		/// 1. If disposing is true, the method has been called directly
		/// or indirectly by a user's code via the Dispose method.
		/// Both managed and unmanaged resources can be disposed.
		///
		/// 2. If disposing is false, the method has been called by the
		/// runtime from inside the finalizer and you should not reference (access)
		/// other managed objects, as they already have been garbage collected.
		/// Only unmanaged resources can be disposed.
		/// </summary>
		/// <param name="disposing"></param>
		/// <remarks>
		/// If any exceptions are thrown, that is fine.
		/// If the method is being done in a finalizer, it will be ignored.
		/// If it is thrown by client code calling Dispose,
		/// it needs to be handled by fixing the bug.
		///
		/// If subclasses override this method, they should call the base implementation.
		/// </remarks>
		protected virtual void Dispose(bool disposing)
		{
			//Debug.WriteLineIf(!disposing, "****************** " + GetType().Name + " 'disposing' is false. ******************");
			// Must not be run more than once.
			if (m_isDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				if (m_out != null)
					m_out.Close();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.
			_actualLogPath = null;
			m_out = null;

			m_isDisposed = true;
		}

		#endregion IDisposable & Co. implementation

		/// <summary>
		/// This is for version-control checkin descriptions. E.g. "Deleted foobar".
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		public void WriteConciseHistoricalEvent(string message, params object[] args)
		{
			WriteEventCore(message, args);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the entire text of the log file
		/// </summary>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public static string LogText
		{
			get
			{
				if (Singleton == null)
					return "No log available.";

				string logText = Singleton.GetLogTextAndStartOver();

				return logText;
			}
		}

		//enhance: why start over?
		private string GetLogTextAndStartOver()
		{
			lock (_singleton)
			{
				CheckDisposed();
				m_out.Flush();
				m_out.Close();

				//get the old
				StringBuilder contents = new StringBuilder();
				if (!File.Exists(LogPath))
				{
					File.Create(LogPath);
				}
				using (StreamReader reader = File.OpenText(LogPath))
				{
					contents.Append(reader.ReadToEnd());
					contents.AppendLine("Details of most recent events:");
					contents.AppendLine(m_minorEvents.ToString());

				}
				StartAppendingToLog();

				return contents.ToString();
			}
		}

		/// <summary>
		/// added this for a case of a catastrophic error so bad I couldn't get the means of finding out what just happened
		/// </summary>
		public static string MinorEventsLog
		{
			get { return Singleton.m_minorEvents.ToString(); }
		}

		/// <summary>
		/// the place on disk where we are storing the log
		/// </summary>
		public static string LogPath
		{
			get
			{
				if (string.IsNullOrEmpty(_actualLogPath))
				{
					SetActualLogPath(_logfilePrefix + "Log.txt");
				}
				return _actualLogPath;
			}
		}
		public static Logger Singleton
		{
			get { return _singleton; }
		}

		private static void SetActualLogPath(string filename)
		{
			_actualLogPath = Path.Combine(Path.GetTempPath(),
				Path.Combine(EntryAssembly.CompanyName, UsageReporter.AppNameToUseInReporting));
			Directory.CreateDirectory(_actualLogPath);
			_actualLogPath = Path.Combine(_actualLogPath, filename);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Writes an event to the logger. This method will do nothing if Init() is not called
		/// first.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public static void WriteEvent(string message, params object[] args)
		{
			if (Singleton != null)
				Singleton.WriteEventCore(message,args);
		}

		private void WriteEventCore(string message, params object[] args)
		{
			lock (_singleton)
			{
#if !DEBUG
				try
				{
#endif
				CheckDisposed();
				if (m_out != null && m_out.BaseStream.CanWrite)
				{
					m_out.Write(DateTime.Now.ToLongTimeString() + "\t");
					m_out.WriteLine(FormatMessage(message, args));
					m_out.Flush();//in case we crash

					//want this to show up in the proper order in the minor event list, too
					WriteMinorEvent(message, args);

					//Debug.WriteLine("-----"+"\r\n"+m_minorEvents.ToString());
				}
#if !DEBUG
				}
				catch (Exception)
				{
					//swallow
				}
#endif
			}
		}

		/// <summary>
		/// Writes an exception and its stack trace to the log. This method will do nothing if
		/// Init() is not called first.
		/// </summary>
		public static void WriteError(Exception e)
		{
			WriteError(null, e);
		}

		/// <summary>
		/// Writes <paramref name="msg"/> and an exception and its stack trace to the log.
		/// This method will do nothing if Init() is not called first.
		/// </summary>
		public static void WriteError(string msg, Exception e)
		{
			Exception dummy = null;
			var bldr = new StringBuilder(msg);
			if (bldr.Length > 0)
				bldr.AppendLine();
			bldr.Append(ExceptionHelper.GetHiearchicalExceptionInfo(e, ref dummy));
			Debug.WriteLine(bldr.ToString());

			if (Singleton != null)
				Singleton.WriteEventCore(bldr.ToString());
		}

		/// <summary>
		/// only a limitted number of the most recent of these events will show up in the log
		/// </summary>
		public static void WriteMinorEvent(string message, params object[] args)
		{
			if (Singleton != null)
				Singleton.WriteMinorEventCore(message,args);
		}

		/// <summary>
		/// Conceptually, returns string.Format(message, args).
		/// However, if there are no args, it just returns message, so unlike string.Format and friends,
		/// it won't choke if message contains curly braces as long as there are no args.
		/// Also, if the attempt to format the args fails, it will replace the usual output with
		/// a message describing the failure to produce the output (and embedding message).
		/// </summary>
		/// <param name="message"></param>
		/// <param name="args"></param>
		/// <returns></returns>
		internal static string FormatMessage(string message, object[] args)
		{
			if (args.Any())
			{
				try
				{
					return string.Format(message, args);
				}
				catch (Exception)
				{
					return string.Format("Error formatting message with {0} args: {1}", args.Length, message);
				}
			}
			return message;
		}

		private void WriteMinorEventCore(string message, params object[] args)
		{
			CheckDisposed();
			lock (_singleton)
			{
				if (m_minorEvents != null)
				{
#if !DEBUG
					try
					{
#endif
					if (m_minorEvents.Length > 5000)
					{
						int roughlyHowMuchToCut = 500;
						int cutoff = m_minorEvents.ToString().IndexOf(System.Environment.NewLine, roughlyHowMuchToCut);
						m_minorEvents.Remove(0, cutoff);
					}
					m_minorEvents.Append(DateTime.Now.ToLongTimeString() + "\t");
					m_minorEvents.Append(FormatMessage(message, args));
					m_minorEvents.AppendLine();
#if !DEBUG
					}
					catch(Exception)
					{
						//swallow
					}
#endif
				}
			}
		}

		public static void ShowUserTheLogFile()
		{
			Singleton.m_out.Flush();
			Process.Start(LogPath);
		}

		/// <summary>
		/// if you're working with unmanaged code and get a System.AccessViolationException, well you're toast, and anything
		/// that requires UI is gonna freeze up.  So call this instead
		/// </summary>
		public static void ShowUserATextFileRelatedToCatastrophicError(Exception reallyBadException)
		{
			//did this special because we don't have an event loop to drive the error reporting dialog if Application.Run() dies
			Debug.WriteLine(Logger.LogText);
			string tempFileName = TempFile.WithExtension(".txt").Path;
			using(var writer = File.CreateText(tempFileName))
			{
				writer.WriteLine("Please report to "+ErrorReport.EmailAddress);
				writer.WriteLine("No really. Please. This kind of error is super hard to track down.");
				writer.WriteLine();
				writer.WriteLine(reallyBadException.Message);
				writer.WriteLine(reallyBadException.StackTrace);
				writer.WriteLine();
				writer.WriteLine(LogText);
				writer.WriteLine();
				writer.WriteLine("Details of recent events:");
				writer.WriteLine(MinorEventsLog);
				writer.Flush();
				writer.Close();
			}
			Process.Start(tempFileName);
		}
	}
}
