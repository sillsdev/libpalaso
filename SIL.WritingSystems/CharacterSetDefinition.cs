using System.Collections.Specialized;
using SIL.ObjectModel;

namespace SIL.WritingSystems
{
	public class CharacterSetDefinition : DefinitionBase<CharacterSetDefinition>
	{
		private readonly string _type;
		private readonly ObservableHashSet<string> _characters; 

		public CharacterSetDefinition(string type)
		{
			_type = type;
			_characters = new ObservableHashSet<string>();
			SetupCollectionChangeListeners();
		}

		public CharacterSetDefinition(CharacterSetDefinition csd)
		{
			_type = csd._type;
			_characters = new ObservableHashSet<string>(csd._characters);
			SetupCollectionChangeListeners();
		}

		private void SetupCollectionChangeListeners()
		{
			_characters.CollectionChanged += _characters_CollectionChanged;
		}

		private void _characters_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}

		public string Type
		{
			get { return _type; }
		}

		public IObservableSet<string> Characters
		{
			get { return _characters; }
		}

		public override bool ValueEquals(CharacterSetDefinition other)
		{
			return other != null && _type == other._type && _characters.SetEquals(other._characters);
		}

		public override CharacterSetDefinition Clone()
		{
			return new CharacterSetDefinition(this);
		}
	}
}
