using System;

namespace SIL.WritingSystems
{
	[Flags]
	public enum FontEngines
	{
		None = 0,
		OpenType = 1,
		Graphite = 2
	}

	[Flags]
	public enum FontRoles
	{
		None = 0,
		Default = 1,
		Heading = 2,
		Emphasis = 4
	}

	public class FontDefinition : DefinitionBase<FontDefinition>
	{
		private const int MinimumFontSize = 7;
		private const int DefaultSizeIfWeDontKnow = 10;

		private readonly string _name;
		private float _defaultSize;
		private string _features;
		private string _language;
		private string _openTypeLanguage;
		private string _minVersion;
		private FontRoles _roles;
		private FontEngines _engines;
		private string _subset;
		private string _url;

		public FontDefinition(string name)
		{
			_name = name;
		}

		public FontDefinition(FontDefinition fd)
		{
			_name = fd._name;
			_defaultSize = fd._defaultSize;
			_features = fd._features;
			_language = fd._language;
			_openTypeLanguage = fd._openTypeLanguage;
			_minVersion = fd._minVersion;
			_roles = fd._roles;
			_engines = fd._engines;
			_subset = fd._subset;
			_url = fd._url;
		}

		public string Name
		{
			get { return _name; }
		}

		public float DefaultSize
		{
			get { return _defaultSize; }
			set
			{
				if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
					throw new ArgumentOutOfRangeException("value");
				UpdateField(ref _defaultSize, value);
			}
		}

		public string Features
		{
			get { return _features ?? string.Empty; }
			set { UpdateString(ref _features, value); }
		}

		public string Language
		{
			get { return _language ?? string.Empty; }
			set { UpdateString(ref _language, value); }
		}

		public string OpenTypeLanguage
		{
			get { return _openTypeLanguage ?? string.Empty; }
			set { UpdateString(ref _openTypeLanguage, value); }
		}

		public string MinVersion
		{
			get { return _minVersion ?? string.Empty; }
			set { UpdateString(ref _minVersion, value); }
		}

		public FontRoles Roles
		{
			get { return _roles; }
			set { UpdateField(ref _roles, value); }
		}

		public FontEngines Engines
		{
			get { return _engines; }
			set { UpdateField(ref _engines, value); }
		}

		public string Subset
		{
			get { return _subset ?? string.Empty; }
			set { UpdateString(ref _subset, value); }
		}

		public string Url
		{
			get { return _url ?? string.Empty; }
			set { UpdateString(ref _url, value); }
		}

		/// <summary>
		/// enforcing a minimum on _defaultFontSize, while reasonable, just messed up too many IO unit tests
		/// </summary>
		/// <returns></returns>
		public float GetDefaultFontSizeOrMinimum()
		{
			if (_defaultSize < MinimumFontSize)
				return DefaultSizeIfWeDontKnow;
			return _defaultSize;
		}

		public override bool ValueEquals(FontDefinition other)
		{
			if (other == null)
				return false;
			return _name == other._name && _defaultSize == other._defaultSize && Features == other.Features && Language == other.Language
				&& OpenTypeLanguage == other.OpenTypeLanguage && MinVersion == other.MinVersion && _roles == other._roles
				&& _engines == other._engines && Subset == other.Subset && Url == other.Url;
		}

		public override FontDefinition Clone()
		{
			return new FontDefinition(this);
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
