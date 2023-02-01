using System.Collections.Specialized;
using System.Linq;
using SIL.ObjectModel;

namespace SIL.WritingSystems
{
	public enum SpellCheckDictionaryFormat
	{
		Hunspell,
		Wordlist,
		Lift
	}

	public class SpellCheckDictionaryDefinition : DefinitionBase<SpellCheckDictionaryDefinition>
	{
		private readonly SpellCheckDictionaryFormat _format;

		private void SetupCollectionChangeListeners()
		{
			Urls.CollectionChanged += _urls_CollectionChanged;
		}

		private void _urls_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryFormat format)
		{
			_format = format;
			Urls = new ObservableSortedSet<string>();
			SetupCollectionChangeListeners();
		}

		public SpellCheckDictionaryDefinition(SpellCheckDictionaryDefinition other)
		{
			_format = other._format;
			Urls = new ObservableSortedSet<string>(other.Urls);
			SetupCollectionChangeListeners();
		}

		public SpellCheckDictionaryFormat Format
		{
			get { return _format; }
		}

		public IObservableSet<string> Urls { get; }

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
