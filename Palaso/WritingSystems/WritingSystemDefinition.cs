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
			/// Default Unicode ordering rules
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
			/// Use the sort rules from another language. When this is set, the SortRules are interpretted as a cultureId for the language to sort like.
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

		/// <summary>
		/// singleton
		/// </summary>
		private static List<ScriptOption> _scriptOptions = new List<ScriptOption>();
	   /// <summary>
		/// singleton
		/// </summary>
		private static List<LanguageCode> _languageCodes;

		/// <summary>
		/// For overridding the other identifier fields, to specify a custom RFC5646
		/// </summary>
		//private string _customLanguageTag;


		public WritingSystemDefinition()
		{
			_rfcTag.Language=_abbreviation = _rfcTag.Script=_languageName = _rfcTag.Variant = _rfcTag.Region =_nativeName = string.Empty;
			_sortUsing = SortRulesType.DefaultOrdering;
			Modified = false;
			IsLegacyEncoded = false;
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
			_rfcTag.Region = region;
			_rfcTag.Language = iso;
			_abbreviation = abbreviation;
			_rfcTag.Script = script;
			_languageName = languageName;
			_rfcTag.Variant = variant;
			_rightToLeftScript = rightToLeftScript;
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
					_languageCodes.Add(new LanguageCode(String.IsNullOrEmpty(fields[3]) ? fields[0] : fields[3], fields[6]));
				}
				_languageCodes.Sort(LanguageCode.CompareByName);
				return _languageCodes;
			}
		}

		public class LanguageCode
		{
			public LanguageCode(string code, string name)
			{
				Code = code;
				Name = name;
			}


			public string Name { get; set; }

			public string Code { get; set; }

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

		public string VersionNumber
		{
			get { return _versionNumber; }
			set { UpdateString(ref _versionNumber, value); }
		}

		public string VersionDescription
		{
			get { return _versionDescription; }
			set { UpdateString(ref _versionDescription, value); }
		}

		public DateTime DateModified
		{
			get { return _dateModified; }
			set { _dateModified = value; }
		}


		/// <summary>
		/// Note: this treats the etic and emic extensions as if they were variants, which we can get
		/// away with for now, but maybe not if this class grows to be extension aware.
		/// Ideally, these should be suffixes rather than private use
		/// </summary>
		public IpaStatusChoices IpaStatus
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
				/* "There are some variant subtags that have no prefix field,
				 * eg. fonipa (International IpaPhonetic Alphabet). Such variants
				 * should appear after any other variant subtags with prefix information."
				 */
				Variant = _rfcTag.Variant.Replace("-x-etic", "");
				Variant = _rfcTag.Variant.Replace("-x-emic", "");
				Variant = _rfcTag.Variant.Replace("fonipa", "");
				switch (value)
				{
					default:
						break;
					case IpaStatusChoices.Ipa:
						Variant = _rfcTag.Variant + "-fonipa";
						break;
					case IpaStatusChoices.IpaPhonemic:
						Variant = _rfcTag.Variant + "-fonipa-x-emic";
						break;
					case IpaStatusChoices.IpaPhonetic:
						Variant = _rfcTag.Variant + "-fonipa-x-etic";
						break;
				}
			}
		}

		public bool IsVoice
		{
			get { return RFC5646Tag.IsRFC5646TagForVoiceWritingSystem(_rfcTag); }
			set
			{
				if (value)
				{
					IpaStatus = IpaStatusChoices.NotIpa;
					Keyboard = string.Empty;
					ISO = ISO.Split('-')[0];
					_rfcTag = RFC5646Tag.RFC5646TagForVoiceWritingSystem(ISO);
				}
				else if (IsVoice == true)
				{
					_rfcTag = new RFC5646Tag(ISO, "", "", "");
				}
				Modified = true;
			}
		}

		/// <summary>
		/// Todo: this could/should become an ordered list of variant tags
		/// </summary>
		[Obsolete("Please use the RFC5646Tag property to set the RFC5646 tag as this avoids invalid intermediate tags.")]
		public string Variant
		{
			get
			{
				return _rfcTag.Variant;
			}
			set
			{
				if (value == Variant) { return; }
				value = value.Trim(new[] { '-' }).Replace("--","-");//cleanup
				Rfc5646Tag = new RFC5646Tag(_rfcTag.Language, _rfcTag.Script, _rfcTag.Region, value);
				if (!RFC5646Tag.IsValid(_rfcTag))
				{
					throw new InvalidOperationException(String.Format("Changing the variant subtag to {0} results in an invalid RFC5646 tag.", value));
				}
				Modified = true;
			}
		}

		[Obsolete("Please use the RFC5646Tag property to set the RFC5646 tag as this avoids invalid intermediate tags.")]
		public string Region
		{
			get
			{
				return _rfcTag.Region;
			}
			set
			{
				if (value == Region) { return; }
				Rfc5646Tag = new RFC5646Tag(_rfcTag.Language, _rfcTag.Script, value, _rfcTag.Variant);
				if (!RFC5646Tag.IsValid(_rfcTag))
				{
					throw new InvalidOperationException(String.Format("Changing the region subtag to {0} results in an invalid RFC5646 tag.", value));
				}
				Modified = true;
			}
		}

		//Set all the parts of the Rfc5646 tag, which include language (iso), script, region and subtags.
		//This method is preferable to setting the individual components independantly, as the order
		//in which they are set can lead to invalid interim Rfc5646 tags
		public RFC5646Tag Rfc5646Tag
		{
			get{ return _rfcTag;}
			set
			{
				if(!RFC5646Tag.IsValid(value))
				{
					value = RFC5646Tag.GetValidTag(value);
				}
				_rfcTag = value;
				Modified = true;
			}
		}

		/// <summary>
		/// The ISO-639 code which is also the Ethnologue code.
		/// </summary>
		[Obsolete("Please use the RFC5646Tag property to set the RFC5646 tag as this avoids invalid intermediate tags.")]
		public string ISO
		{
			get
			{
				return _rfcTag.Language;
			}
			set
			{
				if (value == ISO) { return; }
				Rfc5646Tag = new RFC5646Tag(value, _rfcTag.Script, _rfcTag.Region, _rfcTag.Variant);
				if (!RFC5646Tag.IsValid(_rfcTag))
				{
					throw new InvalidOperationException(String.Format("Changing the language subtag to {0} results in an invalid RFC5646 tag.", value));
				}
				Modified = true;
			}
		}

		public string Abbreviation
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


		[Obsolete("Please use the RFC5646Tag property to set the RFC5646 tag as this avoids invalid intermediate tags.")]
		public string Script
		{
			get
			{
				return _rfcTag.Script;
			}
			set
			{
				if (value == Script) { return; }
				Rfc5646Tag = new RFC5646Tag(_rfcTag.Language, value, _rfcTag.Region, _rfcTag.Variant);
				if (!RFC5646Tag.IsValid(_rfcTag))
				{
					throw new InvalidOperationException(String.Format("Changing the script subtag to {0} results in an invalid RFC5646 tag.", value));
				}
				Modified = true;
			}
		}

		public string LanguageName
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


		private void UpdateString(ref string field, string value)
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
		public string StoreID { get; set; }

		public string DisplayLabel
		{
			get
			{
				//jh (OCt 2010) made it start with RFC5646 because all ws's in a lang start with the
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

		public string ListLabel
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
		public string RFC5646
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

		public string VerboseDescription
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
		public ScriptOption ScriptOption
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

		public bool Modified { get; set; }

		public bool MarkedForDeletion { get; set; }

		public string DefaultFontName
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

		public float DefaultFontSize
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

		public string Keyboard
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

		public bool RightToLeftScript
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
		public string NativeName
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


		public SortRulesType SortUsing
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

		public string SortRules
		{
			get { return _sortRules ?? string.Empty; }
			set
			{
				_collator = null;
				UpdateString(ref _sortRules, value);
			}
		}

		public string SpellCheckingId
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
		public ICollator Collator
		{
			get
			{
				if (_collator == null)
				{
					switch (SortUsing)
					{
						case SortRulesType.DefaultOrdering:
							_collator = new SystemCollator(null);
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

		public bool IsLegacyEncoded
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
		public bool ValidateCollationRules(out string message)
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

		public WritingSystemDefinition Clone()
		{
			var ws = new WritingSystemDefinition(_rfcTag.Language, _rfcTag.Script, _rfcTag.Region, _rfcTag.Variant, _languageName, _abbreviation, _rightToLeftScript);
			ws._defaultFontName = _defaultFontName;
			ws._defaultFontSize = _defaultFontSize;
			ws._keyboard = _keyboard;
			ws._versionNumber = _versionNumber;
			ws._versionDescription = _versionDescription;
			ws._nativeName = _nativeName;
			ws._sortUsing = _sortUsing;
			ws._sortRules = _sortRules;
			ws._spellCheckingId = _spellCheckingId;
			ws._dateModified = _dateModified;
			ws._isLegacyEncoded = _isLegacyEncoded;
			return ws;
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