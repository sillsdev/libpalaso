using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Palaso.Lift;
using Palaso.Reporting;
using Palaso.Text;

namespace Palaso.DictionaryServices.Model
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

		public override bool Equals(Object obj)
		{
			if (obj.GetType() != typeof(LexVariant)) return false;
			return Equals((LexVariant)obj);
		}

		public bool Equals(LexVariant other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (!base.Equals(other)) return false;
			if (!Traits.OrderBy(x => x).SequenceEqual(other.Traits.OrderBy(x => x))) return false;
			if (!Fields.OrderBy(x => x).SequenceEqual(other.Fields.OrderBy(x => x))) return false;
			return true;
		}
	}
}