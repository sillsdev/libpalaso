using System;
using System.Collections.Generic;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Generic
{
	/// <summary />
	public interface IArchivingPackage : IArchivingGenericObject
	{
		/// <summary>This is the languages for descriptions, a language of wider communication</summary>
		ArchivingLanguageCollection MetadataIso3Languages { get; set; }

		/// <summary>This is the languages for content, the language being researched</summary>
		ArchivingLanguageCollection ContentIso3Languages { get; set; }

		/// <summary>Sessions to include in this package</summary>
		List<IArchivingSession> Sessions { get; set; }

		/// <summary>Information about the funding for this project</summary>
		ArchivingProject FundingProject { get; set; }

		/// <summary>Information about the contact for this project</summary>
		ArchivingContactCollection Contacts { get; set; }

		/// <summary>Content Type for this project</summary>
		string ContentType { get; set; }

		/// <summary>Applicationse for this project</summary>
		string Applications { get; set; }

		/// <summary></summary>
		void AddKeyValuePair(string key, string value);

		/// <summary />
		string Author { get; set; }

		/// <summary />
		string Publisher { get; set; }

		/// <summary />
		string Owner { get; set; }

		/// <summary />
		ArchivingAccess Access { get; set; }
	}

	/// <summary>Collects the data needed to produce an archive package to upload</summary>
	public abstract class ArchivingPackage : IArchivingPackage
	{
		private readonly List<KeyValuePair<string, string>> _keys;

		public ArchivingLanguageCollection MetadataIso3Languages { get; set; }

		public ArchivingLanguageCollection ContentIso3Languages { get; set; }

		public List<IArchivingSession> Sessions { get; set; }

		public ArchivingProject FundingProject { get; set; }

		public ArchivingContactCollection Contacts { get; set; }

		/// <summary />
		protected ArchivingPackage()
		{
			MetadataIso3Languages = new ArchivingLanguageCollection();
			ContentIso3Languages = new ArchivingLanguageCollection();
			Sessions = new List<IArchivingSession>();
			Contacts = new ArchivingContactCollection();
			_keys = new List<KeyValuePair<string, string>>();
			Access = new ArchivingAccess();

		}

		protected IEnumerable<KeyValuePair<string, string>> Keys => _keys;

		/// <summary />
		public string Title { get; set; }

		/// <summary />
		public string Name { get; set; }

		/// <summary />
		public void AddDescription(LanguageString description)
		{
			throw new NotImplementedException();
		}

		/// <summary />
		public ArchiveAccessProtocol AccessProtocol { get; set; }

		/// <summary />
		public string AccessCode { get; set; }

		/// <summary />
		public ArchivingLocation Location { get; set; }

		public string ContentType { get; set; }

		public string Applications { get; set; }

		public void AddKeyValuePair(string key, string value)
		{
			_keys.Add(new KeyValuePair<string, string>(key, value));
		}

		public string Author { get; set; }

		public string Publisher { get; set; }

		public string Owner { get; set; }

		public ArchivingAccess Access { get; set; }
	}
}
