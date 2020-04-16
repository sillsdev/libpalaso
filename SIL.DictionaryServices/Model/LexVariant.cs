using System;
using System.Collections.Generic;
using System.Linq;
using SIL.Lift;
using SIL.Text;

namespace SIL.DictionaryServices.Model
{
	/// <summary>
	/// not implemented: ref
	/// not implemented: pronunciation
	/// not implemented: relation
	///
	/// </summary>
	public sealed class LexVariant:  MultiText, IExtensible
	{
		public LexVariant()
		{
			Traits = new List<LexTrait>();
			Fields = new List<LexField>();
		}

		#region IExtensible
		public List<LexTrait> Traits { get; private set; }
		public List<LexField> Fields { get; private set; }
		#endregion

		public override IPalasoDataObjectProperty Clone()
		{
			var clone = new LexVariant();
			clone.Traits.AddRange(Traits.Select(t => t.Clone()));
			clone.Fields.AddRange(Fields.Select(f => (LexField)f.Clone()));
			//copied from MultiText
			clone.Forms = Forms.Select(f => (LanguageForm)f.Clone()).ToArray();
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LexVariant);
		}

		public bool Equals(LexVariant other)
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
				var hash = 17;
				hash *= 37 + Traits.GetHashCode();
				hash *= 37 + Fields.GetHashCode();
				hash *= 37 + base.GetHashCode();
				return hash;
			}
		}
	}
}