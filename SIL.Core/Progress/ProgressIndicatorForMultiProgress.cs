// Copyright (c) 2010-2019 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Threading;

namespace SIL.Progress
{
	public class ProgressIndicatorForMultiProgress : IProgressIndicator
	{
		private int                      _percentCompleted;
		private SynchronizationContext   _syncContext;
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
				throw new ArgumentNullException(nameof(indicator), "indicator was null when passed to ProgressIndicatorForMultiProgress.AddIndicator");
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
}