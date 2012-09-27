using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Palaso.Code;
using Palaso.Data;
using Palaso.i18n;
using Palaso.Lift;
using Palaso.Reporting;
using Palaso.Text;
//using Palaso.UI.WindowsForms.i18n;

namespace Palaso.DictionaryServices.Model
{
	/// <summary>
	/// A Lexical Entry is what makes up our lexicon/dictionary.  In
	/// some languages/dictionaries, these will be indistinguishable from "words".
	/// In others, words are made up of lexical entries.
	/// </summary>
	public class LexEntry: PalasoDataObject, IClonableGeneric<LexEntry>
	{
		private MultiText _lexicalForm;
		private Guid _guid;

		/// <summary>
		/// This use for keeping track of the item when importing an then exporting again,
		/// like for merging. The user doesn't edit this, and if it is null that's fine,
		/// the exporter will make up a reasonable one.
		/// </summary>
		private string _id;

		private int _orderForRoundTripping;
		private int _orderInFile;

		private BindingList<LexSense> _senses;
		//NB: to help with possible confusion: as of wesay 0.9 (oct 2010), wesay doesn't use these lists (it just shoves them in embedded xml), but SOLID does
		private  BindingList<LexVariant> _variants;
		private  BindingList<LexNote> _notes;
		private BindingList<LexPhonetic> _pronunciations;
		private BindingList<LexEtymology> _etymologies;

		private DateTime _creationTime;
		private DateTime _modificationTime;
		private bool _isBeingDeleted;
		private bool _isDirty;

		[NonSerialized]
		private bool _modifiedTimeIsLocked;

		//!!What!! Is this done this way so that we don't end up storing
		//  the data in the object database?
		public new class WellKnownProperties: PalasoDataObject.WellKnownProperties
		{
			public static string Citation = "citation";
			public static string LexicalUnit = "EntryLexicalForm";
			public static string BaseForm = "BaseForm";
			public static string CrossReference = "confer";
			public static string Sense = "sense";
			public static string FlagSkipBaseform = "flag-skip-BaseForm";
			public static string LiteralMeaning = "literal-meaning";

			public static bool Contains(string fieldName)
			{
				List<string> list =
					new List<string>(new string[] { LexicalUnit, Citation, BaseForm, CrossReference, Sense, LiteralMeaning });
				return list.Contains(fieldName);
			}
		} ;

		public LexEntry(): this(null, Guid.NewGuid()) {}

		public LexEntry(string id, Guid guid): base(null)
		{
			DateTime now = PreciseDateTime.UtcNow;
			_isDirty = true;
			Init(id, guid, now, now);
		}

		private void Init(string id, Guid guid, DateTime creationTime, DateTime modifiedTime)
		{
			ModificationTime = modifiedTime;
			ModifiedTimeIsLocked = true;

			Id = id;
			if (guid == Guid.Empty)
			{
				_guid = Guid.NewGuid();
			}
			else
			{
				_guid = guid;
			}
			_lexicalForm = new MultiText(this);
			_senses = new BindingList<LexSense>();
			_variants = new BindingList<LexVariant>();
			_notes = new BindingList<LexNote>();
			_pronunciations = new BindingList<LexPhonetic>();
			_etymologies = new BindingList<LexEtymology>();

			CreationTime = creationTime;

			WireUpEvents();

			ModifiedTimeIsLocked = false;
		}

		public LexEntry Clone()
		{
			var clone = new LexEntry();
			clone._lexicalForm = (MultiText) _lexicalForm.Clone();
			//_lexicalForm and Guid must have been set before _id is set
			if(_id != null)
			{
				clone.GetOrCreateId(false);
			}
			clone.OrderForRoundTripping = _orderForRoundTripping;
			clone._orderInFile = _orderInFile;
			foreach (var senseToClone in _senses)
			{
				clone._senses.Add(senseToClone.Clone());
			}
			foreach (var lexVariantToClone in Variants)
			{
				clone.Variants.Add((LexVariant) lexVariantToClone.Clone());
			}
			foreach (var lexNoteToClone in Notes)
			{
				clone.Notes.Add((LexNote) lexNoteToClone.Clone());
			}
			foreach (var pronunciationToClone in _pronunciations)
			{
				clone._pronunciations.Add((LexPhonetic) pronunciationToClone.Clone());
			}
			foreach (var etymologyToClone in _etymologies)
			{
				clone._etymologies.Add((LexEtymology)etymologyToClone.Clone());
			}
			foreach (var keyValuePairToClone in Properties)
			{
				clone.AddProperty(keyValuePairToClone.Key, keyValuePairToClone.Value.Clone());
			}
			return clone;
		}

