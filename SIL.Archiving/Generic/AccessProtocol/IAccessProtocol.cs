using System;

namespace SIL.Archiving.Generic.AccessProtocol
{
	/// <summary>Who has access, and how do you get access</summary>
	public interface IAccessProtocol
	{
		/// <summary>Returns the code to use in the archive package</summary>
		string GetAccessCode();
	}

	/// <summary>Fields common to all or most access systems</summary>
	public abstract class AccessProtocolBase
	{
		/// <summary />
		public DateTime Date;

		/// <summary />
		public ArchivingContact Contact;

		/// <summary />
		public LanguageString Description;
	}
}
