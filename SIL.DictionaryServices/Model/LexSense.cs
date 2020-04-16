using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SIL.Lift;
using SIL.ObjectModel;
using SIL.Reporting;

namespace SIL.DictionaryServices.Model
{
	public sealed class LexSense : PalasoDataObject, ICloneable<LexSense>, IEquatable<LexSense>
	{
		//private readonly SenseGlossMultiText _gloss;
		private readonly BindingList<LexExampleSentence> _exampleSentences;
		private readonly BindingList<LexNote> _notes;
		private readonly BindingList<LexReversal> _reversals;

		public new class WellKnownProperties: PalasoDataObject.WellKnownProperties
		{
			public static string PartOfSpeech = "POS";
			public static string SemanticDomainDdp4 = "semantic-domain-ddp4";
			public static string SILCAWL = "SILCAWL";

			public static string Definition = "definition";
			//the lower case here is defined by LIFT standard

			public static string Picture = "Picture";
			public static string Gloss = "gloss";
			//static public string Relations = "relations";
			public static bool ContainsAnyCaseVersionOf(string fieldName)
			{
				List<string> list =
					new List<string>(new string[]
										 {
											 PartOfSpeech, Definition, SemanticDomainDdp4, Picture
											 , Note, Gloss
										 });
				return list.Contains(fieldName) || list.Contains(fieldName.ToLower()) ||
					   list.Contains(fieldName.ToUpper());
			}
		} ;

		public LexSense(PalasoDataObject parent): base(parent)
		{
			//   _gloss = new SenseGlossMultiText(this);
			_exampleSentences = new BindingList<LexExampleSentence>();
			_notes = new BindingList<LexNote>();
			_reversals = new BindingList<LexReversal>();
			WireUpEvents();
		}

		/// <summary>
		/// Used when a list of these items adds via "AddNew", where we have to have a default constructor.
		/// The parent is added in an event handler, on the parent, which is called by the list.
		/// </summary>
		public LexSense(): this(null) {}

		protected override void WireUpEvents()
		{
			base.WireUpEvents();
			//WireUpChild(_gloss);
			WireUpList(_exampleSentences, "exampleSentences");
		}

		public string GetOrCreateId()
		{
			if (String.IsNullOrEmpty(Id))
			{
				Id = Guid.NewGuid().ToString();
				NotifyPropertyChanged("id");
			}

			return Id;
		}


		public void AddRelationTarget(string relationName, string targetId)
		{
			LexRelationCollection relations =
				GetOrCreateProperty<LexRelationCollection>(relationName);
			relations.Relations.Add(new LexRelation(relationName, targetId, this));
		}
		public MultiText Gloss => GetOrCreateProperty<MultiText>(WellKnownProperties.Gloss);

		public MultiText Definition => GetOrCreateProperty<MultiText>(WellKnownProperties.Definition);

		public IList<LexExampleSentence> ExampleSentences => _exampleSentences;

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, as it only handles a single, typeless note and uses the well-known-properties approach
		/// </summary>
		public IList<LexNote> Notes => _notes;

		public IList<LexReversal> Reversals => _reversals;

		public override bool IsEmpty => Gloss.Empty && ExampleSentences.Count == 0 && !HasProperties;

		public bool IsEmptyForPurposesOfDeletion
		{
			get
			{
				MultiText gloss =
					GetProperty<MultiText>(WellKnownProperties.Gloss);
				bool noGloss = (gloss == null) || gloss.Empty;
				// careful, just asking the later will actually create a gloss
				return noGloss && ExampleSentences.Count == 0 && !HasPropertiesForPurposesOfDeletion;
			}
		}

		public string Id { get; set; }

		public new IEnumerable<string> PropertiesInUse
		{
			get { return base.PropertiesInUse.Concat(ExampleSentences.SelectMany(ex=>ex.PropertiesInUse)); }
		}

		public override void CleanUpAfterEditting()
		{
			base.CleanUpAfterEditting();
			foreach (LexExampleSentence sentence in _exampleSentences)
			{
				sentence.CleanUpAfterEditting();
			}
			CleanUpEmptyObjects();
		}

		public override void CleanUpEmptyObjects()
		{
			base.CleanUpEmptyObjects();

			for (int i = 0;i < _exampleSentences.Count;i++)
			{
				_exampleSentences[i].CleanUpEmptyObjects();
			}

			// remove any example sentences that are empty
			int count = _exampleSentences.Count;

			for (int i = count - 1;i >= 0;i--)
			{
				if (_exampleSentences[i].IsEmpty)
				{
					_exampleSentences.RemoveAt(i);
				}
			}
			if (count != _exampleSentences.Count)
			{
				Logger.WriteMinorEvent("Empty example removed");
				OnEmptyObjectsRemoved();
			}
		}

		public LexSense Clone()
		{
			var clone = new LexSense();
			foreach (var exampleSentenceToClone in _exampleSentences)
			{
				clone._exampleSentences.Add(exampleSentenceToClone.Clone());
			}
			foreach (var note in _notes)
			{
				clone._notes.Add((LexNote) note.Clone());
			}
			foreach (var lexReversal in Reversals)
			{
				clone.Reversals.Add((LexReversal) lexReversal.Clone());
			}
			foreach (var keyValuePairToClone in Properties)
			{
				clone.AddProperty(keyValuePairToClone.Key, keyValuePairToClone.Value.Clone());
			}
			return clone;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as LexSense);
		}

		public bool Equals(LexSense other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			//The various components may appear in a different order which is fine. So we sort them here so that we can run SequenceEqual (which cares about order) over them.
			if (!_exampleSentences.SequenceEqual(other._exampleSentences)) return false;
			if (!_notes.SequenceEqual(other._notes)) return false;
			if (!Reversals.OrderBy(x=>x).SequenceEqual(other.Reversals.OrderBy(x=>x))) return false;
			if (!base.Equals(other)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// https://stackoverflow.com/a/263416/487503
			unchecked // Overflow is fine, just wrap
			{
				var hash = 17;
				hash *= 67 + _exampleSentences.GetHashCode();
				hash *= 67 + _notes.GetHashCode();
				hash *= 67 + Reversals.GetHashCode();
				hash *= 67 + base.GetHashCode();
				return hash;
			}
		}
	}
}