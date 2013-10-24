
using System.Linq;

namespace SIL.Archiving.IMDI.Lists
{
	/// -------------------------------------------------------------------------------------------
	public class LanguageItem : IMDIListItem
	{
		/// <summary>
		/// Constructs a new LanguageItem
		/// </summary>
		/// <param name="text">Example: "English"</param>
		/// <param name="value">Example: "ISO639-3:eng"</param>
		public LanguageItem(string text, string value) : base(text, value) {}

		/// <summary>
		/// This is provided because the XSD uses the term "LanguageId"
		/// </summary>
		public string Id
		{ get { return Value;  } }
	}

	/// -------------------------------------------------------------------------------------------
	public class LanguageList : IMDIItemList
	{
		/// ---------------------------------------------------------------------------------------
		public LanguageList() : base(ListConstructor.GetNodeList(ListType.MPILanguages)) {}

		/// <summary>
		/// Overriden so that the List contains LanguageItems rather than IMDIListItems
		/// </summary>
		/// <param name="item"></param>
		public override void AddItem(IMDIListItem item)
		{
			Add(new LanguageItem(item.Text, item.Value));
		}

		/// -------------------------------------------------------------------------------------------
		public LanguageItem FindByISO3Code(string iso3Code)
		{
			return (LanguageItem)this.FirstOrDefault(i => i.Value.EndsWith(":" + iso3Code));
		}

		/// -------------------------------------------------------------------------------------------
		public LanguageItem FindByEnglishName(string englishName)
		{
			return (LanguageItem)FindByText(englishName);
		}
	}
}
