#if MONO
using System;
using NDesk.DBus;

namespace IBusDotNet
{
	// Reference counted wrapper around NDesk.DBus.Connection
	[CLSCompliant(false)]
	public class IBusConnection: IDisposable
	{
		private NDesk.DBus.Connection m_connection;
		private int m_Count;

		public event EventHandler Disposed;

		internal IBusConnection(NDesk.DBus.Connection connection)
		{
			m_Count = 1;
			m_connection = connection;
		}

		#region IDisposable implementation
#if DEBUG
		~IBusConnection()
		{
			System.Diagnostics.Debug.WriteLine("******* Missing Dispose() call for " + GetType().ToString() + " ********");
		}
#endif
		public void Dispose()
		{
			m_Count--;
			if (m_Count > 0)
				return;

			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool fDisposing)
		{
			if (fDisposing)
			{
				m_connection.Close();

				if (Disposed != null)
					Disposed(this, EventArgs.Empty);
			}
			m_connection = null;
		}
		#endregion

		internal void AddRef()
		{
			m_Count++;
		}

		#region NDesk.DBus.Connection methods
		public void Close()
		{
			Dispose();
		}

		public bool IsConnected
		{
			get { return m_connection.IsConnected; }
		}

		public void Iterate()
		{
			m_connection.Iterate();
		}

		public void Register(ObjectPath path, object obj)
		{
			m_connection.Register(path, obj);
		}

		public object Unregister(ObjectPath path)
		{
			return m_connection.Unregister(path);
		}
		// Do we need to add GetObject?
		#endregion

		public static explicit operator Connection(IBusConnection connection)
		{
			return connection.m_connection;
		}
	}
}
#endif
