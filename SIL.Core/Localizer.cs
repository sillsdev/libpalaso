using System;

namespace SIL
{
	public interface ILocalizer
	{
		string UILanguageId { get; }
		string GetString(string stringId, string englishText);
		string GetString(string stringId, string englishText, string comment);
		bool GetIsStringAvailableForLangId(string id, string targetLangId);
		string GetDynamicString(string appId, string id, string englishText);
		string GetDynamicString(string appId, string id, string englishText, string comment);
		string GetDynamicStringOrEnglish(string appId, string id, string englishText, string comment, string targetLangId);
	}

	public class Localizer
	{
		private static ILocalizer s_localizer = new EnglishLocalizer();

		public static ILocalizer Default
		{
			get
			{
				if (s_localizer == null)
					throw new ApplicationException("Not initialized. Set Localizer.Default first.");

				return s_localizer;
			}
			set => s_localizer = value;
		}

		public static string UILanguageId => Default.UILanguageId;

		public static string GetString(string stringId, string englishText)
		{
			return Default.GetString(stringId, englishText);
		}

		public static string GetString(string stringId, string englishText, string comment)
		{
			return Default.GetString(stringId, englishText, comment);
		}

		public static bool GetIsStringAvailableForLangId(string id, string targetLangId)
		{
			return Default.GetIsStringAvailableForLangId(id, targetLangId);
		}

		public static string GetDynamicString(string appId, string id, string englishText)
		{
			return Default.GetDynamicString(appId, id, englishText);
		}

		public static string GetDynamicString(string appId, string id, string englishText, string comment)
		{
			return Default.GetDynamicString(appId, id, englishText, comment);
		}

		public static string GetDynamicStringOrEnglish(string appId, string id, string englishText, string comment, string targetLangId)
		{
			return Default.GetDynamicStringOrEnglish(appId, id, englishText, comment, targetLangId);
		}
	}

	class EnglishLocalizer : ILocalizer
	{
		public string UILanguageId => "en";

		public string GetString(string stringId, string englishText)
		{
			return englishText;
		}

		public string GetString(string stringId, string englishText, string comment)
		{
			return englishText;
		}

		public bool GetIsStringAvailableForLangId(string stringId, string targetLangId)
		{
			if (targetLangId == "en")
				return true;
			return false;
		}

		public string GetDynamicString(string appId, string id, string englishText)
		{
			return englishText;
		}

		public string GetDynamicString(string appId, string id, string englishText, string comment)
		{
			return englishText;
		}

		public string GetDynamicStringOrEnglish(string appId, string id, string englishText, string comment, string targetLangId)
		{
			return englishText;
		}
	}
}
