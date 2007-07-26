namespace Palaso.WritingSystems
{
	public class WritingSystemDefinition
	{
		private string _iso;
		private string _region;
		private string _variant;
		private string _languageName;
		private string _script;
		private string _abbreviation;

		/// <summary>
		/// Other classes that persist this need to know when our id changed, so the can
		/// clean up the old copy which is based on the old name.
		/// </summary>
		private string _previousRepositoryIdentifier;

		public string Variant
		{
			get
			{
				return _variant;
			}
			set
			{
				if(_variant == value)
					return;
				_variant = value;
			}
		}

		public string Region
		{
			get
			{
				return _region;
			}
			set
			{
				if (_region == value)
					return;
				_region = value;
			}
		}

		public string ISO
		{
			get
			{
				return _iso;
			}
			set
			{
				if (_iso == value)
					return;
				_iso = value;
			}
		}

		public string Abbreviation
		{
			get
			{
				return _abbreviation;
			}
			set
			{
				if (_abbreviation == value)
					return;
				_abbreviation = value;
			}
		}

		public string Script
		{
			get
			{
				return _script;
			}
			set
			{
				if (_script == value)
					return;
				_script = value;
			}
		}

		public string LanguageName
		{
			get
			{
				return _languageName;
			}
			set
			{
				_languageName = value;
			}
		}

		/// <summary>
		/// Other classes that persist this need to know when our id changed, so the can
		/// clean up the old copy which is based on the old name.
		/// </summary>
		public string PreviousRepositoryIdentifier
		{
			get
			{
				return _previousRepositoryIdentifier;
			}
			set
			{
				_previousRepositoryIdentifier = value;
			}
		}
	}
}