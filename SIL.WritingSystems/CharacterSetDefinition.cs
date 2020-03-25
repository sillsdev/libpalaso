using System;
using System.Collections.Specialized;
using System.Linq;
using SIL.Extensions;
using SIL.ObjectModel;

namespace SIL.WritingSystems
{
	public class CharacterSetDefinition : DefinitionBase<CharacterSetDefinition>
	{
		private static readonly string[] _sequenceTypes = {"footnotes", "crossrefs", "verseSegments" };
		private readonly string _type;
		private readonly ObservableList<string> _characters; 

		public CharacterSetDefinition(string type)
		{
			_type = type;
			_characters = new ObservableList<string>();
			SetupCollectionChangeListeners();
		}

		public CharacterSetDefinition(CharacterSetDefinition csd)
		{
			_type = csd._type;
			_characters = new ObservableList<string>(csd._characters);
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

		public bool IsSequenceType
		{
			get { return _sequenceTypes.Contains(_type); }
		}

		public IObservableList<string> Characters
		{
			get { return _characters; }
		}

		public override bool ValueEquals(CharacterSetDefinition other)
		{
			if (other == null || _type != other._type)
				return false;
			if (IsSequenceType)
				return _characters.SequenceEqual(other._characters);
			return _characters.SetEquals(other._characters);
		}

		public override CharacterSetDefinition Clone()
		{
			return new CharacterSetDefinition(this);
		}

		public override string ToString()
		{
			var bldr = new System.Text.StringBuilder();
			foreach (var c in Characters)
				bldr.Append($" {c}");
			return $"{nameof(CharacterSetDefinition)}[{_type}]: ({Characters.Count}) {bldr}";
		}
	}
}
