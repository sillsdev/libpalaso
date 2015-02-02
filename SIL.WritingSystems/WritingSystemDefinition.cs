using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Palaso.Data;

namespace SIL.WritingSystems
{
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

	public enum QuotationParagraphContinueType
	{
		None,
		All,
		Outermost,
		Innermost
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
		public const int LatestWritingSystemDefinitionVersion = 3;

		private string _languageName;
		private string _languageTag;
		private LanguageSubtag _language;
		private ScriptSubtag _script;
		private RegionSubtag _region;
		private readonly ObservableCollection<VariantSubtag> _variants = new ObservableCollection<VariantSubtag>();
		private string _abbreviation;
		private bool _isUnicodeEncoded = true;
		private string _versionNumber;
		private string _versionDescription;
		private DateTime _dateModified;
		private FontDefinition _defaultFont;
		private string _keyboard;
		private string _nativeName;
		private bool _rightToLeftScript;
		private IKeyboardDefinition _localKeyboard;
		private string _id;
		private string _defaultRegion;
		private string _windowsLcid;
		private CollationDefinition _defaultCollation;
		private SpellCheckDictionaryDefinition _spellCheckDictionary;
		private QuotationParagraphContinueType _quotationParagraphContinueType;
		private readonly ObservableKeyedCollection<string, FontDefinition> _fonts = new ObservableKeyedCollection<string, FontDefinition>(fd => fd.Name);
		private readonly ObservableKeyedCollection<string, IKeyboardDefinition> _knownKeyboards = new ObservableKeyedCollection<string, IKeyboardDefinition>(kd => kd.Id);
		private readonly ObservableKeyedCollection<string, SpellCheckDictionaryDefinition> _spellCheckDictionaries = new ObservableKeyedCollection<string, SpellCheckDictionaryDefinition>(scdd => string.Format("{0}_{1}", scdd.LanguageTag, scdd.Format));
		private readonly ObservableKeyedCollection<string, CollationDefinition> _collations = new ObservableKeyedCollection<string, CollationDefinition>(cd => cd.Type);
		private readonly ObservableSet<MatchedPair> _matchedPairs;
		private readonly ObservableSet<PunctuationPattern> _punctuationPatterns;
		private readonly ObservableCollection<QuotationMark> _quotationMarks = new ObservableCollection<QuotationMark>();
		private readonly ObservableKeyedCollection<string, CharacterSetDefinition> _characterSets = new ObservableKeyedCollection<string, CharacterSetDefinition>(csd => csd.Type);
		private readonly SimpleMonitor _ignoreVariantChanges = new SimpleMonitor();
		private bool _requiresValidLanguageTag = true;

		/// <summary>
		/// Creates a new WritingSystemDefinition with Language subtag set to "qaa"
		/// </summary>
		public WritingSystemDefinition()
		{
			_language = WellKnownSubtags.UnlistedLanguage;
			_matchedPairs = new ObservableSet<MatchedPair>();
			_punctuationPatterns = new ObservableSet<PunctuationPattern>();
			_defaultCollation = new CollationDefinition("standard");
			_collations.Add(_defaultCollation);
			_languageTag = IetfLanguageTag.ToLanguageTag(_language, _script, _region, _variants);
			_id = _languageTag;
			SetupCollectionChangeListeners();
		}

		/// <summary>
		/// Creates a new WritingSystemDefinition by parsing a valid BCP47 tag
		/// </summary>
		/// <param name="languageTag">A valid BCP47 tag</param>
		public WritingSystemDefinition(string languageTag)
		{
			_languageTag = languageTag;
			IEnumerable<VariantSubtag> variantSubtags;
			if (!IetfLanguageTag.TryGetSubtags(languageTag, out _language, out _script, out _region, out variantSubtags))
				throw new ArgumentException("The language tag is invalid.", "languageTag");
			foreach (VariantSubtag variantSubtag in variantSubtags)
				_variants.Add(variantSubtag);
			CheckVariantAndScriptRules();
			_id = _languageTag;
			_matchedPairs = new ObservableSet<MatchedPair>();
			_punctuationPatterns = new ObservableSet<PunctuationPattern>();
			_defaultCollation = new CollationDefinition("standard");
			_collations.Add(_defaultCollation);
			_abbreviation = _languageName = _nativeName = string.Empty;
			SetupCollectionChangeListeners();
		}

