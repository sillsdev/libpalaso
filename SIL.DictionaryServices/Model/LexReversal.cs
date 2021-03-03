using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Lift;
using SIL.Text;

namespace SIL.DictionaryServices.Model
{
	/// <summary>
	/// Not implemented: main (waiting on liftwriter support)
	/// Not implemented: grammatical-info
	/// Not implemented: extensible.date
	/// </summary>
	public sealed class LexReversal:  MultiText
	{
		/// <summary>
		/// From a reversal-type rangeset
		/// </summary>
		public string Type { get; set; }

		public override IPalasoDataObjectProperty Clone()
		{
			var clone = new LexReversal();
			clone.Type = Type;
			//copied directly from MultiText.Clone as we unfortunately can't just call that method and have it do the right thing.
			//If this class were composed of a multitext rather than inheriting this wouldn't be a problem.
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Forms = Forms.Select(f => (LanguageForm)f.Clone()).ToArray();
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LexReversal);
		}

		public bool Equals(LexReversal other)
		{
			if (!base.Equals(other)) return false;
			if (Type != other.Type) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// For this class we want a hash code based on the the object's reference so that we
			// can store and retrieve the object in the LiftLexEntryRepository. However, this is
			// not ideal and Microsoft warns: "Do not use the hash code as the key to retrieve an
			// object from a keyed collection."
			// https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=netframework-4.8#remarks
			return base.GetHashCode();
		}
	}
}