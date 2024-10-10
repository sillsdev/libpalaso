// Copyright (c) 2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System;
using JetBrains.Annotations;
using Mono.Unix.Native;
using SIL.PlatformUtilities;

namespace SIL.IO
{
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Overrides the system File mode permissions on Linux
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	public class FileModeOverride : IDisposable
	{
		private uint m_prevMask;

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Overrides the system File permissions with the default permissions of "002"
		/// </summary>
		/// ------------------------------------------------------------------------------------
		public FileModeOverride()
		{
			if (Platform.IsUnix)
				SetFileCreationMask((uint) FilePermissions.S_IWOTH);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Overrides the system File permissions with the value passed in FilePermissions
		/// </summary>
		/// <param name="fp">file permissions value</param>
		/// ------------------------------------------------------------------------------------
		[CLSCompliant(false)]
		public FileModeOverride(uint fp)
		{
			if (Platform.IsUnix)
				SetFileCreationMask(fp);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// </summary>
		/// ------------------------------------------------------------------------------------
		~FileModeOverride()
		{
			Dispose(false);
			// The base class finalizer is called automatically.
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Set the File creation mode passed in by filePermissions.
		/// This will also set the previous value for m_prevMask used to reset the
		/// FilePermissions during Dispose.
		/// </summary>
		/// <param name="filePermissions">file permissions value</param>
		/// ------------------------------------------------------------------------------------
		private void SetFileCreationMask(uint filePermissions)
		{
			m_prevMask = (uint) Syscall.umask((FilePermissions) filePermissions);
		}

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
		[PublicAPI]
		public void CheckDisposed()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(
					$"'{GetType().Name}' in use after being disposed.");
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
			// Therefore, you should call GC.SuppressFinalize to
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
				if (Platform.IsUnix)
					SetFileCreationMask(m_prevMask);
			}
			IsDisposed = true;
		}
	}
}
