using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
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

		private RFC5646Tag _rfcTag = new RFC5646Tag(String.Empty, String.Empty, String.Empty, String.Empty);

		private string _languageName;

		private string _abbreviation;
		private bool _isLegacyEncoded;

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
		private RFC5646Tag _rfcTagOnLoad;

		/// <summary>
		/// singleton
		/// </summary>
		private static List<ScriptOption> _scriptOptions = new List<ScriptOption>();
	   /// <summary>
		/// singleton
		/// </summary>
		private static List<LanguageCode> _languageCodes;

		/// <summary>
		/// For overriding the other identifier fields, to specify a custom RFC5646
		/// </summary>
		//private string _customLanguageTag;

		private static string AudioVariantMarker
		{
			get { return "x-audio"; }
		}

		private static string AudioScriptMarker
		{
			get { return "Zxxx"; }
		}

		public WritingSystemDefinition()
		{
			_sortUsing = SortRulesType.DefaultOrdering;
			_isLegacyEncoded = false;
		   // _defaultFontSize = 10; //arbitrary
		}

		public WritingSystemDefinition(string iso)
			: this()
		{
			_rfcTag.Language = iso;
			_abbreviation = _rfcTag.Script = _languageName = _rfcTag.Variant = _rfcTag.Region = _nativeName = string.Empty;
		}

		public WritingSystemDefinition(string iso, string script, string region, string variant, string languageName, string abbreviation, bool rightToLeftScript)
			: this()
		{
			_rfcTag.Language = iso;
			_rfcTag.Script = script;
			_rfcTag.Region = region;
			_rfcTag.Variant = variant;
			_abbreviation = abbreviation;
			_languageName = languageName;
			_rightToLeftScript = rightToLeftScript;
		}

		/// <summary>
		/// Copy constructor.
		/// </summary>
		/// <param name="ws">The ws.</param>
		public WritingSystemDefinition(WritingSystemDefinition ws)
			: this(ws._rfcTag.Language, ws._rfcTag.Script, ws._rfcTag.Region, ws._rfcTag.Variant, ws._languageName, ws._abbreviation, ws._rightToLeftScript)
		{
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
			_isLegacyEncoded = ws._isLegacyEncoded;
			_rfcTagOnLoad = ws._rfcTagOnLoad;
		}

		/// <summary>
		/// parse in the text of the script registry we get from http://unicode.org/iso15924/iso15924-text.html
		/// </summary>
		private static void LoadScriptOptionsIfNeeded()
		{
		  if (_scriptOptions.Count > 0)
			  return;

		  //this one isn't an official script: REVIEW: we're not using fonipa, whichis a VARIANT, not a script
		  _scriptOptions.Add(new ScriptOption("IPA", "Zipa"));
			//to help people find Latin
		  _scriptOptions.Add(new ScriptOption("Roman (Latin)", "Latn"));

			string[] scripts = Resource.scriptNames.Split('\n');
			foreach (string line in scripts)
			{
				string tline = line.Trim();
				if (tline.Length==0 || (tline.Length > 0 && tline[0]=='#'))
					continue;
				string[] fields = tline.Split(';');
				string label = fields[2];

				//these looks awful: "Korean (alias for Hangul + Han)"
				// and "Japanese (alias for Han + Hiragana + Katakana"
				if (label.IndexOf(" (alias") > -1)
				{
					label = label.Substring(0, fields[2].IndexOf(" (alias "));
				}
				_scriptOptions.Add(new ScriptOption(label, fields[0]));

			}

			_scriptOptions.Sort(ScriptOption.CompareScriptOptions);
		}

		/// <summary>
		/// Provides a list of ISO language codes.  Uses ISO 639-1 and 639-3 where ISO 639-1 is not available.
		/// </summary>
		public static IList<LanguageCode> LanguageCodes
		{
			get
			{
				if (_languageCodes != null)
				{
					return _languageCodes;
				}
				_languageCodes = new List<LanguageCode>();
				string[] languages = Resource.languageCodes.Split('\n');
				foreach (string line in languages)
				{
					if(line.Contains("Ref_Name"))//skip first line
						continue;
					string tline = line.Trim();
					if (tline.Length == 0)
						continue;
					string[] fields = tline.Split('\t');
					// use ISO 639-1 code where available, otherwise use ISO 639-3 code
					_languageCodes.Add(new LanguageCode(String.IsNullOrEmpty(fields[3]) ? fields[0] : fields[3], fields[6], fields[0]));
				}
				_languageCodes.Sort(LanguageCode.CompareByName);
				return _languageCodes;
			}
		}

		public class LanguageCode
		{
			public LanguageCode(string code, string name, string iso3Code)
			{
				Code = code;
				Name = name;
				ISO3Code = iso3Code;
			}


			public string Name { get; set; }

			public string Code { get; set; }

			public string ISO3Code { get; set; }

			public static int CompareByName(LanguageCode x, LanguageCode y)
			{
				if (x == null)
				{
					if (y == null)
					{
						return 0;
					}
					else
					{
						return -1;
					}
				}
				else
				{
					if (y == null)
					{
						return 1;
					}
					else
					{
						return x.Name.CompareTo(y.Name);
					}
				}
			}

		}

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
				if( _rfcTag.Variant.Contains("fonipa-x-emic"))
				{
					return IpaStatusChoices.IpaPhonemic;
				}
				if (_rfcTag.Variant.Contains("fonipa-x-etic"))
				{
					return IpaStatusChoices.IpaPhonetic;
				}
				if (_rfcTag.Variant.Contains("fonipa"))
				{
					return IpaStatusChoices.Ipa;
				}
				return IpaStatusChoices.NotIpa;
			}

			set
			{
				RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-audio");
				/* "There are some variant subtags that have no prefix field,
				 * eg. fonipa (International IpaPhonetic Alphabet). Such variants
				 * should appear after any other variant subtags with prefix information."
				 */
				RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-etic");
				RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "x-emic");
				RemoveFromSubtag(RFC5646Tag.SubTag.Variant, "fonipa");

				switch (value)
				{
					default:
						break;
					case IpaStatusChoices.Ipa:
						_rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "fonipa");
						break;
					case IpaStatusChoices.IpaPhonemic:
						_rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "fonipa-x-emic");
						break;
					case IpaStatusChoices.IpaPhonetic:
						_rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, "fonipa-x-etic");
						break;
				}
			}
		}

		private void RemoveFromSubtag(RFC5646Tag.SubTag subtag, string stringToRemove)
		{
			if (_rfcTag.SubtagContainsPart(subtag, stringToRemove))
			{
				_rfcTag.RemoveFromSubtag(subtag, stringToRemove);
			}
		}

		virtual public bool IsVoice
		{
			get
			{
				bool scriptSubTagIsAudioConform = Script.Equals(AudioScriptMarker, StringComparison.OrdinalIgnoreCase);
				bool variantSubTagIsAudioConform = Variant.Contains(AudioVariantMarker, StringComparison.OrdinalIgnoreCase);
				if(scriptSubTagIsAudioConform && variantSubTagIsAudioConform){ return true; }
				return false;
			}
			set
			{
				if(IsVoice == value) {return;}
				if (value)
				{
					IpaStatus = IpaStatusChoices.NotIpa;
					Keyboard = string.Empty;
					Script = AudioScriptMarker;
					_rfcTag.AddToSubtag(RFC5646Tag.SubTag.Variant, AudioVariantMarker);
				}
				else
				{
					_rfcTag.RemoveFromSubtag(RFC5646Tag.SubTag.Variant, AudioVariantMarker);
				}
				Modified = true;
			}
		}

		/// <summary>
		/// Todo: this could/should become an ordered list of variant tags
		/// </summary>
		virtual public string Variant
		{
			get
			{
				return _rfcTag.Variant;
			}
			set
			{
				if (value == null || value == Variant) { return; }
				_rfcTag.Variant = value;
				Modified = true;
			}
		}

		virtual public string Region
		{
			get
			{
				return _rfcTag.Region;
			}
			set
			{
				if (value == Region) { return; }
				_rfcTag.Region = value;
				Modified = true;
			}
		}

		//Set all the parts of the Rfc5646 tag, which include language (iso), script, region and subtags.
		//private RFC5646Tag Rfc5646Tag
		//{
		//    get{ return _rfcTag;}
		//    set
		//    {
		//        if (_rfcTag == value){ return; }
		//        _rfcTag = value;
		//        Modified = true;
		//    }
		//}

		//Set all the parts of the Rfc5646 tag, which include language (iso), script, region and subtags.
		//This method is preferable to setting the individual components independantly, as the order
		//in which they are set can lead to invalid interim Rfc5646 tags
		public RFC5646Tag Rfc5646TagOnLoad
		{
			get { return _rfcTagOnLoad; }
			set { _rfcTagOnLoad = value; }
		}

		/// <summary>
		/// The ISO-639 code which is also the Ethnologue code.
		/// </summary>
		virtual public string ISO
		{
			get
			{
				return _rfcTag.Language;
			}
			set
			{
				if (value == ISO) { return; }
				_rfcTag.Language = value;
				Modified = true;
			}
		}

		virtual public string Abbreviation
		{
			get
			{
				return _abbreviation;
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
				if (value == Script) { return; }
				_rfcTag.Script = value;
				Modified = true;
			}
		}

		virtual public string LanguageName
		{
			get
			{
				return _languageName;
			}
			set
			{
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
				if (!String.IsNullOrEmpty(RFC5646))
				{
					return RFC5646;
				}
				if (!String.IsNullOrEmpty(_abbreviation))
				{
					return _abbreviation;
				}
				if (!String.IsNullOrEmpty(_rfcTag.Language))
				{
					return _rfcTag.Language;
				}
				if (!String.IsNullOrEmpty(_languageName))
				{
					string n = _languageName;
					return n.Substring(0, n.Length > 4 ? 4 : n.Length);
				}
				return "???";
			}
		}

		virtual public string ListLabel
		{
			get
			{
				string n = string.Empty;
				if (!String.IsNullOrEmpty(_languageName))
				{
					n=_languageName;
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
//                if(!string.IsNullOrEmpty(_customLanguageTag))
//                {
//                    return _customLanguageTag;
//                }
				return _rfcTag.CompleteTag;
			}
//            set
//            {
//                _customLanguageTag=value;
//            }
		}

		public string Id
		{
			get
			{
				return RFC5646;
			}
		}

		virtual public string VerboseDescription
		{
			get
			{
				var summary = new StringBuilder();
				if (!String.IsNullOrEmpty(_rfcTag.Variant))
				{
					summary.AppendFormat("{0}", _rfcTag.Variant);
				}
				summary.AppendFormat(" {0}", string.IsNullOrEmpty(_languageName)?"???":_languageName);
				if (!String.IsNullOrEmpty(_rfcTag.Region))
				{
					summary.AppendFormat(" in {0}", _rfcTag.Region);
				}
				if (!String.IsNullOrEmpty(_rfcTag.Script))
				{
					summary.AppendFormat(" written in {0} script", CurrentScriptOptionLabel);
				}

				summary.AppendFormat(". ({0})", RFC5646);
				return summary.ToString().Trim();
			}
		}

		private string CurrentScriptOptionLabel
		{
			get
			{
				ScriptOption option = ScriptOption;
				return option == null ? _rfcTag.Script : option.Label;
			}
		}

		/// <summary>
		/// If we don't have an option for the current script, returns null
		/// </summary>
		virtual public ScriptOption ScriptOption
		{
			get
			{
				string script = Script;
				if (String.IsNullOrEmpty(script))
				{
					script = "latn";
				}
				foreach (var option in ScriptOptions)
				{
					if (option.Code == script)
					{
						return option;
					}
				}
				return null;
			}
		}

		public static List<ScriptOption> ScriptOptions
		{
			get
			{
				LoadScriptOptionsIfNeeded();
				return _scriptOptions;
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

		virtual public bool IsLegacyEncoded
		{
			get
			{
				return _isLegacyEncoded;
			}
			set
			{
				if(value != _isLegacyEncoded)
				{
					Modified = true;
					_isLegacyEncoded = value;
				}
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

	}

	public class ScriptOption
	{
		public ScriptOption(string label, string code)
		{
			Label = label;
			Code = code;
		}

		public string Code { get; private set; }

		public string Label { get; private set; }

		public override string ToString()
		{
			return Label;
		}

		public static int CompareScriptOptions(ScriptOption x, ScriptOption y)
		{
			if (x == null)
			{
				if (y == null)
				{
					return 0;
				}
				return -1;
			}
			if (y == null)
			{
				return 1;
			}
			return x.Label.CompareTo(y.Label);
		}
	}

	public enum IpaStatusChoices
		{
			NotIpa,
			Ipa,
			IpaPhonetic,
			IpaPhonemic
		}
}