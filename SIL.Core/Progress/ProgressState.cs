using System;
using System.Text;

//For UML diagram, see ProgressSystem.uml (done in StarUML)

//NOTE: this "ProgressState" system is deprecated. Newer clients use the extensive SIL.Progress.IProgress classes, often along with the logbox.
namespace SIL.Progress
{
	/// <summary>
	/// Long-running tasks can be written to take one of these as an argument, and use it to notify others of their progress
	/// </summary>
	// Should be deprecated - [Obsolete("Use the extensive SIL.Progress.IProgress classes instead, possibly along with `SIL.Windows.Forms.Progress.LogBox`.")]
	public class ProgressState : IDisposable
	{
		private object _arguments;
		private int _totalNumberOfSteps;
		private int _numberOfStepsCompleted;
		private string _statusLabel;
		private StateValue _state = StateValue.NotStarted;
		private StringBuilder _logBuilder;

		private bool _doCancel = false;

		public event EventHandler StatusLabelChanged;
		public event EventHandler TotalNumberOfStepsChanged;
		public event EventHandler NumberOfStepsCompletedChanged;

		public event EventHandler StateChanged;
		public event EventHandler<LogEvent> Log;

		public class LogEvent : EventArgs
		{
			public string message;

			public LogEvent(string message)
			{
				this.message = message;
			}
		}

		public enum StateValue
		{
			NotStarted = 0,
			Busy,
			Finished,
			StoppedWithError
		} ;

		public ProgressState()
		{
			_numberOfStepsCompleted = 0;
			_logBuilder = new StringBuilder();
		}

		public void WriteToLog(string message)
		{
			if (Log != null)
			{
				Log.Invoke(this, new LogEvent(message));
			}
			_logBuilder.AppendLine(message);
		}

		/// <summary>
		/// How much the task is done
		/// </summary>
		public virtual int NumberOfStepsCompleted
		{
			get { return _numberOfStepsCompleted; }
			set
			{
				_numberOfStepsCompleted = value;
				if (NumberOfStepsCompletedChanged != null)
				{
					NumberOfStepsCompletedChanged(this, null);
				}
			}
		}

		/// <summary>
		/// a label which describes what we are busy doing
		/// </summary>
		public virtual string StatusLabel
		{
			get { return _statusLabel; }

			set
			{
				_statusLabel = value;
				if (StatusLabelChanged != null)
				{
					StatusLabelChanged(this, null);
				}
			}
		}

		public virtual int TotalNumberOfSteps
		{
			get {
			   // return 50;// disables freeze
				return _totalNumberOfSteps;
			}
			set
			{
				_totalNumberOfSteps = value;
				if (TotalNumberOfStepsChanged != null)
				{
					TotalNumberOfStepsChanged(this, null);
				}
			}
		}

		/// <summary>
		/// Normally, you'll wire the cancel button or whatever of the ui to this,
		/// then let the worker check our Cancel status in its inner loop.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void CancelRequested(object sender, EventArgs e)
		{
			_doCancel = true;
		}

		public virtual bool Cancel
		{
			get { return _doCancel; }
			set { _doCancel = value; }
		}

		#region IDisposable & Co. implementation

		//Courtesy  of Randy Regnier

		/// <summary>
		/// True, if the object has been disposed.
		/// </summary>
		private bool _isDisposed = false;

		private Exception _encounteredException;

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		public virtual StateValue State
		{
			get { return _state; }
			set
			{
				_state = value;
				if (StateChanged != null)
				{
					StateChanged(this, null);
				}
			}
		}

		public Exception ExceptionThatWasEncountered
		{
			get { return _encounteredException; }
			set { _encounteredException = value; }
		}

		public string LogString
		{
			get { return _logBuilder.ToString(); }
		}

		//set this to and object containing any info your worker method will need
		public object Arguments
		{
			get { return _arguments; }
			set { _arguments = value; }
		}

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. _isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~ProgressState()
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
			// Therefore, you should call GC.SuppressFinalize to
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
			if (_isDisposed)
			{
				return;
			}

			_statusLabel = null;

			_isDisposed = true;
		}

		#endregion IDisposable & Co. implementation
	}
}