using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SIL.Keyboarding;
using SIL.Windows.Forms.Miscellaneous;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.WSTree
{
	public interface IWritingSystemDefinitionSuggestion
	{
		string Label { get; }
		WritingSystemDefinition ShowDialogIfNeededAndGetDefinition();
	}

	public abstract class WritingSystemSuggestion : IWritingSystemDefinitionSuggestion
	{
		private readonly IWritingSystemFactory _wsFactory;

		protected WritingSystemSuggestion(IWritingSystemFactory wsFactory)
		{
			_wsFactory = wsFactory;
		}

		protected IWritingSystemFactory WritingSystemFactory
		{
			get { return _wsFactory; }
		}

		public string Label { get; protected set; }
		public abstract WritingSystemDefinition ShowDialogIfNeededAndGetDefinition();
	}

	public class VoiceSuggestion : WritingSystemSuggestion
	{
		private readonly WritingSystemDefinition _templateWritingSystemDefinition;

		public VoiceSuggestion(IWritingSystemFactory wsFactory, WritingSystemDefinition primary)
			: base(wsFactory)
		{
			_templateWritingSystemDefinition = primary;
			Label = string.Format("{0} (voice)", primary.Language.Name);
		}

		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			WritingSystemDefinition ws = WritingSystemFactory.Create(_templateWritingSystemDefinition);
			ws.IsVoice = true;
			return ws;
		}

		public static bool ShouldSuggest(IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			return !existingWritingSystemsForLanguage.Any(def => def.IsVoice);
		}
	}

	public class DialectSuggestion : WritingSystemSuggestion
	{
		private readonly WritingSystemDefinition _templateWritingSystemDefinition;

		public DialectSuggestion(IWritingSystemFactory wsFactory, WritingSystemDefinition primary)
			: base(wsFactory)
		{
			_templateWritingSystemDefinition = primary;
			Label = string.Format("new dialect of {0}", primary.Language.Name);
		}

		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			var dlg = new GetDialectNameDialog();
			if (DialogResult.OK != dlg.ShowDialog())
				return null;

			WritingSystemDefinition ws = WritingSystemFactory.Create(_templateWritingSystemDefinition);
			IEnumerable<VariantSubtag> variantSubtags;
			if (IetfLanguageTag.TryGetVariantSubtags(dlg.DialectName.ToValidVariantString(), out variantSubtags))
				ws.Variants.ReplaceAll(variantSubtags);
			return ws;
		}
	}

	public class IpaSuggestion : WritingSystemSuggestion
	{
		/// <summary>
		/// these are ordered in terms of preference, so the last one is just the fallback
		/// </summary>
		private readonly string[] _fontsForIpa = {"arial unicode ms", "lucinda sans unicode", "doulos sil", FontFamily.GenericSansSerif.Name};

		private readonly WritingSystemDefinition _templateWritingSystemDefinition;

		public IpaSuggestion(IWritingSystemFactory wsFactory, WritingSystemDefinition primary)
			: base(wsFactory)
		{
			_templateWritingSystemDefinition = primary;
			Label = string.Format("IPA input system for {0}", primary.Language.Name);
		}

		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			WaitCursor.Show();
			try
			{
				var variants = new List<VariantSubtag> {WellKnownSubtags.IpaVariant};
				variants.AddRange(_templateWritingSystemDefinition.Variants);
				WritingSystemDefinition ws;
				WritingSystemFactory.Create(IetfLanguageTag.Create(_templateWritingSystemDefinition.Language, null,
					_templateWritingSystemDefinition.Region, variants), out ws);
				string ipaFontName = _fontsForIpa.FirstOrDefault(FontExists);
				if (!string.IsNullOrEmpty(ipaFontName))
					ws.DefaultFont = new FontDefinition(ipaFontName);
				ws.Abbreviation = "ipa";
				ws.DefaultFontSize = _templateWritingSystemDefinition.DefaultFontSize;
				IKeyboardDefinition ipaKeyboard = Keyboard.Controller.AvailableKeyboards.FirstOrDefault(k => k.Id.ToLower().Contains("ipa"));
				if (ipaKeyboard != null)
					ws.Keyboard = ipaKeyboard.Id;
				return ws;
			}
			finally
			{
				WaitCursor.Hide();
			}
		}

		private static bool FontExists(string name)
		{
			var f = new Font(name, 12);
			return f.Name.ToLower() == name.ToLower();
		}

		public static bool ShouldSuggest(IEnumerable<WritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			return existingWritingSystemsForLanguage.All(def => def.IpaStatus == IpaStatusChoices.NotIpa);
		}
	}

	public class OtherSuggestion : WritingSystemSuggestion
	{
		private readonly WritingSystemDefinition _templateWritingSystemDefinition;
		private readonly List<WritingSystemDefinition> _existingWritingSystemsForLanguage; 

		public OtherSuggestion(IWritingSystemFactory wsFactory, WritingSystemDefinition primary, IEnumerable<WritingSystemDefinition> exisitingWritingSystemsForLanguage)
			: base(wsFactory)
		{
			_templateWritingSystemDefinition = primary;
			_existingWritingSystemsForLanguage = exisitingWritingSystemsForLanguage.ToList();
			Label = string.Format("other input system for {0}", primary.Language.Name);
		}

		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			WritingSystemDefinition ws = WritingSystemFactory.Create(_templateWritingSystemDefinition);
			ws.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(
				ws.LanguageTag, _existingWritingSystemsForLanguage.Select(w => w.LanguageTag));
			return ws;
		}
	}

	/// <summary>
	/// used to suggest languages not yet represented. Contrast with the other classes here,
	/// which are use to suggest new writing systems based on already in-use languages
	/// </summary>
	public class LanguageSuggestion : WritingSystemSuggestion
	{
		private readonly string _languageTag;
		private readonly string _keyboardLayout;

		public LanguageSuggestion(IWritingSystemFactory wsFactory, string languageTag, string keyboardLayout)
			: base(wsFactory)
		{
			_languageTag = languageTag;
			_keyboardLayout = keyboardLayout;
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			IetfLanguageTag.TryGetSubtags(languageTag, out languageSubtag, out scriptSubtag, out regionSubtag,
				out variantSubtags);

			var s = new StringBuilder();
			if (!string.IsNullOrEmpty(languageSubtag.Name))
				s.Append(languageSubtag.Name);
			if (scriptSubtag != null && !string.IsNullOrEmpty(scriptSubtag.Name) && !IetfLanguageTag.IsScriptImplied(languageTag))
				s.AppendFormat("-{0}", scriptSubtag.Name);
			if (regionSubtag != null && !string.IsNullOrEmpty(regionSubtag.Name))
				s.AppendFormat("-{0}", regionSubtag.Name);
			Label = s.ToString();
		}

		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			WritingSystemDefinition ws = WritingSystemFactory.CreateAndWarnUserIfOutOfDate(_languageTag);
			if (ws != null)
			{
				ws.Keyboard = _keyboardLayout;
				ws.DefaultFontSize = 12;
			}
			return ws;
		}
	}
}