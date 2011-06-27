using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Palaso.Code;
using Palaso.WritingSystems.Collation;

namespace Palaso.WritingSystems
{
	/// <summary>
	/// This class stores the information used to define various writing system properties.
	/// </summary>
	public class WritingSystemDefinition
	{
		public enum SortRulesType
		{
			/// <summary>
			/// Default Unicode ordering rules (actually CustomICU without any rules)
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
			CustomICU,
			/// <summary>
			/// Use the sort rules from another language. When this is set, the SortRules are interpreted as a cultureId for the language to sort like.
			/// </summary>
			[Description("Same as another language")]
			OtherLanguage
		}

		//This is the version of our writingsystemDefinition implementation and is mostly used for migration purposes.
		//This should not be confused with the version of the locale data contained in this writing system.
		//That information is stored in the "VersionNumber" property.
		public const int LatestWritingSystemDefinitionVersion = 1;

		private RFC5646Tag _rfcTag;

		private string _languageName;

		private string _abbreviation;
		private bool _isUnicodeEncoded;

		private string _versionNumber;
		private string _versionDescription;

		private DateTime _dateModified;

		private string _defaultFontName;
		private float _defaultFontSize;
		private string _keyboard;

		private SortRulesType _sortUsing;
		private string _sortRules;
		private string _spellCheckingId;

		private string _nativeName;
		private bool _rightToLeftScript;
		private ICollator _collator;

		public WritingSystemDefinition()
		{
			_sortUsing = SortRulesType.DefaultOrdering;
			_isUnicodeEncoded = true;
			_rfcTag = new RFC5646Tag();
		}

		public WritingSystemDefinition(string rfctag)
			: this()
		{
			_rfcTag = RFC5646Tag.Parse(rfctag);
			_abbreviation = _languageName = _nativeName = string.Empty;
		}

		public WritingSystemDefinition(string language, string script, string region, string variant, string abbreviation, bool rightToLeftScript)
			: this()
		{
			string variantPart;
			string privateUsePart;
			SplitVariantAndPrivateUse(variant, out variantPart, out privateUsePart);
			_rfcTag = new RFC5646Tag(language, script, region, variantPart, privateUsePart);

			_abbreviation = abbreviation;
			_rightToLeftScript = rightToLeftScript;
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="ws">The ws.</param>
		public WritingSystemDefinition(WritingSystemDefinition ws)
		{
			_abbreviation = ws._abbreviation;
			_rightToLeftScript = ws._rightToLeftScript;
			_defaultFontName = ws._defaultFontName;
			_defaultFontSize = ws._defaultFontSize;
			_keyboard = ws._keyboard;
			_versionNumber = ws._versionNumber;
			_versionDescription = ws._versionDescription;
			_nativeName = ws._nativeName;
			_sortUsing = ws._sortUsing;
			_sortRules = ws._sortRules;
			_spellCheckingId = ws._spellCheckingId;
			_dateModified = ws._dateModified;
			_isUnicodeEncoded = ws._isUnicodeEncoded;
			_rfcTag = new RFC5646Tag(ws._rfcTag);
			_languageName = ws._languageName;
		}

		/// <summary>
		/// Provides a list of ISO639 language codes.  Uses ISO639 639-1 and 639-3 where ISO639 639-1 is not available.
		/// </summary>
		// TODO Move this to some other class that provides WritingSystemResources? Info?
		public static IList<Iso639LanguageCode> ValidIso639LanguageCodes
		{
			get
			{
				return StandardTags.ValidIso639LanguageCodes;
			}
		}

		//This is the version of the locale data contained in this writing system.
		//This should not be confused with the version of our writingsystemDefinition implementation which is mostly used for migration purposes.
		//That information is stored in the "LatestWritingSystemDefinitionVersion" property.
		virtual public string VersionNumber
		{
			get { return _versionNumber; }
			set { UpdateString(ref _versionNumber, value); }
		}

		virtual public string VersionDescription
		{
			get { return _versionDescription; }
			set { UpdateString(ref _versionDescription, value); }
		}

		virtual public DateTime DateModified
		{
			get { return _dateModified; }
			set { _dateModified = value; }
		}


		/// <summary>
		/// Note: this treats the etic and emic extensions as if they were variants, which we can get
		/// away with for now, but maybe not if this class grows to be extension aware.
		/// Ideally, these should be suffixes rather than private use
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
					_rfcTag.Language = WellKnownSubTags.Unlisted.Language;
				}
				_rfcTag.RemoveFromPrivateUse(WellKnownSubTags.Audio.PrivateUseSubtag);
				/* "There are some variant subtags that have no prefix field,
				 * eg. fonipa (International IpaPhonetic Alphabet). Such variants
				 * should appear after any other variant subtags with prefix information."
				 */
				_rfcTag.RemoveFromPrivateUse("x-etic");
				_rfcTag.RemoveFromPrivateUse("x-emic");
				_rfcTag.RemoveFromVariant("fonipa");