		public override bool Equals(object other)
		{
			if (!(other is LexEntry)) return false;
			return Equals((LexEntry) other);
		}

		public bool Equals(LexEntry other)
		{
			if (other == null) return false;
			if (!_lexicalForm.Equals(other._lexicalForm)) return false;
			if (!_orderForRoundTripping.Equals(other._orderForRoundTripping)) return false;
			if (!_orderInFile.Equals(other._orderInFile)) return false;
			if (!_senses.SequenceEqual(other._senses)) return false;
			if (!_variants.SequenceEqual(other._variants)) return false;
			if (!_notes.SequenceEqual(other._notes)) return false;
			if (!_pronunciations.SequenceEqual(other._pronunciations)) return false;
			if (!_etymologies.SequenceEqual(other._etymologies)) return false;
			if (!base.Equals(other)) return false;
			return true;
		}

		public override string ToString()
		{
			//hack
			if (_lexicalForm != null)
			{
				return _lexicalForm.GetFirstAlternative();
			}
			else
			{
				return "";
			}
		}

		protected override void WireUpEvents()
		{
			Debug.Assert(CreationTime.Kind == DateTimeKind.Utc);
			Debug.Assert(ModificationTime.Kind == DateTimeKind.Utc);
			base.WireUpEvents();
			WireUpChild(_lexicalForm);
			WireUpList(_senses, "senses");
			WireUpList(_variants, "variants");
		}

		public IEnumerable<string> PropertiesInUse
		{
			get {
				return base.PropertiesInUse.Concat(Senses.SelectMany(sense => sense.PropertiesInUse));
			}
		}

		public override void SomethingWasModified(string propertyModified)
		{
			base.SomethingWasModified(propertyModified);
			ModificationTime = PreciseDateTime.UtcNow;
			_isDirty = true;
			//too soon to make id: this method is called after first keystroke
			//  GetOrCreateId(false);
		}

		public void Clean()
		{
			_isDirty = false;
		}

		public override void NotifyPropertyChanged(string propertyName)
		{
			if(!_isBeingDeleted)
				base.NotifyPropertyChanged(propertyName);
		}

		public string GetOrCreateId(bool doCreateEvenIfNoLexemeForm)
		{
			if (String.IsNullOrEmpty(_id))
			{
				//review: I think we could rapidly search the db for exact id matches,
				//so we could come up with a smaller id... but this has the nice
				//property of being somewhat readable and unique even across merges
				if (!String.IsNullOrEmpty(_lexicalForm.GetFirstAlternative()))
				{
					_id =
						_lexicalForm.GetFirstAlternative().Trim().Normalize(
							NormalizationForm.FormD) + "_" + Guid;
					NotifyPropertyChanged("id");
				}
				else if (doCreateEvenIfNoLexemeForm)
				{
					_id = "Id'dPrematurely_" + Guid;
					NotifyPropertyChanged("id");
				}
			}

			return _id;
		}



		/// <summary>
		///
		/// </summary>
		/// <remarks>The signature here is MultiText rather than LexicalFormMultiText because we want
		/// to hide this (hopefully temporary) performance implementation detail. </remarks>
		public MultiText LexicalForm
		{
			get { return _lexicalForm; }
		}

		public DateTime CreationTime
		{
			get { return GetSafeDateTime(_creationTime); }
			set
			{
				Debug.Assert(value.Kind == DateTimeKind.Utc);
				//_creationTime = value;
				//converting time to LiftFormatResolution
				_creationTime = new DateTime(value.Year,
											 value.Month,
											 value.Day,
											 value.Hour,
											 value.Minute,
											 value.Second,
											 value.Kind);
			}
		}

