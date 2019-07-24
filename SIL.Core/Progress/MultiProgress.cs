// Copyright (c) 2010-2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SIL.Progress
{
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

		private readonly List<ProgressHandler>             _progressHandlers =new List<ProgressHandler>();
		private          bool                              _cancelRequested;
		private          ProgressIndicatorForMultiProgress _indicatorForMultiProgress;

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
}