				switch (value)
				{
					default:
						break;
					case IpaStatusChoices.Ipa:
						_rfcTag.AddToVariant(WellKnownSubTags.Ipa.VariantSubtag);
						break;
					case IpaStatusChoices.IpaPhonemic:
						_rfcTag.AddToVariant(WellKnownSubTags.Ipa.VariantSubtag);
						_rfcTag.AddToPrivateUse(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag);
						break;
					case IpaStatusChoices.IpaPhonetic:
						_rfcTag.AddToVariant(WellKnownSubTags.Ipa.VariantSubtag);
						_rfcTag.AddToPrivateUse(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag);
						break;
				}
				Modified = true;
			}
		}

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
					if(ISO639 == "")
					{
						ISO639 = WellKnownSubTags.Unlisted.Language;
					}
					Script = WellKnownSubTags.Audio.Script;
					_rfcTag.AddToPrivateUse(WellKnownSubTags.Audio.PrivateUseSubtag);
				}
				else
				{
					_rfcTag.Script = String.Empty;
					_rfcTag.RemoveFromPrivateUse(WellKnownSubTags.Audio.PrivateUseSubtag);
				}
				Modified = true;
				CheckVariantAndScriptRules();
			}
		}

		private bool VariantSubTagIsAudio
		{
			get
			{
				return _rfcTag.PrivateUseContains(WellKnownSubTags.Audio.PrivateUseSubtag);
			}
		}

		private bool ScriptSubTagIsAudio
		{
			get { return _rfcTag.Script.Equals(WellKnownSubTags.Audio.Script,StringComparison.OrdinalIgnoreCase); }
		}

		/// <summary>
		/// Todo: this could/should become an ordered list of variant tags
		/// </summary>
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

				Modified = true;
				CheckVariantAndScriptRules();
			}
		}

		public void AddToVariant(string tag)
		{
			if (StandardTags.IsValidRegisteredVariant(tag))
			{
				_rfcTag.AddToVariant(tag);
			}
			else
			{
				_rfcTag.AddToPrivateUse(tag);
			}
			CheckVariantAndScriptRules();
		}

		public void AddToPrivateUse(string tag)
		{
			_rfcTag.AddToPrivateUse(tag);
			CheckVariantAndScriptRules();
		}

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

		public static string ConcatenateVariantAndPrivateUse(string variant, string privateUse)
		{
			if(String.IsNullOrEmpty(privateUse))
			{
				return variant;
			}
			if(!privateUse.StartsWith("x-", StringComparison.OrdinalIgnoreCase))
			{
				privateUse = String.Concat("x-", privateUse);
			}

			string variantToReturn = variant;
			if (!String.IsNullOrEmpty(privateUse))
			{
				if (!String.IsNullOrEmpty(variantToReturn))
				{
					variantToReturn += "-";
				}
				variantToReturn += privateUse;
			}
			return variantToReturn;
		}



		private void CheckVariantAndScriptRules()
		{
			if (VariantSubTagIsAudio && !ScriptSubTagIsAudio)
			{
				throw new ArgumentException("The script subtag must be set to " + WellKnownSubTags.Audio.Script + " when the variant tag indicates an audio writing system.");
			}
			bool rfcTagHasAnyIpa = VariantSubTagIsIpaConform ||
									_rfcTag.PrivateUseContains(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag) ||
									_rfcTag.PrivateUseContains(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag);
			if (VariantSubTagIsAudio && rfcTagHasAnyIpa)
			{
				throw new ArgumentException("A writing system may not be marked as audio and ipa at the same time.");
			}
			if((_rfcTag.PrivateUseContains(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag)  ||
				_rfcTag.PrivateUseContains(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag)) &&
				!VariantSubTagIsIpaConform)
			{
				throw new ArgumentException("A writing system may not be marked as phonetic (x-etic) or phonemic (x-emic) and lack the variant marker fonipa.");
			}
		}

		private bool VariantSubTagIsIpaConform
		{
			get
			{
				return _rfcTag.VariantContains(WellKnownSubTags.Ipa.VariantSubtag);
			}
		}

		private bool Rfc5646TagIsPhoneticConform
		{
			get
			{
				return  _rfcTag.VariantContains(WellKnownSubTags.Ipa.VariantSubtag) &&
					_rfcTag.PrivateUseContains(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag);
			}
		}

		private bool Rfc5646TagIsPhonemicConform
		{
			get
			{
				return _rfcTag.VariantContains(WellKnownSubTags.Ipa.VariantSubtag) &&
					_rfcTag.PrivateUseContains(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag);
			}
		}

		public void SetAllRfc5646LanguageTagComponents(string language, string script, string region, string variant)
		{
			string variantPart;
			string privateUsePart;
			SplitVariantAndPrivateUse(variant, out variantPart, out privateUsePart);
			_rfcTag = new RFC5646Tag(language, script, region, variantPart, privateUsePart);
			CheckVariantAndScriptRules();
		}

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
				Modified = true;
			}
		}

		/// <summary>
		/// The ISO639-639 code which is also the Ethnologue code.
		/// </summary>
		[Obsolete("Please use ISO639")]
		virtual public string ISO
		{
			get { return ISO639; }
			set { ISO639 = value; }
		}

		/// <summary>
		/// The ISO-639 code which is also the Ethnologue code.
		/// </summary>
		virtual public string ISO639
		{
			get
			{
				return _rfcTag.Language;
			}
			set
			{
				value = value ?? "";
				if (value == ISO639)
				{
					return;
				}
				_rfcTag.Language = value;
				Modified = true;
			}
		}

		virtual public string Abbreviation
		{
			get
			{
				return String.IsNullOrEmpty(_abbreviation) ? ISO639 : _abbreviation;
			}
			set
			{
				UpdateString(ref _abbreviation, value);
			}
		}

		virtual public string Script
		{
			get
			{
				return _rfcTag.Script;
			}
			set
			{
				value = value ?? "";
				if (value == Script)
				{
					return;
				}
				_rfcTag.Script = value;
				Modified = true;
				CheckVariantAndScriptRules();
			}
		}

		virtual public string LanguageName
		{
			get
			{
				if (!String.IsNullOrEmpty(_languageName))
				{
					return _languageName;
				}
				var code = ValidIso639LanguageCodes.FirstOrDefault(c => c.Code.Equals(ISO639));
				if (code != null)
				{
					return code.Name;
				}
				return "Unknown Language";

				// TODO Make the below work.
				//return StandardTags.LanguageName(ISO639) ?? "Unknown Language";
			}
			set
			{
				value = value ?? "";
				UpdateString(ref _languageName, value);
			}
		}


		protected void UpdateString(ref string field, string value)
		{
			if (field == value)
				return;

			//count null as same as ""
			if (String.IsNullOrEmpty(field) && String.IsNullOrEmpty(value))
			{
				return;
			}
			Modified = true;
			field = value;
		}

		/// <summary>
		/// Other classes that persist this need to know when our id changed, so they can
		/// clean up the old copy which is based on the old name.
		/// </summary>
		virtual public string StoreID { get; set; }

		virtual public string DisplayLabel
		{
			get
			{
				//jh (Oct 2010) made it start with RFC5646 because all ws's in a lang start with the
				//same abbreviation, making imppossible to see (in SOLID for example) which you chose.
				bool languageIsUnknown = RFC5646.Equals(WellKnownSubTags.Unlisted.Language, StringComparison.OrdinalIgnoreCase);
				if (!String.IsNullOrEmpty(RFC5646) && !languageIsUnknown)
				{
					return RFC5646;
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

		virtual public string ListLabel
		{
			get
			{
				string n = string.Empty;
				if (!String.IsNullOrEmpty(LanguageName))
				{
					n = LanguageName;
				}
				else
				{
					n = DisplayLabel;
				}
				string details = "";
				if(IpaStatus != IpaStatusChoices.NotIpa)
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
				}
				else if (!String.IsNullOrEmpty(_rfcTag.Script))
				{
					details += _rfcTag.Script+"-";
				}
				if (!String.IsNullOrEmpty(_rfcTag.Region))
				{
					details += _rfcTag.Region + "-";
				}
				if (IpaStatus == IpaStatusChoices.NotIpa && !String.IsNullOrEmpty(_rfcTag.Variant))
				{
					details += _rfcTag.Variant + "-";
				}

				if (IsVoice)
				{
					details = details.Replace("Zxxx-", "");
					details += "voice";
				}
				details = details.Trim(new char[] { '-' });
				if (details.Length > 0)
					details = " ("+details + ")";
				return n+details;
			}
		}

		virtual public string RFC5646
		{
			get
			{
				return _rfcTag.CompleteTag;
			}
		}

		public string Id
		{
			get
			{
				return RFC5646;
			}
		}


		[Obsolete("Use StandardTags directly")]
		public static List<Iso15924Script> ScriptOptions
		{
			get
			{
				return StandardTags.ValidIso15924Scripts;
			}
		}

		virtual public bool Modified { get; set; }

		virtual public bool MarkedForDeletion { get; set; }

		virtual public string DefaultFontName
		{
			get
			{
				return _defaultFontName;
			}
			set
			{
				UpdateString(ref _defaultFontName, value);
			}
		}

		virtual public float DefaultFontSize
		{
			get
			{
				return _defaultFontSize;
			}
			set
			{
				if (value == _defaultFontSize)
				{
					return;
				}
				if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
				{
					throw new ArgumentOutOfRangeException();
				}
				_defaultFontSize = value;
				Modified = true;
			}
		}

		virtual public string Keyboard
		{
			get
			{
				if(String.IsNullOrEmpty(_keyboard))
				{
					return "";
				}
				return _keyboard;
			}
			set
			{
				UpdateString(ref _keyboard, value);
			}
		}

		virtual public bool RightToLeftScript
		{
			get
			{
				return _rightToLeftScript;
			}
			set
			{
				if(value != _rightToLeftScript)
				{
					Modified = true;
					_rightToLeftScript = value;
				}
			}
		}

		/// <summary>
		/// The windows "NativeName" from the Culture class
		/// </summary>
		virtual public string NativeName
		{
			get
			{
				return _nativeName;
			}
			set
			{
				UpdateString(ref _nativeName, value);
			}
		}

		virtual public SortRulesType SortUsing
		{
			get { return _sortUsing; }
			set
			{
				if (value != _sortUsing)
				{
					_sortUsing = value;
					_collator = null;
					Modified = true;
				}
			}
		}

		virtual public string SortRules
		{
			get { return _sortRules ?? string.Empty; }
			set
			{
				_collator = null;
				UpdateString(ref _sortRules, value);
			}
		}

		public void SortUsingOtherLanguage(string sortRules)
		{
			SortUsing = SortRulesType.OtherLanguage;
			SortRules = sortRules;
		}

		public void SortUsingCustomICU(string sortRules)
		{
			SortUsing = SortRulesType.CustomICU;
			SortRules = sortRules;
		}

		public void SortUsingCustomSimple(string sortRules)
		{
			SortUsing = SortRulesType.CustomSimple;
			SortRules = sortRules;
		}

		virtual public string SpellCheckingId
		{
			get
			{
				if (string.IsNullOrEmpty(_spellCheckingId))
				{
					return _rfcTag.Language;
				}
				return _spellCheckingId;
			}
			set { UpdateString(ref _spellCheckingId, value); }
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
					switch (SortUsing)
					{
						case SortRulesType.DefaultOrdering:
							_collator = new IcuRulesCollator(String.Empty); // was SystemCollator(null);
							break;
						case SortRulesType.CustomSimple:
							_collator = new SimpleRulesCollator(SortRules);
							break;
						case SortRulesType.CustomICU:
							_collator = new IcuRulesCollator(SortRules);
							break;
						case SortRulesType.OtherLanguage:
							_collator = new SystemCollator(SortRules);
							break;
					}
				}
				return _collator;
			}
		}

		[Obsolete("Use IsUnicodeEncoded instead - which is the inverse of IsLegacyEncoded")]
		virtual public bool IsLegacyEncoded
		{
			get
			{
				return !IsUnicodeEncoded;
			}
			set
			{
				IsUnicodeEncoded = !value;
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
			switch (SortUsing)
			{
				case SortRulesType.DefaultOrdering:
					return String.IsNullOrEmpty(SortRules);
				case SortRulesType.CustomICU:
					return IcuRulesCollator.ValidateSortRules(SortRules, out message);
				case SortRulesType.CustomSimple:
					return SimpleRulesCollator.ValidateSimpleRules(SortRules, out message);
				case SortRulesType.OtherLanguage:
					try
					{
						new SystemCollator(SortRules);
					}
					catch (Exception e)
					{
						message = String.Format("Error while validating sorting rules: {0}", e.Message);
						return false;
					}
					return true;
			}
			return false;
		}

		public override string ToString()
		{
			return _rfcTag.ToString();
		}

		virtual public WritingSystemDefinition Clone()
		{
			return new WritingSystemDefinition(this);
		}

		public bool IsUnicodeEncoded
		{
			get
			{
				return _isUnicodeEncoded;
			}
			set
			{
				if (value != _isUnicodeEncoded)
				{
					Modified = true;
					_isUnicodeEncoded = value;
				}
			}
		}

		public void SetRfc5646FromString(string completeTag)
		{
			_rfcTag = RFC5646Tag.Parse(completeTag);
			Modified = true;
		}

		public static WritingSystemDefinition Parse(string completeTag)
		{
			var writingSystemDefinition = new WritingSystemDefinition();
			writingSystemDefinition.SetRfc5646FromString(completeTag);
			return writingSystemDefinition;
		}

		public static WritingSystemDefinition FromLanguage(string language)
		{
			return new WritingSystemDefinition(language);
		}

		public static WritingSystemDefinition FromRFC5646Subtags(string language, string script, string region, string variantAndPrivateUse)
		{
			return new WritingSystemDefinition(language, script, region, variantAndPrivateUse, string.Empty, false);
		}


		public static IEnumerable<string> FilterWellKnownPrivateUseTags(IEnumerable<string> privateUseTokens)
		{
			foreach (var privateUseToken in privateUseTokens)
			{
				string strippedToken = RFC5646Tag.StripLeadingPrivateUseMarker(privateUseToken);
				if (strippedToken.Equals(RFC5646Tag.StripLeadingPrivateUseMarker(WellKnownSubTags.Audio.PrivateUseSubtag), StringComparison.OrdinalIgnoreCase) ||
					strippedToken.Equals(RFC5646Tag.StripLeadingPrivateUseMarker(WellKnownSubTags.Ipa.PhonemicPrivateUseSubtag), StringComparison.OrdinalIgnoreCase) ||
					strippedToken.Equals(RFC5646Tag.StripLeadingPrivateUseMarker(WellKnownSubTags.Ipa.PhoneticPrivateUseSubtag), StringComparison.OrdinalIgnoreCase))
					continue;
				yield return privateUseToken;
			}
		}
	}

	public enum IpaStatusChoices
	{
		NotIpa,
		Ipa,
		IpaPhonetic,
		IpaPhonemic
	}

	public class WellKnownSubTags
	{
		public class Unlisted
		{
			public const string Language = "qaa";
		}

		public class Unwritten
		{
			public const string Script = "Zxxx";
		}

		//The "x-" is required before each of the strings below, since WritingSystemDefinition needs "x-" to distinguish RFC5646 private use from variant
		//Without the "x-"  a consumer who wanted to set a writing ystem as audio would have to write: ws.Variant = "x-" + WellKnownSubtags.Audio.PrivateUseSubtag
		public class Audio
		{
			public const string PrivateUseSubtag = "x-audio";
			public const string Script= Unwritten.Script;
		}

		public class Ipa
		{
			public const string VariantSubtag = "fonipa";
			public const string PhonemicPrivateUseSubtag = "x-emic";
			public const string PhoneticPrivateUseSubtag = "x-etic";
		}
	}
}