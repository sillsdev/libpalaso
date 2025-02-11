using System;

namespace SIL.PlatformUtilities
{
	/// <summary>
	/// This exception class represents errors returned by native system calls.
	/// </summary>
	public class NativeException : Exception
	{
		private readonly int _error;

		/// <summary>
		/// Initializes a new instance of the <see cref="NativeException"/> class.
		/// </summary>
		public NativeException(int error)
			: base(string.Format("An error with the number, {0}, occurred.", error))
		{
			_error = error;
		}

		/// <summary>
		/// Gets the error number.
		/// </summary>
		public int Error
		{
			get { return _error; }
		}
	}
}
