
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

	/// <summary>Location information for the data in the package</summary>
	public class ArchivingLocation
	{
		// ReSharper disable CSharpWarnings::CS1591
		public string Continent;
		public string Country;
		public string Region;
		public string Address;
		// ReSharper restore CSharpWarnings::CS1591
	}

	/// <summary>Information about a contact</summary>
	public class ArchivingContact
	{
		/// <summary />
		public string Name;
		/// <summary />
		public string Address;
		/// <summary />
		public string Email;
		/// <summary />
		public string OrganizationName;
	}

	/// <summary>Information on the funding for this project</summary>
	public class ArchivingProject
	{
		/// <summary>Name of the funding project</summary>
		public string Name;

		/// <summary>Title of the funding project</summary>
		public string Title;

		/// <summary>Author of the funding project</summary>
		public string Author;

		/// <summary />
		public ArchivingContact ContactPerson;
	}
}