		private static DateTime GetSafeDateTime(DateTime dt)
		{
			//workaround db4o 6 bug
			if (dt.Kind != DateTimeKind.Utc)
			{
				return new DateTime(dt.Ticks, DateTimeKind.Utc);
			}
			return dt;
		}

		public DateTime ModificationTime
		{
			get { return GetSafeDateTime(_modificationTime); }
			set
			{
				if (!ModifiedTimeIsLocked)
				{
					Debug.Assert(value.Kind == DateTimeKind.Utc);
					//_modificationTime = value;
					//converting time to LiftFormatResolution
					_modificationTime = new DateTime(value.Year,
													 value.Month,
													 value.Day,
													 value.Hour,
													 value.Minute,
													 value.Second,
													 value.Kind);
					_isDirty = true;
				}
			}
		}

		public IList<LexSense> Senses
		{
			get { return _senses; }
		}

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, but SOLID does
		/// </summary>
		public IList<LexVariant> Variants
		{
			get { return _variants; }
		}
		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, as it only handles a single, typeless note and uses the well-known-properties approach
		/// </summary>
		public IList<LexNote> Notes
		{
			get { return _notes; }
		}

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, but SOLID does
		/// </summary>
		public IList<LexPhonetic> Pronunciations
		{
			get { return _pronunciations; }
		}

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, but SOLID does
		/// </summary>
		public IList<LexEtymology> Etymologies
		{
			get { return _etymologies; }
		}

		/// <summary>
		/// Used to track this entry across programs, for the purpose of merging and such.
		/// </summary>
		public Guid Guid
		{
			get { return _guid; }
			set
			{
				if (_guid != value)
				{
					_guid = value;
					NotifyPropertyChanged("GUID");
				}
			}
		}

		public override bool IsEmpty
		{
			get { return Senses.Count == 0 && LexicalForm.Empty && !HasProperties; }
		}

		/// <summary>
		/// This use for keeping track of the item when importing an then exporting again,
		/// like for merging. Also used for relations (e.g. superentry). we purposefully
		/// delay making one of these (if we aren't contructed with one) in order to give
		/// time to get a LexemeForm to make the id more readable.
		/// </summary>
		public string Id
		{
			get { return GetOrCreateId(false); }
			set
			{
				string id = value;
				if (value != null)
				{
					id = value.Trim().Normalize(NormalizationForm.FormD);
					if (id.Length == 0)
					{
						id = null;
					}
				}
				_id = id;
			}
		}

		public override void CleanUpAfterEditting()
		{
			if (IsBeingDeleted)
			{
				return;
			}
			base.CleanUpAfterEditting();
			foreach (LexSense sense in _senses)
			{
				sense.CleanUpAfterEditting();
			}
			//enhance if ever WeSay does variants, we may need to add this kind of cleanup
			CleanUpEmptyObjects();
		}

		public override void CleanUpEmptyObjects()
		{
			if (IsBeingDeleted)
			{
				return;
			}
			Logger.WriteMinorEvent("LexEntry CleanUpEmptyObjects()");
			base.CleanUpEmptyObjects();

			for (int i = 0;i < _senses.Count;i++)
			{
				_senses[i].CleanUpEmptyObjects();
			}

			// remove any senses that are empty
			int count = _senses.Count;
			for (int i = count - 1;i >= 0;i--)
			{
				if (_senses[i].IsEmptyForPurposesOfDeletion)
				{
					_senses.RemoveAt(i);
				}
			}
			if (count != _senses.Count)
			{
				Logger.WriteMinorEvent("Empty sense removed");
				OnEmptyObjectsRemoved();
			}
		}

		public LexSense GetOrCreateSenseWithMeaning(MultiText meaning) //Switch to meaning
		{
			foreach (LexSense sense in Senses)
			{
#if GlossMeaning
				if (meaning.HasFormWithSameContent(sense.Gloss))
#else
				if (meaning.HasFormWithSameContent(sense.Definition))
#endif
				{
					return sense;
				}
			}
			LexSense newSense = new LexSense();
			Senses.Add(newSense);
#if GlossMeaning
			newSense.Gloss.MergeIn(meaning);
#else
			newSense.Definition.MergeIn(meaning);
#endif
			return newSense;
		}

