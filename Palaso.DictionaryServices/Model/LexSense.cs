using System;
using System.Collections.Generic;
using System.ComponentModel;
using Palaso.Lift;
using Palaso.Reporting;

namespace Palaso.DictionaryServices.Model
{
	public sealed class LexSense: PalasoDataObject
	{
		//private readonly SenseGlossMultiText _gloss;
		private readonly BindingList<LexExampleSentence> _exampleSentences;
		private readonly BindingList<LexNote> _notes;
		private string _id;

		public new class WellKnownProperties: PalasoDataObject.WellKnownProperties
		{
			public static string PartOfSpeech = "POS";
			public static string SemanticDomainDdp4 = "semantic-domain-ddp4";

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
			if (String.IsNullOrEmpty(_id))
			{
				_id = Guid.NewGuid().ToString();
				NotifyPropertyChanged("id");
			}

			return _id;
		}

		public MultiText Gloss
		{
			get { return GetOrCreateProperty<MultiText>(WellKnownProperties.Gloss); }
		}

		public MultiText Definition
		{
			get { return GetOrCreateProperty<MultiText>(WellKnownProperties.Definition); }
		}

		public IList<LexExampleSentence> ExampleSentences
		{
			get { return _exampleSentences; }
		}

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, as it only handles a single, typeless note and uses the well-known-properties approach
		/// </summary>
		public IList<LexNote> Notes
		{
			get { return _notes; }
		}

		public override bool IsEmpty
		{
			get { return Gloss.Empty && ExampleSentences.Count == 0 && !HasProperties; }
		}

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

		public string Id
		{
			get { return _id; }
			set { _id = value; }
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
	}
}