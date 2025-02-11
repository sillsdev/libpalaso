using System;
using System.Diagnostics;

namespace SIL.ObjectModel
{
	/// <summary>
	/// base class for helper classes who don't want to copy the same basic code
	/// trying to implement IDisposable
	/// </summary>
	public abstract class DisposableBase : IDisposable
	{
		#region IDisposable & Co. implementation
		/// <summary>
		/// Check to see if the object has been disposed.
		/// All public Properties and Methods should call this
		/// before doing anything else.
		/// </summary>
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		public bool IsDisposed
		{
			get; private set;
		}

		/// <summary>
		/// Finalizer, in case client doesn't dispose it.
		/// Force Dispose(false) if not already called (i.e. m_isDisposed is true)
		/// </summary>
		/// <remarks>
		/// In case some clients forget to dispose it directly.
		/// </remarks>
		~DisposableBase()
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
			Debug.WriteLineIf(!disposing, "******************* Missing Dispose() call for " + GetType().Name + " ******************");

			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
				// Dispose managed resources here.
				DisposeManagedResources();
			}

			// Dispose unmanaged resources here, whether disposing is true or false.
			DisposeUnmanagedResources();
			IsDisposed = true;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Override to dispose managed resources.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual void DisposeManagedResources()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Override to dispose unmanaged resources.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		protected virtual void DisposeUnmanagedResources()
		{
		}

		#endregion IDisposable & Co. implementation
	}
}
