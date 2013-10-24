
using System;
using SIL.Archiving.Generic;

namespace SIL.Archiving
{
	/// <summary>Who has access, and how do you get access</summary>
	public interface IAccessProtocol
	{
		/// <summary>Returns the code to use in the archive package</summary>
		string GetAccessCode();
	}

	/// <summary>Fields common to all or most access systems</summary>
	public abstract class AccessProtocol
	{
		/// <summary />
		public DateTime Date;

		/// <summary />
		public ArchivingContact Contact;

		/// <summary />
		public LanguageString Description;
	}

	/// <summary>Access protocol for AILCA (The Archive of Indigenous Languages and Cultures of Asia)</summary>
	/// <see cref="http://diha.ntu.edu.sg/home.html"/>
	public class AilcaAccessProtocol : AccessProtocol, IAccessProtocol
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
		public AilcaAccessProtocol(AccessOption access)
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

	/// <summary>Access protocol for AILLA (The Archive of the Indigenous Languages of Latin America)</summary>
	/// <see cref="http://www.ailla.utexas.org/site/access_restrict.html#levels"/>
	public class AillaAccessProtocol : AccessProtocol, IAccessProtocol
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
		public AillaAccessProtocol(AccessOption access)
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
