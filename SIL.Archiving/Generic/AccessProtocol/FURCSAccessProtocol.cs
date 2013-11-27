using System;

namespace SIL.Archiving.Generic.AccessProtocol
{
	/// <summary>Access protocol for AILCA (The Archive of Indigenous Languages and Cultures of Asia)</summary>
	/// <see cref="http://diha.ntu.edu.sg/home.html"/>
	public class FURCSAccessProtocol : AccessProtocolBase, IAccessProtocol
	{
		protected AccessOption _access;

		/// <summary>List of access options for The Language Archive</summary>
		public enum AccessOption
		{
			/// <summary>access is Free to all</summary>
			F,
			/// <summary>all Users can access (requires registration)</summary>
			U,
			/// <summary>Researchers and Community members are allowed access</summary>
			// ReSharper disable once InconsistentNaming
			RC,
			/// <summary>only Community members are allowed access (normally requires application to Depositor)</summary>
			C,
			/// <summary>only Subscribers are allowed access (requires application to Depositor)</summary>
			S,
			/// <summary>only the Depositor and delegate can access</summary>
			None
		}

		/// <summary>Who has access, and how do you get access</summary>
		public FURCSAccessProtocol(AccessOption access)
		{
			_access = access;
		}

		/// <summary>Returns the code to use in the archive package</summary>
		public string GetAccessCode()
		{
			switch (_access)
			{
				case AccessOption.F:
					return "F";

				case AccessOption.U:
					return "U";

				case AccessOption.RC:
					return "RC";

				case AccessOption.C:
					return "C";

				case AccessOption.S:
					return "S";

				case AccessOption.None:
					return "";
			}

			throw new NotSupportedException("The selected access protocol is not supported.");
		}
	}
}
