using System;

namespace SIL
{
	public interface ILocalizer
	{
		string UILanguageId { get; }
		string GetString(string stringId, string englishText);
		string GetString(string stringId, string englishText, string comment);
		string GetDynamicString(string appId, string id, string englishText);
		string GetDynamicString(string appId, string id, string englishText, string comment);
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

		public static string GetDynamicString(string appId, string id, string englishText)
		{
			return Default.GetDynamicString(appId, id, englishText);
		}

		public static string GetDynamicString(string appId, string id, string englishText, string comment)
		{
			return Default.GetDynamicString(appId, id, englishText, comment);
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

		public string GetDynamicString(string appId, string id, string englishText)
		{
			return englishText;
		}

		public string GetDynamicString(string appId, string id, string englishText, string comment)
		{
			return englishText;
		}
	}
}
