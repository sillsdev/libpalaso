using System;
using System.Diagnostics;
using System.Threading.Tasks;

//For UML diagram, see ProgressSystem.uml (done in StarUML)

namespace SIL.Progress.Commands
{
	/// <summary>
	/// An abstract base for an implementation of a command that will spawn a long operation
	/// </summary>
	/// <remarks>
	/// You should override the <see cref="BeginInvokeCore()"/> method to
	/// invoke your operation using whichever asynchronous mechanism you prefer
	/// </remarks>
	public abstract class AsyncCommand
	{
		private bool enabled = true;
		private volatile bool _canceling;

		/// <summary>
		/// Standard constructor, protected because this is an abstract
		/// class
		/// </summary>
		protected AsyncCommand()
		{
		}

		/// <summary>
		/// This property can be used to enable/disable
		/// the command, and should be called only from
		/// the UI thread
		/// </summary>
		public bool Enabled
		{
			get
			{
				return enabled;
			}
			set
			{
				if( enabled != value )
				{
					enabled = value;
					OnEnabledChanged( EventArgs.Empty );
				}
			}
		}

		/// <summary>
		/// Raised when the command is enabled and disabled
		/// </summary>
		public event EventHandler EnabledChanged;

		/// <summary>
		/// Asynchronously executes the operation, returning immediately.
		/// </summary>
		/// <returns>
		/// Returns false if the operation was not <see cref="Enabled"/>
		/// when we called the method, otherwise true.
		/// </returns>
		/// <remarks>
		/// The method automatically disables the operation once it is
		/// running, to prevent it accidentally being started again. It should
		/// only be called from the UI thread.
		/// </remarks>
		public bool BeginInvoke()
		{
			if( !enabled )
			{
				return false;
			}
			// Disable the operation while it is started, notifying everyone
			// of that fact
			Enabled = false;
			_canceling = false;
			BeginInvokeCore();
			return true;
		}

		public bool BeginInvoke(ProgressState progress)
		{
			if( !enabled )
			{
				return false;
			}
			// Disable the operation while it is started, notifying everyone
			// of that fact
			Enabled = false;
			_canceling = false;
			BeginInvokeCore2(progress);
			return true;
		}

		/// <summary>
		/// Begin an attempt to cancel the operation
		/// </summary>
		/// <remarks>This should only be called from the UI thread</remarks>
		public void Cancel()
		{
			_canceling = true;
			OnBeginCancel( EventArgs.Empty );
		}

		/// <summary>
		/// Gets a value which determines whether the operation is being cancelled
		/// </summary>
		/// <remarks>
		/// This may be called either from the UI, or by derived classes from the worker thread
		/// </remarks>
		public bool Canceling
		{
			get
			{
				return _canceling;
			}
		}

		/// <summary>
		/// Raised when a cancel request is raised.
		/// </summary>
		/// <remarks>This will only be raised on the UI
		/// thread. Note that the operation may already have completed by the
		/// time the cancel event is raised, but you are guaranteed not to
		/// receive a <see cref="Finish"/> or <see cref="Error"/> event between a call to <see cref="Cancel"/>
		/// and the BeginCancel event itself.
		/// </remarks>
		public event EventHandler BeginCancel;

		/// <summary>
		/// Raised when the operation has finished
		/// </summary>
		/// <remarks>
		/// This will be raised on the worker thread, not the GUI thread. There are guaranteed to be
		/// no further events from the command after this event has been raised.
		/// </remarks>
		public event EventHandler Finish;

		/// <summary>
		/// Raised when the operation has finished, and an exception has occurred
		/// </summary>
		/// <remarks>This will be raised on the worker thread, not the GUI thread. There are guaranteed to be
		/// no further events from the command after this event has been raised.</remarks>
		public event ErrorEventHandler Error;

		/// <summary>
		/// Override this method to invoke the actual
		/// long operation using your preferred async mode.
		/// </summary>
		protected abstract Task BeginInvokeCore();

		protected abstract Task BeginInvokeCore2(ProgressState progress);

		/// <summary>
		/// Raises the Finish event.
		/// </summary>
		/// <param name="e">Event data</param>
		protected void OnFinish( EventArgs e )
		{
			Debug.WriteLine("AsyncCommand:Finish");

			EventHandler finish = Finish;
			if( finish != null )
			{
				finish( this, e );
			}
		}

		/// <summary>
		/// Raises the Error event
		/// </summary>
		/// <param name="e">Event data</param>
		protected void OnError( ErrorEventArgs e )
		{
			ErrorEventHandler error = Error;
			if( error != null )
			{
				error( this, e );
			}
		}

		private void OnBeginCancel( EventArgs e )
		{
			EventHandler beginCancel = BeginCancel;
			if( beginCancel != null )
			{
				beginCancel( this, e );
			}
		}

		private void OnEnabledChanged( EventArgs e )
		{
			EventHandler enabledChanged = EnabledChanged;
			if( enabledChanged != null )
			{
				enabledChanged( this, e );
			}
		}


	}

	/// <summary>
	/// Delegate for the <see cref="AsyncCommand.Error"/> event
	/// </summary>
	public delegate void ErrorEventHandler( object sender, ErrorEventArgs e );

	/// <summary>
	/// Class to contain the event data for the error event
	/// </summary>
	public class ErrorEventArgs : EventArgs
	{
		private Exception exception;

		/// <summary>
		/// Gets the exception that occurred, or null if there was an unspecified error
		/// </summary>
		public Exception Exception
		{
			get
			{
				return exception;
			}
		}
		/// <summary>
		/// Standard constructor
		/// </summary>
		/// <param name="exception">The exception that occurred, or null if an unspecified error occurred</param>
		public ErrorEventArgs( Exception exception )
		{
			this.exception = exception;
		}
	}
}