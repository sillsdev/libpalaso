using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Palaso.UI.WindowsForms.Keyboarding;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems
{
//    public interface IWritingSystemVariantSuggestor
//    {
//        IEnumerable<IWritingSystemDefinitionSuggestion> GetSuggestions(WritingSystemDefinition primary, IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage);
//        IEnumerable<IWritingSystemDefinitionSuggestion> GetOtherLanguageSuggestions(IEnumerable<WritingSystemDefinition> existingDefinitions);
//    }

	public class WritingSystemSuggestor
	{
		/// <summary>
		/// Consider setting this to true in linguistic applications
		/// </summary>
		public bool SuggestIpa { get; set; }
		/// <summary>
		/// Consider setting this to true in linguistic applications
		/// </summary>
		public bool SuggestDialects { get; set; }

		public bool SuggestVoice { get; set; }

		public bool SuggestOther { get; set; }

		public IEnumerable<WritingSystemDefinition> OtherKnownWritingSystems { get; set; }


		public WritingSystemSuggestor()
		{
			OtherKnownWritingSystems = new WritingSystemFromWindowsLocaleProvider();
		   SuppressSuggestionsForMajorWorldLanguages=true;
			SuggestIpa=true;
			SuggestDialects=true;
			SuggestOther = true;
			SuggestVoice=false;
		}

		/// <summary>
		/// When true, no suggestions will be made some languages which may be supplied by the OS
		/// but which are unlikely to be the study of language documentation efforst
		/// </summary>
		public bool SuppressSuggestionsForMajorWorldLanguages { get; set; }

		public IEnumerable<IWritingSystemDefinitionSuggestion> GetSuggestions(WritingSystemDefinition primary, IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			if(string.IsNullOrEmpty(primary.ISO))
				yield break;

			if(SuppressSuggestionsForMajorWorldLanguages
				&& new[]{"en", "th", "es", "fr", "de", "hi", "id", "vi","my","pt", "fi", "ar", "it","sv", "ja", "ko", "ch", "nl", "ru"}.Contains(primary.ISO))
				yield break;

			if (SuggestIpa && IpaSuggestion.ShouldSuggest(existingWritingSystemsForLanguage))
			{
				yield return new IpaSuggestion(primary);
			}

			if (SuggestVoice && VoiceSuggestion.ShouldSuggest(existingWritingSystemsForLanguage))
			{
				yield return new VoiceSuggestion(primary);
			}

			if (SuggestDialects)
			{
				yield return new DialectSuggestion(primary);
			}

			if (SuggestOther)
			{
				yield return new OtherSuggestion(primary);
			}
		}


		public IEnumerable<IWritingSystemDefinitionSuggestion> GetOtherLanguageSuggestions(IEnumerable<WritingSystemDefinition> existingDefinitions)
		{
			if (OtherKnownWritingSystems != null)
			{
				foreach (WritingSystemDefinition language in OtherKnownWritingSystems)
				{
					if (!existingDefinitions.Any(def => def.RFC5646 == language.RFC5646))
						yield return new LanguageSuggestion(language);
				}
			}
		}
	}


	public interface IWritingSystemDefinitionSuggestion
	{
		//public WritingSystemDefinition TemplateDefinition { get; private set; }
		 string Label { get; }
		 string ToolTip { get; }
		 WritingSystemDefinition ShowDialogIfNeededAndGetDefinition();
	}

	public abstract class WritingSystemSuggestion : IWritingSystemDefinitionSuggestion
	{
		internal WritingSystemDefinition _templateDefinition;
		public WritingSystemDefinition TemplateDefinition
		{
			get { return _templateDefinition; }
		}

		public string Label { get; protected set; }
		public string ToolTip { get; private set; }
		public abstract WritingSystemDefinition ShowDialogIfNeededAndGetDefinition();

		protected void SetLabelDetail(string detail)
		{
			this.Label = string.Format("{0} ({1})",_templateDefinition.LanguageName, detail);
		}
	}
	public class VoiceSuggestion : WritingSystemSuggestion
	{
		public VoiceSuggestion(WritingSystemDefinition primary)
		{
			_templateDefinition = primary.Clone();
			_templateDefinition.IsVoice = true;
			SetLabelDetail("voice");
		}

		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			return TemplateDefinition;
		}
		public static bool ShouldSuggest(IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			return !existingWritingSystemsForLanguage.Any(def => def.IsVoice);
		}
	}
	public class DialectSuggestion : WritingSystemSuggestion
	{
		public DialectSuggestion(WritingSystemDefinition primary)
		{
			_templateDefinition = primary.Clone();
			_templateDefinition.Variant = string.Empty;
			this.Label = string.Format("new dialect of {0}", _templateDefinition.LanguageName);
		}
		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			var dlg = new GetDialectNameDialog();
			if(DialogResult.OK!= dlg.ShowDialog())
				return null;
			TemplateDefinition.Variant+= "-"+ dlg.DialectName;
			return TemplateDefinition;
		}
	}
	public class IpaSuggestion : WritingSystemSuggestion
	{
		/// <summary>
		/// these are ordered in terms of perference, so the last one is just the fallback
		/// </summary>
		private readonly string[] _fontsForIpa = { "arial unicode ms", "lucinda sans unicode", "doulous sil", FontFamily.GenericSansSerif.Name };

		public IpaSuggestion(WritingSystemDefinition primary)
		{
			_templateDefinition = new WritingSystemDefinition(primary.ISO, string.Empty, primary.Region, string.Empty, primary.LanguageName, "ipa", false)
									  {
										  DefaultFontSize = primary.DefaultFontSize,
										  DefaultFontName = _fontsForIpa.FirstOrDefault(FontExists),
										  IpaStatus = IpaStatusChoices.Ipa,
										  Keyboard = Palaso.UI.WindowsForms.Keyboarding.KeyboardController.GetIpaKeyboardIfAvailable()
									  };
			SetLabelDetail("IPA");
		}
		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			return TemplateDefinition;
		}


		private static bool FontExists(string name)
		{
			var f = new Font(name, 12);
			return f.Name.ToLower() == name.ToLower();
		}

		public static bool ShouldSuggest(IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			return  !existingWritingSystemsForLanguage.Any(def => def.IpaStatus!=IpaStatusChoices.NotIpa);
		}
	}

	public class OtherSuggestion : WritingSystemSuggestion
	{
		public OtherSuggestion(WritingSystemDefinition primary)
		{
			_templateDefinition = primary.Clone();
			this.Label=string.Format("other writing system for {0}", _templateDefinition.LanguageName);
		}
		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			return TemplateDefinition;
		}
	}

	/// <summary>
	/// used to suggest languages not yet represented. Contrast with the other classes here,
	/// which are use to suggest new writing systems based on already in-use languages
	/// </summary>
	public class LanguageSuggestion : WritingSystemSuggestion
	{
		public LanguageSuggestion(WritingSystemDefinition definition)
		{
			_templateDefinition = definition;
			this.Label=string.Format(_templateDefinition.DisplayLabel);
		}
		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			return TemplateDefinition;
		}
	}
}