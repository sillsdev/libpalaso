using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SIL.Code;
using SIL.Extensions;
using SIL.Keyboarding;
using SIL.ObjectModel;

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
	/// This class stores the information used to define various writing system properties. The Language, Script, Region, and Variants
	/// properties conform to the subtags of the same name defined in BCP 47 (RFC 5646). It is worth noting that any of these properties
	/// can contain private use subtags. Regardless, a valid IETF language tag is generated.
	/// Furthermore the WritingSystemDefinition.WellknownSubtags class provides certain well defined Subtags that carry special meaning
	/// apart from the IANA subtag registry. In particular this class defines "qaa" as the default "unlisted language" language subtag.
	/// It should be used when there is no match for a language in the IANA subtag registry. Private use properties are "emic" and "etic"
	/// which mark phonemic and phonetic writing systems respectively. These must always be used in conjunction with the "fonipa" variant.
	/// Likewise "audio" marks a writing system as audio and must always be used in conjunction with script "Zxxx". Convenience methods
	/// are provided for Ipa and Audio properties as IpaStatus and IsVoice respectively.
	/// </summary>
	public class WritingSystemDefinition : DefinitionBase<WritingSystemDefinition>
	{
		private const int MinimumFontSize = 7;
		private const int DefaultSizeIfWeDontKnow = 10;

		private LanguageSubtag _language;
		private ScriptSubtag _script;
		private RegionSubtag _region;
		private readonly BulkObservableList<VariantSubtag> _variants;
		private string _abbreviation;
		private string _versionNumber;
		private string _versionDescription;
		private DateTime _dateModified;
		private float _defaultFontSize;
		private FontDefinition _defaultFont;
		private string _keyboard;
		private bool _rightToLeftScript;
		private IKeyboardDefinition _localKeyboard;
		private string _languageTag;
		private string _defaultRegion;
		private string _windowsLcid;
		private string _spellCheckingId;
		private string _defaultCollationType;
		private CollationDefinition _defaultCollation;
		private QuotationParagraphContinueType _quotationParagraphContinueType;
		private readonly KeyedBulkObservableList<string, FontDefinition> _fonts;
		private readonly KeyedBulkObservableList<string, IKeyboardDefinition> _knownKeyboards;
		private readonly KeyedBulkObservableList<SpellCheckDictionaryFormat, SpellCheckDictionaryDefinition> _spellCheckDictionaries;
		private readonly KeyedBulkObservableList<string, CollationDefinition> _collations;
		private readonly ObservableHashSet<MatchedPair> _matchedPairs;
		private readonly ObservableHashSet<PunctuationPattern> _punctuationPatterns;
		private readonly BulkObservableList<QuotationMark> _quotationMarks;
		private readonly KeyedBulkObservableList<string, CharacterSetDefinition> _characterSets;
		private NumberingSystemDefinition _numberingSystem;
		private readonly SimpleMonitor _ignoreVariantChanges = new SimpleMonitor();
		private string _legacyMapping;
		private bool _isGraphiteEnabled = true;

		/// <summary>
		/// Creates a new WritingSystemDefinition with language tag set to "qaa".
		/// </summary>
		public WritingSystemDefinition()
			: this(WellKnownSubtags.UnlistedLanguage)
		{
		}

		public WritingSystemDefinition(string language, string script, string region, string variant)
			: this(IetfLanguageTag.Create(language, script, region, variant))
		{
		}

		public WritingSystemDefinition(string language, string script, string region, string variant, string abbreviation, bool rightToLeftScript)
			: this(IetfLanguageTag.Create(language, script, region, variant))
		{
			_abbreviation = abbreviation;
			_rightToLeftScript = rightToLeftScript;
		}

		/// <summary>
		/// Creates a new WritingSystemDefinition by parsing a valid IETF language tag.
		/// </summary>
		public WritingSystemDefinition(string languageTag)
		{
			if (!IetfLanguageTag.IsValid(languageTag))
				throw new ArgumentException("The language tag is invalid.", languageTag);
			_numberingSystem = NumberingSystemDefinition.Default;
			_languageTag = IetfLanguageTag.Canonicalize(languageTag);
			IEnumerable<VariantSubtag> variantSubtags;
			IetfLanguageTag.TryGetSubtags(_languageTag, out _language, out _script, out _region, out variantSubtags);
			_variants = new BulkObservableList<VariantSubtag>(variantSubtags);
			string message;
			if (!ValidateLanguageTag(out message))
				throw new ArgumentException(message, "languageTag");
			_fonts = new KeyedBulkObservableList<string, FontDefinition>(fd => fd.Name, StringComparer.InvariantCultureIgnoreCase);
			_knownKeyboards = new KeyedBulkObservableList<string, IKeyboardDefinition>(kd => kd.Id);
			_spellCheckDictionaries = new KeyedBulkObservableList<SpellCheckDictionaryFormat, SpellCheckDictionaryDefinition>(scdd => scdd.Format);
			_collations = new KeyedBulkObservableList<string, CollationDefinition>(cd => cd.Type);
			_matchedPairs = new ObservableHashSet<MatchedPair>();
			_punctuationPatterns = new ObservableHashSet<PunctuationPattern>();
			_quotationMarks = new BulkObservableList<QuotationMark>();
			_characterSets = new KeyedBulkObservableList<string, CharacterSetDefinition>(csd => csd.Type);
			SetupCollectionChangeListeners();
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		public WritingSystemDefinition(WritingSystemDefinition ws, bool cloneId = false)
		{
			if (cloneId)
				Id = ws.Id;

			_language = ws._language;
			_script = ws._script;
			_region = ws._region;
			_variants = new BulkObservableList<VariantSubtag>(ws._variants);
			_abbreviation = ws._abbreviation;
			_rightToLeftScript = ws._rightToLeftScript;
			_fonts = new KeyedBulkObservableList<string, FontDefinition>(ws._fonts.CloneItems(), fd => fd.Name, StringComparer.InvariantCultureIgnoreCase);
			if (ws._defaultFont != null)
				_defaultFont = _fonts[ws._fonts.IndexOf(ws._defaultFont)];
			_keyboard = ws._keyboard;
			_versionNumber = ws._versionNumber;
			_versionDescription = ws._versionDescription;
			_spellCheckingId = ws._spellCheckingId;
			_spellCheckDictionaries = new KeyedBulkObservableList<SpellCheckDictionaryFormat, SpellCheckDictionaryDefinition>(ws._spellCheckDictionaries.CloneItems(), scdd => scdd.Format);
			_dateModified = ws._dateModified;
			_localKeyboard = ws._localKeyboard;
			_windowsLcid = ws._windowsLcid;
			_defaultRegion = ws._defaultRegion;
			_defaultFontSize = ws._defaultFontSize;
			_knownKeyboards = new KeyedBulkObservableList<string, IKeyboardDefinition>(ws._knownKeyboards, kd => kd.Id);
			_matchedPairs = new ObservableHashSet<MatchedPair>(ws._matchedPairs);
			_punctuationPatterns = new ObservableHashSet<PunctuationPattern>(ws._punctuationPatterns);
			_quotationMarks = new BulkObservableList<QuotationMark>(ws._quotationMarks);
			_quotationParagraphContinueType = ws._quotationParagraphContinueType;
			_languageTag = ws._languageTag;
			_defaultCollationType = ws._defaultCollationType;
			_collations = new KeyedBulkObservableList<string, CollationDefinition>(ws._collations.CloneItems(), cd => cd.Type);
			if (ws._defaultCollation != null)
				_defaultCollation = _collations[ws._collations.IndexOf(ws._defaultCollation)];
			_characterSets = new KeyedBulkObservableList<string, CharacterSetDefinition>(ws._characterSets.CloneItems(), csd => csd.Type);
			_numberingSystem = new NumberingSystemDefinition(ws.NumberingSystem);
			_isGraphiteEnabled = ws._isGraphiteEnabled;
			_legacyMapping = ws._legacyMapping;
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
			_characterSets.CollectionChanged += _characterSets_CollectionChanged;
		}

		private void _characterSets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		private void _variants_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_ignoreVariantChanges.Busy)
				return;

			// if the variant codes haven't changed, then no need to update language tag
			if (e.Action != NotifyCollectionChangedAction.Replace
			    || !e.OldItems.Cast<VariantSubtag>().Select(v => v.Code).SequenceEqual(e.NewItems.Cast<VariantSubtag>().Select(v => v.Code)))
			{
				UpdateLanguageTag();
			}

			IsChanged = true;
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
			set { Set(() => VersionNumber, ref _versionNumber, value); }
		}

		/// <summary>
		/// Gets or sets the version description.
		/// </summary>
		public string VersionDescription
		{
			get { return _versionDescription ?? string.Empty; }
			set { Set(() => VersionDescription, ref _versionDescription, value); }
		}

		/// <summary>
		/// Gets or sets the date modified.
		/// </summary>
		public DateTime DateModified
		{
			get { return _dateModified; }
			set { _dateModified = value; }
		}

		private static int GetIndexOfFirstPrivateUseVariant(IEnumerable<VariantSubtag> variantSubtags)
		{
			int i = 0;
			foreach (VariantSubtag variantSubtag in variantSubtags)
			{
				if (variantSubtag.IsPrivateUse)
					break;
				i++;
			}
			return i;
		}

		/// <summary>
		/// Adjusts the IETF language tag to indicate the desired form of Ipa by inserting fonipa in the variant and emic or etic in private use where necessary.
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
					using (_variants.BulkUpdate())
					{
						RemoveVariants(WellKnownSubtags.IpaVariant, WellKnownSubtags.IpaPhonemicPrivateUse, WellKnownSubtags.IpaPhoneticPrivateUse, WellKnownSubtags.AudioPrivateUse);

						int index = GetIndexOfFirstPrivateUseVariant(_variants);

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
		/// Adjusts the IETF language tag to indicate that this is an "audio writing system" by inserting "audio" in the private use and "Zxxx" in the script
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
					IpaStatus = IpaStatusChoices.NotIpa;
					Keyboard = string.Empty;
					Set(() => Script, ref _script, WellKnownSubtags.AudioScript);
					_variants.Add(WellKnownSubtags.AudioPrivateUse);
				}
				else
				{
					Set(() => Script, ref _script, null);
					RemoveVariants(WellKnownSubtags.AudioPrivateUse);
				}
			}
		}

		private bool ScriptSubTagIsAudio
		{
			get { return _script != null && _script.Code.Equals(WellKnownSubtags.AudioScript, StringComparison.OrdinalIgnoreCase); }
		}

		public bool ValidateLanguageTag(out string message)
		{
			if (!IetfLanguageTag.Validate(_language, _script, _region, _variants, out message))
				return false;

			if (_variants.Contains(WellKnownSubtags.AudioPrivateUse) && !ScriptSubTagIsAudio)
			{
				message = "The script subtag must be set to Zxxx when the variant tag indicates an audio writing system.";
				return false;
			}
			bool rfcTagHasAnyIpa = _variants.Contains(WellKnownSubtags.IpaVariant)
				|| _variants.Contains(WellKnownSubtags.IpaPhonemicPrivateUse) || _variants.Contains(WellKnownSubtags.IpaPhoneticPrivateUse);
			if (_variants.Contains(WellKnownSubtags.AudioPrivateUse) && rfcTagHasAnyIpa)
			{
				message = "A writing system may not be marked as audio and ipa at the same time.";
				return false;
			}
			if ((_variants.Contains(WellKnownSubtags.IpaPhonemicPrivateUse) || _variants.Contains(WellKnownSubtags.IpaPhoneticPrivateUse))
				&& !_variants.Contains(WellKnownSubtags.IpaVariant))
			{
				message = "A writing system may not be marked as phonetic (x-etic) or phonemic (x-emic) and lack the variant marker fonipa.";
				return false;
			}

			message = null;
			return true;
		}

		/// <summary>
		/// Gets or sets the language.
		/// </summary>
		public LanguageSubtag Language
		{
			get { return _language; }
			set
			{
				string oldCode = _language == null ? string.Empty : _language.Code;
				Set(() => Language, ref _language, value);
				if (oldCode != (_language == null ? string.Empty : _language.Code))
					UpdateLanguageTag();
			}
		}

		/// <summary>
		/// Gets or sets the script. If the language tag for the writing system has an implicit script,
		/// this property will return the implicit script.
		/// </summary>
		public ScriptSubtag Script
		{
			get { return _script; }
			set
			{
				string oldCode = _script == null ? string.Empty : _script.Code;
				Set(() => Script, ref _script, value);
				if (oldCode != (_script == null ? string.Empty : _script.Code))
					UpdateLanguageTag();
			}
		}

		/// <summary>
		/// Gets or sets the region.
		/// </summary>
		public RegionSubtag Region
		{
			get { return _region; }
			set
			{
				string oldCode = _region == null ? string.Empty : _region.Code;
				Set(() => Region, ref _region, value);
				if (oldCode != (_region == null ? string.Empty : _region.Code))
					UpdateLanguageTag();
			}
		}

		/// <summary>
		/// The variant and private use subtags.
		/// </summary>
		public BulkObservableList<VariantSubtag> Variants
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
					if (_language == null || _language == WellKnownSubtags.UnlistedLanguage)
					{
						int idx = LanguageTag.IndexOf("x-", StringComparison.Ordinal);
						if (idx > 0 && LanguageTag.Length > idx + 2)
						{
							var abbr = LanguageTag.Substring(idx + 2);
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
			set { Set(() => Abbreviation, ref _abbreviation, value); }
		}

		/// <summary>
		/// Used by IWritingSystemRepository to identify writing systems. This is an IETF language tag, but does not necessarily
		/// correspond to the writing system's current language tag. Id and LanguageTag can be different if the language
		/// tag has changed, but IWritingSystemRepository.Set() hasn't been called on it yet. Only change this if you would like
		/// to replace a writing system with the same Id already contained in the repo. This is useful creating a temporary copy
		/// of a writing system that you may or may not care to persist to the IWritingSystemRepository.
		/// Typical use would therefore be:
		/// ws.Clone(wsorig);
		/// ws.Id = wsOrig.Id;
		/// **make changes to ws**
		/// repo.Set(ws);
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// A automatically generated descriptive label for the writing system definition.
		/// </summary>
		public virtual string DisplayLabel
		{
			get
			{
				//jh (Oct 2010) made it start with RFC5646 because all ws's in a lang start with the
				//same abbreviation, making imppossible to see (in SOLID for example) which you chose.
				bool languageIsUnknown = _languageTag.Equals(WellKnownSubtags.UnlistedLanguage, StringComparison.OrdinalIgnoreCase);
				if (!string.IsNullOrEmpty(_languageTag) && !languageIsUnknown)
				{
					return _languageTag;
				}
				if (languageIsUnknown)
				{
					if (!string.IsNullOrEmpty(_abbreviation))
					{
						return _abbreviation;
					}
					if (_language != null && _language != WellKnownSubtags.UnlistedLanguage && !string.IsNullOrEmpty(_language.Name))
					{
						string n = _language.Name;
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

				if (_script != null && !IsVoice && !IetfLanguageTag.IsScriptImplied(_languageTag))
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

				string name = _language != null && !string.IsNullOrEmpty(_language.Name) ? _language.Name : DisplayLabel;
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
		/// The IETF language tag for this writing system.
		/// </summary>
		public string LanguageTag
		{
			get { return _languageTag; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");
				if (!IetfLanguageTag.IsValid(value))
					throw new ArgumentException("The language tag is invalid.", "value");

				string newLangTag = IetfLanguageTag.Canonicalize(value);
				if (!newLangTag.Equals(_languageTag, StringComparison.InvariantCultureIgnoreCase))
				{
					LanguageSubtag language;
					ScriptSubtag script;
					RegionSubtag region;
					IEnumerable<VariantSubtag> variantSubtags;
					IetfLanguageTag.TryGetSubtags(newLangTag, out language, out script, out region, out variantSubtags);
					Set(() => Language, ref _language, language);
					Set(() => Script, ref _script, script);
					Set(() => Region, ref _region, region);
					using (_ignoreVariantChanges.Enter())
						_variants.ReplaceAll(variantSubtags);
					string message;
					if (!ValidateLanguageTag(out message))
						throw new ArgumentException(message, "value");
					Set(() => LanguageTag, ref _languageTag, newLangTag);
				}
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
			set { Set(() => Keyboard, ref _keyboard, value); }
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
			set { Set(() => WindowsLcid, ref _windowsLcid, value); }
		}

		/// <summary>
		/// Indicates whether this writing system is read and written from left to right or right to left
		/// </summary>
		public virtual bool RightToLeftScript
		{
			get { return _rightToLeftScript; }
			set { Set(() => RightToLeftScript, ref _rightToLeftScript, value); }
		}

		public virtual string DefaultCollationType
		{
			get { return _defaultCollationType ?? "standard"; }
			set { Set(() => DefaultCollationType, ref _defaultCollationType, value); }
		}

		public virtual CollationDefinition DefaultCollation
		{
			get
			{
				if (_defaultCollation != null)
					return _defaultCollation;
				CollationDefinition cd;
				if (_collations.TryGet(DefaultCollationType, out cd))
					return cd;
				return _collations.FirstOrDefault();
			}
			set
			{
				if (Set(() => DefaultCollation, ref _defaultCollation, value) && value != null)
				{
					CollationDefinition cd;
					if (_collations.TryGet(value.Type, out cd))
					{
						if (cd == value)
							return;

						// if a collation with the same type already exists, replace it
						int index = _collations.IndexOf(cd);
						_collations[index] = value;
					}
					else
					{
						_collations.Add(value);
					}
				}
			}
		}

		public KeyedBulkObservableList<string, CollationDefinition> Collations
		{
			get { return _collations; }
		}

		public KeyedBulkObservableList<string, CharacterSetDefinition> CharacterSets
		{
			get { return _characterSets; }
		}

		public NumberingSystemDefinition NumberingSystem
		{
			get => _numberingSystem;
			set { Set(() => NumberingSystem, ref _numberingSystem, value); }
		}

		protected virtual void UpdateLanguageTag()
		{
			if (_language == null && (_script != null || _region != null || _variants.Any(v => !v.IsPrivateUse)))
				Set(() => Language, ref _language, WellKnownSubtags.UnlistedLanguage);
			Set(() => LanguageTag, ref _languageTag, IetfLanguageTag.Create(_language, _script, _region, _variants, false));

			if (_script == null && IetfLanguageTag.IsValid(_languageTag) && IetfLanguageTag.IsScriptImplied(_languageTag))
				Set(() => Script, ref _script, IetfLanguageTag.GetScriptSubtag(_languageTag));
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
					keyboard = _knownKeyboards.FirstOrDefault(k => k.IsAvailable);
				if (keyboard == null)
					keyboard = LegacyKeyboard ?? Keyboarding.Keyboard.Controller.DefaultKeyboard;
				return keyboard;
			}
			set
			{
				if (Set(() => LocalKeyboard, ref _localKeyboard, value) && value != null && !_knownKeyboards.Contains(value.Id))
					_knownKeyboards.Add(value);
			}
		}

		/// <summary>
		/// Finds a keyboard specified using one of the legacy fields. If such a keyboard is found, it is appropriate to
		/// automatically add it to KnownKeyboards. If one is not, a general DefaultKeyboard should NOT be added.
		/// </summary>
		public IKeyboardDefinition LegacyKeyboard
		{
			get
			{
				if (!string.IsNullOrEmpty(WindowsLcid))
				{
					IKeyboardDefinition keyboard = GetFWLegacyKeyboard();
					if (keyboard != null)
						return keyboard;
				}

				if (!string.IsNullOrEmpty(Keyboard))
				{
					IKeyboardDefinition keyboard = GetPalasoLegacyKeyboard();
					if (keyboard != null)
						return keyboard;
				}

				return null;
			}
		}

		private IKeyboardDefinition GetFWLegacyKeyboard()
		{
			int lcid;
			if (int.TryParse(WindowsLcid, out lcid))
			{
				if (string.IsNullOrEmpty(Keyboard))
				{
					// FW system keyboard
					IKeyboardDefinition keyboard;
					if (Keyboarding.Keyboard.Controller.TryGetKeyboard(lcid, out keyboard))
						return keyboard;
				}
				else
				{
					try
					{
						// FW keyman keyboard
						var culture = new CultureInfo(lcid);
						IKeyboardDefinition keyboard;
						if (Keyboarding.Keyboard.Controller.TryGetKeyboard(Keyboard, culture.Name, out keyboard))
							return keyboard;
					}
					catch (CultureNotFoundException)
					{
						// Culture specified by LCID is not supported on current system. Just ignore.
					}
				}
			}
			return null;
		}

		private IKeyboardDefinition GetPalasoLegacyKeyboard()
		{
			IKeyboardDefinition keyboard;
			if (Keyboarding.Keyboard.Controller.TryGetKeyboard(Keyboard, out keyboard))
				return keyboard;

			// Palaso WinIME keyboard
			string locale = GetLocaleName(Keyboard);
			string layout = GetLayoutName(Keyboard);
			if (Keyboarding.Keyboard.Controller.TryGetKeyboard(layout, locale, out keyboard))
				return keyboard;

			// Palaso Keyman or Ibus keyboard
			if (Keyboarding.Keyboard.Controller.TryGetKeyboard(layout, out keyboard))
				return keyboard;

			return null;
		}

		private static string GetLocaleName(string name)
		{
			var split = name.Split(new[] { '-' });
			string localeName;
			if (split.Length <= 1)
			{
				localeName = string.Empty;
			}
			else if (split.Length > 1 && split.Length <= 3)
			{
				localeName = string.Join("-", split.Skip(1).ToArray());
			}
			else
			{
				localeName = string.Join("-", split.Skip(split.Length - 2).ToArray());
			}
			return localeName;
		}

		private static string GetLayoutName(string name)
		{
			//Just cut off the length of the locale + 1 for the dash
			var locale = GetLocaleName(name);
			if (string.IsNullOrEmpty(locale))
			{
				return name;
			}
			var layoutName = name.Substring(0, name.Length - (locale.Length + 1));
			return layoutName;
		}

		public IKeyboardDefinition RawLocalKeyboard
		{
			get { return _localKeyboard; }
		}

		/// <summary>
		/// Keyboards known to have been used with this writing system. Not all may be available on this system.
		/// Enhance: document (or add to this class?) a way of getting available keyboards.
		/// </summary>
		public KeyedBulkObservableList<string, IKeyboardDefinition> KnownKeyboards
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
				return Keyboarding.Keyboard.Controller.AvailableKeyboards.Except(KnownKeyboards);
			}
		}

		/// <summary>
		/// the preferred font size to use for data encoded in this writing system.
		/// </summary>
		public virtual float DefaultFontSize
		{
			get { return _defaultFontSize; }
			set
			{
				if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
					throw new ArgumentOutOfRangeException("value");

				Set(() => DefaultFontSize, ref _defaultFontSize, value);
			}
		}

		/// <summary>
		/// enforcing a minimum on _defaultFontSize, while reasonable, just messed up too many IO unit tests
		/// </summary>
		/// <returns></returns>
		public virtual float GetDefaultFontSizeOrMinimum()
		{
			if (_defaultFontSize < MinimumFontSize)
				return DefaultSizeIfWeDontKnow;
			return _defaultFontSize;
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
				if (Set(() => DefaultFont, ref _defaultFont, value) && value != null)
				{
					FontDefinition fd;
					if (_fonts.TryGet(value.Name, out fd))
					{
						if (fd == value)
							return;

						// if a font with the same name already exists, replace it
						int index = _fonts.IndexOf(fd);
						_fonts[index] = value;
					}
					else
					{
						_fonts.Add(value);
					}
				}
			}
		}

		public KeyedBulkObservableList<string, FontDefinition> Fonts
		{
			get { return _fonts; }
		}

		/// <summary>
		/// The Id used to select the spell checker.
		/// </summary>
		public virtual string SpellCheckingId
		{
			get { return _spellCheckingId ?? string.Empty; }
			set { Set(() => SpellCheckingId, ref _spellCheckingId, value); }
		}

		public KeyedBulkObservableList<SpellCheckDictionaryFormat, SpellCheckDictionaryDefinition> SpellCheckDictionaries
		{
			get { return _spellCheckDictionaries; }
		}

		public virtual string DefaultRegion
		{
			get { return _defaultRegion ?? string.Empty; }
			set { Set(() => DefaultRegion, ref _defaultRegion, value); }
		}

		public IObservableSet<MatchedPair> MatchedPairs
		{
			get { return _matchedPairs; }
		}

		public IObservableSet<PunctuationPattern> PunctuationPatterns
		{
			get { return _punctuationPatterns; }
		}

		public BulkObservableList<QuotationMark> QuotationMarks
		{
			get { return _quotationMarks; }
		}

		public QuotationParagraphContinueType QuotationParagraphContinueType
		{
			get { return _quotationParagraphContinueType; }
			set { Set(() => QuotationParagraphContinueType, ref _quotationParagraphContinueType, value); }
		}

		/// <summary>
		/// Gets or sets the legacy mapping.
		/// </summary>
		/// <value>The legacy mapping.</value>
		public string LegacyMapping
		{
			get { return _legacyMapping ?? string.Empty; }
			set { Set(() => LegacyMapping, ref _legacyMapping, value); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether Graphite is enabled for this writing system.
		/// </summary>
		/// <value><c>true</c> if Graphite is enabled, otherwise <c>false</c>.</value>
		public bool IsGraphiteEnabled
		{
			get { return _isGraphiteEnabled; }
			set { Set(() => IsGraphiteEnabled, ref _isGraphiteEnabled, value); }
		}

		/// <summary>
		/// Gets or sets the template that was used to create this writing system.
		/// This is not persisted and only used by repositories.
		/// </summary>
		public string Template { get; set; }

		public override string ToString()
		{
			return _languageTag;
		}

		/// <summary>
		/// Creates a clone of the current writing system.
		/// Note that this excludes the properties: Modified, MarkedForDeletion and Id
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
			if (Abbreviation != other.Abbreviation)
				return false;
			if (VersionNumber != other.VersionNumber)
				return false;
			if (VersionDescription != other.VersionDescription)
				return false;
			if (Keyboard != other.Keyboard)
				return false;
			if (_languageTag != other._languageTag)
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
			if (_isGraphiteEnabled != other._isGraphiteEnabled)
				return false;
			if (LegacyMapping != other.LegacyMapping)
				return false;
			if (SpellCheckingId != other.SpellCheckingId)
				return false;
			if (_defaultFontSize != other._defaultFontSize)
				return false;
			if (DefaultCollationType != other.DefaultCollationType)
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
			foreach (CharacterSetDefinition csd in _characterSets)
			{
				CharacterSetDefinition otherCsd;
				if (!other._characterSets.TryGet(csd.Type, out otherCsd) || !csd.ValueEquals(otherCsd))
					return false;
			}
			if (!_numberingSystem.ValueEquals(other._numberingSystem))
				return false;

			return true;
		}
	}
}