		public WritingSystemDefinition(string language, string script, string region, string variant)
			: this(IetfLanguageTag.ToLanguageTag(language, script, region, variant))
		{
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
			: this(IetfLanguageTag.ToLanguageTag(language, script, region, variant))
		{
			_abbreviation = abbreviation;
			_rightToLeftScript = rightToLeftScript;
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="ws">The ws.</param>
		public WritingSystemDefinition(WritingSystemDefinition ws)
		{
			_language = ws._language;
			_script = ws._script;
			_region = ws._region;
			_variants = new ObservableCollection<VariantSubtag>();
			foreach (VariantSubtag variantSubtag in ws._variants)
				_variants.Add(variantSubtag);
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
			foreach (SpellCheckDictionaryDefinition scdd in ws._spellCheckDictionaries)
				_spellCheckDictionaries.Add(scdd.Clone());
			if (ws._spellCheckDictionary != null)
				_spellCheckDictionary = _spellCheckDictionaries[ws._spellCheckDictionaries.IndexOf(ws._spellCheckDictionary)];
			_dateModified = ws._dateModified;
			_isUnicodeEncoded = ws._isUnicodeEncoded;
			_languageName = ws._languageName;
			_languageTag = ws._languageTag;
			_localKeyboard = ws._localKeyboard;
			_windowsLcid = ws._windowsLcid;
			_defaultRegion = ws._defaultRegion;
			foreach (IKeyboardDefinition kbd in ws._knownKeyboards)
				_knownKeyboards.Add(kbd);
			_matchedPairs = new ObservableSet<MatchedPair>(ws._matchedPairs);
			_punctuationPatterns = new ObservableSet<PunctuationPattern>(ws._punctuationPatterns);
			foreach (QuotationMark qm in ws.QuotationMarks)
				_quotationMarks.Add(qm);
			_quotationParagraphContinueType = ws._quotationParagraphContinueType;
			_id = ws._id;
			foreach (CollationDefinition cd in ws._collations)
				_collations.Add(cd.Clone());
			if (ws._defaultCollation != null)
				_defaultCollation = _collations[ws._collations.IndexOf(ws._defaultCollation)];
			foreach (CharacterSetDefinition csd in ws._characterSets)
				_characterSets.Add(csd.Clone());
			_requiresValidLanguageTag = ws._requiresValidLanguageTag;
			SetupCollectionChangeListeners();
		}

		private void SetupCollectionChangeListeners()
		{
			_fonts.CollectionChanged += _fonts_CollectionChanged;
			_knownKeyboards.CollectionChanged += _knownKeyboards_CollectionChanged;
			_spellCheckDictionaries.CollectionChanged += _spellCheckDictionaries_CollectionChanged;
			_matchedPairs.CollectionChanged += _matchedPairs_CollectionChanged;
			_punctuationPatterns.CollectionChanged += _punctuationPatterns_CollectionChanged;
			_quotationMarks.CollectionChanged += _quotationMarks_CollectionChanged;
			_collations.CollectionChanged += _collations_CollectionChanged;
			_variants.CollectionChanged += _variants_CollectionChanged;
		}

		private void _variants_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (!_ignoreVariantChanges.Busy)
			{
				CheckVariantAndScriptRules();
				UpdateLanguageTag();
				IsChanged = true;
			}
		}

		private void _quotationMarks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		private void _punctuationPatterns_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		private void _matchedPairs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
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

		private void _collations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_defaultCollation != null && !_collations.Contains(_defaultCollation))
				_defaultCollation = null;
			IsChanged = true;
		}

		///<summary>
		///This is the version of the locale data contained in this writing system.
		///This should not be confused with the version of our writingsystemDefinition implementation which is mostly used for migration purposes.
		///That information is stored in the "LatestWritingSystemDefinitionVersion" property.
		///</summary>
		public string VersionNumber
		{
			get { return _versionNumber ?? string.Empty; }
			set { UpdateString(() => VersionNumber, ref _versionNumber, value); }
		}

		/// <summary>
		/// Gets or sets the version description.
		/// </summary>
		public string VersionDescription
		{
			get { return _versionDescription ?? string.Empty; }
			set { UpdateString(() => VersionDescription, ref _versionDescription, value); }
		}

