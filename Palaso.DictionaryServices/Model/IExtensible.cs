using System;
using System.Collections.Generic;
using System.Linq;
using Palaso.Lift;
using SIL.ObjectModel;
using SIL.Text;

namespace Palaso.DictionaryServices.Model
{
	public interface IExtensible //: PalasoDataObject
	{
		List<LexTrait> Traits { get; }
		List<LexField> Fields { get; }
	}

	public class LexTrait : ICloneable<LexTrait>, IEquatable<LexTrait>
	{
		public string Name;
		public string Value;

		public LexTrait(string name, string value)
		{
			Name = name;
			Value = value;
		}

		public LexTrait Clone()
		{
			return new LexTrait(Name, Value);
		}

		public override bool Equals(Object obj)
		{
			if (!(obj is LexTrait)) return false;
			return Equals((LexTrait)obj);
		}

		public bool Equals(LexTrait other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Name != other.Name) return false;
			if (Value != other.Value) return false;
			return true;
		}
	}

	public class LexField : MultiText
	{
		public string Type;
		public List<LexTrait> Traits = new List<LexTrait>();

		public LexField(string type)
		{
			Type = type;
		}

		public override IPalasoDataObjectProperty Clone()
		{
			var clone = new LexField(Type);
			clone.Traits = new List<LexTrait>(Traits.Select(t=>t.Clone()));
			//copied directly from MultiText.Clone as we unfortunately can't just call that method and have it do the right thing.
			//If this class were composed of a multitext rather than inheriting this wouldn't be a problem.
			clone.EmbeddedXmlElements = new List<string>(EmbeddedXmlElements);
			clone.Forms = Forms.Select(f => (LanguageForm) f.Clone()).ToArray();
			return clone;
		}

		public override bool Equals(IPalasoDataObjectProperty other)
		{
			return Equals((LexField) other);
		}

		public override bool Equals(Object obj)
		{
			if (!(obj is LexField)) return false;
			return Equals((LexField)obj);
		}

		public bool Equals(LexField other)
		{
			if (!base.Equals(other)) return false;
			if (!Type.Equals(other.Type)) return false;
			if (!Traits.SequenceEqual(other.Traits)) return false;
			return true;
		}
	}
}