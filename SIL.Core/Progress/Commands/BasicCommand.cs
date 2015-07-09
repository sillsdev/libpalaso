using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;

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
		/// <remarks>
		/// We're using the delegate BeginInvoke() / EndInvoke() pattern here
		/// </remarks>
		protected override void BeginInvokeCore()
		{
			WorkInvoker worker = new WorkInvoker( DoWork );
			worker.BeginInvoke(
				_initializeCallback,
				_progressCallback ,
				_primaryStatusTextCallback ,
				_secondaryStatusTextCallback,
				new AsyncCallback( EndWork ), null );
		}

		protected override void BeginInvokeCore2(ProgressState progress)
		{
			WorkInvoker2 worker = new WorkInvoker2(DoWork2);
			worker.BeginInvoke(progress, new AsyncCallback(EndWork2), null);
		}

		protected abstract void DoWork(
			InitializeProgressCallback initializeCallback,
			ProgressCallback progressCallback,
			StatusCallback primaryStatusTextCallback,
			StatusCallback secondaryStatusTextCallback
			);

		protected abstract void DoWork2(ProgressState progress);

		private void EndWork( IAsyncResult result )
		{
			AsyncResult asyncResult = (AsyncResult)result;
			WorkInvoker asyncDelegate = (WorkInvoker)asyncResult.AsyncDelegate;
			try
			{
				asyncDelegate.EndInvoke( result );
				OnFinish( EventArgs.Empty );
			}
			catch( Exception e )
			{
				// Marshal exceptions back to the UI
				OnError( new ErrorEventArgs( e ) );
			}
//            catch
//            {
//                // Do our exception handling; include a default catch
//                // block because this is the final handler on the stack for this
//                // thread, and we need to log these kinds of problems
//                OnError( new ErrorEventArgs( null ) );
//            }
		}
		private void EndWork2(IAsyncResult result)
		{
			Debug.WriteLine("BasicCOmmand:EndWork2");
			AsyncResult asyncResult = (AsyncResult)result;
			WorkInvoker2 asyncDelegate = (WorkInvoker2)asyncResult.AsyncDelegate;
			try
			{
				asyncDelegate.EndInvoke(result);
				OnFinish(EventArgs.Empty);
			}
			catch (Exception e)
			{
				// Marshal exceptions back to the UI
				OnError(new ErrorEventArgs(e));
			}
			//            catch
			//            {
			//                // Do our exception handling; include a default catch
			//                // block because this is the final handler on the stack for this
			//                // thread, and we need to log these kinds of problems
			//                OnError( new ErrorEventArgs( null ) );
			//            }
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