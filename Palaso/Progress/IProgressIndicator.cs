using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace Palaso.Progress
{
	public interface IProgressIndicator
	{
		int GetTotalNumberOfSteps();
		int NumberOfStepsCompleted { get; set; }
		int PercentCompleted { get; set; }
		void NextStep(); // advance indicator one step
		void Finish();
		void Initialize(int numberOfSteps);
		SynchronizationContext SyncContext { get; set; }
	}

	public class SimpleProgressIndicator : ProgressBar, IProgressIndicator
	{
		public SimpleProgressIndicator()
		{
			Style = ProgressBarStyle.Continuous;
			NumberOfStepsCompleted = 0;
			Maximum = 100;
		}

		public SynchronizationContext SyncContext { get; set; }

		public int PercentCompleted
		{
			get { return (NumberOfStepsCompleted*100/Maximum); }
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentOutOfRangeException("PercentCompleted must be between 0 and 100");
				}
				NumberOfStepsCompleted = value*Maximum/100;
			}
		}

		public void NextStep()
		{
			if (SyncContext != null)
			{
				SyncContext.Post(Increment, 1);
			}
			else
			{
				Increment(1);
			}
		}

		private void Increment(object state)
		{
			Increment(state as string);
		}

		public void Finish()
		{
			NumberOfStepsCompleted = Maximum;
		}

		public void Initialize(int numberOfSteps)
		{
			NumberOfStepsCompleted = 0;
			Maximum = numberOfSteps;
		}

		public int GetTotalNumberOfSteps()
		{
			return Maximum;
		}

		public int NumberOfStepsCompleted
		{
			get { return Value; }

			set
			{
				 if (SyncContext != null)
				 {
					SyncContext.Post(UpdateValue, value);
				 }
				 else
				 {
					 Value = value;
				 }
			}
		}

		private void UpdateValue(object state)
		{
			Value = (int) state;
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
		private int _stepInCurrentPhase;
		private int _currentPhaseNumberOfSteps;

		public MultiPhaseProgressIndicator(IProgressIndicator indicator, int numberOfPhases)
		{
			_globalIndicator = indicator;
			_stepInCurrentPhase = 0;
			_currentPhaseNumberOfSteps = 100;
			_globalIndicator.Initialize(1000000);  // we pick a really high number so that we don't have to expand it

			_numberOfPhases = numberOfPhases;
			_currentPhase = 0;  // must call Initialize() to increment the _currentProcess
		}

		public SynchronizationContext SyncContext
		{
			get { return _globalIndicator.SyncContext; }
			set { _globalIndicator.SyncContext = value; }
		}

		public int PercentCompleted  // per process
		{
			get { return (_stepInCurrentPhase * 100 / _currentPhaseNumberOfSteps); }
			set
			{
				if (value < 0 || value > 100)
				{
					throw new ArgumentOutOfRangeException("PercentCompleted must be between 0 and 100");
				}
				NumberOfStepsCompleted = value * _currentPhaseNumberOfSteps / 100;
			}
		}

		public void NextStep() // for current process
		{
			if (NumberOfStepsCompleted < GetTotalNumberOfSteps())
			{
				NumberOfStepsCompleted = NumberOfStepsCompleted + 1;
			}
		}

		public void Finish() // Finish current process
		{
			NumberOfStepsCompleted = _currentPhaseNumberOfSteps;
			//_globalIndicator.CurrentStep = _currentPhase * NumberOfGlobalStepsPerProcess();
		}

		private int NumberOfGlobalStepsPerPhase
		{
			get { return _globalIndicator.GetTotalNumberOfSteps()/_numberOfPhases; }
		}

		public void Initialize(int numberOfSteps)  // Initialize/begin next process
		{
			if (_currentPhase != _numberOfPhases)
			{
				_currentPhase++;
			}
			NumberOfStepsCompleted = 0;
			_currentPhaseNumberOfSteps = numberOfSteps;
		}

		public int GetTotalNumberOfSteps()
		{
			return _currentPhaseNumberOfSteps;
		}

		public int NumberOfStepsCompleted // current step for the current process
		{
			get { return _stepInCurrentPhase; }
			set
			{
				if (value > GetTotalNumberOfSteps() || value < 0)
				{
					throw new ArgumentOutOfRangeException(string.Format("CurrentStep must be set to a number between 0 and {0}", GetTotalNumberOfSteps()));
				}
				_stepInCurrentPhase = value;
				_globalIndicator.NumberOfStepsCompleted = (value*NumberOfGlobalStepsPerPhase/_currentPhaseNumberOfSteps) + (NumberOfGlobalStepsPerPhase*(_currentPhase - 1));
			}
		}
	}
}