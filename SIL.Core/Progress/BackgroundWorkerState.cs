using System;
using System.ComponentModel;

namespace SIL.Progress
{
	/// <summary>
	/// Long-running tasks can be written to take one of these as an argument, and use it to notify others of their progress
	/// </summary>
	// Should be deprecated - [Obsolete]
	public class BackgroundWorkerState : ProgressState
	{
		private BackgroundWorker _worker;
		private int _maxPercentageReachedBefore=0;

		public BackgroundWorkerState(BackgroundWorker worker )
			: base()
		{
			_worker = worker;
			_worker.WorkerReportsProgress = true; //if they're taking one of these guys, they must report progress
		}

		public override string StatusLabel
		{
			set
			{
				base.StatusLabel = value;
				_worker.ReportProgress(this.PercentCompleted, this);
			}
		}
		public override StateValue State
		{

			set
			{
				base.State = value;
				if (value == StateValue.StoppedWithError)
				{
					_worker.ReportProgress(0, this);
				}
			}
		}

		private int PercentCompleted
		{
			get
			{
				if (this.TotalNumberOfSteps <= 0)
				{
					return 0;
				}
				//limit to 100
				int p = Math.Min(100, (int)((((double)this.NumberOfStepsCompleted) / ((double)this.TotalNumberOfSteps)) * 100));
				return p;
			}
		}

		public override int NumberOfStepsCompleted
		{
			set
			{
				base.NumberOfStepsCompleted = value;
				//see the unit test "FreezeBugRegression" for details on the BAD bug this check avoids
				if (this.PercentCompleted > _maxPercentageReachedBefore)
				{
					_maxPercentageReachedBefore = this.PercentCompleted;
					_worker.ReportProgress(this.PercentCompleted, this);
				}
			}
			get
			{
				return base.NumberOfStepsCompleted;//helps the debugger
			}
		}

		public override int TotalNumberOfSteps
		{
			set
			{
				base.TotalNumberOfSteps= value;// disabling this removes the freeze
				int percentage= this.PercentCompleted;

				if (percentage > _maxPercentageReachedBefore)
				{
					_maxPercentageReachedBefore = percentage;
					_worker.ReportProgress(percentage, this);
				}
			}
			get
			{
				return base.TotalNumberOfSteps;//helps the debugger
			}
		}
	}
}