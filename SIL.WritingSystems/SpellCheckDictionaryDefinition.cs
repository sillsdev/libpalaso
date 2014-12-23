namespace SIL.WritingSystems
{
	public enum SpellCheckDictionaryFormat
	{
		Unknown,
		Hunspell,
		Wordlist,
		Lift
	}

	public class SpellCheckDictionaryDefinition : DefinitionBase<SpellCheckDictionaryDefinition>
	{
		private readonly string _id;
		private string _url;
		private SpellCheckDictionaryFormat _format;

		public SpellCheckDictionaryDefinition(string id)
		{
			_id = id;
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryDefinition other)
		{
			_id = other._id;
			_format = other._format;
			_url = other._url;
		}

		public string Id
		{
			get { return _id; }
		}

		public SpellCheckDictionaryFormat Format
		{
			get { return _format; }
			set { UpdateField(ref _format, value); }
		}

		public string Url
		{
			get { return _url ?? string.Empty; }
			set { UpdateString(ref _url, value); }
		}

		public override bool ValueEquals(SpellCheckDictionaryDefinition other)
		{
			if (other == null)
				return false;
			return _id == other._id && _format == other._format && Url == other.Url;
		}

		public override SpellCheckDictionaryDefinition Clone()
		{
			return new SpellCheckDictionaryDefinition(this);
		}
	}
}
