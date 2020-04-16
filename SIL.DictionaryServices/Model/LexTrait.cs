// Copyright (c) 2010-2020 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using SIL.ObjectModel;

namespace SIL.DictionaryServices.Model
{
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

		public override bool Equals(object obj)
		{
			return Equals(obj as LexTrait);
		}

		public bool Equals(LexTrait other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Name != other.Name) return false;
			if (Value != other.Value) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 47;
				hash *= 67 + Name.GetHashCode();
				hash *= 67 + Value.GetHashCode();
				return hash;
			}
		}
	}
}