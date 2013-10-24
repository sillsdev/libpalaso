using System.Collections.Generic;

namespace SIL.Archiving.Generic
{
	/// <summary>Collects the data needed to produce an archive package to upload</summary>
	public abstract class ArchivingPackage : ArchivingGenericObject
	{
		/// <summary>This is the languages for descriptions, a language of wider communication</summary>
		public HashSet<string> MetadataIso3LanguageIds;

		/// <summary>This is the languages for content, the language being researched</summary>
		public HashSet<string> ContentIso3LanguageIds;

		/// <summary>Sessions to include in this package</summary>
		public List<ArchivingSession> Sessions;

		/// <summary>Location information for this package</summary>
		public ArchivingLocation Location;

		/// <summary>Information about the funding for this project</summary>
		public ArchivingProject FundingProject;

		/// <summary>Information about the contact for this project</summary>
		public ArchivingContact ProjectContact;

		/// <summary />
		protected ArchivingPackage()
		{
			MetadataIso3LanguageIds = new HashSet<string>();
			ContentIso3LanguageIds = new HashSet<string>();
			Sessions = new List<ArchivingSession>();
		}
	}
}
