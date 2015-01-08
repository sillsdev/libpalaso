using System.Collections.Generic;
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
		private readonly SpellCheckDictionaryFormat _format;
		private ObservableCollection<string> _urls;

		private void SetupCollectionChangeListeners()
		{
			_urls.CollectionChanged += UrlsCollectionChanged;
		}

		private void UrlsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat format)
		{
			_format = format;
			_urls = new ObservableCollection<string>();
			SetupCollectionChangeListeners();
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryDefinition other)
		{
			_format = other._format;
			_urls = new ObservableCollection<string>();
			foreach (string url in other._urls)
			{
				_urls.Add(url);
			}
			SetupCollectionChangeListeners();
		}

		public SpellCheckDictionaryFormat Format
		{
			get { return _format; }
		}

		public IList<string> Urls
		{
			get { return _urls; }
		}

		public override bool ValueEquals(SpellCheckDictionaryDefinition other)
		{
			if (other == null)
				return false;
			return _format == other._format && Urls.SequenceEqual(other.Urls);
		}

		public override SpellCheckDictionaryDefinition Clone()
		{
			return new SpellCheckDictionaryDefinition(this);
		}
	}
}
