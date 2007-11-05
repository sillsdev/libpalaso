using System;
using System.ComponentModel;
using System.Diagnostics;


namespace Palaso.Progress
{
	/// <summary>
	/// Long-running tasks can be written to take one of these as an argument, and use it to notify others of their progress
	/// </summary>
	public class BackgroundWorkerState : ProgressState
	{
		private BackgroundWorker _worker;

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
				return (this.NumberOfStepsCompleted / this.TotalNumberOfSteps) * 100;
			}
		}

		public override int NumberOfStepsCompleted
		{
			set
			{
				base.NumberOfStepsCompleted = value;
				_worker.ReportProgress(this.PercentCompleted, this);
			}
		}

		public override int TotalNumberOfSteps
		{
			set
			{
				base.TotalNumberOfSteps= value;
				_worker.ReportProgress(this.PercentCompleted, this);
			}
		}
	}
}