using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Generic
{
	/// <summary>Base class for archiving objects</summary>
	public interface IArchivingGenericObject
	{
		/// <summary>If needed but not given, Name will be used</summary>
		string Title { get; set; }

		/// <summary>If needed but not given, Title will be used</summary>
		string Name { get; set; }

		/// <summary />
		void AddDescription(LanguageString description);

		/// <summary>Who has access, and how do you get access. Different archives use this differently</summary>
		ArchiveAccessProtocol AccessProtocol { get; set; }

		/// <summary>The access level code for this object</summary>
		string AccessCode { get; set; }

		/// <summary />
		ArchivingLocation Location { get; set; }
	}
}
