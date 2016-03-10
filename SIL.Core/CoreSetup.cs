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
	/// This class was originally created to help close NDesk.DBus on Linux. We keep it around
	/// for backwards compatibility.
	/// </remarks>
	[Obsolete("No longer needed.")]
	public class CoreSetup : IDisposable
	{
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
			}
			disposed = true;
		}
	}
}

