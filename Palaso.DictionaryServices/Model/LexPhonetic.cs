using System.Collections.Generic;
using Palaso.Lift;

namespace Palaso.DictionaryServices.Model
{
	/// <summary>
	/// Not implemented: media
	/// Not implemented: extensible.date
	/// </summary>
	public sealed class LexPhonetic: MultiText, IExtensible
	{
		public LexPhonetic()
		{
			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
		}

		#region IExtensible
		public List<LexTrait> Traits{get;private set;}
		public List<LexField> Fields { get; private set; }
		#endregion

	}
}