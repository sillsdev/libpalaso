using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Palaso.Code;

namespace SIL.WritingSystems
{
	[Flags]
	public enum FontEngines
	{
		OpenType,
		Graphite
	}

	public class FontDefinition : ICloneable<FontDefinition>
	{
		private const int MinimumFontSize = 7;
		private const int DefaultSizeIfWeDontKnow = 10;

		public const string DefaultRole = "default";
		public const string HeadingRole = "heading";
		public const string EmphasisRole = "emphasis";

		private readonly string _name;
		private float _defaultSize;
		private string _features;
		private string _language;
		private string _openTypeLanguage;
		private string _minVersion;
		private readonly ObservableCollection<string> _roles;
		private FontEngines _engines;
		private string _subset;

		public FontDefinition(string name)
		{
			_name = name;
			_roles = new ObservableCollection<string>();
		}

		public FontDefinition(FontDefinition fd)
		{
			_name = fd._name;
			_defaultSize = fd._defaultSize;
			_features = fd._features;
			_language = fd._language;
			_openTypeLanguage = fd._openTypeLanguage;
			_minVersion = fd._minVersion;
			_roles = new ObservableCollection<string>(fd._roles);
			_engines = fd._engines;
			_subset = fd._subset;
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
			get { return _features; }
			set { UpdateString(ref _features, value); }
		}

		public string Language
		{
			get { return _language; }
			set { UpdateString(ref _language, value); }
		}

		public string OpenTypeLanguage
		{
			get { return _openTypeLanguage; }
			set { UpdateString(ref _openTypeLanguage, value); }
		}

		public string MinVersion
		{
			get { return _minVersion; }
			set { UpdateString(ref _minVersion, value); }
		}

		public ICollection<string> Roles
		{
			get { return _roles; }
		}

		public FontEngines Engines
		{
			get { return _engines; }
			set { UpdateField(ref _engines, value); }
		}

		public string Subset
		{
			get { return _subset; }
			set { UpdateString(ref _subset, value); }
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

		private bool UpdateString(ref string field, string value)
		{
			//count null as same as ""
			if (String.IsNullOrEmpty(field) && String.IsNullOrEmpty(value))
				return false;

			return UpdateField(ref field, value);
		}

		/// <summary>
		/// Updates the specified field and marks the writing system as modified.
		/// </summary>
		private bool UpdateField<T>(ref T field, T value)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
				return false;

			Modified = true;
			field = value;
			return true;
		}

		public bool Modified { get; private set; }

		public void ResetModified()
		{
			Modified = false;
		}

		public bool ValueEquals(FontDefinition other)
		{
			if (other == null)
				return false;
			return _name == other._name && DefaultSize == other.DefaultSize && Features == other.Features && Language == other.Language
				&& OpenTypeLanguage == other.OpenTypeLanguage && MinVersion == other.MinVersion && _roles.SequenceEqual(other._roles)
				&& Engines == other.Engines && Subset == other.Subset;
		}

		public FontDefinition Clone()
		{
			return new FontDefinition(this);
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
