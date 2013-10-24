
using System;

namespace SIL.Archiving.Generic
{
	public abstract class ArchivingGenericObject
	{
		/// <summary>If needed but not given, Name will be used</summary>
		public string Title;

		/// <summary>If needed but not given, Title will be used</summary>
		public string Name;

		/// <summary />
		public LanguageString Description;

		/// <summary>Date the first entry was created</summary>
		public DateTime DateCreatedFirst;

		/// <summary>Date the last entry was created. Different archives use this differently</summary>
		public DateTime DateCreatedLast;

		/// <summary />
		public DateTime DateModified;

		/// <summary>Who has access, and how do you get access. Different archives use this differently</summary>
		public IAccessProtocol AccessProtocol;
	}

}
