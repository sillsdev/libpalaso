using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Palaso.WritingSystems;

namespace Palaso.UI.WindowsForms.WritingSystems.WSTree
{
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
		public VoiceSuggestion(IWritingSystemDefinition primary)
		{
			_templateDefinition = primary.Clone();
			_templateDefinition.IsVoice = true;
			SetLabelDetail("voice");
		}

		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			return TemplateDefinition;
		}
		public static bool ShouldSuggest(IEnumerable<IWritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			return !existingWritingSystemsForLanguage.Any(def => def.IsVoice);
		}
	}

	public class DialectSuggestion : WritingSystemSuggestion
	{
		public DialectSuggestion(IWritingSystemDefinition primary)
		{
			_templateDefinition = primary.Clone();
			this.Label = string.Format("new dialect of {0}", _templateDefinition.LanguageName);
		}
		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			var dlg = new GetDialectNameDialog();
			if (DialogResult.OK != dlg.ShowDialog())
				return null;
			TemplateDefinition.Variant = WritingSystemDefinitionVariantHelper.ValidVariantString(dlg.DialectName);
			return TemplateDefinition;
		}
	}

	public class IpaSuggestion : WritingSystemSuggestion
	{
		/// <summary>
		/// these are ordered in terms of perference, so the last one is just the fallback
		/// </summary>
		private readonly string[] _fontsForIpa = { "arial unicode ms", "lucinda sans unicode", "doulous sil", FontFamily.GenericSansSerif.Name };

		public IpaSuggestion(IWritingSystemDefinition primary)
		{
			_templateDefinition = new WritingSystemDefinition(primary.Language, "", primary.Region, primary.Variant, "ipa", false)
									  {
										  LanguageName = primary.LanguageName,
										  DefaultFontSize = primary.DefaultFontSize,
										  DefaultFontName = _fontsForIpa.FirstOrDefault(FontExists),
										  IpaStatus = IpaStatusChoices.Ipa,
										  Keyboard = Palaso.UI.WindowsForms.Keyboarding.KeyboardController.GetIpaKeyboardIfAvailable()
									  };
			this.Label = string.Format("IPA input system for {0}", _templateDefinition.LanguageName);
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

		public static bool ShouldSuggest(IEnumerable<IWritingSystemDefinition> existingWritingSystemsForLanguage)
		{
			return !existingWritingSystemsForLanguage.Any(def => def.IpaStatus != IpaStatusChoices.NotIpa);
		}
	}

	public class OtherSuggestion : WritingSystemSuggestion
	{
		public OtherSuggestion(IWritingSystemDefinition primary, IEnumerable<IWritingSystemDefinition> exisitingWritingSystemsForLanguage)
		{
			_templateDefinition = WritingSystemDefinition.CreateCopyWithUniqueId(primary, exisitingWritingSystemsForLanguage.Select(ws=>ws.Id));
			this.Label = string.Format("other input system for {0}", _templateDefinition.LanguageName);
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
			this.Label = string.Format(_templateDefinition.ListLabel);
		}
		public override WritingSystemDefinition ShowDialogIfNeededAndGetDefinition()
		{
			return TemplateDefinition;
		}
	}
}