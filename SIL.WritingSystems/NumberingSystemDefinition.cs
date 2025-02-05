// Copyright (c) 2025 SIL Global
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)

using System;
using System.Diagnostics;
using System.Globalization;

namespace SIL.WritingSystems
{
	/// <summary>
	/// This class defines the numbering system portion of a writing system definition using the API for
	/// tracking changes and notifying listeners.
	/// </summary>
	[DebuggerDisplay("{_id}:{_digits}")]
	public class NumberingSystemDefinition : DefinitionBase<NumberingSystemDefinition>,
		IEquatable<NumberingSystemDefinition>
	{
		private const string CUSTOM = "CUSTOM_ID";

		public const string CustomNumbersMarker = "other";
		private string _digits;
		private readonly string _id = CUSTOM;

		public NumberingSystemDefinition()
		{
		}

		public NumberingSystemDefinition(NumberingSystemDefinition orig)
		{
			_id = orig._id;
			_digits = orig._digits;
		}

		public NumberingSystemDefinition(string id)
		{
			var digits = CLDRNumberingSystems.GetDigitsForID(id);
			if (digits == null)
				throw new ArgumentException($"{id} is not defined in the CLDR", nameof(id));
			_id = id;
			_digits = CLDRNumberingSystems.GetDigitsForID(id);
		}

		/// <summary/>
		public bool IsCustom => _id == CUSTOM;

		/// <summary>
		/// returns a string containing the 10 digits used by this numbering system 0-9
		/// </summary>
		public string Digits
		{
			get
			{
				return _digits;
			}
		}

		/// <summary>
		/// Returns the CLDR numbering system id or 'other(0123456789)' for a custom set
		/// </summary>
		public string Id => IsCustom ? $"{CustomNumbersMarker}({_digits})" : _id;

		public bool Equals(NumberingSystemDefinition other)
		{
			return ValueEquals(other);
		}

		public override bool Equals(object other)
		{
			return other is NumberingSystemDefinition definition && Equals(definition);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		/// <summary>
		/// Factory method to create a custom numbering system with specific digits
		/// </summary>
		/// <remarks>If the digits given are invalid it will return the Default instead.</remarks>
		public static NumberingSystemDefinition CreateCustomSystem(string digits)
		{
			if (new StringInfo(digits).LengthInTextElements != 10)
			{
				Debug.WriteLine("Numbering systems must contain exactly 10 digits. Tried to create one with '{0}'");
				return Default;
			}
			return new NumberingSystemDefinition {_digits = digits};
		}

		public override bool ValueEquals(NumberingSystemDefinition other)
		{
			return Id.Equals(other.Id);
		}

		public override NumberingSystemDefinition Clone()
		{
			return new NumberingSystemDefinition(this);
		}

		static NumberingSystemDefinition()
		{
			Default = new NumberingSystemDefinition("latn");
		}

		public static NumberingSystemDefinition Default { get; }
	}
}