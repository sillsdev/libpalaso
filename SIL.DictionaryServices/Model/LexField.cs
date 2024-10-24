// Copyright (c) 2010-2024 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using System.Collections.Generic;
using System.Linq;
using SIL.Lift;
using SIL.Text;

namespace SIL.DictionaryServices.Model
{
	public class LexField : MultiText
	{
		public string         Type;
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
			return Equals(other as LexField);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LexField);
		}

		public bool Equals(LexField other)
		{
			if (!base.Equals(other)) return false;
			if (!Type.Equals(other.Type)) return false;
			if (!Traits.SequenceEqual(other.Traits)) return false;
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