// Copyright (c) 2010-2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
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
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 47;
				hash *= 61 + Type.GetHashCode();
				hash *= 61 + Traits.GetHashCode();
				hash *= 61 + base.GetHashCode();
				return hash;
			}
		}
	}
}