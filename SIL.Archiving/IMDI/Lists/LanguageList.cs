
using System;
using System.Linq;
using L10NSharp;
using SIL.Archiving.IMDI.Schema;

namespace SIL.Archiving.IMDI.Lists
{
	/// -------------------------------------------------------------------------------------------
	public class LanguageItem : IMDIListItem
	{
		/// <summary>Constructs a new LanguageItem</summary>
		/// <param name="text">Example: "English"</param>
		/// <param name="value">Example: "ISO639-3:eng"</param>
		public LanguageItem(string text, string value) : base(text, value) {}

		/// <summary>This is provided because the XSD uses the term "LanguageId"</summary>
		public string Id { get { return Value;  } }

		/// <summary>Convert to a Language_Type object</summary>
		public LanguageType ToLanguageType()
		{
			return new LanguageType
			{
				Id = Id,
				Name = new[] { new LanguageNameType { Value = Text, Link = ListType.Link(ListType.MPILanguages) } },
			};
		}
	}

	/// -------------------------------------------------------------------------------------------
	public class LanguageList : IMDIItemList
	{
		private static LanguageList _instance;

		/// <summary>Get the list of languages</summary>
		/// <returns></returns>
		public static LanguageList GetList()
		{
			return _instance ?? (_instance = new LanguageList());
		}

		/// ---------------------------------------------------------------------------------------
		protected LanguageList() : base(ListConstructor.GetNodeList(ListType.MPILanguages)) {}

		/// <summary>Overriden so that the List contains LanguageItems rather than IMDIListItems</summary>
		/// <param name="item"></param>
		public override void AddItem(IMDIListItem item)
		{
			Add(new LanguageItem(item.Text, item.Value));
		}

		/// -------------------------------------------------------------------------------------------
		public static LanguageItem FindByISO3Code(string iso3Code)
		{
			var item = GetList().FirstOrDefault(i => i.Value.EndsWith(":" + iso3Code));

			// return language item if found
			if (item != null) return (LanguageItem) item;

			// if not found, throw exception
			var msg = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.InvalidLanguageCode", "Invalid ISO 639-3 code: {0}");
			throw new ArgumentException(string.Format(msg, iso3Code), "iso3Code");
		}

		/// -------------------------------------------------------------------------------------------
		public static LanguageItem FindByEnglishName(string englishName)
		{
			return (LanguageItem)(GetList().FindByText(englishName));
		}
	}
}
