using System;
using System.Threading.Tasks;

//For UML diagram, see ProgressSystem.uml (done in StarUML)

namespace SIL.Progress.Commands
{
	/// <summary>
	/// Delegate for a method which allows the progress range to be reset
	/// </summary>
	public delegate void InitializeProgressCallback(int minimum, int maximum);

	/// <summary>
	/// Delegate for a method which allows the progress to be updated
	/// </summary>
	public delegate void ProgressCallback(int progress);

	/// <summary>
	/// Delegate for a method which allows the status text to be updated
	/// </summary>
	public delegate void StatusCallback(string statusText);

	public abstract class BasicCommand : AsyncCommand
	{
		private InitializeProgressCallback _initializeCallback;
		private ProgressCallback _progressCallback;
		private StatusCallback _primaryStatusTextCallback;
		private StatusCallback _secondaryStatusTextCallback;

		public InitializeProgressCallback InitializeCallback
		{
			set { _initializeCallback = value; }
		}

		public ProgressCallback ProgressCallback
		{
			set { _progressCallback = value; }
		}

		public StatusCallback PrimaryStatusTextCallback
		{
			set { _primaryStatusTextCallback = value; }
		}

		public StatusCallback SecondaryStatusTextCallback
		{
			set { _secondaryStatusTextCallback = value; }
		}

		/// <summary>
		/// Implementation of the async work invoker
		/// </summary>
		protected override async Task BeginInvokeCore()
		{
			WorkInvoker worker = DoWork;
			var workTask = Task.Run(() => worker.Invoke(
				_initializeCallback,
				_progressCallback,
				_primaryStatusTextCallback,
				_secondaryStatusTextCallback));
			await EndWork(workTask).ConfigureAwait(false);
		}

		protected override async Task BeginInvokeCore2(ProgressState progress)
		{
			WorkInvoker2 worker = DoWork2;
			var workTask = Task.Run(() => worker.Invoke(progress));
			await EndWork(workTask).ConfigureAwait(false);
		}

		protected abstract void DoWork(
			InitializeProgressCallback initializeCallback,
			ProgressCallback progressCallback,
			StatusCallback primaryStatusTextCallback,
			StatusCallback secondaryStatusTextCallback
			);

		protected abstract void DoWork2(ProgressState progress);

		private async Task EndWork(Task workTask)
		{
			try
			{
				await workTask.ConfigureAwait(false);
				OnFinish(EventArgs.Empty);
			}
			catch (Exception e)
			{
				// Marshal exceptions back to the UI
				OnError(new ErrorEventArgs(e));
			}
		}
	}

	/// <summary>
	/// Delegate for a worker method which provides additional callbacks
	/// </summary>
	public delegate void WorkInvoker(
		InitializeProgressCallback initializeCallback,
		ProgressCallback progressCallback,
		StatusCallback primaryStatusTextCallback,
		StatusCallback secondaryStatusTextCallback
		);

	public delegate void WorkInvoker2(ProgressState progress);

	public delegate void WorkInvoker3(
		InitializeProgressCallback initializeCallback,
		ProgressCallback progressCallback,
		StatusCallback primaryStatusTextCallback,
		StatusCallback secondaryStatusTextCallback
		);
}