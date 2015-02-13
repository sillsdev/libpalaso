using System.Collections.Specialized;
using System.Linq;
using SIL.ObjectModel;

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
		private readonly ObservableList<string> _urls;

		private void SetupCollectionChangeListeners()
		{
			_urls.CollectionChanged += _urls_CollectionChanged;
		}

		private void _urls_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat format)
		{
			_format = format;
			_urls = new ObservableList<string>();
			SetupCollectionChangeListeners();
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryDefinition other)
		{
			_format = other._format;
			_urls = new ObservableList<string>(other._urls);
			SetupCollectionChangeListeners();
		}

		public SpellCheckDictionaryFormat Format
		{
			get { return _format; }
		}

		public IObservableList<string> Urls
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
