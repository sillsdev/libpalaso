using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Lift;
using SIL.Text;

namespace SIL.DictionaryServices.Model
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

		public override IPalasoDataObjectProperty Clone()
		{
			var clone = new LexPhonetic();
			clone.Traits.AddRange(Traits.Select(t => t.Clone()));
			clone.Fields.AddRange(Fields.Select(f=>(LexField) f.Clone()));
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Forms = Forms.Select(f => (LanguageForm)f.Clone()).ToArray();
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LexPhonetic);
		}

		public bool Equals(LexPhonetic other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!base.Equals(other)) return false;
			if (!Traits.SequenceEqual(other.Traits)) return false;
			if (!Fields.SequenceEqual(other.Fields)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 19;
				hash *= 53 + Traits.GetHashCode();
				hash *= 53 + Fields.GetHashCode();
				hash *= 53 + base.GetHashCode();
				return hash;
			}
		}
	}
}