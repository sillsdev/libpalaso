namespace SIL.WritingSystems
{
	/// <summary>
	/// This class represents a language from the IANA language subtag registry.
	/// </summary>
	public class LanguageSubtag : Subtag
	{
		private readonly string _iso3Code;

		public LanguageSubtag(string code, bool isPrivateUse)
			: this(code, null, isPrivateUse, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:LanguageSubtag"/> class.
		/// </summary>
		/// <param name="code">The code.</param>
		/// <param name="name">The name.</param>
		/// <param name="isPrivateUse">if set to <c>true</c> this is a private use subtag.</param>
		/// <param name="iso3Code">The ISO 639-3 language code.</param>
		public LanguageSubtag(string code, string name, bool isPrivateUse, string iso3Code)
			: base(code, name, isPrivateUse)
		{
			_iso3Code = iso3Code;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:LanguageSubtag"/> class.
		/// </summary>
		/// <param name="subtag">The subtag.</param>
		/// <param name="name">The name.</param>
		public LanguageSubtag(LanguageSubtag subtag, string name)
			: this(subtag.Code, name, subtag.IsPrivateUse, subtag._iso3Code)
		{
		}

		/// <summary>
		/// Gets the ISO 639-3 language code.
		/// </summary>
		/// <value>The ISO 639-3 language code.</value>
		public string Iso3Code
		{
			get { return _iso3Code; }
		}

		public static implicit operator LanguageSubtag(string code)
		{
			if (string.IsNullOrEmpty(code))
				return null;

			LanguageSubtag subtag;
			if (!StandardSubtags.Iso639Languages.TryGet(code, out subtag))
				subtag = new LanguageSubtag(code, true);
			return subtag;
		}
	}
}
