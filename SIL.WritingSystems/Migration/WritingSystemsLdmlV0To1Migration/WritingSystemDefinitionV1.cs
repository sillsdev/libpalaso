using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SIL.WritingSystems.Migration.WritingSystemsLdmlV0To1Migration
{
	internal class WritingSystemDefinitionV1
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

		private Rfc5646Tag _rfcTag;
		private string _languageName;
		private string _id;
		private string _abbreviation;
		private string _sortRules;

		public WritingSystemDefinitionV1()
		{
			SortUsing = SortRulesType.DefaultOrdering;
			IsUnicodeEncoded = true;
			_rfcTag = new Rfc5646Tag();
			_id = Bcp47Tag;
		}

		public string Id
		{
			get
			{
				return _id;
			}
			internal set
			{
				value = value ?? "";
				_id = value;
			}
		}

		public string Bcp47Tag
		{
			get
			{
				return _rfcTag.CompleteTag;
			}
		}

		public string Language
		{
			get { return _rfcTag.Language; }
		}

		public string Region
		{
			get { return _rfcTag.Region; }
		}

		public string Script
		{
			get { return _rfcTag.Script; }
		}

		public string Variant
		{
			get { return IetfLanguageTag.ConcatenateVariantAndPrivateUse(_rfcTag.Variant, _rfcTag.PrivateUse); }
		}

		public string StoreID { get; set; }

		public string VersionNumber { get; set; }

		public string VersionDescription { get; set; }

		public DateTime DateModified { get; set; }

		public string DefaultFontName { get; set; }

		public float DefaultFontSize { get; set; }

		public string Keyboard { get; set; }

		public bool RightToLeftScript { get; set; }

		public SortRulesType SortUsing { get; set; }

		public string SortRules
		{
			get => _sortRules ?? string.Empty;
			set => _sortRules = value;
		}

		public string SpellCheckingId { get; set; }

		public string Abbreviation
		{
			get
			{
				if (String.IsNullOrEmpty(_abbreviation))
				{
					// Use the language subtag unless it's an unlisted language.
					// If it's an unlisted language, use the private use area language subtag.
					if (Language == WellKnownSubtags.UnlistedLanguage)
					{
						int idx = Id.IndexOf("-x-");
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
			set { _abbreviation = value; }
		}

		public string LanguageName
		{
			get
			{
				if (!string.IsNullOrEmpty(_languageName))
					return _languageName;
				LanguageSubtag languageSubtag;
				if (StandardSubtags.RegisteredLanguages.TryGet(Language, out languageSubtag))
					return languageSubtag.Name;
				return "Unknown Language";
			}

			set { _languageName = value ?? string.Empty; }
		}

		public bool IsUnicodeEncoded { get; set; }

		public string WindowsLcid { get; set; }

		public void SetAllComponents(string language, string script, string region, string variant)
		{
			IetfLanguageTag.SplitVariantAndPrivateUse(variant, out var variantPart, out var privateUsePart);
			_rfcTag = new Rfc5646Tag(language, script, region, variantPart, privateUsePart);
			_id = Bcp47Tag;
		}

		public void SetTagFromString(string completeTag)
		{
			_rfcTag = Rfc5646Tag.Parse(completeTag);
			_id = Bcp47Tag;
		}

		readonly List<KeyboardDefinitionV1> _knownKeyboards = new List<KeyboardDefinitionV1>();

		public IEnumerable<KeyboardDefinitionV1> KnownKeyboards => _knownKeyboards;

		public void AddKnownKeyboard(KeyboardDefinitionV1 newKeyboard)
		{
			if (newKeyboard == null)
				return;
			// Review JohnT: how should this affect order?
			// e.g.: last added should be first?
			// Current algorithm keeps them in the order added, hopefully meaning the most likely one, added first,
			// remains the default.
			if (KnownKeyboards.Contains(newKeyboard))
				return;
			_knownKeyboards.Add(newKeyboard);
		}
	}
}
