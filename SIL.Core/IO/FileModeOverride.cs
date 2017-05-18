// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
#if MONO
using Mono.Unix.Native;
#endif

namespace SIL.IO
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Overrides the system File mode permissions on Linux
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class FileModeOverride : IDisposable
	{
#if MONO
		private FilePermissions m_prevMask;
#endif

#if MONO
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Overrides the system File permissions with the default permissions of "002"
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public FileModeOverride()
			: this(FilePermissions.S_IWOTH)
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Overrides the system File permissions with the value passed in FilePermissions
		/// </summary>
		/// <param name="fp">file permissions value</param>
		/// ------------------------------------------------------------------------------------
		public FileModeOverride(FilePermissions fp)
		{
			SetFileCreationMask(fp);
		}
#endif

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		/// ------------------------------------------------------------------------------------
		~FileModeOverride()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

#if MONO
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set the File creation mode passed in by filePermissions.
		/// This will also set the previous value for m_prevMask used to reset the
		/// FilePermissions during Dispose.
		/// </summary>
		/// <param name="filePermissions">file permissions value</param>
		/// ------------------------------------------------------------------------------------
		private void SetFileCreationMask(FilePermissions filePermissions)
		{
			m_prevMask = Syscall.umask(filePermissions);
		}
#endif

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// See if the object has been disposed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public bool IsDisposed { get; private set; }

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Check to see if the object has been disposed.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(String.Format("'{0}' in use after being disposed.", GetType().Name));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		/// <remarks>Must not be virtual.</remarks>
		/// ------------------------------------------------------------------------------------
		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		/// <param name="disposing"></param>
		/// ------------------------------------------------------------------------------------
		protected virtual void Dispose(bool disposing)
		{
			System.Diagnostics.Debug.WriteLineIf(!disposing, "****** Missing Dispose() call for " + GetType().Name + ". ****** ");
			// Must not be run more than once.
			if (IsDisposed)
				return;

			if (disposing)
			{
#if MONO
				SetFileCreationMask(m_prevMask);
#endif
			}
			IsDisposed = true;
		}
	}
}
