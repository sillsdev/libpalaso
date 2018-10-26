using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml;
using SIL.Archiving.Generic;
using SIL.Archiving.IMDI.Schema;
using SIL.Extensions;
using SIL.WritingSystems;

namespace SIL.Archiving.IMDI.Lists
{
	/// <summary>Generic class to handle items in the IMDI lists</summary>
	public class IMDIListItem : IComparable
	{
		/// <summary>Suitable for display to the user (potentially localized)</summary>
		public string Text { get; protected set; }

		/// <summary>Value suitable for storing in metadata files</summary>
		public string Value { get; private set; }

		/// <summary>Description to the user (often the same as the text)</summary>
		public string Definition { get; private set; }

		/// <summary>Constructor</summary>
		/// <param name="value">Value suitable for storing in metadata files</param>
		/// <param name="definition">Description to the user (often the same as the Text)</param>
		public IMDIListItem(string value, string definition)
		{
			Text = value;
			Value = value;
			// most of the time InnerText is empty - use the "Value" attribute for both if it is empty
			Definition = string.IsNullOrEmpty(definition) ? value : definition;
		}

		public override string ToString() { return Text; }

		/// <summary>Convert to VocabularyType</summary>
		public VocabularyType ToVocabularyType(VocabularyTypeValueType vocabularyType, string link)
		{
			return new VocabularyType { Type = vocabularyType, Value = Value, Link = link };
		}

		/// <summary>Convert to BooleanType</summary>
		public BooleanType ToBooleanType()
		{
			BooleanEnum boolEnum;
			return Enum.TryParse(Value, true, out boolEnum)
				? new BooleanType { Type = VocabularyTypeValueType.ClosedVocabulary, Value = boolEnum, Link = ListType.Link(ListType.Boolean) }
				: new BooleanType { Type = VocabularyTypeValueType.ClosedVocabulary, Value = BooleanEnum.Unknown, Link = ListType.Link(ListType.Boolean) };
		}

		/// <summary>
		/// Localize the Text and Defnition using the given localize function
		/// </summary>
		/// <param name="listName">The name of the list containing this item</param>
		/// <param name="localize">Delegate to use for getting localized versions of the Text and
		/// Definition of list items. The parameters are: 1) the list name; 2) list item value;
		/// 3) "Definition" or "Text"; 4) default (English) value.</param>
		internal void Localize(string listName, Func<string, string, string, string, string> localize)
		{
			if (localize != null && !string.IsNullOrEmpty(Value))
			{
				if (Text == Definition)
				{
					Text = Definition = localize(listName, Value, "Text", Text);
				}
				else
				{
					Text = localize(listName, Value, "Text", Text);
					Definition = localize(listName, Value, "Definition", Definition);
				}
			}
		}

		/// <summary>Used for sorting. Compare 2 IMDIListItem objects. They are identical if the Text properties are the same</summary>
		public int CompareTo(object obj)
		{
			if (obj == null)
				return 1;

			IMDIListItem other = obj as IMDIListItem;

			if (other == null)
				throw new ArgumentException();

			// handle special cases
			var thisValue = CheckSpecialCase(this);
			var thatValue = CheckSpecialCase(other);

			// compare the Text properties
			return String.Compare(thisValue, thatValue, StringComparison.InvariantCultureIgnoreCase);
		}

		/// <summary>Used for sorting. Compare 2 IMDIListItem objects. They are identical if the Text properties are the same</summary>
		public static int Compare(IMDIListItem itemA, IMDIListItem itemB)
		{
			return itemA.CompareTo(itemB);
		}

		/// <summary>This allows forcing specific entries to the top of the sort order</summary>
		private static string CheckSpecialCase(IMDIListItem inputValue)
		{
			switch (inputValue.Value)
			{
				case null:
				case "":
					return "A";

				case "Unknown":
					return "B";

				case "Undefined":
					return "C";

				case "Unspecified":
					return "D";
			}

			return "Z" + inputValue.Text;
		}
	}

	/// <summary>
	/// A list of IMDI list items, constructed using the imdi:Entry Nodes from the XML file.
	/// <example>
	/// Example 1:
	/// ----------
	/// var lc = IMDI_Schema.ListConstructor.GetList(ListType.MPILanguages);
	/// comboBox1.Items.AddRange(lc.ToArray());
	/// </example>
	/// <example>
	/// Example 2:
	/// ----------
	/// var lc = IMDI_Schema.ListConstructor.GetList(ListType.MPILanguages);
	/// comboBox1.DataSource = lc;
	/// comboBox1.DisplayMember = "Text";
	/// comboBox1.ValueMember = "Value";
	/// </example>
	/// </summary>
	public class IMDIItemList : List<IMDIListItem>
	{
		private static string _listPath;
		private readonly string _listname;
		private bool _closed;

