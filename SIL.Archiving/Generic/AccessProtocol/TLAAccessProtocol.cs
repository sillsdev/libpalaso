using System;

namespace SIL.Archiving.Generic.AccessProtocol
{
	/// <summary>Access protocol for The Language Archive</summary>
	/// <see cref="http://tla.mpi.nl/resources/access-permissions/"/>
	public class TLAAccessProtocol : AccessProtocolBase, IAccessProtocol
	{
		protected AccessOption _access;

		/// <summary>List of access options for The Language Archive</summary>
		public enum AccessOption
		{
			/// <summary>can be accessed immediately</summary>
			Open,
			/// <summary>can be accessed by registered users</summary>
			Restricted,
			/// <summary>can be accessed on request only</summary>
			Protected,
			/// <summary>can be accessed only by the depositors</summary>
			Closed
		}

		/// <summary>Who has access, and how do you get access</summary>
		public TLAAccessProtocol(AccessOption access)
		{
			_access = access;
		}

		/// <summary>Returns the code to use in the archive package</summary>
		public string GetAccessCode()
		{
			switch (_access)
			{
				case AccessOption.Open:
					return "Open Access";

				case AccessOption.Restricted:
					return "Restricted Access";

				case AccessOption.Protected:
					return "Protected Access";

				case AccessOption.Closed:
					return "Closed Access";
			}

			throw new NotSupportedException("The selected access protocol is not supported.");
		}
	}
}