		/// <summary>
		/// Gets or sets the date modified.
		/// </summary>
		public DateTime DateModified
		{
			get { return _dateModified; }
			set { _dateModified = value; }
		}

		/// <summary>
		/// True when the validity of the writing system defn's tag is being enforced. This is the normal and default state.
		/// Setting this true will throw unless the tag has previously been put into a valid state.
		/// Attempting to Save the writing system defn will set this true (and may throw).
		/// </summary>
		public bool RequiresValidLanguageTag
		{
			get { return _requiresValidLanguageTag; }
			set
			{
				if (_requiresValidLanguageTag != value)
				{
					_requiresValidLanguageTag = value;
					CheckVariantAndScriptRules();
					UpdateLanguageTag();
				}
			}
		}

		/// <summary>
		/// Adjusts the BCP47 tag to indicate the desired form of Ipa by inserting fonipa in the variant and emic or etic in private use where necessary.
		/// </summary>
		public IpaStatusChoices IpaStatus
		{
			get
			{
				if (_variants.Contains(WellKnownSubtags.IpaVariant))
				{
					if (_variants.Contains(WellKnownSubtags.IpaPhonemicPrivateUse))
						return IpaStatusChoices.IpaPhonemic;
					if (_variants.Contains(WellKnownSubtags.IpaPhoneticPrivateUse))
						return IpaStatusChoices.IpaPhonetic;
					return IpaStatusChoices.Ipa;
				}
				return IpaStatusChoices.NotIpa;
			}

			set
			{
				if (IpaStatus != value)
				{
					RemoveVariants(WellKnownSubtags.IpaVariant, WellKnownSubtags.IpaPhonemicPrivateUse, WellKnownSubtags.IpaPhoneticPrivateUse, WellKnownSubtags.AudioPrivateUse);

					if (_language == null)
						_language = WellKnownSubtags.UnlistedLanguage;

					int index = IetfLanguageTag.GetIndexOfFirstPrivateUseVariant(_variants);

					switch (value)
					{
						case IpaStatusChoices.Ipa:
							_variants.Insert(index, WellKnownSubtags.IpaVariant);
							break;
						case IpaStatusChoices.IpaPhonemic:
							_variants.Insert(index, WellKnownSubtags.IpaVariant);
							_variants.Insert(index + 1, WellKnownSubtags.IpaPhonemicPrivateUse);
							break;
						case IpaStatusChoices.IpaPhonetic:
							_variants.Insert(index, WellKnownSubtags.IpaVariant);
							_variants.Insert(index + 1, WellKnownSubtags.IpaPhoneticPrivateUse);
							break;
					}
				}
			}
		}

		private void RemoveVariants(params VariantSubtag[] variantSubtags)
		{
			var variantSubtagsSet = new HashSet<VariantSubtag>(variantSubtags);
			for (int i = _variants.Count - 1; i >= 0; i--)
			{
				if (variantSubtagsSet.Contains(_variants[i]))
				{
					variantSubtagsSet.Remove(_variants[i]);
					_variants.RemoveAt(i);
					if (variantSubtagsSet.Count == 0)
						break;
				}
			}
		}

		/// <summary>
		/// Adjusts the BCP47 tag to indicate that this is an "audio writing system" by inserting "audio" in the private use and "Zxxx" in the script
		/// </summary>
		public bool IsVoice
		{
			get
			{
				return ScriptSubTagIsAudio && _variants.Contains(WellKnownSubtags.AudioPrivateUse);
			}
			set
			{
				if (IsVoice == value)
					return;

				if (value)
				{
					if (_language == null)
						_language = WellKnownSubtags.UnlistedLanguage;
					IpaStatus = IpaStatusChoices.NotIpa;
					Keyboard = string.Empty;
					_script = StandardSubtags.Iso15924Scripts[WellKnownSubtags.AudioScript];
					_variants.Add(StandardSubtags.CommonPrivateUseVariants[WellKnownSubtags.AudioPrivateUse]);
				}
				else
				{
					_script = null;
					RemoveVariants(WellKnownSubtags.AudioPrivateUse);
				}
			}
		}

