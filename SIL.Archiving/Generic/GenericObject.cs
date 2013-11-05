using System;
using Palaso.Extensions;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Generic
{
	/// <summary>Base class for IMDI archiving objects</summary>
	public abstract class ArchivingGenericObject
	{
		/// <summary>If needed but not given, Name will be used</summary>
		public string Title;

		/// <summary>If needed but not given, Title will be used</summary>
		public string Name;

		/// <summary />
		public LanguageStringCollection Descriptions;

		/// <summary>Date the first entry was created</summary>
		public DateTime DateCreatedFirst;

		/// <summary>Date the last entry was created. Different archives use this differently</summary>
		public DateTime DateCreatedLast;

		/// <summary />
		public DateTime DateModified;

		/// <summary>Who has access, and how do you get access. Different archives use this differently</summary>
		public IAccessProtocol AccessProtocol;

		/// <summary />
		public ArchivingLocation Location;

		/// <summary>Constructor</summary>
		protected ArchivingGenericObject()
		{
			Descriptions = new LanguageStringCollection();
		}

		/// <summary />
		public string GetName()
		{
			return Name ?? Title;
		}

		/// <summary />
		public string GetTitle()
		{
			return Title ?? Name;
		}

		/// <summary>Get single value or year range</summary>
		public string GetDateCreated()
		{
			var emptyDate = default(DateTime);

			// no date given
			if (DateCreatedFirst == emptyDate)
				return null;

			// just one date given
			if (DateCreatedLast == emptyDate)
				return DateCreatedFirst.ToISO8601DateOnlyString();

			// both dates in same year
			if (DateCreatedFirst.Year == DateCreatedLast.Year)
				return string.Format("{0}", DateCreatedFirst.Year);

			// return the date range
			return string.Format("{0}-{1}", DateCreatedFirst.Year, DateCreatedLast.Year);
		}
	}

}
