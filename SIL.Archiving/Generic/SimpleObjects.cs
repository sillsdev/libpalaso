
namespace SIL.Archiving.Generic
{
	/// <summary>Class for string values that have a language attribute</summary>
	public class LanguageString
	{
		/// <summary>The text/string value of this object</summary>
		public string Value;

		/// <summary>The ISO3 code for the language this object is in</summary>
		public string Iso3LanguageId;

		public override string ToString()
		{
			return Value;
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