		private bool ScriptSubTagIsAudio
		{
			get { return _script != null && _script.Code.Equals(WellKnownSubtags.AudioScript, StringComparison.OrdinalIgnoreCase); }
		}

		private void CheckVariantAndScriptRules()
		{
			if (!_requiresValidLanguageTag)
				return;

			if (_variants.Contains(WellKnownSubtags.AudioPrivateUse) && !ScriptSubTagIsAudio)
			{
				throw new ValidationException("The script subtag must be set to " + WellKnownSubtags.AudioScript + " when the variant tag indicates an audio writing system.");
			}
			bool rfcTagHasAnyIpa = _variants.Contains(WellKnownSubtags.IpaVariant)
				|| _variants.Contains(WellKnownSubtags.IpaPhonemicPrivateUse) || _variants.Contains(WellKnownSubtags.IpaPhoneticPrivateUse);
			if (_variants.Contains(WellKnownSubtags.AudioPrivateUse) && rfcTagHasAnyIpa)
			{
				throw new ValidationException("A writing system may not be marked as audio and ipa at the same time.");
			}
			if ((_variants.Contains(WellKnownSubtags.IpaPhonemicPrivateUse) || _variants.Contains(WellKnownSubtags.IpaPhoneticPrivateUse))
				&& !_variants.Contains(WellKnownSubtags.IpaVariant))
			{
				throw new ValidationException("A writing system may not be marked as phonetic (x-etic) or phonemic (x-emic) and lack the variant marker fonipa.");
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
			_requiresValidLanguageTag = true;
			string oldId = _languageTag;
			_languageTag = IetfLanguageTag.ToLanguageTag(language, script, region, variant);
			if (oldId != _languageTag)
			{
				IEnumerable<VariantSubtag> variantSubtags;
				IetfLanguageTag.TryGetSubtags(_languageTag, out _language, out _script, out _region, out variantSubtags);
				using (_ignoreVariantChanges.Enter())
				{
					_variants.Clear();
					foreach (VariantSubtag variantSubtag in variantSubtags)
						_variants.Add(variantSubtag);
				}
				CheckVariantAndScriptRules();
				_id = _languageTag;
				IsChanged = true;
			}
		}

		public LanguageSubtag Language
		{
			get { return _language; }
			set
			{
				if (UpdateField(() => Language, ref _language, value))
					UpdateLanguageTag();
			}
		}

		public ScriptSubtag Script
		{
			get { return _script; }
			set
			{
				if (UpdateField(() => Script, ref _script, value))
				{
					CheckVariantAndScriptRules();
					UpdateLanguageTag();
				}
			}
		}

		public RegionSubtag Region
		{
			get { return _region; }
			set
			{
				if (UpdateField(() => Region, ref _region, value))
					UpdateLanguageTag();
			}
		}

		/// <summary>
		/// The variant and private use subtags.
		/// </summary>
		public ObservableCollection<VariantSubtag> Variants
		{
			get { return _variants; }
		}

		/// <summary>
		/// The desired abbreviation for the writing system
		/// </summary>
		public virtual string Abbreviation
		{
			get
			{
				if (String.IsNullOrEmpty(_abbreviation))
				{
					// Use the language subtag unless it's an unlisted language.
					// If it's an unlisted language, use the private use area language subtag.
					if (_language == WellKnownSubtags.UnlistedLanguage)
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
					return _language;
				}
				return _abbreviation;
			}
			set { UpdateString(() => Abbreviation, ref _abbreviation, value); }
		}

		/// <summary>
		/// The language name to use. Typically this is the language name associated with the BCP47 language subtag as defined by the IANA subtag registry
		/// </summary>
		public virtual string LanguageName
		{
			get
			{
				if (!string.IsNullOrEmpty(_languageName))
					return _languageName;
				if (_language != null && !string.IsNullOrEmpty(_language.Name))
					return _language.Name;
				return "Unknown Language";
			}
			set { UpdateString(() => LanguageName, ref _languageName, value); }
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
		public static WritingSystemDefinition CreateCopyWithUniqueId(WritingSystemDefinition writingSystemToCopy, IEnumerable<string> otherWritingsystemIds)
		{
			WritingSystemDefinition newWs = writingSystemToCopy.Clone();
			var lastAppended = string.Empty;
			int duplicateNumber = 0;
			string[] wsIds = otherWritingsystemIds.ToArray();
			while (wsIds.Any(id => id.Equals(newWs.Id, StringComparison.OrdinalIgnoreCase)))
			{
				if (!string.IsNullOrEmpty(lastAppended))
					newWs.RemoveVariants(lastAppended);
				string currentToAppend = string.Format("dupl{0}", duplicateNumber);
				if (!newWs._variants.Contains(currentToAppend))
				{
					newWs.Variants.Add(new VariantSubtag(currentToAppend, null, true, null));
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
		public string StoreID { get; set; }

		/// <summary>
		/// A automatically generated descriptive label for the writing system definition.
		/// </summary>
		public virtual string DisplayLabel
		{
			get
			{
				//jh (Oct 2010) made it start with RFC5646 because all ws's in a lang start with the
				//same abbreviation, making imppossible to see (in SOLID for example) which you chose.
				bool languageIsUnknown = LanguageTag.Equals(WellKnownSubtags.UnlistedLanguage, StringComparison.OrdinalIgnoreCase);
				if (!string.IsNullOrEmpty(LanguageTag) && !languageIsUnknown)
				{
					return LanguageTag;
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
		public virtual string ListLabel
		{
			get
			{
				//the idea here is to give writing systems a nice legible label for. For this reason subtags are replaced with nice labels
				var details = new StringBuilder();
				if (IpaStatus != IpaStatusChoices.NotIpa)
				{
					switch (IpaStatus)
					{
						case IpaStatusChoices.Ipa:
							details.Append("IPA-");
							break;
						case IpaStatusChoices.IpaPhonetic:
							details.Append("IPA-etic-");
							break;
						case IpaStatusChoices.IpaPhonemic:
							details.Append("IPA-emic-");
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				if (IsVoice)
					details.Append("Voice-");

				if (IsDuplicate)
				{
					foreach (string number in DuplicateNumbers)
					{
						details.Append("Copy");
						if (number != "0")
							details.Append(number);
						details.Append("-");
					}
				}

				if (_script != null && !IsVoice)
					details.AppendFormat("{0}-", _script.Code);
				if (_region != null)
					details.AppendFormat("{0}-", _region.Code);
				foreach (VariantSubtag variantSubtag in _variants.Where(v => !v.IsPrivateUse))
				{
					// skip variant tags that have already been added to the details
					if (variantSubtag == WellKnownSubtags.IpaVariant)
						continue;
					details.AppendFormat("{0}-", variantSubtag.Code);
				}

				bool first = true;
				foreach (VariantSubtag variantSubtag in _variants.Where(v => v.IsPrivateUse))
				{
					// skip variant tags that have already been added to the details
					if (variantSubtag == WellKnownSubtags.AudioPrivateUse
						|| variantSubtag == WellKnownSubtags.IpaPhonemicPrivateUse
						|| variantSubtag == WellKnownSubtags.IpaPhoneticPrivateUse
						|| variantSubtag.Code.StartsWith("dupl"))
					{
						continue;
					}

					if (first)
					{
						details.Append("x-");
						first = false;
					}
					details.AppendFormat("{0}-", variantSubtag.Code);
				}

				string name = !string.IsNullOrEmpty(LanguageName) ? LanguageName : DisplayLabel;
				if (details.Length > 0)
				{
					// remove trailing dash
					details.Remove(details.Length - 1, 1);
					return string.Format("{0} ({1})", name, details);
				}
				return name;
			}
		}

		/// <summary>
		/// Gets a value indicating whether this instance is a duplicate.
		/// </summary>
		private bool IsDuplicate
		{
			get { return _variants.Any(v => v.Code.StartsWith("dupl")); }
		}

		/// <summary>
		/// Gets the duplicate numbers.
		/// </summary>
		private IEnumerable<string> DuplicateNumbers
		{
			get
			{
				return _variants.Where(v => v.Code.StartsWith("dupl")).Select(v => Regex.Match(v.Code, @"\d*$").Value);
			}
		}


		/// <summary>
		/// The current IETF language tag which is a concatenation of the Language, Script, Region and Variant properties.
		/// </summary>
		public string LanguageTag
		{
			get { return _languageTag; }
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
				_id = value ?? string.Empty;
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
				return base.IsChanged || ChildrenIsChanged(_fonts) || ChildrenIsChanged(_spellCheckDictionaries)
					|| ChildrenIsChanged(_collations) || ChildrenIsChanged(_characterSets);
			}
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			ChildrenAcceptChanges(_fonts);
			ChildrenAcceptChanges(_spellCheckDictionaries);
			ChildrenAcceptChanges(_collations);
			ChildrenAcceptChanges(_characterSets);
		}

		public void ForceChanged()
		{
			IsChanged = true;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the writing system will be deleted.
		/// </summary>
		public bool MarkedForDeletion { get; set; }

		/// <summary>
		/// The preferred keyboard to use to generate data encoded in this writing system.
		/// </summary>
		public virtual string Keyboard
		{
			get { return _keyboard ?? string.Empty; }
			set { UpdateString(() => Keyboard, ref _keyboard, value); }
		}

		/// <summary>
		/// This field retrieves the value obtained from the FieldWorks LDML extension fw:windowsLCID.
		/// This is used only when current information in LocalKeyboard or KnownKeyboards is not useable.
		/// It is not useful to modify this or set it in new LDML files; however, we need a public setter
		/// because FieldWorks overrides the code that normally reads this from the LDML file.
		/// </summary>
		public virtual string WindowsLcid
		{
			get { return _windowsLcid ?? string.Empty; }
			set { UpdateString(() => WindowsLcid, ref _windowsLcid, value); }
		}

		/// <summary>
		/// Indicates whether this writing system is read and written from left to right or right to left
		/// </summary>
		public virtual bool RightToLeftScript
		{
			get { return _rightToLeftScript; }
			set { UpdateField(() => RightToLeftScript, ref _rightToLeftScript, value); }
		}

		/// <summary>
		/// The windows "NativeName" from the Culture class
		/// </summary>
		public virtual string NativeName
		{
			get { return _nativeName ?? string.Empty; }
			set { UpdateString(() => NativeName, ref _nativeName, value); }
		}

		public virtual CollationDefinition DefaultCollation
		{
			get { return _defaultCollation ?? _collations.FirstOrDefault(); }
			set
			{
				if (UpdateField(() => DefaultCollation, ref _defaultCollation, value))
				{
					if (value != null && !_collations.Contains(value))
						_collations.Add(value);
				}
			}
		}

		public ObservableKeyedCollection<string, CollationDefinition> Collations
		{
			get { return _collations; }
		}

		public ObservableKeyedCollection<string, CharacterSetDefinition> CharacterSets
		{
			get { return _characterSets; }
		}

		protected virtual void UpdateLanguageTag()
		{
			_languageTag = IetfLanguageTag.ToLanguageTag(_language, _script, _region, _variants, _requiresValidLanguageTag);
			_id = _languageTag;
		}

		/// <summary>
		/// Indicates whether this writing system is unicode encoded or legacy encoded
		/// </summary>
		public virtual bool IsUnicodeEncoded
		{
			get { return _isUnicodeEncoded; }
			set { UpdateField(() => IsUnicodeEncoded, ref _isUnicodeEncoded, value); }
		}

		/// <summary>
		/// This tracks the keyboard that should be used for this writing system on this computer.
		/// It is not shared with other users of the project.
		/// </summary>
		public virtual IKeyboardDefinition LocalKeyboard
		{
			get
			{
				IKeyboardDefinition keyboard = _localKeyboard;
				if (keyboard == null)
				{
					var available = new HashSet<IKeyboardDefinition>(WritingSystems.Keyboard.Controller.AllAvailableKeyboards);
					keyboard = _knownKeyboards.FirstOrDefault(available.Contains);
				}
				if (keyboard == null)
					keyboard = WritingSystems.Keyboard.Controller.DefaultForWritingSystem(this);
				return keyboard;
			}
			set
			{
				if (UpdateField(() => LocalKeyboard, ref _localKeyboard, value))
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
		/// Keyboards known to have been used with this writing system. Not all may be available on this system.
		/// Enhance: document (or add to this class?) a way of getting available keyboards.
		/// </summary>
		public ObservableKeyedCollection<string, IKeyboardDefinition> KnownKeyboards
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

		/// <summary>
		/// The font used to display data encoded in this writing system
		/// </summary>
		public virtual FontDefinition DefaultFont
		{
			get
			{
				FontDefinition font = _defaultFont;
				if (font == null)
					font = _fonts.FirstOrDefault(fd => fd.Roles.HasFlag(FontRoles.Default));
				if (font == null)
					font = _fonts.FirstOrDefault();
				return font;
			}
			set
			{
				if (UpdateField(() => DefaultFont, ref _defaultFont, value))
				{
					if (value != null && !_fonts.Contains(value))
						_fonts.Add(value);
				}
			}
		}

		public ObservableKeyedCollection<string, FontDefinition> Fonts
		{
			get { return _fonts; }
		}

		public virtual SpellCheckDictionaryDefinition SpellCheckDictionary
		{
			get { return _spellCheckDictionary ?? _spellCheckDictionaries.FirstOrDefault(); }
			set
			{
				if (UpdateField(() => SpellCheckDictionary, ref _spellCheckDictionary, value))
				{
					if (value != null && !_spellCheckDictionaries.Contains(value))
						_spellCheckDictionaries.Add(value);
				}
			}
		}

		public ObservableKeyedCollection<string, SpellCheckDictionaryDefinition> SpellCheckDictionaries
		{
			get { return _spellCheckDictionaries; }
		}

		public virtual string DefaultRegion
		{
			get { return _defaultRegion ?? string.Empty; }
			set { UpdateString(() => DefaultRegion, ref _defaultRegion, value); }
		}

		public ObservableSet<MatchedPair> MatchedPairs
		{
			get { return _matchedPairs; }
		}

		public ObservableSet<PunctuationPattern> PunctuationPatterns
		{
			get { return _punctuationPatterns; }
		}

		public ObservableCollection<QuotationMark> QuotationMarks
		{
			get { return _quotationMarks; }
		}

		public QuotationParagraphContinueType QuotationParagraphContinueType
		{
			get { return _quotationParagraphContinueType; }
			set { UpdateField(() => QuotationParagraphContinueType, ref _quotationParagraphContinueType, value); }
		}

		public override string ToString()
		{
			return _languageTag;
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
			if (_language != other._language)
				return false;
			if (_script != other._script)
				return false;
			if (_region != other._region)
				return false;
			if (!_variants.SequenceEqual(other._variants))
				return false;
			if (_languageName != other._languageName)
				return false;
			if (!_languageTag.Equals(other._languageTag))
				return false;
			if (Abbreviation != other.Abbreviation)
				return false;
			if (VersionNumber != other.VersionNumber)
				return false;
			if (VersionDescription != other.VersionDescription)
				return false;
			if (Keyboard != other.Keyboard)
				return false;
			if (NativeName != other.NativeName)
				return false;
			if (_id != other._id)
				return false;
			if (_isUnicodeEncoded != other._isUnicodeEncoded)
				return false;
			if (_dateModified != other._dateModified)
				return false;
			if (_rightToLeftScript != other._rightToLeftScript)
				return false;
			if (WindowsLcid != other.WindowsLcid)
				return false;
			if (DefaultRegion != other.DefaultRegion)
				return false;
			if (!_matchedPairs.SetEquals(other._matchedPairs))
				return false;
			if (!_punctuationPatterns.SetEquals(other._punctuationPatterns))
				return false;
			if (!_quotationMarks.SequenceEqual(other._quotationMarks))
				return false;
			if (_quotationParagraphContinueType != other._quotationParagraphContinueType)
				return false;
			if (_requiresValidLanguageTag != other._requiresValidLanguageTag)
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

			// collations
			if (_collations.Count != other._collations.Count)
				return false;
			for (int i = 0; i < _collations.Count; i++)
			{
				if (!_collations[i].ValueEquals(other._collations[i]))
					return false;
			}
			if (DefaultCollation == null)
			{
				if (other.DefaultCollation != null)
					return false;
			}
			else if (!DefaultCollation.ValueEquals(other.DefaultCollation))
			{
				return false;
			}

			// character sets
			if (_characterSets.Count != other._characterSets.Count)
				return false;
			for (int i = 0; i < _characterSets.Count; i++)
			{
				if (!_characterSets[i].ValueEquals(other._characterSets[i]))
					return false;
			}

			return true;
		}
	}
}