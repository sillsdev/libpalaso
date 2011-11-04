using NDesk.DBus;

namespace IBusDotNet
{
	public class InputBusWrapper
	{
		IIBus _inputBus;

		public InputBusWrapper(IBusConnection connection)
		{
			_inputBus = ((NDesk.DBus.Connection)connection).GetObject<IIBus>("org.freedesktop.IBus", new ObjectPath("/org/freedesktop/IBus"));
		}

		/// <summary>
		/// Allow Access to the underlying IIBus
		/// </summary>
		public IIBus InputBus {
			get { return _inputBus; }
		}
		/// <summary>
		/// Return the DBUS 'path' name for the currently focused InputContext
		/// Throws: System.Exception with message 'org.freedesktop.DBus.Error.Failed: No input context focused'
		/// if nothing is currently focused.
		/// </summary>
		public string GetFocusedInputContextPath()
		{
			return _inputBus.CurrentInputContext();
		}
	}
}
