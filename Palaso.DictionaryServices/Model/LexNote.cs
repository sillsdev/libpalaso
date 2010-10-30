using System.Collections.Generic;
using Palaso.Lift;

namespace Palaso.DictionaryServices.Model
{
	/// <summary>
	/// A note is used for storing descriptive information of many kinds including comments, bibliographic information and domain specific notes. Notes are used to hold informational content rather than meta-information about an element, for which an annotation should be used.
	/// </summary>
	public sealed class LexNote:  MultiText, IExtensible
	{
		/// <summary>
		/// optional
		/// </summary>
		public string Type;

		public LexNote()
		{
			Type = string.Empty;
		}

		public LexNote(string type)
		{
			Type = type;
		}

		public List<LexTrait> Traits{get;private set;}
		public List<LexField> Fields { get; private set; }

	}
}