using System;
using System.Collections.Specialized;
using System.Linq;
using SIL.ObjectModel;

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
		private readonly string _name;
		private float _relativeSize;
		private string _features;
		private string _language;
		private string _openTypeLanguage;
		private string _minVersion;
		private FontRoles _roles;
		private FontEngines _engines;
		private string _subset;

		private void SetupCollectionChangeListeners()
		{
			Urls.CollectionChanged += UrlsCollectionChanged;
		}

		private void UrlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public FontDefinition(string name)
		{
			_name = name;
			_relativeSize = 1.0f;
			_engines = FontEngines.Graphite | FontEngines.OpenType;
			_roles = FontRoles.Default;
			Urls = new ObservableSortedSet<string>();
			SetupCollectionChangeListeners();
		}

		public FontDefinition(FontDefinition fd)
		{
			_name = fd._name;
			_relativeSize = fd._relativeSize;
			_features = fd._features;
			_language = fd._language;
			_openTypeLanguage = fd._openTypeLanguage;
			_minVersion = fd._minVersion;
			_roles = fd._roles;
			_engines = fd._engines;
			_subset = fd._subset;
			Urls = new ObservableSortedSet<string>(fd.Urls);
			SetupCollectionChangeListeners();
		}

		public string Name
		{
			get { return _name; }
		}

		public float RelativeSize
		{
			get { return _relativeSize; }
			set
			{
				if (value < 0 || float.IsNaN(value) || float.IsInfinity(value))
					throw new ArgumentOutOfRangeException("value");
				Set(() => RelativeSize, ref _relativeSize, value);
			}
		}

		public string Features
		{
			get { return _features ?? string.Empty; }
			set { Set(() => Features, ref _features, value); }
		}

		public string Language
		{
			get { return _language ?? string.Empty; }
			set { Set(() => Language, ref _language, value); }
		}

		public string OpenTypeLanguage
		{
			get { return _openTypeLanguage ?? string.Empty; }
			set { Set(() => OpenTypeLanguage, ref _openTypeLanguage, value); }
		}

		public string MinVersion
		{
			get { return _minVersion ?? string.Empty; }
			set { Set(() => MinVersion, ref _minVersion, value); }
		}

		public FontRoles Roles
		{
			get { return _roles; }
			set { Set(() => Roles, ref _roles, value); }
		}

		public FontEngines Engines
		{
			get { return _engines; }
			set { Set(() => Engines, ref _engines, value); }
		}

		public string Subset
		{
			get { return _subset ?? string.Empty; }
			set { Set(() => Subset, ref _subset, value); }
		}

		public IObservableSet<string> Urls { get; }

		public override bool ValueEquals(FontDefinition other)
		{
			if (other == null)
				return false;
			// ReSharper disable once CompareOfFloatsByEqualityOperator
			return _name == other._name && _relativeSize == other._relativeSize && Features == other.Features && Language == other.Language
				&& OpenTypeLanguage == other.OpenTypeLanguage && MinVersion == other.MinVersion && _roles == other._roles
				&& _engines == other._engines && Subset == other.Subset && Urls.SequenceEqual(other.Urls);
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
