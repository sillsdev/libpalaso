using System;

namespace Palaso
{	/// <summary>
	/// Provide global setup and shutdown for the Palaso assembly.
	/// </summary>
	/// <remarks>
	/// This would be used something like the following in a program's Main() method:
	/// using (new PalasoSetup())
	/// {
	/// 	Application.Run(new MainWindow(args));
	/// }
	/// </remarks>
	/// <remarks>
	/// This class was originally created to help close NDesk.DBus on
	/// Linux, which it is no longer used for, but might be useful for
	/// another purpose in the future.
	/// </remarks>
	public class PalasoSetup : IDisposable
	{
		public PalasoSetup()
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
			}
			disposed = true;
		}
	}
}

