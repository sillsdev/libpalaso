// Copyright (c) 2010-2025 SIL Global
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
			return Value == other.Value;
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