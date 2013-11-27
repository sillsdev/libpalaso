using System;

namespace Palaso.Data
{
	///<summary>
	/// General data validation exception
	///</summary>
	public class ValidationException : Exception
	{
		///<summary>
		/// Constructor with a helpful error message.
		///</summary>
		///<param name="message"></param>
		public ValidationException(string message) : base(message)
		{
		}
	}
}
