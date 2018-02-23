using System;

namespace SIL
{
	public abstract class Localizer
	{
		private static Localizer s_localizer = new EnglishLocalizer();

		public static Localizer Default
		{
			get
			{
				if (s_localizer == null)
					throw new ApplicationException("Not initialized. Set Localizer.Default first.");

				return s_localizer;
			}
			set => s_localizer = value;
		}

		public static string GetString(string stringId, string englishText)
		{
			return Default.GetLocalizedString(stringId, englishText);
		}

		public static string GetString(string stringId, string englishText, string comment)
		{
			return Default.GetLocalizedString(stringId, englishText, comment);
		}

		public abstract string GetLocalizedString(string stringId, string englishText);

		public abstract string GetLocalizedString(string stringId, string englishText, string comment);
	}

	class EnglishLocalizer : Localizer
	{
		public override string GetLocalizedString(string stringId, string englishText)
		{
			return englishText;
		}

		public override string GetLocalizedString(string stringId, string englishText, string comment)
		{
			return englishText;
		}
	}
}
