using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Palaso.Extensions;
using SIL.WritingSystems.Collation;

namespace SIL.WritingSystems
{
	/// <summary>
	/// Collation rules types
	/// </summary>
	public enum CollationRulesTypes
	{
		/// <summary>
		/// Default Unicode ordering rules (actually CustomIcu without any rules)
		/// </summary>
		[Description("Default Ordering")]
		DefaultOrdering,
		/// <summary>
		/// Custom Simple (Shoebox/Toolbox) style rules
		/// </summary>
		[Description("Custom Simple (Shoebox style) rules")]
		CustomSimple,
		/// <summary>
		/// Custom ICU rules
		/// </summary>
		[Description("Custom ICU rules")]
		CustomIcu,
		/// <summary>
		/// Use the sort rules from another language. When this is set, the SortRules are interpreted as a cultureId for the language to sort like.
		/// </summary>
		[Description("Same as another language")]
		OtherLanguage
	}

	/// <summary>
	/// IPA status choices
	/// </summary>
	public enum IpaStatusChoices
	{
		/// <summary>
		/// Not IPA
		/// </summary>
		NotIpa,
		/// <summary>
		/// Generic IPA
		/// </summary>
		Ipa,
		/// <summary>
		/// Phonetic IPA
		/// </summary>
		IpaPhonetic,
		/// <summary>
		/// Phonemic IPA
		/// </summary>
		IpaPhonemic
	}

	/// <summary>
	/// This class stores the information used to define various writing system properties. The Language, Script, Region and Variant
	/// properties conform to the subtags of the same name defined in BCP47 (Rfc5646) and are enforced by the Rfc5646Tag class. it is worth
	/// noting that for historical reasons this class does not provide seperate fields for variant and private use components as
	/// defined in BCP47. Instead the ConcatenateVariantAndPrivateUse and SplitVariantAndPrivateUse methods are provided for consumers
	/// to generate a single variant subtag that contains both fields seperated by "-x-".
	/// Furthermore the WritingSystemDefinition.WellknownSubtags class provides certain well defined Subtags that carry special meaning
	/// apart from the IANA subtag registry. In particular this class defines "qaa" as the default "unlisted language" language subtag.
	/// It should be used when there is no match for a language in the IANA subtag registry. Private use properties are "emic" and "etic"
	/// which mark phonemic and phonetic writing systems respectively. These must always be used in conjunction with the "fonipa" variant.
	/// Likewise "audio" marks a writing system as audio and must always be used in conjunction with script "Zxxx". Convenience methods
	/// are provided for Ipa and Audio properties as IpaStatus and IsVoice respectively.
	/// </summary>
	public class WritingSystemDefinition : DefinitionBase<WritingSystemDefinition>
	{
		/// <summary>
		/// This is the version of our writingsystemDefinition implementation and is mostly used for migration purposes.
		/// This should not be confused with the version of the locale data contained in this writing system.
		/// That information is stored in the "VersionNumber" property.
		/// </summary>
		public const int LatestWritingSystemDefinitionVersion = 2;

		private Rfc5646Tag _rfcTag;
		private string _languageName;
		private string _abbreviation;
		private bool _isUnicodeEncoded;
		private string _versionNumber;
		private string _versionDescription;
		private DateTime _dateModified;
		private FontDefinition _defaultFont;
		private string _keyboard;
		private CollationRulesTypes _collationRulesType;
		private string _collationRules;
		private string _nativeName;
		private bool _rightToLeftScript;
		private ICollator _collator;
		private IKeyboardDefinition _localKeyboard;
		private string _id;
		private SpellCheckDictionaryDefinition _spellCheckDictionary;
		private readonly FontDefinitionCollection _fonts = new FontDefinitionCollection();
		private readonly KeyboardDefinitionCollection _knownKeyboards = new KeyboardDefinitionCollection();
		private readonly SpellCheckDictionaryDefinitionCollection _spellCheckDictionaries = new SpellCheckDictionaryDefinitionCollection();

		/// <summary>
		/// Creates a new WritingSystemDefinition with Language subtag set to "qaa"
		/// </summary>
		public WritingSystemDefinition()
		{
			_collationRulesType = CollationRulesTypes.DefaultOrdering;
			_isUnicodeEncoded = true;
			_rfcTag = new Rfc5646Tag();
			UpdateIdFromRfcTag();
			_fonts.CollectionChanged += _fonts_CollectionChanged;
			_knownKeyboards.CollectionChanged += _knownKeyboards_CollectionChanged;
			_spellCheckDictionaries.CollectionChanged += _spellCheckDictionaries_CollectionChanged;
		}

