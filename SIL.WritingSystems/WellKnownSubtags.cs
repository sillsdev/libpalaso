namespace SIL.WritingSystems
{
	/// <summary>
	/// Well known subtags
	/// </summary>
	public static class WellKnownSubtags
	{
		/// <summary>
		/// An unlisted language.
		/// According to https://datatracker.ietf.org/doc/html/rfc5646, the subtags in the range
		/// 'qaa' through 'qtz' (corresponding to codes reserved by ISO 639-2 for private use)
		/// are reserved for private use in language tags. These codes MAY be used for non-
		/// registered primary language subtags (instead of using private use subtags following
		/// 'x-'). Our practice in SIL is to use qaa as a generic "Unlisted Language". When used
		/// to represent a specific language (typically one whose registration is in process), it
		/// will generally be followed by x- and a provisional language code.
		/// </summary>
		public const string UnlistedLanguage = "qaa";

		/// <summary>
		/// An unwritten script.
		/// </summary>
		public const string UnwrittenScript = "Zxxx";

		/// <summary>
		/// An audio private use subtag.
		/// </summary>
		public const string AudioPrivateUse = "audio";

		/// <summary>
		/// An audio script.
		/// </summary>
		public const string AudioScript = UnwrittenScript;

		/// <summary>
		/// An IPA variant.
		/// </summary>
		public const string IpaVariant = "fonipa";

		/// <summary>
		/// A phonemic IPA private use subtag.
		/// </summary>
		public const string IpaPhonemicPrivateUse = "emic";

		/// <summary>
		/// A phonetic IPA private use subtag.
		/// </summary>
		public const string IpaPhoneticPrivateUse = "etic";

		/// <summary>
		/// A pinyin variant.
		/// </summary>
		public const string PinyinVariant = "pinyin";

		/// <summary>
		/// Simplified (as opposed to traditional) Chinese
		/// </summary>
		public const string ChineseSimplifiedTag = "zh-CN";

		/// <summary>
		/// Traditional (as opposed to simplified) Chinese
		/// </summary>
		public const string ChineseTraditionalTag = "zh-TW";
	}
}