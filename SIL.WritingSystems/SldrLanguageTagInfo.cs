namespace SIL.WritingSystems
{
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

		public string LanguageTag
		{
			get { return _langTag; }
		}

		public string ImplicitScriptCode
		{
			get { return _implicitScriptCode; }
		}

		public string SldrLanguageTag
		{
			get { return _sldrLanguageTag; }
		}

		public bool IsAvailable
		{
			get { return _isAvailable; }
		}
	}
}
