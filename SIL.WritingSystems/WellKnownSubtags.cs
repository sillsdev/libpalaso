namespace SIL.WritingSystems
{
	/// <summary>
	/// Well known subtags
	/// </summary>
	public static class WellKnownSubtags
	{
		/// <summary>
		/// An unlisted language.
		/// </summary>
		public const string UnlistedLanguage = "qaa";

		/// <summary>
		/// An unwritten script.
		/// </summary>
		public const string UnwrittenScript = "Zxxx";

		/// <summary>
		/// An audio private use subtag.
		/// </summary>
		public const string AudioPrivateUse = "x-audio";

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
		public const string IpaPhonemicPrivateUse = "x-emic";

		/// <summary>
		/// A phonetic IPA private use subtag.
		/// </summary>
		public const string IpaPhoneticPrivateUse = "x-etic";
	}
}