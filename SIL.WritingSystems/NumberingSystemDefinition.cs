// Copyright (c) 2018 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;

namespace SIL.WritingSystems
{
	[DebuggerDisplay("{_id}:{_characters}")]
	public class NumberingSystemDefinition : DefinitionBase<NumberingSystemDefinition>,
		IEquatable<NumberingSystemDefinition>
	{
		private const string CUSTOM = "CUSTOM_ID";

		public const string CustomNumbersMarker = "other";
		private string _characters;
		private readonly string _id = CUSTOM;

		public NumberingSystemDefinition()
		{
		}

		public NumberingSystemDefinition(NumberingSystemDefinition orig)
		{
			_id = orig._id;
			_characters = orig._characters;
		}

		public NumberingSystemDefinition(string id)
		{
			var digits = CLDRNumberingSystems.GetDigitsForID(id);
			if (digits == null)
				throw new ArgumentException($"{id} is not defined in the CLDR", nameof(id));
			_id = id;
			_characters = CLDRNumberingSystems.GetDigitsForID(id);
		}

		public bool IsCustom => _id == CUSTOM;

		/// <summary>
		/// Returns the CLDR numbering system id or 'other(0123456789)' for a custom set
		/// </summary>
		public string Id => IsCustom ? $"{CustomNumbersMarker}({_characters})" : _id;

		public bool Equals(NumberingSystemDefinition other)
		{
			return ValueEquals(other);
		}

		/// <summary>
		/// Factory method to create a custom numbering system with specific digits
		/// </summary>
		public static NumberingSystemDefinition CreateCustomSystem(string digits)
		{
			if (digits.Length != 10)
			{
				throw new ArgumentException("numbering systems must contain exactly 10 digits");
			}
			return new NumberingSystemDefinition {_characters = digits};
		}

		public override bool ValueEquals(NumberingSystemDefinition other)
		{
			return Id.Equals(other.Id);
		}

		public override NumberingSystemDefinition Clone()
		{
			return new NumberingSystemDefinition(this);
		}
	}
}