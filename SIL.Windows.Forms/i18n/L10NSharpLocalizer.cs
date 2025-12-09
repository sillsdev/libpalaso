using L10NSharp;

namespace SIL.Windows.Forms.i18n
{
	public class L10NSharpLocalizer : ILocalizer
	{
		public string UILanguageId => LocalizationManager.UILanguageId;

		public string GetString(string stringId, string englishText)
		{
			return LocalizationManager.GetString(stringId, englishText);
		}

		public string GetString(string stringId, string englishText, string comment)
		{
			return LocalizationManager.GetString(stringId, englishText, comment);
		}

		public bool GetIsStringAvailableForLangId(string id, string targetLangId)
		{
			return LocalizationManager.GetIsStringAvailableForLangId(id, targetLangId);
		}
		public string GetDynamicString(string appId, string id, string englishText)
		{
			return LocalizationManager.GetDynamicString(appId, id, englishText);
		}

		public string GetDynamicString(string appId, string id, string englishText, string comment)
		{
			return LocalizationManager.GetDynamicString(appId, id, englishText, comment);
		}

		public string GetDynamicStringOrEnglish(string appId, string id, string englishText, string comment, string targetLangId)
		{
			return LocalizationManager.GetDynamicStringOrEnglish(appId, id, englishText, comment, targetLangId);
		}
	}
}
