using System;

namespace SIL.Archiving.IMDI
{
	/// <summary>Access protocol for ELAR (Endangered Languages Archive)</summary>
	/// <see cref="http://www.elar-archive.org/depositing/preparing-access-protocol.php"/>
	public class ElarAccessProtocol : AccessProtocol, IAccessProtocol
	{
		protected AccessOption _access;

		/// <summary>List of access options for The Language Archive</summary>
		public enum AccessOption
		{
			/// <summary>all Users can access</summary>
			U,
			/// <summary>Researchers and Community members are allowed access</summary>
			R,
			/// <summary>only Community members are allowed access</summary>
			C,
			/// <summary>only Subscribers are allowed access</summary>
			S
		}

		/// <summary>Who has access, and how do you get access</summary>
		public ElarAccessProtocol(AccessOption access)
		{
			_access = access;
		}

		/// <summary>Returns the code to use in the archive package</summary>
		public string GetAccessCode()
		{
			switch (_access)
			{
				case AccessOption.U:
					return "U";

				case AccessOption.R:
					return "R";

				case AccessOption.C:
					return "C";

				case AccessOption.S:
					return "S";
			}

			throw new NotSupportedException("The selected access protocol is not supported.");
		}
	}
}
