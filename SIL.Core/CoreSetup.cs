using System;

namespace SIL
{	/// <summary>
	/// Provide global setup and shutdown for the SIL.Core assembly.
	/// </summary>
	/// <remarks>
	/// This should be used something like the following in a program's Main() method:
	/// using (new CoreSetup())
	/// {
	/// 	Application.Run(new MainWindow(args));
	/// }
	/// </remarks>
	/// <remarks>
	/// At the moment, this is needed only if the program implicitly or explicitly
	/// uses SIL.UsbDrive on Linux/Mono.
	/// </remarks>
	public class CoreSetup : IDisposable
	{
		public CoreSetup()
		{
		}
		private bool disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
				return;
			if (disposing)
			{
#if __MonoCS__
				// Using Palaso.UsbDrive on Linux/Mono results in NDesk spinning up a thread that
				// continues until NDesk Bus is closed.  Failure to close the thread results in a
				// program hang when closing.  Closing the system bus allows the thread to close,
				// and thus the program to close.  Closing the system bus can happen safely only
				// at the end of the program.
				NDesk.DBus.Bus.System.Close();
#endif
			}
			disposed = true;
		}
	}
}

