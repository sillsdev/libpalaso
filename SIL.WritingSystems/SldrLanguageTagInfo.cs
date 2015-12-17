namespace SIL.WritingSystems
{
	/// <summary>
	/// This class encapsulates all of the information for a language tag in the SLDR.
	/// </summary>
	public class SldrLanguageTagInfo
	{
		private readonly string _langTag;
		private readonly string _implicitScriptCode;
		private readonly string _sldrLanguageTag;
		private readonly bool _isAvailable;

		public SldrLanguageTagInfo(string langTag, string implicitScriptCode, string sldrLanguageTag, bool isAvailable)
		{
			_langTag = langTag;
			_implicitScriptCode = implicitScriptCode;
			_sldrLanguageTag = sldrLanguageTag;
			_isAvailable = isAvailable;
		}

		/// <summary>
		/// The canonical language tag.
		/// </summary>
		public string LanguageTag
		{
			get { return _langTag; }
		}

		/// <summary>
		/// Gets the implicit script code for this language tag. This will be null if there is no script code or
		/// the script is explicit.
		/// </summary>
		public string ImplicitScriptCode
		{
			get { return _implicitScriptCode; }
		}

		/// <summary>
		/// Gets the corresponding language tag that the SLDR uses for this language tag. This allows us to map
		/// a canonical language tag back to the corresponding SLDR language tag, so we can lookup the correct
		/// LDML file.
		/// </summary>
		public string SldrLanguageTag
		{
			get { return _sldrLanguageTag; }
		}

		/// <summary>
		/// Gets a value indicating whether there is a LDML file available in the SLDR for this language tag.
		/// </summary>
		public bool IsAvailable
		{
			get { return _isAvailable; }
		}
	}
}
