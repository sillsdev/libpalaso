using System.Globalization;

namespace SIL.WritingSystems
{
	public static class CultureInfoExtensions
	{
		public static bool IsUnknownCulture(this CultureInfo cultureInfo)
		{
			// Windows 10 changed the behavior of CultureInfo, in that unknown cultures no longer return a CultureInfo containing an "Unknown Language" indication.
			// The proper way to detect fully unknown cultures (for Windows 11 and prior) is to:
			//    1. Check for the custom culture flag
			//    2. Check if the three-letter language name is set to default
			// Source: https://stackoverflow.com/a/71388328/1964319
			return cultureInfo.CultureTypes.HasFlag(CultureTypes.UserCustomCulture) &&
			       cultureInfo.ThreeLetterWindowsLanguageName == "ZZZ";
		}
	}
}