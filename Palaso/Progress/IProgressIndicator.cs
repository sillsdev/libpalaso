using System;
using System.Threading;
using System.Windows.Forms;

namespace Palaso.Progress
{
	public interface IProgressIndicator
	{
		int PercentCompleted { get; set; }
		void Finish();
		void Initialize();
		void IndicateUnknownProgress();
		SynchronizationContext SyncContext { get; set; }
	}

	public class SimpleProgressIndicator : ProgressBar, IProgressIndicator
	{
		public SimpleProgressIndicator()
		{
			UpdateStyle(ProgressBarStyle.Continuous);
			MarqueeAnimationSpeed = 50; // only used when IndicateUnknownProgress() is in effect
			UpdateValue(0);
			UpdateMaximum(100);
		}

		public void IndicateUnknownProgress()
		{
			UpdateStyle(ProgressBarStyle.Marquee);
		}

		public SynchronizationContext SyncContext { get; set; }

		public int PercentCompleted
		{
			get { return Value; }
			set
			{
				int valueToSet = value;
				if (value < 0)
				{
					valueToSet = 0;
				}
				else if (value > 100)
				{
					valueToSet = 100;
				}
				UpdateStyle(ProgressBarStyle.Continuous);
				UpdateValue(valueToSet);
			}
		// This method does nothing, but cause a stack overflow exception, since the provided int (above) ends up being cast as null, and the death sriral begins.
		}

		public void Finish()
		{
			UpdateStyle(ProgressBarStyle.Continuous);
			UpdateValue(Maximum);
		}

		public void Initialize()
		{
			UpdateStyle(ProgressBarStyle.Continuous);
			UpdateValue(0);
			UpdateMaximum(100);
		}

		private void UpdateStyle(ProgressBarStyle x)
		{
			if (SyncContext != null)
			{
				SyncContext.Post(SetStyle, x);
			}
			else
			{
				Style = x;
			}
		}

		private void SetStyle(object state)
		{
			Style = (ProgressBarStyle) state;
		}

		private void SetVal(object state)
		{
			Value = (int) state;
		}

		private void UpdateValue(int x)
		{
			if (SyncContext != null)
			{
				SyncContext.Post(SetVal, x);
			}
			else
			{
				Value = x;
			}
		}

		private void SetMax(object state)
		{
			Maximum = (int) state;
		}

		private void UpdateMaximum(int x)
		{
			if (SyncContext != null)
			{
				SyncContext.Post(SetMax, x);
			}
			else
			{
				Maximum = x;
			}
		}

	}

	///<summary>
	/// MultiPhaseProgressIndicator makes updating an IProgressIndicator representing THE WHOLE PROCESS when in fact
	/// there are several sub processes or phases that are run sequentially but which do not know about each other.
	///
	/// This class manages a global IProgressIndicator
	///</summary>
	public class MultiPhaseProgressIndicator : IProgressIndicator
	{
		private int _currentPhase;
		private int _numberOfPhases;
		private IProgressIndicator _globalIndicator;
		private int _currentPhasePercentComplete;

		public MultiPhaseProgressIndicator(IProgressIndicator indicator, int numberOfPhases)
		{
			_globalIndicator = indicator;
			_globalIndicator.Initialize();

			_numberOfPhases = numberOfPhases;
			_currentPhase = 0;  // must call Initialize() to increment the _currentProcess
			_currentPhasePercentComplete = 0;
		}

		public void IndicateUnknownProgress()
		{
			_globalIndicator.IndicateUnknownProgress();
		}

		public SynchronizationContext SyncContext
		{
			get { return _globalIndicator.SyncContext; }
			set { _globalIndicator.SyncContext = value; }
		}

		public int PercentCompleted  // per process
		{
			get { return _currentPhasePercentComplete; }
			set
			{
				int valueToSet = value;
				if (value < 0)
				{
					valueToSet = 0;
				} else if (value > 100)
				{
					valueToSet = 100;
				}

				_currentPhasePercentComplete = valueToSet;
				_globalIndicator.PercentCompleted = (_currentPhasePercentComplete + 100*(_currentPhase - 1)) / _numberOfPhases;
			}
		}

		public void Finish() // Finish current process
		{
			PercentCompleted = 100;
		}

		public void Initialize()  // Initialize/begin next process
		{
			if (_currentPhase != _numberOfPhases)
			{
				_currentPhase++;
			}

			PercentCompleted = 0;
		}
	}
}