using System;
using System.Collections.Generic;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Generic
{
	/// <summary>Collects the data needed to produce an archive package to upload</summary>
	public abstract class ArchivingPackage : IArchivingGenericObject
	{
		/// <summary>This is the languages for descriptions, a language of wider communication</summary>
		public HashSet<string> MetadataIso3LanguageIds;

		/// <summary>This is the languages for content, the language being researched</summary>
		public HashSet<string> ContentIso3LanguageIds;

		/// <summary>Sessions to include in this package</summary>
		public List<IArchivingSession> Sessions;

		/// <summary>Information about the funding for this project</summary>
		public ArchivingProject FundingProject;

		/// <summary>Information about the contact for this project</summary>
		public ArchivingContactCollection Contacts;

		/// <summary />
		protected ArchivingPackage()
		{
			MetadataIso3LanguageIds = new HashSet<string>();
			ContentIso3LanguageIds = new HashSet<string>();
			Sessions = new List<IArchivingSession>();
			Contacts = new ArchivingContactCollection();
		}

		/// <summary></summary>
		public string Title { get; set; }

		/// <summary></summary>
		public string Name { get; set; }

		/// <summary></summary>
		public void AddDescription(LanguageString description)
		{
			throw new NotImplementedException();
		}

		/// <summary></summary>
		public ArchiveAccessProtocol AccessProtocol { get; set; }

		/// <summary></summary>
		public string AccessCode { get; set; }

		/// <summary></summary>
		public ArchivingLocation Location { get; set; }
	}
}
