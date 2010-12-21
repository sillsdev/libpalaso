using System.Collections.Generic;
using Palaso.Lift;
using Palaso.Text;

namespace Palaso.DictionaryServices.Model
{
	/// <summary>
	/// Not implemented: media
	/// Not implemented: extensible.date
	/// </summary>
	public sealed class LexEtymology: MultiText, IExtensible
	{
		public string Type { get; set; }
		public string Source { get; set; }

		public LexEtymology(string type, string source)
		{
			Type = type;
			Source = source;
			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
			Gloss = new MultiText();
			Comment = new MultiText();
		}
		public MultiText Gloss { get; set; }


		/// <summary>
		/// as of lift 0.13, this doesn't exist, so the writer will have to make a <field/> or something
		/// </summary>
		public MultiText Comment { get; set; }

		#region IExtensible
		public List<LexTrait> Traits{get;private set;}
		public List<LexField> Fields { get; private set; }
		#endregion

	}
}