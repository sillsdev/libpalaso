
using System.Collections.Generic;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Interface to simplify access to session files (media and written)</summary>
	public interface IIMDISessionFile
	{
		ResourceLink_Type ResourceLink { get; set; }
		Vocabulary_Type Format { get; set; }
		Vocabulary_Type Type { get; set; }
		String_Type Size { get; set; }
		List<Description_Type> Description { get; set; }
		Access_Type Access { get; set; }
	}
}
