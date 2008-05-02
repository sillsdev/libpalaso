using System;
using System.Threading;

namespace Palaso.Services.ForServers
{
		internal class IpcServiceDescriptor : IDisposable
		{
			private Mutex _portAndChannelMutex;
			private Mutex _portMutex;

		   public IpcServiceDescriptor(Mutex portMutex, Mutex portAndChannelMutex)
			{
#if OptimizeRawPortChoosing
				if (portMutex == null)
				{
					throw new ArgumentNullException("portMutex");
				}
#endif
				if (portAndChannelMutex == null)
				{
					throw new ArgumentNullException("portAndChannelMutex");
				}
			   _portMutex = portMutex;
				_portAndChannelMutex = portAndChannelMutex;
			}

		#region IDisposable Members
		#if DEBUG
		~IpcServiceDescriptor()
		{
			if (!this._disposed)
			{
				throw new InvalidOperationException("Disposed not explicitly called on " + GetType().FullName + ".");
			}
		}
		#endif

		private bool _disposed = false;
		private bool _delayWritingCachesUntilDispose = false;


			public bool IsDisposed
		{
			get {
				return _disposed;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!IsDisposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
					if (_portMutex != null)
					{
						_portMutex.ReleaseMutex();
						_portMutex = null;
					}
					if (_portAndChannelMutex != null)
					{
						_portAndChannelMutex.ReleaseMutex();
						_portAndChannelMutex = null;
					}
				}

				// shared (dispose and finalizable) cleanup logic
				this._disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (this._disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		#endregion
		}
}
