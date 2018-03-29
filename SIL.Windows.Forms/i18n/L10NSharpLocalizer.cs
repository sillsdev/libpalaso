using L10NSharp;

namespace SIL.Windows.Forms.i18n
{
	public class L10NSharpLocalizer : Localizer
	{
		public override string GetLocalizedString(string stringId, string englishText)
		{
			return LocalizationManager.GetString(stringId, englishText);
		}

		public override string GetLocalizedString(string stringId, string englishText, string comment)
		{
			return LocalizationManager.GetString(stringId, englishText, comment);
		}
	}
}
