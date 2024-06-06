using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Unix.Native;
using SIL.ObjectModel;
using SIL.PlatformUtilities;

namespace SIL.Threading
{
	/// <summary>
	/// This is a cross-platform, global, named mutex that can be used to synchronize access to data across processes.
	/// It supports reentrant locking.
	///
	/// This is needed because Mono does not support system-wide, named mutexes. Mono does implement the Mutex class,
	/// but even when using the constructors with names, it only works within a single process.
	/// </summary>
	public class GlobalMutex : DisposableBase
	{
		private readonly IGlobalMutexAdapter _adapter;
		private readonly string _name;
		private bool _initialized;

		/// <summary>
		/// Initializes a new instance of the <see cref="GlobalMutex"/> class.
		/// </summary>
		public GlobalMutex(string name)
		{
			_name = name;
			if (Platform.IsWindows)
				_adapter = new WindowsGlobalMutexAdapter(name);
			else if (Platform.IsLinux)
				_adapter = new LinuxGlobalMutexAdapter(name);
			else
				_adapter = new ExplicitGlobalMutexAdapter(name);
		}

		/// <summary>
		/// Gets the mutex name.
		/// </summary>
		public string Name
		{
			get
			{
				CheckDisposed();

				return _name;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this mutex is initialized.
		/// </summary>
		public bool IsInitialized
		{
			get
			{
				CheckDisposed();

				return _initialized;
			}
		}

		/// <summary>
		/// Unlinks or removes the mutex from the system. This only has an effect on Linux.
		/// Windows will automatically unlink the mutex. This can be called while the mutex is locked.
		/// </summary>
		public bool Unlink()
		{
			CheckDisposed();

			return _adapter.Unlink();
		}

		/// <summary>
		/// Initializes this mutex.
		/// </summary>
		public bool Initialize()
		{
			CheckDisposed();

			bool res = _adapter.Init(false);
			_initialized = true;
			return res;
		}

		/// <summary>
		/// Initializes and locks this mutex. On Windows, this is an atomic operation, so the "createdNew"
		/// variable is guaranteed to return a correct value. On Linux, this is not an atomic operation,
		/// so "createdNew" is guaranteed to be correct only if it returns true.
		/// </summary>
		public IDisposable InitializeAndLock(out bool createdNew)
		{
			CheckDisposed();

			createdNew = _adapter.Init(true);
			return new ReleaseDisposable(_adapter);
		}

		/// <summary>
		/// Initializes and locks this mutex. This is an atomic operation on Windows, but not Linux.
		/// </summary>
		/// <returns></returns>
		public IDisposable InitializeAndLock()
		{
			bool createdNew;
			return InitializeAndLock(out createdNew);
		}

		/// <summary>
		/// Locks this mutex.
		/// </summary>
		public IDisposable Lock()
		{
			CheckDisposed();

			_adapter.Wait();
			return new ReleaseDisposable(_adapter);
		}

		/// <summary>
		/// Disposes managed resources.
		/// </summary>
		protected override void DisposeManagedResources()
		{
			_adapter.Dispose();
			_initialized = false;
		}

		[SuppressMessage("Gendarme.Rules.Correctness", "DisposableFieldsShouldBeDisposedRule",
			Justification = "m_adapter is a reference.")]
		private sealed class ReleaseDisposable : DisposableBase
		{
			private readonly IGlobalMutexAdapter _adapter;

			public ReleaseDisposable(IGlobalMutexAdapter adapter)
			{
				_adapter = adapter;
			}

			protected override void DisposeManagedResources()
			{
				_adapter.Release();
			}
		}

		private interface IGlobalMutexAdapter : IDisposable
		{
			bool Init(bool initiallyOwned);
			void Wait();
			void Release();
			bool Unlink();
		}

		/// <summary>
		/// On Linux, the global mutex is implemented using file locks.
		/// </summary>
		private class LinuxGlobalMutexAdapter : DisposableBase, IGlobalMutexAdapter
		{
			private int _handle;
			private readonly string _name;
			private readonly object _syncObject = new object();
			private readonly ThreadLocal<int> _waitCount = new ThreadLocal<int>();

			private const int LOCK_EX = 2;
			private const int LOCK_UN = 8;

			[DllImport("libc", SetLastError = true)]
			private static extern int flock(int handle, int operation);

			public LinuxGlobalMutexAdapter(string name)
			{
				_name = Path.Combine("/var/lock", name);
			}

			public bool Init(bool initiallyOwned)
			{
				bool result;
				_handle = Syscall.open(_name, OpenFlags.O_CREAT | OpenFlags.O_EXCL, FilePermissions.S_IWUSR | FilePermissions.S_IRUSR);
				if (_handle != -1)
				{
					result = true;
				}
				else
				{
					Errno errno = Stdlib.GetLastError();
					if (errno != Errno.EEXIST)
						throw new NativeException((int) errno);
					_handle = Syscall.open(_name, OpenFlags.O_CREAT, FilePermissions.S_IWUSR | FilePermissions.S_IRUSR);
					if (_handle == -1)
						throw new NativeException((int) Stdlib.GetLastError());
					result = false;
				}
				if (initiallyOwned)
					Wait();
				return result;
			}

			public void Wait()
			{
				if (_waitCount.Value == 0)
				{
					Monitor.Enter(_syncObject);
					if (flock(_handle, LOCK_EX) == -1)
						throw new NativeException(Marshal.GetLastWin32Error());
				}
				_waitCount.Value++;
			}

			public void Release()
			{
				_waitCount.Value--;
				if (_waitCount.Value == 0)
				{
					if (flock(_handle, LOCK_UN) == -1)
						throw new NativeException(Marshal.GetLastWin32Error());
					Monitor.Exit(_syncObject);
				}
			}

			public bool Unlink()
			{
				lock (_syncObject)
				{
					if (Syscall.unlink(_name) == -1)
					{
						Errno errno = Stdlib.GetLastError();
						if (errno == Errno.ENOENT)
							return false;
						throw new NativeException((int) errno);
					}
					return true;
				}
			}

			protected override void DisposeManagedResources()
			{
				Unlink();
			}

			protected override void DisposeUnmanagedResources()
			{
				Syscall.close(_handle);
			}
		}

		/// <summary>
		/// On Windows, the global mutex is implemented using a named mutex.
		/// </summary>
		private class WindowsGlobalMutexAdapter : DisposableBase, IGlobalMutexAdapter
		{
			private readonly string _name;
			private Mutex _mutex;

			public WindowsGlobalMutexAdapter(string name)
			{
				_name = name;
			}

			public bool Init(bool initiallyOwned)
			{
				bool createdNew;
				_mutex = new Mutex(initiallyOwned, _name, out createdNew);
				if (initiallyOwned && !createdNew)
					Wait();
				return createdNew;
			}

			public void Wait()
			{
				_mutex.WaitOne();
			}

			public void Release()
			{
				_mutex.ReleaseMutex();
			}

			public bool Unlink()
			{
				return true;
			}

			protected override void DisposeManagedResources()
			{
				_mutex.Dispose();
			}
		}

		/// <summary>
		/// A .NET native Mutex object works cross-process on all OSes if we prepend its name with "Global\".
		/// On multi-user systems (e.g., Terminal Server on Windows) this can cause one user to grab the lock that another user
		/// would like to get. If this is an important scenario, then a login session ID and/or username could be included in
		/// the name of the Mutex. Without prepending "Global\", though, named Mutexes don't work cross-process on OSes other
		/// than Windows.
		/// </summary>
		private class ExplicitGlobalMutexAdapter: WindowsGlobalMutexAdapter
		{
			private const string GLOBAL = "Global\\";

			public ExplicitGlobalMutexAdapter(string name) : base(name.StartsWith(GLOBAL) ? name : $"{GLOBAL}{name}") {}
		}
	}
}
