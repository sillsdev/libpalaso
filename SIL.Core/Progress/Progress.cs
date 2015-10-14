using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using SIL.Extensions;

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
		bool CancelRequested { get;  set; }
		bool ErrorEncountered { get; set; }
		IProgressIndicator ProgressIndicator { get; set; }
		SynchronizationContext SyncContext { get; set; }
	}

	public class NullProgress : IProgress
	{
		public NullProgress()
		{
			ProgressIndicator = new NullProgressIndicator();
		}
		public void WriteStatus(string message, params object[] args)
		{

		}

		public void WriteMessage(string message, params object[] args)
		{
		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{

		}

		public void WriteWarning(string message, params object[] args)
		{
		}

		public void WriteException(Exception error)
		{

		}

		public void WriteError(string message, params object[] args)
		{
			ErrorEncountered = true;
		}

		public void WriteVerbose(string message, params object[] args)
		{

		}

		public bool ShowVerbose
		{
			get { return false; }
			set {  }
		}

		public virtual bool CancelRequested { get; set; }

		public virtual bool ErrorEncountered {get;set;}

		public IProgressIndicator ProgressIndicator { get; set; }

		public SynchronizationContext SyncContext { get; set; }
	}

	public class NullProgressIndicator : IProgressIndicator
	{
		public int PercentCompleted { get; set; }

		public void Finish()
		{
		}

		public void Initialize()
		{
		}

		public void IndicateUnknownProgress()
		{
		}

		public SynchronizationContext SyncContext { get; set; }
	}

	public class ProgressIndicatorForMultiProgress : IProgressIndicator
	{
		private int _percentCompleted;
		private SynchronizationContext _syncContext;
		private List<IProgressIndicator> _indicators;
		public ProgressIndicatorForMultiProgress()
		{
			_percentCompleted = 0;
			_indicators = new List<IProgressIndicator>();
		}

		public void AddIndicator(IProgressIndicator indicator)
		{
			if (indicator == null)
			{
				throw new ArgumentNullException("indicator", "indicator was null when passed to ProgressIndicatorForMultiProgress.AddIndicator");
			}
			_indicators.Add(indicator);
		}

		public int PercentCompleted
		{
			get { return _percentCompleted; }
			set
			{
				_percentCompleted = value;
				foreach (IProgressIndicator progressIndicator in _indicators)
				{
					progressIndicator.PercentCompleted = value;
				}
			}
		}

		public void Finish()
		{
			PercentCompleted = 100;
		}

		public void Initialize()
		{
			_percentCompleted = 0;
			foreach (IProgressIndicator progressIndicator in _indicators)
			{
				progressIndicator.Initialize();
			}
		}

		public void IndicateUnknownProgress()
		{
			foreach (IProgressIndicator progressIndicator in _indicators)
			{
				progressIndicator.IndicateUnknownProgress();
			}
		}

		public SynchronizationContext SyncContext
		{
			get
			{
				return _syncContext;
			}
			set
			{
				_syncContext = value;
				foreach (IProgressIndicator progressIndicator in _indicators)
				{
					progressIndicator.SyncContext = value;
				}
			}
		}
	}

	public class MultiProgress : IProgress, IDisposable
	{
		private class ProgressHandler
		{
			public IProgress Handler { get; private set; }
			public bool CanHandleStatus { get; private set; }
			public bool CanHandleMessages { get; private set; }
			public ProgressHandler(IProgress p, Capabilities c)
			{
				Handler = p;
				CanHandleStatus = false;
				CanHandleMessages = false;
				if (c == Capabilities.Status || c == Capabilities.All)
				{
					CanHandleStatus = true;
				}
				if (c == Capabilities.Message || c == Capabilities.All)
				{
					CanHandleMessages = true;
				}
			}
		}

		private enum Capabilities
		{
			Status,
			Message,
			All
		}

		private readonly List<ProgressHandler> _progressHandlers=new List<ProgressHandler>();
		private bool _cancelRequested;
		private ProgressIndicatorForMultiProgress _indicatorForMultiProgress;
		private bool _errorsEncountered;

		public MultiProgress(IEnumerable<IProgress> progressHandlers)
		{
			ErrorEncountered = false;
			WarningsEncountered = false;
			foreach (IProgress progressHandler in progressHandlers)
			{
				_progressHandlers.Add(new ProgressHandler(progressHandler, Capabilities.All));
			}
			_indicatorForMultiProgress = new ProgressIndicatorForMultiProgress();
		}

		public MultiProgress()
		{
			_indicatorForMultiProgress = new ProgressIndicatorForMultiProgress();
		}

		public SynchronizationContext SyncContext {
			get { return _indicatorForMultiProgress.SyncContext; }
			set
			{
				foreach (ProgressHandler progressHandler in _progressHandlers)
				{
					progressHandler.Handler.SyncContext = value;
					if (progressHandler.Handler.ProgressIndicator != null)
					{
						progressHandler.Handler.ProgressIndicator.SyncContext = value;
					}
				}
				_indicatorForMultiProgress.SyncContext = value;
			}
		}


		public bool CancelRequested
		{

			get
			{
				if (_progressHandlers.Any(h => h.Handler.CancelRequested))
				{
					return true;
				}
				return _cancelRequested;
			}
			set
			{
				_cancelRequested = value;
			}
		}

		public bool ErrorEncountered
		{
			get
			{ return _progressHandlers.Any(h => h.Handler.ErrorEncountered); }
			set
			{
				foreach (var h in _progressHandlers)
				{
					h.Handler.ErrorEncountered = value;
				}
			}
		}

		public bool WarningsEncountered { get; set; }


		public IProgressIndicator ProgressIndicator
		{
			get { return _indicatorForMultiProgress; }
			set
			{
				// cjh: this could cause confusion by wrapping AddIndicator() with a property setter.  There's no way to "undo" the set later on.
				_indicatorForMultiProgress.AddIndicator(value);
			}
		}

		public void WriteStatus(string message, params object[] args)
		{
			foreach (var h in _progressHandlers)
			{
				if (h.CanHandleStatus)
				{
					h.Handler.WriteStatus(message, args);
				}
				if (h.CanHandleMessages)
				{
					h.Handler.WriteVerbose(message, args);
				}
			}
		}

		public void WriteMessage(string message, params object[] args)
		{
			foreach (var h in _progressHandlers.Where(h => h.CanHandleMessages))
			{
				h.Handler.WriteMessage(message, args);
			}
		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			foreach (var h in _progressHandlers.Where(h => h.CanHandleMessages))
			{
				h.Handler.WriteMessageWithColor(colorName, message, args);
			}
		}

		public void WriteWarning(string message, params object[] args)
		{
			foreach (var h in _progressHandlers.Where(h => h.CanHandleMessages))
			{
				h.Handler.WriteWarning(message, args);
			}
			WarningsEncountered = true;
		}

		public void WriteException(Exception error)
		{
			foreach (var h in _progressHandlers)
			{
				if (h.CanHandleMessages)
				{
					h.Handler.WriteException(error);
				}
			}
			ErrorEncountered = true;
		}

		public void WriteError(string message, params object[] args)
		{
			foreach (var h in _progressHandlers.Where(h => h.CanHandleMessages))
			{
				h.Handler.WriteError(message, args);
			}
			ErrorEncountered = true;
		}

		public void WriteVerbose(string message, params object[] args)
		{
			foreach (var h in _progressHandlers.Where(h => h.CanHandleMessages))
			{
				h.Handler.WriteVerbose(message, args);
			}
		}

		public bool ShowVerbose
		{
			set //review: the best policy isn't completely clear here
			{
				foreach (var handler in _progressHandlers)
				{
					handler.Handler.ShowVerbose = value;
				}
			}
		}

		public void Dispose()
		{
			foreach (ProgressHandler handler in _progressHandlers)
			{
				var d = handler as IDisposable;
				if (d != null)
					d.Dispose();
			}
		}

		public void Add(IProgress progress)
		{
			_progressHandlers.Add(new ProgressHandler(progress, Capabilities.All));
			if (progress.ProgressIndicator != null)
			{
				_indicatorForMultiProgress.AddIndicator(progress.ProgressIndicator);
			}
		}

		public void AddStatusProgress(IProgress p)
		{
			_progressHandlers.Add(new ProgressHandler(p, Capabilities.Status));
			if (p.ProgressIndicator != null)
			{
				_indicatorForMultiProgress.AddIndicator(p.ProgressIndicator);
			}
		}

		public void AddMessageProgress(IProgress p)
		{
			_progressHandlers.Add(new ProgressHandler(p, Capabilities.Message));
			if (p.ProgressIndicator != null)
			{
				_indicatorForMultiProgress.AddIndicator(p.ProgressIndicator);
			}
		}
	}

	public class ConsoleProgress : IProgress, IDisposable
	{
		public static int indent = 0;
		private bool _verbose;

		public ConsoleProgress()
		{
		}

		public ConsoleProgress(string mesage, params string[] args)
		{
			WriteStatus(mesage, args);
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
			set { _verbose = value; }
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

public class StringBuilderProgress : GenericProgress
	{
		private StringBuilder _builder = new StringBuilder();

		public override void WriteMessage(string message, params object[] args)
		{
			try
			{
				_builder.Append("                          ".Substring(0, indent * 2));
				_builder.AppendFormat(message + Environment.NewLine, args);
			}
			catch //in case someone sneaks a { } into a user string, and cause that format to fail
			{
				_builder.Append(message + Environment.NewLine);//better than nothing
			}
		}

		public override void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			WriteMessage(message,args);
		}

		public string Text
		{
			get { return _builder.ToString(); }
		}

		public override string ToString()
		{
			return Text;
		}

		public void Clear()
		{
			_builder = new StringBuilder();
		}
	}

	public class StatusProgress : IProgress
	{
		public SynchronizationContext SyncContext { get; set; }
		public string LastStatus { get; private set; }
		public string LastWarning { get; private set; }
		public string LastError { get; private set; }
		public bool CancelRequested { get; set; }
		public bool WarningEncountered { get { return !string.IsNullOrEmpty(LastWarning); } }
		public bool ErrorEncountered { get { return !string.IsNullOrEmpty(LastError); }
			set { }
		}

		public IProgressIndicator ProgressIndicator { get; set; }


		public  void WriteStatus(string message, params object[] args)
		{
			LastStatus = message.FormatWithErrorStringInsteadOfException(args);
		}

		public void WriteMessageWithColor(string colorName, string message, params object[] args)
		{

		}

		public void WriteWarning(string message, params object[] args)
		{
			LastWarning = message.FormatWithErrorStringInsteadOfException(args);
			LastStatus = LastWarning;
		}

		public void WriteException(Exception error)
		{
			WriteError(error.Message);
		}

		public void WriteError(string message, params object[] args)
		{
			LastError = message.FormatWithErrorStringInsteadOfException(args);
			LastStatus = LastError;
		}

		public void WriteMessage(string message, params object[] args)
		{
		}

		public void WriteVerbose(string message, params object[] args)
		{
		}

		public bool ShowVerbose
		{
			set {  }
		}

		public bool WasCancelled
		{
			get
			{
				if(LastWarning!=null)
					return LastWarning.ToLower().Contains("cancelled");
				return false;
			}//improve: this is pretty flimsy
		}

		public void Clear()
		{
			LastError = LastWarning = LastStatus = string.Empty;
		}
	}

	public abstract class GenericProgress : IProgress
	{
		public int indent = 0;
		private bool _verbose;

		public GenericProgress()
		{
		}

		public static string SafeFormat(string format, params object[] args)
		{
			return format.FormatWithErrorStringInsteadOfException(args);
		}

		public bool CancelRequested { get; set; }
		public abstract void WriteMessage(string message, params object[] args);
		public abstract void WriteMessageWithColor(string colorName, string message, params object[] args);
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
			set { _verbose = value; }
		}
	}

	public class FileLogProgress : GenericProgress
	{
		private readonly string _path;

		public FileLogProgress(string path)
		{
			ShowVerbose = true;
			_path = path;
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}

		public override void WriteMessage(string message, params object[] args)
		{
			File.AppendAllText(_path, message.FormatWithErrorStringInsteadOfException(args) + Environment.NewLine);
		}
		public override void WriteMessageWithColor(string colorName, string message, params object[] args)
		{
			WriteMessage(message, args);
		}

	}
}

