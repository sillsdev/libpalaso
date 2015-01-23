using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

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
		private readonly string _languageTag;
		private readonly SpellCheckDictionaryFormat _format;
		private readonly ObservableCollection<string> _urls;

		private void SetupCollectionChangeListeners()
		{
			_urls.CollectionChanged += _urls_CollectionChanged;
		}

		private void _urls_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public SpellCheckDictionaryDefinition(string languageTag, SpellCheckDictionaryFormat format)
		{
			_languageTag = languageTag;
			_format = format;
			_urls = new ObservableCollection<string>();
			SetupCollectionChangeListeners();
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryDefinition other)
		{
			_languageTag = other._languageTag;
			_format = other._format;
			_urls = new ObservableCollection<string>();
			foreach (string url in other._urls)
				_urls.Add(url);
			SetupCollectionChangeListeners();
		}

		public string Id
		{
			get { return string.Format("{0}_{1}", _languageTag, _format); }
		}

		public string LanguageTag
		{
			get { return _languageTag; }
		}

		public SpellCheckDictionaryFormat Format
		{
			get { return _format; }
		}

		public ObservableCollection<string> Urls
		{
			get { return _urls; }
		}

		public override bool ValueEquals(SpellCheckDictionaryDefinition other)
		{
			if (other == null)
				return false;
			return _languageTag == other._languageTag && _format == other._format && Urls.SequenceEqual(other.Urls);
		}

		public override SpellCheckDictionaryDefinition Clone()
		{
			return new SpellCheckDictionaryDefinition(this);
		}
	}
}
