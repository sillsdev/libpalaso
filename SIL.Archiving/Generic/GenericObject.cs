using System;
using SIL.Archiving.Generic.AccessProtocol;

namespace SIL.Archiving.Generic
{
	/// <summary>Base class for archiving objects</summary>
	public interface IArchivingGenericObject
	{
		/// <summary>If needed but not given, Name will be used</summary>
		string Title { get; set; }

		/// <summary>If needed but not given, Title will be used</summary>
		string Name { get; set; }

		/// <summary />
		void AddDescription(LanguageString description);

		/// <summary>Date the first entry was created</summary>
		DateTime DateCreatedFirst { get; set; }

		/// <summary>Date the last entry was created. Different archives use this differently</summary>
		DateTime DateCreatedLast { get; set; }

		/// <summary />
		DateTime DateModified { get; set; }

		/// <summary>Who has access, and how do you get access. Different archives use this differently</summary>
		IAccessProtocol AccessProtocol { get; set; }

		/// <summary />
		ArchivingLocation Location { get; set; }

	//    /// <summary />
	//    public string GetName()
	//    {
	//        return Name ?? Title;
	//    }

	//    /// <summary />
	//    public string GetTitle()
	//    {
	//        return Title ?? Name;
	//    }

	//    /// <summary>Get single value or year range</summary>
	//    public string GetDateCreated()
	//    {
	//        var emptyDate = default(DateTime);

	//        // no date given
	//        if (DateCreatedFirst == emptyDate)
	//            return null;

	//        // just one date given
	//        if (DateCreatedLast == emptyDate)
	//            return DateCreatedFirst.ToISO8601DateOnlyString();

	//        // both dates in same year
	//        if (DateCreatedFirst.Year == DateCreatedLast.Year)
	//            return string.Format("{0}", DateCreatedFirst.Year);

	//        // return the date range
	//        return string.Format("{0}-{1}", DateCreatedFirst.Year, DateCreatedLast.Year);
	//    }
	}
}