		/// ---------------------------------------------------------------------------------------
		public bool Closed
		{
			get { return _closed; }
			internal set
			{
				if (_closed && !value)
					throw new InvalidOperationException("Existing closed list cannot be made open!");
				_closed = value;
			}
		}

		/// ---------------------------------------------------------------------------------------
		/// <summary>Constructs a list of IMDIListItems that can be used as the data source of a
		/// combo or list box</summary>
		/// <param name="listName">Name of the XML file that contains the desired list. It is suggested to
		///     use values from IMDI_Schema.ListTypes class. If not found on the local system, we will attempt
		///     to download from http://www.mpi.nl/IMDI/Schema.
		/// </param>
		/// <param name="uppercaseFirstCharacter">Make first character of each item uppercase</param>
		internal IMDIItemList(string listName, bool uppercaseFirstCharacter) 
			: this(listName, uppercaseFirstCharacter, ListConstructor.RemoveUnknown.RemoveNone)
		{
		}

		/// ---------------------------------------------------------------------------------------
		/// <summary>Constructs a list of IMDIListItems that can be used as the data source of a
		/// combo or list box</summary>
		/// <param name="listName">Name of the XML file that contains the desired list. It is suggested to
		///     use values from IMDI_Schema.ListTypes class. If not found on the local system, we will attempt
		///     to download from http://www.mpi.nl/IMDI/Schema.
		/// </param>
		/// <param name="uppercaseFirstCharacter">Make first character of each item uppercase</param>
		/// <param name="removeUnknown">Specify which values of "unknown" to remove</param>
		internal IMDIItemList(string listName, bool uppercaseFirstCharacter, ListConstructor.RemoveUnknown removeUnknown)
		{
			Debug.Assert(listName.EndsWith(".xml"));
			_listname = listName.Substring(0, listName.Length - 4);

			if (listName == "MPI-Languages.xml")
			{
				// This file is woefully incomplete (only ~345 languages, less than 5% of the total).  A comment inside it even says
				// "When a language name and identifier that you need is not in this list, please look it up under www.ethnologue.com/web.asp.".
				// So we use the information from SIL.WritingSystems, which is based on the complete Ethnologue data.
				AddLanguagesFromEthnologue();
			}
			else
			{
				PopulateList(GetNodeList(listName), uppercaseFirstCharacter, removeUnknown);
			}
			InitializeThis();
		}

		private void AddLanguagesFromEthnologue()
		{
			// We won't worry about how slow list lookup is with thousands of languages because we'll only
			// be looking up a few languages in all likelihood.  Storing the names in a list that is searched
			// linearly does ensure matching on the major name of a language which happens to match an alias
			// for another language.  It also ensures returning the major (English) name of the language when
			// looked up by ISO code.
			var langLookup = new LanguageLookup();
			var languages = langLookup.SuggestLanguages("*").ToList();
			foreach (var lang in languages)
			{
				if (lang.ThreeLetterTag == null || lang.ThreeLetterTag.Length != 3)
					continue;	// Data includes codes like "pt-PT" and "zh-Hans": ignore those for now.
				AddItem(GetEnglishName(lang), "ISO639-3:" + lang.ThreeLetterTag);
			}
			foreach (var lang in languages)
			{
				if (lang.ThreeLetterTag == null || lang.ThreeLetterTag.Length != 3)
					continue;	// Data includes codes like "pt-PT" and "zh-Hans": ignore those for now.
				foreach (var name in lang.Names)
				{
					if (name != GetEnglishName(lang))
						AddItem(name, "ISO639-3:" + lang.ThreeLetterTag);
				}
			}
		}

		private static string GetEnglishName(LanguageInfo lang)
		{
			// For these languages, the data gives the language name in its own language and script first,
			// which is great for UI choosers. For IMDI, we prefer the English name as the default.
			switch (lang.ThreeLetterTag)
			{
				case "ara": return "Arabic";
				case "ben": return "Bengali";
				case "fra": return "French";
				case "spa": return "Spanish";
				case "tam": return "Tamil";
				case "tel": return "Telegu";
				case "tha": return "Thai";
				case "urd": return "Urdu";
				case "zho": return "Chinese";
				// DesiredName is either the name the user prefers, or the first (and possibly only) name in the Names list.
				default: return lang.DesiredName;
			}
		}

		private void InitializeThis()
		{
			Initialize();
		}

		/// ---------------------------------------------------------------------------------------
		protected virtual void Initialize()
		{
			// Add blank option
			Insert(0, new IMDIListItem(string.Empty, string.Empty));
		}

