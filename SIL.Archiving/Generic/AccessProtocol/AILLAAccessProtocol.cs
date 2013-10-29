using System;

namespace SIL.Archiving.Generic.AccessProtocol
{
	/// <summary>Access protocol for AILLA (The Archive of the Indigenous Languages of Latin America)</summary>
	/// <see cref="http://www.ailla.utexas.org/site/access_restrict.html#levels"/>
	public class AILLAAccessProtocol : AccessProtocolBase, IAccessProtocol
	{
		protected AccessOption _access;

		/// <summary>List of access options for The Language Archive</summary>
		public enum AccessOption
		{
			/// <summary>users have full access to these materials after agreeing to our Terms and Conditions and logging in</summary>
			Level1PublicAccess,
			/// <summary>you define a password with an optional hint</summary>
			Level2Password,
			/// <summary>after the date specified, access will change to Level 1</summary>
			Level3TimeLimit,
			/// <summary>users must contact you directly to ask you for the password</summary>
			Level4DepositorControl
		}

		/// <summary>Who has access, and how do you get access</summary>
		public AILLAAccessProtocol(AccessOption access)
		{
			_access = access;
		}

		/// <summary>Returns the code to use in the archive package</summary>
		public string GetAccessCode()
		{
			switch (_access)
			{
				case AccessOption.Level1PublicAccess:
					return "U";

				case AccessOption.Level2Password:
					return "R";

				case AccessOption.Level3TimeLimit:
					return "C";

				case AccessOption.Level4DepositorControl:
					return "S";
			}

			throw new NotSupportedException("The selected access protocol is not supported.");
		}
	}
}
