using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SIL.Archiving.Generic;

namespace SIL.Archiving.IMDI.Schema
{
	/// <summary>Common description code for multiple IMDI objects</summary>
	public abstract class IMDIDescription
	{
		private DescriptionTypeCollection _descriptionField;

		/// <remarks/>
		[XmlElement("Description")]
		public DescriptionTypeCollection Description
		{
			get { return _descriptionField ?? (_descriptionField = new DescriptionTypeCollection()); }
			set { _descriptionField = value; }
		}

		/// <summary>Adds a description (in a particular language)</summary>
		public void AddDescription(LanguageString description)
		{
			Description.Add(description.ToIMDIDescriptionType());
		}
	}

	/// <summary>Class for string values that have a language attribute</summary>
	public partial class DescriptionType : IComparable
	{
		/// <summary>Compare 2 LanguageString objects. They are identical if they have the same Iso3LanguageId</summary>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			DescriptionType other = obj as DescriptionType;

			if (other == null)
				throw new ArgumentException();

			return String.Compare(LanguageId, other.LanguageId, StringComparison.Ordinal);
		}

		/// <summary>Compare 2 LanguageString objects. They are identical if they have the same Iso3LanguageId</summary>
		public static int Compare(DescriptionType langStrA, DescriptionType langStrB)
		{
			return langStrA.CompareTo(langStrB);
		}
	}

	/// <summary>Compare 2 LanguageString objects. They are identical if they have the same Iso3LanguageId</summary>
	public class DescriptionTypeComparer : IEqualityComparer<DescriptionType>
	{
		public bool Equals(DescriptionType x, DescriptionType y)
		{
			return (x.CompareTo(y) == 0);
		}

		public int GetHashCode(DescriptionType obj)
		{
			return ((obj.Name ?? string.Empty) + (obj.LanguageId ?? string.Empty)).GetHashCode();
		}
	}

	/// <summary>Simplify creating and managing LanguageString collections</summary>
	public class DescriptionTypeCollection : HashSet<DescriptionType>
	{
		/// <summary>Default constructor</summary>
		public DescriptionTypeCollection()
			: base(new DescriptionTypeComparer())
		{
			// additional constructor code can go here
		}

		/// <summary>Adds a description (in a particular language)</summary>
		public void Add(LanguageString description)
		{
			Add(description.ToIMDIDescriptionType());
		}
	}

}
