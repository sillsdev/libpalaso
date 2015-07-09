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

		public override bool Equals(Object obj)
		{
			if (!(obj is LexReversal)) return false;
			return Equals((LexReversal) obj);
		}

		public bool Equals(LexReversal other)
		{
			if (!base.Equals(other)) return false;
			if (Type != other.Type) return false;
			return true;
		}
	}
}