		public string GetToolTipText()
		{
			string s = "";
			foreach (LexSense sense in Senses)
			{
				string x = sense.Definition.GetFirstAlternative();
				if (string.IsNullOrEmpty(x))
				{
					x = sense.Gloss.GetFirstAlternative();
				}
				s += x + ", ";
			}
			if (s == "")
			{
				return StringCatalog.Get("~No Senses");
			}
			return s.Substring(0, s.Length - 2); // chop off the trailing separator
		}

		/// <summary>
		/// checks if it looks like the user has added info. this is used when changing spelling
		/// in a word-gathering task
		/// </summary>
		public bool IsEmptyExceptForLexemeFormForPurposesOfDeletion
		{
			get
			{
				if (LexicalForm.Count > 1)
				{
					return false;
				}
				foreach (LexSense sense in _senses)
				{
					if (!sense.IsEmptyForPurposesOfDeletion)
					{
						return false;
					}
				}
				if (HasPropertiesForPurposesOfDeletion)
				{
					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// this is used to prevent things like cleanup of an object that is being deleted, which
		/// can lead to update notifications that get the dispossed entry back in the db, or in some cache
		/// </summary>
		public bool IsBeingDeleted
		{
			get { return _isBeingDeleted; }
			set { _isBeingDeleted = value; }
		}

		/// <summary>
		/// used during import so we don't accidentally change the modified time while building up the entry
		/// </summary>
		public bool ModifiedTimeIsLocked
		{
			get { return _modifiedTimeIsLocked; }
			set { _modifiedTimeIsLocked = value; }
		}

		public MultiText CitationForm
		{
			get { return GetOrCreateProperty<MultiText>(WellKnownProperties.Citation); }
		}

		/// <summary>
		/// The name here is to remind us that our homograph number
		/// system doesn't know how to take this into account
		/// </summary>
		public int OrderForRoundTripping
		{
			get { return _orderForRoundTripping; }
			set
			{
				_orderForRoundTripping = value;
				NotifyPropertyChanged("order");
			}
		}

		public int OrderInFile
		{
			get
			{
				return this._orderInFile;
			}
			set
			{
				this._orderInFile = value;
				NotifyPropertyChanged("order");
			}
		}

		public MultiText VirtualHeadWord
		{
			get
			{
				MultiText headword = new MultiText();
				headword.MergeIn(LexicalForm);
				headword.MergeIn(CitationForm);
				return headword;
			}
		}

		public bool IsDirty
		{
			get { return _isDirty; }
			//ideally, this wouldn't be needed, but in making the homograph merger, I (jh) found that adding a property (a citation form)
			// left _isDirty still false. I dont have the stomach to spend a day figure out why, so I'm making this setable.
			set { _isDirty = value; }
		}

		public LanguageForm GetHeadWord(string writingSystemId)
		{
			if (string.IsNullOrEmpty(writingSystemId))
			{
				throw new ArgumentException("writingSystemId");
			}
			MultiText citationMT = GetProperty<MultiText>(WellKnownProperties.Citation);
			LanguageForm headWord;
			if (citationMT == null || (headWord = citationMT.Find(writingSystemId)) == null)
			{
				headWord = LexicalForm.Find(writingSystemId);
			}
			return headWord;
		}

		/// <summary>
		/// this is safer
		/// </summary>
		/// <param name="writingSystemId"></param>
		/// <returns>string.emtpy if no headword</returns>
		public string GetHeadWordForm(string writingSystemId)
		{
			LanguageForm form = GetHeadWord(writingSystemId);
			if (form == null)
			{
				return string.Empty;
			}
			return form.Form;
		}

		public void AddRelationTarget(string relationName, string targetId)
		{
			LexRelationCollection relations =
				GetOrCreateProperty<LexRelationCollection>(relationName);
			relations.Relations.Add(new LexRelation(relationName, targetId, this));
		}

		public string GetSimpleFormForLogging()
		{
			string formForLogging ;
			try
			{
				formForLogging = LexicalForm.GetFirstAlternative();
			}
			catch (Exception)
			{
				formForLogging="(unknown)";
			}
			return formForLogging;
		}
	}
}