		private void _spellCheckDictionaries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_spellCheckDictionary != null && !_spellCheckDictionaries.Contains(_spellCheckDictionary))
				_spellCheckDictionary = null;
			IsChanged = true;
		}

		private void _fonts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_defaultFont != null && !_fonts.Contains(_defaultFont))
				_defaultFont = null;
			IsChanged = true;
		}

		private void _knownKeyboards_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_localKeyboard != null && !_knownKeyboards.Contains(_localKeyboard))
				_localKeyboard = null;
			IsChanged = true;
		}

		/// <summary>
		/// Creates a new WritingSystemDefinition by parsing a valid BCP47 tag
		/// </summary>
		/// <param name="bcp47Tag">A valid BCP47 tag</param>
		public WritingSystemDefinition(string bcp47Tag)
			: this()
		{
			_rfcTag = Rfc5646Tag.Parse(bcp47Tag);
			_abbreviation = _languageName = _nativeName = string.Empty;
			UpdateIdFromRfcTag();
		}

		/// <summary>
		/// True when the validity of the writing system defn's tag is being enforced. This is the normal and default state.
		/// Setting this true will throw unless the tag has previously been put into a valid state.
		/// Attempting to Save the writing system defn will set this true (and may throw).
		/// </summary>
		public bool RequiresValidTag
		{
			get { return _rfcTag.RequiresValidTag; }
			set
			{
				_rfcTag.RequiresValidTag = value;
				CheckVariantAndScriptRules();
			}
		}

		/// <summary>
		/// Creates a new WritingSystemDefinition
		/// </summary>
		/// <param name="language">A valid BCP47 language subtag</param>
		/// <param name="script">A valid BCP47 script subtag</param>
		/// <param name="region">A valid BCP47 region subtag</param>
		/// <param name="variant">A valid BCP47 variant subtag</param>
		/// <param name="abbreviation">The desired abbreviation for this writing system definition</param>
		/// <param name="rightToLeftScript">Indicates whether this writing system uses a right to left script</param>
		public WritingSystemDefinition(string language, string script, string region, string variant, string abbreviation, bool rightToLeftScript)
			: this()
		{
			string variantPart;
			string privateUsePart;
			SplitVariantAndPrivateUse(variant, out variantPart, out privateUsePart);
			_rfcTag = new Rfc5646Tag(language, script, region, variantPart, privateUsePart);

			_abbreviation = abbreviation;
			_rightToLeftScript = rightToLeftScript;
			UpdateIdFromRfcTag();
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="ws">The ws.</param>
		public WritingSystemDefinition(WritingSystemDefinition ws)
		{
			_abbreviation = ws._abbreviation;
			_rightToLeftScript = ws._rightToLeftScript;
			foreach (FontDefinition fd in ws._fonts)
				_fonts.Add(fd.Clone());
			if (ws._defaultFont != null)
				_defaultFont = _fonts[ws._fonts.IndexOf(ws._defaultFont)];
			_keyboard = ws._keyboard;
			_versionNumber = ws._versionNumber;
			_versionDescription = ws._versionDescription;
			_nativeName = ws._nativeName;
			_collationRulesType = ws._collationRulesType;
			_collationRules = ws._collationRules;
			foreach (SpellCheckDictionaryDefinition scdd in ws._spellCheckDictionaries)
				_spellCheckDictionaries.Add(scdd.Clone());
			if (ws._spellCheckDictionary != null)
				_spellCheckDictionary = _spellCheckDictionaries[ws._spellCheckDictionaries.IndexOf(ws._spellCheckDictionary)];
			_dateModified = ws._dateModified;
			_isUnicodeEncoded = ws._isUnicodeEncoded;
			_rfcTag = new Rfc5646Tag(ws._rfcTag);
			_languageName = ws._languageName;
			_localKeyboard = ws._localKeyboard;
			WindowsLcid = ws.WindowsLcid;
			foreach (IKeyboardDefinition kbd in ws._knownKeyboards)
				_knownKeyboards.Add(kbd);
			_id = ws._id;
		}

		///<summary>
		///This is the version of the locale data contained in this writing system.
		///This should not be confused with the version of our writingsystemDefinition implementation which is mostly used for migration purposes.
		///That information is stored in the "LatestWritingSystemDefinitionVersion" property.
		///</summary>
		virtual public string VersionNumber
		{
			get { return _versionNumber; }
			set { UpdateString(ref _versionNumber, value); }
		}

		/// <summary>
		/// Gets or sets the version description.
		/// </summary>
		virtual public string VersionDescription
		{
			get { return _versionDescription; }
			set { UpdateString(ref _versionDescription, value); }
		}

		/// <summary>
		/// Gets or sets the date modified.
		/// </summary>
		virtual public DateTime DateModified
		{
			get { return _dateModified; }
			set { _dateModified = value; }
		}

		/// <summary>
		/// Adjusts the BCP47 tag to indicate the desired form of Ipa by inserting fonipa in the variant and emic or etic in private use where necessary.
		/// </summary>
		virtual public IpaStatusChoices IpaStatus
		{
			get
			{
				if (Rfc5646TagIsPhonemicConform)
				{
					return IpaStatusChoices.IpaPhonemic;
				}
				if (Rfc5646TagIsPhoneticConform)
				{
					return IpaStatusChoices.IpaPhonetic;
				}
				if (VariantSubTagIsIpaConform)
				{
					return IpaStatusChoices.Ipa;
				}
				return IpaStatusChoices.NotIpa;
			}

			set
			{
				if (IpaStatus == value)
				{
					return;
				}
				//We need this to make sure that our language tag won't start with the variant "fonipa"
				if(_rfcTag.Language == "")
				{
					_rfcTag.Language = WellKnownSubtags.UnlistedLanguage;
				}
				_rfcTag.RemoveFromPrivateUse(WellKnownSubtags.AudioPrivateUse);
				/* "There are some variant subtags that have no prefix field,
				 * eg. fonipa (International IpaPhonetic Alphabet). Such variants
				 * should appear after any other variant subtags with prefix information."
				 */
				_rfcTag.RemoveFromPrivateUse("x-etic");
				_rfcTag.RemoveFromPrivateUse("x-emic");
				_rfcTag.RemoveFromVariant("fonipa");

				switch (value)
				{
					case IpaStatusChoices.Ipa:
						_rfcTag.AddToVariant(WellKnownSubtags.IpaVariant);
						break;
					case IpaStatusChoices.IpaPhonemic:
						_rfcTag.AddToVariant(WellKnownSubtags.IpaVariant);
						_rfcTag.AddToPrivateUse(WellKnownSubtags.IpaPhonemicPrivateUse);
						break;
					case IpaStatusChoices.IpaPhonetic:
						_rfcTag.AddToVariant(WellKnownSubtags.IpaVariant);
						_rfcTag.AddToPrivateUse(WellKnownSubtags.IpaPhoneticPrivateUse);
						break;
				}
				IsChanged = true;
				UpdateIdFromRfcTag();
			}
		}

		/// <summary>
		/// Adjusts the BCP47 tag to indicate that this is an "audio writing system" by inserting "audio" in the private use and "Zxxx" in the script
		/// </summary>
		virtual public bool IsVoice
		{
			get
			{
				return ScriptSubTagIsAudio && VariantSubTagIsAudio;
			}
			set
			{
				if (IsVoice == value) { return; }
				if (value)
				{
					IpaStatus = IpaStatusChoices.NotIpa;
					Keyboard = string.Empty;
					if(Language == "")
					{
						Language = WellKnownSubtags.UnlistedLanguage;
					}
					Script = WellKnownSubtags.AudioScript;
					_rfcTag.AddToPrivateUse(WellKnownSubtags.AudioPrivateUse);
				}
				else
				{
					_rfcTag.Script = String.Empty;
					_rfcTag.RemoveFromPrivateUse(WellKnownSubtags.AudioPrivateUse);
				}
				IsChanged = true;
				UpdateIdFromRfcTag();
				CheckVariantAndScriptRules();
			}
		}

		private bool VariantSubTagIsAudio
		{
			get
			{
				return _rfcTag.PrivateUseContains(WellKnownSubtags.AudioPrivateUse);
			}
		}

		private bool ScriptSubTagIsAudio
		{
			get { return _rfcTag.Script.Equals(WellKnownSubtags.AudioScript, StringComparison.OrdinalIgnoreCase); }
		}

		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// Note that the variant also includes the private use subtags. These are appended to the variant subtags seperated by "-x-"
		/// Also note the convenience methods "SplitVariantAndPrivateUse" and "ConcatenateVariantAndPrivateUse" for easier
		/// variant/ private use handling
		/// </summary>
		// Todo: this could/should become an ordered list of variant tags
		virtual public string Variant
		{
			get
			{
				string variantToReturn = ConcatenateVariantAndPrivateUse(_rfcTag.Variant, _rfcTag.PrivateUse);
				return variantToReturn;
			}
			set
			{
				value = value ?? "";
				if (value == Variant)
				{
					return;
				}
				// Note that the WritingSystemDefinition provides no direct support for private use except via Variant set.
				string variant;
				string privateUse;
				SplitVariantAndPrivateUse(value, out variant, out privateUse);
				_rfcTag.Variant = variant;
				_rfcTag.PrivateUse = privateUse;

				IsChanged = true;
				UpdateIdFromRfcTag();
				CheckVariantAndScriptRules();
			}
		}

		/// <summary>
		/// Adds a valid BCP47 registered variant subtag to the variant. Any other tag is inserted as private use.
		/// </summary>
		/// <param name="registeredVariantOrPrivateUseSubtag">A valid variant tag or another tag which will be inserted into private use.</param>
		public void AddToVariant(string registeredVariantOrPrivateUseSubtag)
		{
			if (StandardTags.IsValidRegisteredVariant(registeredVariantOrPrivateUseSubtag))
			{
				_rfcTag.AddToVariant(registeredVariantOrPrivateUseSubtag);
			}
			else
			{
				_rfcTag.AddToPrivateUse(registeredVariantOrPrivateUseSubtag);
			}
			UpdateIdFromRfcTag();
			CheckVariantAndScriptRules();
		}

		/// <summary>
		/// A convenience method to help consumers deal with variant and private use subtags both being stored in the Variant property.
		/// This method will search the Variant part of the BCP47 tag for an "x" extension marker and split the tag into variant and private use sections
		/// Note the complementary method "ConcatenateVariantAndPrivateUse"
		/// </summary>
		/// <param name="variantAndPrivateUse">The string containing variant and private use sections seperated by an "x" private use subtag</param>
		/// <param name="variant">The resulting variant section</param>
		/// <param name="privateUse">The resulting private use section</param>
		public static void SplitVariantAndPrivateUse(string variantAndPrivateUse, out string variant, out string privateUse)
		{
			if (variantAndPrivateUse.StartsWith("x-",StringComparison.OrdinalIgnoreCase)) // Private Use at the beginning
			{
				variantAndPrivateUse = variantAndPrivateUse.Substring(2); // Strip the leading x-
				variant = "";
				privateUse = variantAndPrivateUse;
			}
			else if (variantAndPrivateUse.Contains("-x-", StringComparison.OrdinalIgnoreCase)) // Private Use from the middle
			{
				string[] partsOfVariant = variantAndPrivateUse.Split(new[] { "-x-" }, StringSplitOptions.None);
				if(partsOfVariant.Length == 1)  //Must have been a capital X
				{
					partsOfVariant = variantAndPrivateUse.Split(new[] { "-X-" }, StringSplitOptions.None);
				}
				variant = partsOfVariant[0];
				privateUse = partsOfVariant[1];
			}
			else // No Private Use, it's contains variants only
			{
				variant = variantAndPrivateUse;
				privateUse = "";
			}
		}

		/// <summary>
		/// A convenience method to help consumers deal with registeredVariantSubtags and private use subtags both being stored in the Variant property.
		/// This method will insert a "x" private use subtag between a set of registered BCP47 variants and a set of private use subtags
		/// Note the complementary method "ConcatenateVariantAndPrivateUse"
		/// </summary>
		/// <param name="registeredVariantSubtags">A set of registered variant subtags</param>
		/// <param name="privateUseSubtags">A set of private use subtags</param>
		/// <returns>The resulting combination of registeredVariantSubtags and private use.</returns>
		public static string ConcatenateVariantAndPrivateUse(string registeredVariantSubtags, string privateUseSubtags)
		{
			if(String.IsNullOrEmpty(privateUseSubtags))
			{
				return registeredVariantSubtags;
			}
			if(!privateUseSubtags.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
			{
				privateUseSubtags = String.Concat("x-", privateUseSubtags);
			}

			string variantToReturn = registeredVariantSubtags;
			if (!String.IsNullOrEmpty(privateUseSubtags))
			{
				if (!String.IsNullOrEmpty(variantToReturn))
				{
					variantToReturn += "-";
				}
				variantToReturn += privateUseSubtags;
			}
			return variantToReturn;
		}

		private void CheckVariantAndScriptRules()
		{
			if (!RequiresValidTag)
				return;
			if (VariantSubTagIsAudio && !ScriptSubTagIsAudio)
			{
				throw new ArgumentException("The script subtag must be set to " + WellKnownSubtags.AudioScript + " when the variant tag indicates an audio writing system.");
			}
			bool rfcTagHasAnyIpa = VariantSubTagIsIpaConform ||
									_rfcTag.PrivateUseContains(WellKnownSubtags.IpaPhonemicPrivateUse) ||
									_rfcTag.PrivateUseContains(WellKnownSubtags.IpaPhoneticPrivateUse);
			if (VariantSubTagIsAudio && rfcTagHasAnyIpa)
			{
				throw new ArgumentException("A writing system may not be marked as audio and ipa at the same time.");
			}
			if((_rfcTag.PrivateUseContains(WellKnownSubtags.IpaPhonemicPrivateUse)  ||
				_rfcTag.PrivateUseContains(WellKnownSubtags.IpaPhoneticPrivateUse)) &&
				!VariantSubTagIsIpaConform)
			{
				throw new ArgumentException("A writing system may not be marked as phonetic (x-etic) or phonemic (x-emic) and lack the variant marker fonipa.");
			}
		}

		private bool VariantSubTagIsIpaConform
		{
			get
			{
				return _rfcTag.VariantContains(WellKnownSubtags.IpaVariant);
			}
		}

		private bool Rfc5646TagIsPhoneticConform
		{
			get
			{
				return  _rfcTag.VariantContains(WellKnownSubtags.IpaVariant) &&
					_rfcTag.PrivateUseContains(WellKnownSubtags.IpaPhoneticPrivateUse);
			}
		}

		private bool Rfc5646TagIsPhonemicConform
		{
			get
			{
				return _rfcTag.VariantContains(WellKnownSubtags.IpaVariant) &&
					_rfcTag.PrivateUseContains(WellKnownSubtags.IpaPhonemicPrivateUse);
			}
		}

		/// <summary>
		/// Sets all BCP47 language tag components at once.
		/// This method is useful for avoiding invalid intermediate states when switching from one valid tag to another.
		/// </summary>
		/// <param name="language">A valid BCP47 language subtag.</param>
		/// <param name="script">A valid BCP47 script subtag.</param>
		/// <param name="region">A valid BCP47 region subtag.</param>
		/// <param name="variant">A valid BCP47 variant subtag.</param>
		public void SetAllComponents(string language, string script, string region, string variant)
		{
			string oldId = _rfcTag.CompleteTag;
			string variantPart;
			string privateUsePart;
			SplitVariantAndPrivateUse(variant, out variantPart, out privateUsePart);
			_rfcTag = new Rfc5646Tag(language, script, region, variantPart, privateUsePart);
			UpdateIdFromRfcTag();
			if(oldId == _rfcTag.CompleteTag)
			{
				return;
			}
			IsChanged = true;
			CheckVariantAndScriptRules();
		}

		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// </summary>
		virtual public string Region
		{
			get
			{
				return _rfcTag.Region;
			}
			set
			{
				value = value ?? "";
				if (value == Region)
				{
					return;
				}
				_rfcTag.Region = value;
				UpdateIdFromRfcTag();
				IsChanged = true;
			}
		}

		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// </summary>
		virtual public string Language
		{
			get
			{
				return _rfcTag.Language;
			}
			set
			{
				value = value ?? "";
				if (value == Language)
				{
					return;
				}
				_rfcTag.Language = value;
				UpdateIdFromRfcTag();
				IsChanged = true;
			}
		}

		/// <summary>
		/// The desired abbreviation for the writing system
		/// </summary>
		virtual public string Abbreviation
		{
			get
			{
				if (String.IsNullOrEmpty(_abbreviation))
				{
					// Use the language subtag unless it's an unlisted language.
					// If it's an unlisted language, use the private use area language subtag.
					if (Language == "qaa")
					{
						int idx = Id.IndexOf("-x-", StringComparison.Ordinal);
						if (idx > 0 && Id.Length > idx + 3)
						{
							var abbr = Id.Substring(idx + 3);
							idx = abbr.IndexOf('-');
							if (idx > 0)
								abbr = abbr.Substring(0, idx);
							return abbr;
						}
					}
					return Language;
				}
				return _abbreviation;
			}
			set { UpdateString(ref _abbreviation, value); }
		}


		/// <summary>
		/// A string representing the subtag of the same name as defined by BCP47.
		/// </summary>
		virtual public string Script
		{
			get { return _rfcTag.Script; }
			set
			{
				value = value ?? "";
				if (value == Script)
				{
					return;
				}
				_rfcTag.Script = value;
				IsChanged = true;
				UpdateIdFromRfcTag();
				CheckVariantAndScriptRules();
			}
		}


		/// <summary>
		/// The language name to use. Typically this is the language name associated with the BCP47 language subtag as defined by the IANA subtag registry
		/// </summary>
		virtual public string LanguageName
		{
			get
			{
				if (!String.IsNullOrEmpty(_languageName))
				{
					return _languageName;
				}
				var code = StandardTags.ValidIso639LanguageCodes.FirstOrDefault(c => c.Code.Equals(Language));
				if (code != null)
				{
					return code.Name;
				}
				return "Unknown Language";

				// TODO Make the below work.
				//return StandardTags.LanguageName(Language) ?? "Unknown Language";
			}
			set
			{
				value = value ?? "";
				UpdateString(ref _languageName, value);
			}
		}

		/// <summary>
		/// This method will make a copy of the given writing system and
		/// then make the Id unique compared to list of Ids passed in by
		/// appending dupl# where # is a digit that increases with the
		/// number of duplicates found.
		/// </summary>
		/// <param name="writingSystemToCopy"></param>
		/// <param name="otherWritingsystemIds"></param>
		/// <returns></returns>
		public static WritingSystemDefinition CreateCopyWithUniqueId(
			WritingSystemDefinition writingSystemToCopy, IEnumerable<string> otherWritingsystemIds)
		{
			WritingSystemDefinition newWs = writingSystemToCopy.Clone();
			var lastAppended = String.Empty;
			int duplicateNumber = 0;
			string[] wsIds = otherWritingsystemIds.ToArray();
			while (wsIds.Any(id => id.Equals(newWs.Id, StringComparison.OrdinalIgnoreCase)))
			{
				newWs._rfcTag.RemoveFromPrivateUse(lastAppended);
				var currentToAppend = String.Format("dupl{0}", duplicateNumber);
				if (!newWs._rfcTag.PrivateUse.Contains(currentToAppend))
				{
					newWs._rfcTag.AddToPrivateUse(currentToAppend);
					newWs.UpdateIdFromRfcTag();
					lastAppended = currentToAppend;
				}
				duplicateNumber++;
			}
			return newWs;
		}

		/// <summary>
		/// Used by IWritingSystemRepository to identify writing systems. Only change this if you would like to replace a writing system with the same StoreId
		/// already contained in the repo. This is useful creating a temporary copy of a writing system that you may or may not care to persist to the
		/// IWritingSystemRepository.
		/// Typical use would therefor be:
		/// ws.Clone(wsorig);
		/// ws.StoreId=wsOrig.StoreId;
		/// **make changes to ws**
		/// repo.Set(ws);
		/// </summary>
		virtual public string StoreID { get; set; }

		/// <summary>
		/// A automatically generated descriptive label for the writing system definition.
		/// </summary>
		virtual public string DisplayLabel
		{
			get
			{
				//jh (Oct 2010) made it start with RFC5646 because all ws's in a lang start with the
				//same abbreviation, making imppossible to see (in SOLID for example) which you chose.
				bool languageIsUnknown = Bcp47Tag.Equals(WellKnownSubtags.UnlistedLanguage, StringComparison.OrdinalIgnoreCase);
				if (!String.IsNullOrEmpty(Bcp47Tag) && !languageIsUnknown)
				{
					return Bcp47Tag;
				}
				if (languageIsUnknown)
				{
					if (!String.IsNullOrEmpty(_abbreviation))
					{
						return _abbreviation;
					}
					if (!String.IsNullOrEmpty(_languageName))
					{
						string n = _languageName;
						return n.Substring(0, n.Length > 4 ? 4 : n.Length);
					}
				}
				return "???";
			}
		}

		/// <summary>
		/// Gets the list label.
		/// </summary>
		virtual public string ListLabel
		{
			get
			{
				//the idea here is to give writing systems a nice legible label for. For this reason subtags are replaced with nice labels
				WritingSystemDefinition wsToConstructLabelFrom = Clone();
				string n = !String.IsNullOrEmpty(wsToConstructLabelFrom.LanguageName) ? wsToConstructLabelFrom.LanguageName : wsToConstructLabelFrom.DisplayLabel;
				string details = "";

				if (wsToConstructLabelFrom.IpaStatus != IpaStatusChoices.NotIpa)
				{
					switch (IpaStatus)
					{
						case IpaStatusChoices.Ipa:
							details += "IPA-";
							break;
						case IpaStatusChoices.IpaPhonetic:
							details += "IPA-etic-";
							break;
						case IpaStatusChoices.IpaPhonemic:
							details += "IPA-emic-";
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
					wsToConstructLabelFrom.IpaStatus = IpaStatusChoices.NotIpa;
				}
				if (wsToConstructLabelFrom.IsVoice)
				{
					details += "Voice-";
					wsToConstructLabelFrom.IsVoice = false;
				}
				if (wsToConstructLabelFrom.IsDuplicate)
				{
					var duplicateNumbers = new List<string>(wsToConstructLabelFrom.DuplicateNumbers);
					foreach (var number in duplicateNumbers)
					{
						details += "Copy";
						if (number != "0")
						{
							details += number;
						}
						details += "-";
						wsToConstructLabelFrom._rfcTag.RemoveFromPrivateUse("dupl" + number);
					}
				}

				if (!String.IsNullOrEmpty(wsToConstructLabelFrom.Script))
				{
					details += wsToConstructLabelFrom.Script+"-";
				}
				if (!String.IsNullOrEmpty(wsToConstructLabelFrom.Region))
				{
					details += wsToConstructLabelFrom.Region + "-";
				}
				if (!String.IsNullOrEmpty(wsToConstructLabelFrom.Variant))
				{
					details += wsToConstructLabelFrom.Variant + "-";
				}

				details = details.Trim(new[] { '-' });
				if (details.Length > 0)
					details = " ("+details + ")";
				return n+details;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is a duplicate.
		/// </summary>
		protected bool IsDuplicate
		{
			get { return _rfcTag.GetPrivateUseSubtagsMatchingRegEx(@"^dupl\d$").Count() != 0; }
		}

		/// <summary>
		/// Gets the duplicate numbers.
		/// </summary>
		protected IEnumerable<string> DuplicateNumbers
		{
			get
			{
				return _rfcTag.GetPrivateUseSubtagsMatchingRegEx(@"^dupl\d$").Select(subtag => Regex.Match(subtag, @"\d*$").Value);
			}
		}


		/// <summary>
		/// The current BCP47 tag which is a concatenation of the Language, Script, Region and Variant properties.
		/// </summary>
		public string Bcp47Tag
		{
			get
			{
				return _rfcTag.CompleteTag;
			}
		}

		/// <summary>
		/// The identifier for this writing syetm definition. Use this in files and as a key to the IWritingSystemRepository.
		/// Note that this is usually identical to the Bcp47 tag and should rarely differ.
		/// </summary>
		public string Id
		{
			get { return _id; }

			internal set
			{
				value = value ?? "";
				_id = value;
				IsChanged = true;
			}
		}

		/// <summary>
		/// Indicates whether the writing system definition has been modified.
		/// Note that this flag is automatically set by all methods that cause a modification and is reset by the IWritingSystemRepository.Save() method
		/// </summary>
		public override bool IsChanged
		{
			get
			{
				return base.IsChanged || _fonts.Any(fd => fd.IsChanged) || _spellCheckDictionaries.Any(scdd => scdd.IsChanged);
			}
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			foreach (FontDefinition fd in _fonts)
				fd.AcceptChanges();
			foreach (SpellCheckDictionaryDefinition scdd in _spellCheckDictionaries)
				scdd.AcceptChanges();
		}

		/// <summary>
		/// Gets or sets a value indicating whether the writing system will be deleted.
		/// </summary>
		virtual public bool MarkedForDeletion { get; set; }

		/// <summary>
		/// The font used to display data encoded in this writing system
		/// </summary>
		virtual public FontDefinition DefaultFont
		{
			get
			{
				if (_defaultFont == null)
					_defaultFont = _fonts.FirstOrDefault(fd => fd.Roles.HasFlag(FontRoles.Default));
				if (_defaultFont == null)
					_defaultFont = _fonts.FirstOrDefault();
				return _defaultFont;
			}
			set
			{
				if (UpdateField(ref _defaultFont, value))
				{
					if (value != null && !_fonts.Contains(value))
						_fonts.Add(value);
				}
			}
		}

		/// <summary>
		/// The preferred keyboard to use to generate data encoded in this writing system.
		/// </summary>
		virtual public string Keyboard
		{
			get
			{
				if (string.IsNullOrEmpty(_keyboard))
					return "";
				return _keyboard;
			}
			set { UpdateString(ref _keyboard, value); }
		}

		/// <summary>
		/// This field retrieves the value obtained from the FieldWorks LDML extension fw:windowsLCID.
		/// This is used only when current information in LocalKeyboard or KnownKeyboards is not useable.
		/// It is not useful to modify this or set it in new LDML files; however, we need a public setter
		/// because FieldWorks overrides the code that normally reads this from the LDML file.
		/// </summary>
		public string WindowsLcid { get; set; }

		/// <summary>
		/// This tracks the keyboard that should be used for this writing system on this computer.
		/// It is not shared with other users of the project.
		/// </summary>
		public IKeyboardDefinition LocalKeyboard
		{
			get
			{
				if (_localKeyboard == null)
				{
					var available = new HashSet<IKeyboardDefinition>(WritingSystems.Keyboard.Controller.AllAvailableKeyboards);
					_localKeyboard = _knownKeyboards.FirstOrDefault(available.Contains);
				}
				if (_localKeyboard == null)
					_localKeyboard = WritingSystems.Keyboard.Controller.DefaultForWritingSystem(this);
				return _localKeyboard;
			}
			set
			{
				if (UpdateField(ref _localKeyboard, value))
				{
					if (value != null)
						_knownKeyboards.Add(value);
				}
			}
		}

		internal IKeyboardDefinition RawLocalKeyboard
		{
			get { return _localKeyboard; }
		}

		/// <summary>
		/// Indicates whether this writing system is read and written from left to right or right to left
		/// </summary>
		virtual public bool RightToLeftScript
		{
			get { return _rightToLeftScript; }
			set { UpdateField(ref _rightToLeftScript, value); }
		}

		/// <summary>
		/// The windows "NativeName" from the Culture class
		/// </summary>
		virtual public string NativeName
		{
			get { return _nativeName; }
			set { UpdateString(ref _nativeName, value); }
		}

		/// <summary>
		/// Indicates the type of sort rules used to encode the sort order.
		/// Note that the actual sort rules are contained in the SortRules property
		/// </summary>
		virtual public CollationRulesTypes CollationRulesType
		{
			get { return _collationRulesType; }
			set
			{
				if (UpdateField(ref _collationRulesType, value))
					_collator = null;
			}
		}

		/// <summary>
		/// The sort rules that efine the sort order.
		/// Note that you must indicate the type of sort rules used by setting the "SortUsing" property
		/// </summary>
		virtual public string CollationRules
		{
			get { return _collationRules ?? string.Empty; }
			set
			{
				if (UpdateString(ref _collationRules, value))
					_collator = null;
			}
		}

		/// <summary>
		/// A convenience method for sorting like anthoer language
		/// </summary>
		/// <param name="languageCode">A valid language code</param>
		public void SortUsingOtherLanguage(string languageCode)
		{
			CollationRulesType = CollationRulesTypes.OtherLanguage;
			CollationRules = languageCode;
		}

		/// <summary>
		/// A convenience method for sorting with custom ICU rules
		/// </summary>
		/// <param name="sortRules">custom ICU sortrules</param>
		public void SortUsingCustomIcu(string sortRules)
		{
			CollationRulesType = CollationRulesTypes.CustomIcu;
			CollationRules = sortRules;
		}

		/// <summary>
		/// A convenience method for sorting with "shoebox" style rules
		/// </summary>
		/// <param name="sortRules">"shoebox" style rules</param>
		public void SortUsingCustomSimple(string sortRules)
		{
			CollationRulesType = CollationRulesTypes.CustomSimple;
			CollationRules = sortRules;
		}

		/// <summary>
		/// Returns an ICollator interface that can be used to sort strings based
		/// on the custom collation rules.
		/// </summary>
		virtual public ICollator Collator
		{
			get
			{
				if (_collator == null)
				{
					switch (CollationRulesType)
					{
						case CollationRulesTypes.DefaultOrdering:
							_collator = new IcuRulesCollator(String.Empty); // was SystemCollator(null);
							break;
						case CollationRulesTypes.CustomSimple:
							_collator = new SimpleRulesCollator(CollationRules);
							break;
						case CollationRulesTypes.CustomIcu:
							_collator = new IcuRulesCollator(CollationRules);
							break;
						case CollationRulesTypes.OtherLanguage:
							_collator = new SystemCollator(CollationRules);
							break;
					}
				}
				return _collator;
			}
		}

		/// <summary>
		/// Tests whether the current custom collation rules are valid.
		/// </summary>
		/// <param name="message">Used for an error message if rules do not validate.</param>
		/// <returns>True if rules are valid, false otherwise.</returns>
		virtual public bool ValidateCollationRules(out string message)
		{
			message = null;
			switch (CollationRulesType)
			{
				case CollationRulesTypes.DefaultOrdering:
					return String.IsNullOrEmpty(CollationRules);
				case CollationRulesTypes.CustomIcu:
					return IcuRulesCollator.ValidateSortRules(CollationRules, out message);
				case CollationRulesTypes.CustomSimple:
					return SimpleRulesCollator.ValidateSimpleRules(CollationRules, out message);
				case CollationRulesTypes.OtherLanguage:
					return SystemCollator.ValidateCultureID(CollationRules, out message);
			}
			return false;
		}

		public override string ToString()
		{
			return _rfcTag.ToString();
		}

		/// <summary>
		/// Creates a clone of the current writing system.
		/// Note that this excludes the properties: Modified, MarkedForDeletion and StoreID
		/// </summary>
		/// <returns></returns>
		public override WritingSystemDefinition Clone()
		{
			return new WritingSystemDefinition(this);
		}

		/// <summary>
		/// Checks for value equality with the specified writing system.
		/// </summary>
		public override bool ValueEquals(WritingSystemDefinition other)
		{
			if (other == null)
				return false;

			if (!_rfcTag.Equals(other._rfcTag))
				return false;
			if (_languageName != other._languageName)
				return false;
			if (_abbreviation != other._abbreviation)
				return false;
			if (_versionNumber != other._versionNumber)
				return false;
			if (_versionDescription != other._versionDescription)
				return false;
			if (_keyboard != other._keyboard)
				return false;
			if (_collationRules != other._collationRules)
				return false;
			if (_nativeName != other._nativeName)
				return false;
			if (_id != other._id)
				return false;
			if (_isUnicodeEncoded != other._isUnicodeEncoded)
				return false;
			if (_dateModified != other._dateModified)
				return false;
			if (CollationRulesType != other.CollationRulesType)
				return false;
			if (_rightToLeftScript != other._rightToLeftScript)
				return false;
			if (WindowsLcid != other.WindowsLcid)
				return false;

			// fonts
			if (_fonts.Count != other._fonts.Count)
				return false;
			for (int i = 0; i < _fonts.Count; i++)
			{
				if (!_fonts[i].ValueEquals(other._fonts[i]))
					return false;
			}
			if (DefaultFont == null)
			{
				if (other.DefaultFont != null)
					return false;
			}
			else if (!DefaultFont.ValueEquals(other.DefaultFont))
			{
				return false;
			}

			// spell checking dictionaries
			if (_spellCheckDictionaries.Count != other._spellCheckDictionaries.Count)
				return false;
			for (int i = 0; i < _spellCheckDictionaries.Count; i++)
			{
				if (!_spellCheckDictionaries[i].ValueEquals(other._spellCheckDictionaries[i]))
					return false;
			}
			if (SpellCheckDictionary == null)
			{
				if (other.SpellCheckDictionary != null)
					return false;
			}
			else if (!SpellCheckDictionary.ValueEquals(other.SpellCheckDictionary))
			{
				return false;
			}

			// keyboards
			if (!_knownKeyboards.SequenceEqual(other._knownKeyboards))
				return false;
			if (LocalKeyboard != other.LocalKeyboard)
				return false;
			return true;
		}

		private void UpdateIdFromRfcTag()
		{
			_id = Bcp47Tag;
		}

		/// <summary>
		/// Indicates whether this writing system is unicode encoded or legacy encoded
		/// </summary>
		public bool IsUnicodeEncoded
		{
			get { return _isUnicodeEncoded; }
			set { UpdateField(ref _isUnicodeEncoded, value); }
		}

		/// <summary>
		/// Parses the supplied BCP47 tag and sets the Language, Script, Region and Variant properties accordingly
		/// </summary>
		/// <param name="completeTag">A valid BCP47 tag</param>
		public void SetTagFromString(string completeTag)
		{
			_rfcTag = Rfc5646Tag.Parse(completeTag);
			UpdateIdFromRfcTag();
			IsChanged = true;
		}

		/// <summary>
		/// Parses the supplied BCP47 tag and return a new writing system definition with the correspnding Language, Script, Region and Variant properties
		/// </summary>
		/// <param name="bcp47Tag">A valid BCP47 tag</param>
		public static WritingSystemDefinition Parse(string bcp47Tag)
		{
			var writingSystemDefinition = new WritingSystemDefinition();
			writingSystemDefinition.SetTagFromString(bcp47Tag);
			return writingSystemDefinition;
		}

		/// <summary>
		/// Returns a new writing system definition with the corresponding Language, Script, Region and Variant properties set
		/// </summary>
		public static WritingSystemDefinition FromSubtags(string language, string script, string region, string variantAndPrivateUse)
		{
			return new WritingSystemDefinition(language, script, region, variantAndPrivateUse, string.Empty, false);
		}

		/// <summary>
		/// Filters out all "WellKnownSubTags" out of a list of subtags
		/// </summary>
		/// <param name="privateUseTokens"></param>
		/// <returns></returns>
		public static IEnumerable<string> FilterWellKnownPrivateUseTags(IEnumerable<string> privateUseTokens)
		{
			foreach (var privateUseToken in privateUseTokens)
			{
				string strippedToken = Rfc5646Tag.StripLeadingPrivateUseMarker(privateUseToken);
				if (strippedToken.Equals(Rfc5646Tag.StripLeadingPrivateUseMarker(WellKnownSubtags.AudioPrivateUse), StringComparison.OrdinalIgnoreCase) ||
					strippedToken.Equals(Rfc5646Tag.StripLeadingPrivateUseMarker(WellKnownSubtags.IpaPhonemicPrivateUse), StringComparison.OrdinalIgnoreCase) ||
					strippedToken.Equals(Rfc5646Tag.StripLeadingPrivateUseMarker(WellKnownSubtags.IpaPhoneticPrivateUse), StringComparison.OrdinalIgnoreCase))
					continue;
				yield return privateUseToken;
			}
		}

		/// <summary>
		/// Keyboards known to have been used with this writing system. Not all may be available on this system.
		/// Enhance: document (or add to this class?) a way of getting available keyboards.
		/// </summary>
		public KeyboardDefinitionCollection KnownKeyboards
		{
			get { return _knownKeyboards; }
		}

		/// <summary>
		/// Returns the available keyboards (known to Keyboard.Controller) that are not KnownKeyboards for this writing system.
		/// </summary>
		public IEnumerable<IKeyboardDefinition> OtherAvailableKeyboards
		{
			get
			{
				return WritingSystems.Keyboard.Controller.AllAvailableKeyboards.Except(KnownKeyboards);
			}
		}

		public FontDefinitionCollection Fonts
		{
			get { return _fonts; }
		}

		public SpellCheckDictionaryDefinitionCollection SpellCheckDictionaries
		{
			get { return _spellCheckDictionaries; }
		}

		public SpellCheckDictionaryDefinition SpellCheckDictionary
		{
			get
			{
				if (_spellCheckDictionary == null)
					_spellCheckDictionary = _spellCheckDictionaries.FirstOrDefault();
				return _spellCheckDictionary;
			}
			set
			{
				if (UpdateField(ref _spellCheckDictionary, value))
				{
					if (value != null && !_spellCheckDictionaries.Contains(value))
						_spellCheckDictionaries.Add(value);
				}
			}
		}
	}
}