		/// <summary>Gets a list of the Entry nodes from the selected XML file.</summary>
		/// <param name="listName">Name of the XML file that contains the desired list. It is suggested to
		/// use values from IMDI_Schema.ListTypes class. If not found on the local system, we will attempt
		/// to download from http://www.mpi.nl/IMDI/Schema.
		/// </param>
		/// <returns></returns>
		private XmlNodeList GetNodeList(string listName)
		{
			var listFileName = CheckFile(listName);

			// if the file was not found, throw an exception
			if (string.IsNullOrEmpty(listFileName))
				throw new FileNotFoundException(string.Format("The list {0} was not found.", listName));

			XmlDocument doc = new XmlDocument();
			doc.Load(listFileName);

			var nsmgr = new XmlNamespaceManager(doc.NameTable);
			nsmgr.AddNamespace("imdi", "http://www.mpi.nl/IMDI/Schema/IMDI");

			// if not a valid XML file, throw an exception
			if (doc.DocumentElement == null)
				throw new XmlException(string.Format("The file {0} was not a valid XML file.", listFileName));

			var nodes = doc.DocumentElement.SelectNodes("//imdi:VocabularyDef/imdi:Entry", nsmgr);

			// if no entries were found, throw an exception
			if (nodes == null)
				throw new XmlException(string.Format("The file {0} does not contain any list entries.", listFileName));

			return nodes;
		}

		/// ---------------------------------------------------------------------------------------
		private static string CheckFile(string listName)
		{
			var listFileName = Path.Combine(ListPath, listName);

			// if file already exists locally, return it now
			if (File.Exists(listFileName))
				return listFileName;

			// attempt to download if not already in list folder
			var url = ListType.Link(listName);
			Debug.WriteLine("Downloading from {0} to {1}", url, listFileName);
			var wc = new WebClient();
			wc.DownloadFile(url, listFileName);

			// return full name, or null if not able to download
			return File.Exists(listFileName) ? listFileName : null;
		}

		/// ---------------------------------------------------------------------------------------
		private static string ListPath
		{
			get
			{
				if (!string.IsNullOrEmpty(_listPath)) return _listPath;

				var thisPath = ArchivingFileSystem.SilCommonIMDIDataFolder;

				// check if path exists
				if (!Directory.Exists(thisPath))
					throw new DirectoryNotFoundException("Not able to find the IMDI lists directory.");

				_listPath = thisPath;

				return _listPath;
			}
		}

		/// ---------------------------------------------------------------------------------------
		public virtual void Localize(Func<string, string, string, string, string> localize)
		{
			if (localize != null)
			{
				foreach (var itm in this)
					itm.Localize(_listname, localize);
			}
		}

		/// ---------------------------------------------------------------------------------------
		protected void PopulateList(XmlNodeList nodes, bool uppercaseFirstCharacter, ListConstructor.RemoveUnknown removeUnknown)
		{
			foreach (XmlNode node in nodes)
			{
				if (node.Attributes == null) continue;

				var value = node.Attributes["Value"].Value; // the "Value" attribute contains the meta-data value to save

				switch (value.ToLower())
				{
					case "unknown":
						if (removeUnknown == ListConstructor.RemoveUnknown.RemoveAll) continue;
						break;

					case "unspecified":
					case "undetermined":
					case "undefined":
						if (removeUnknown != ListConstructor.RemoveUnknown.RemoveNone) continue;
						break;
				}

				var definition = node.InnerText.Replace("Definition:", "").Replace("\t", " ").Replace("\r", "").Replace("\n", " ").Trim();  // if InnerText is set, it may contain the value for the meta-data file (some files do, some don't)

				if (uppercaseFirstCharacter && !string.IsNullOrEmpty(value) &&
					(value.Substring(0, 1) != value.Substring(0, 1).ToUpper()))
				{
					value = value.ToUpperFirstLetter();
				}

				AddItem(value, definition);
			}
		}

		/// ---------------------------------------------------------------------------------------
		public virtual void AddItem(string value, string definition)
		{
			Add(new IMDIListItem(value, definition));
		}

		/// ---------------------------------------------------------------------------------------
		/// <summary>Returns the first item found with the given Value. If not found in a closed list, the
		/// "Unspecified" item will be returned; otherwise returns null.</summary>
		/// <param name="value">The meta-data value of the item to find</param>
		public IMDIListItem FindByValue(string value)
		{
			try
			{
				return this.First(i => String.Equals(i.Value, value, StringComparison.CurrentCultureIgnoreCase));
			}
			catch
			{
				return Closed ? this.FirstOrDefault(i => String.Equals(i.Value, "Unspecified", StringComparison.CurrentCultureIgnoreCase)) : null;
			}
		}

		/// <summary>Returns the first item found with the given Text, or null if not found</summary>
		/// <param name="text">The (potentially localized) UI text of the item to find</param>
		public IMDIListItem FindByText(string text)
		{
			var itm = this.FirstOrDefault(i => String.Equals(i.Text, text, StringComparison.CurrentCultureIgnoreCase));
			return itm;
		}
	}
}
