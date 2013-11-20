
using System.Collections.Generic;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Interface to simplify access to session files (media and written)</summary>
	public interface IIMDISessionFile
	{
		ResourceLinkType ResourceLink { get; set; }
		VocabularyType Format { get; set; }
		VocabularyType Type { get; set; }
		string Size { get; set; }
		List<DescriptionType> Description { get; set; }
		AccessType Access { get; set; }
	}
}
