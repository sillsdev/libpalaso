
using System.Linq;
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
		public Language_Type ToLanguageType()
		{
			return new Language_Type
			{
				Id = new LanguageId_Type { Value = Id },
				Name = new[] { new LanguageName_Type { Value = Text, Link = ListType.Link(ListType.MPILanguages) } },
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
			if (_instance == null)
				_instance = new LanguageList();

			return _instance;
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
			return (LanguageItem)(GetList().FirstOrDefault(i => i.Value.EndsWith(":" + iso3Code)));
		}

		/// -------------------------------------------------------------------------------------------
		public static LanguageItem FindByEnglishName(string englishName)
		{
			return (LanguageItem)(GetList().FindByText(englishName));
		}
	}
}
