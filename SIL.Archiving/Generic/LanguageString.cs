using System;
using System.Collections.Generic;

namespace SIL.Archiving.Generic
{
	/// <summary>Class for string values that have a language attribute</summary>
	public class LanguageString : IComparable
	{
		/// <summary>The text/string value of this object</summary>
		public string Value;

		/// <summary>The ISO3 code for the language this object is in</summary>
		public string Iso3LanguageId;

		/// <summary></summary>
		public LanguageString()
		{
		}

		/// <summary></summary>
		public LanguageString(string value, string languageId)
		{
			Value = value;
			Iso3LanguageId = languageId;
		}

		public override string ToString()
		{
			return Value;
		}

		/// <summary>Compare 2 LanguageString objects. They are identical if they have the same Iso3LanguageId</summary>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			LanguageString other = obj as LanguageString;

			if (other == null)
				throw new ArgumentException();

			return String.Compare(Iso3LanguageId, other.Iso3LanguageId, StringComparison.Ordinal);
		}

		/// <summary>Compare 2 LanguageString objects. They are identical if they have the same Iso3LanguageId</summary>
		public static int Compare(LanguageString langStrA, LanguageString langStrB)
		{
			return langStrA.CompareTo(langStrB);
		}
	}

	/// <summary>Compare 2 LanguageString objects. They are identical if they have the same Iso3LanguageId</summary>
	public class LanguageStringComparer : IEqualityComparer<LanguageString>
	{
		public bool Equals(LanguageString x, LanguageString y)
		{
			return (x.CompareTo(y) == 0);
		}

		public int GetHashCode(LanguageString obj)
		{
			return obj.Iso3LanguageId.GetHashCode();
		}
	}

	/// <summary>Simplify creating and managing LanguageString collections</summary>
	public class LanguageStringCollection : HashSet<LanguageString>
	{
		/// <summary>Default constructor</summary>
		public LanguageStringCollection()
			: base(new LanguageStringComparer())
		{
			// additional constructor code can go here
		}
	}

}
