﻿
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
		/// <param name="englishName">Example: "English"</param>
		/// <param name="imdiCode">Example: "ISO639-3:eng"</param>
		internal LanguageItem(string englishName, string imdiCode) : base(imdiCode, englishName)
		{
			Text = englishName;
		}

		internal LanguageItem(string englishName, string imdiCode, string otherName) : this(englishName, imdiCode)
		{
			OtherName = otherName;
		}

		/// <summary>This is provided because the XSD uses the term "LanguageId"</summary>
		public string Id { get { return Value;  } }

		/// <summary></summary>
		public string OtherName { get; set; }

		/// <summary></summary>
		public string DisplayName
		{
			get
			{
				return string.IsNullOrEmpty(OtherName) ? EnglishName : OtherName;
			}
		}

		/// <summary>Convert to a LanguageType object</summary>
		public LanguageType ToLanguageType()
		{
			var langName = Text;

			// check for "und" code
			if ((Id.EndsWith("und")) && (!string.IsNullOrEmpty(OtherName)))
				langName = OtherName;

			return new LanguageType
			{
				Id = Id,
				Name = new[] { new LanguageNameType { Value = langName, Link = ListType.Link(ListType.MPILanguages) } },
			};
		}

		/// <summary>Convert to a SimpleLanguageType object</summary>
		public SimpleLanguageType ToSimpleLanguageType()
		{
			return new SimpleLanguageType
			{
				Id = Id,
				Name = new LanguageNameType { Value = Text, Link = ListType.Link(ListType.MPILanguages) }
			};
		}

		/// <summary>Convert to a SimpleLanguageType object</summary>
		public SubjectLanguageType ToSubjectLanguageType()
		{
			return new SubjectLanguageType
			{
				Id = Id,
				Name = new LanguageNameType { Value = Text, Link = ListType.Link(ListType.MPILanguages) }
			};
		}

		/// <summary></summary>
		public string EnglishName
		{
			get { return Text; }
			set { Text = value; }
		}

		/// <summary></summary>
		public string Iso3Code
		{
			get { return Value.Substring(Value.Length - 3); }
		}
	}

	/// -------------------------------------------------------------------------------------------
	public class LanguageList : IMDIItemList
	{
		private static LanguageList _instance;

		/// <summary>Get the list of languages</summary>
		/// <returns></returns>
		private static LanguageList GetList()
		{
			return _instance ?? (_instance = new LanguageList());
		}

		/// ---------------------------------------------------------------------------------------
		protected LanguageList() : base(ListType.MPILanguages, false)
		{
		}

		/// ---------------------------------------------------------------------------------------
		protected override void Initialize()
		{
			// no-op
		}

		/// ---------------------------------------------------------------------------------------
		public override void Localize(Func<string, string, string, string, string> localize)
		{
			// no-op
		}

		/// ---------------------------------------------------------------------------------------
		/// <summary>Overriden so that the List contains LanguageItems rather than IMDIListItems</summary>
		public override void AddItem(string englishName, string code)
		{
			Add(new LanguageItem(englishName, code));
		}

		/// -------------------------------------------------------------------------------------------
		public static LanguageItem FindByISO3Code(string iso3Code)
		{
			// language is an open list, so allow items not in the official Arbil list
			return FindByISO3Code(iso3Code, false);
		}

		/// -------------------------------------------------------------------------------------------
		public static LanguageItem FindByISO3Code(string iso3Code, bool mustBeInList)
		{
			// check for und
			if (iso3Code == "und") return new LanguageItem("Undetermined", "ISO639-3:und");

			// look on the official list
			var item = GetList().FirstOrDefault(i => i.Value.EndsWith(":" + iso3Code));

			// return language item if found
			if (item != null) return (LanguageItem)item;

			// if not found, and not limited to list, just return the code passed in
			if (!mustBeInList) return new LanguageItem(string.Empty, "ISO639-3:" + iso3Code.ToLower());

			// if not found, throw exception
			var msg = LocalizationManager.GetString("DialogBoxes.ArchivingDlg.InvalidLanguageCode",
				"Invalid ISO 639-3 code: {0}");
			throw new ArgumentException(string.Format(msg, iso3Code), "iso3Code");
		}

		/// -------------------------------------------------------------------------------------------
		public static LanguageItem FindByEnglishName(string englishName)
		{
			if (string.IsNullOrEmpty(englishName))
				return null;

			var item = GetList().FindByText(englishName);

			// if not on list, return "und"
			if (item == null)
				return new LanguageItem(englishName, "ISO639-3:und", englishName);

			return (LanguageItem)(GetList().FindByText(englishName));
		}

		/// -------------------------------------------------------------------------------------------
		public static LanguageItem Find(ArchivingLanguage archLanguage)
		{
			LanguageItem item = null;

			if (!string.IsNullOrEmpty(archLanguage.Iso3Code))
				item = FindByISO3Code(archLanguage.Iso3Code);

			if (item == null)
			{
				if (!string.IsNullOrEmpty(archLanguage.EnglishName))
					item = FindByEnglishName(archLanguage.EnglishName);
			}

			if (item != null)
			{
				item.OtherName = archLanguage.LanguageName;
				if (string.IsNullOrEmpty(item.EnglishName))
					item.EnglishName = archLanguage.EnglishName;
			}
			
			return item;
		}
	}
}
