using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SIL.Data;
using SIL.i18n;
using SIL.Lift;
using SIL.ObjectModel;
using SIL.Reporting;
using SIL.Text;

namespace SIL.DictionaryServices.Model
{
	/// <summary>
	/// A Lexical Entry is what makes up our lexicon/dictionary.  In
	/// some languages/dictionaries, these will be indistinguishable from "words".
	/// In others, words are made up of lexical entries.
	/// </summary>
	public class LexEntry : PalasoDataObject, ICloneable<LexEntry>, IEquatable<LexEntry>
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
		private BindingList<LexVariant> _variants;
		private BindingList<LexNote> _notes;
		private BindingList<LexPhonetic> _pronunciations;
		private BindingList<LexEtymology> _etymologies;
		private BindingList<LexField> _fields;

		private DateTime _creationTime;
		private DateTime _modificationTime;

		//!!What!! Is this done this way so that we don't end up storing
		//  the data in the object database?
		public new class WellKnownProperties : PalasoDataObject.WellKnownProperties
		{
			public static string Citation = "citation";
			public static string LexicalUnit = "EntryLexicalForm";
			public static string BaseForm = "BaseForm";
			public static string CrossReference = "confer";
			public static string Sense = "sense";
			public static string FlagSkipBaseForm = "flag-skip-BaseForm";
			public static string LiteralMeaning = "literal-meaning";

			public new static bool Contains(string fieldName)
			{
				List<string> list =
					new List<string>(new string[] { LexicalUnit, Citation, BaseForm, CrossReference, Sense, LiteralMeaning });
				return list.Contains(fieldName);
			}
		}

		public LexEntry() : this(null, Guid.NewGuid()) { }

		public LexEntry(string id, Guid guid) : base(null)
		{
			DateTime now = PreciseDateTime.UtcNow;
			IsDirty = true;
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
			_fields = new BindingList<LexField>();

			CreationTime = creationTime;

			WireUpEvents();

			ModifiedTimeIsLocked = false;
		}

		public LexEntry Clone()
		{
			var clone = new LexEntry();
			clone._lexicalForm = (MultiText)_lexicalForm.Clone();
			//_lexicalForm and Guid must have been set before _id is set
			if (_id != null)
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
				clone.Variants.Add((LexVariant)lexVariantToClone.Clone());
			}
			foreach (var lexNoteToClone in Notes)
			{
				clone.Notes.Add((LexNote)lexNoteToClone.Clone());
			}
			foreach (var pronunciationToClone in _pronunciations)
			{
				clone._pronunciations.Add((LexPhonetic)pronunciationToClone.Clone());
			}
			foreach (var etymologyToClone in _etymologies)
			{
				clone._etymologies.Add((LexEtymology)etymologyToClone.Clone());
			}
			foreach (var fieldToClone in _fields)
			{
				clone._fields.Add((LexField)fieldToClone.Clone());
			}
			foreach (var keyValuePairToClone in Properties)
			{
				clone.AddProperty(keyValuePairToClone.Key, keyValuePairToClone.Value.Clone());
			}
			return clone;
		}

		public override bool Equals(object other)
		{
			return Equals(other as LexEntry);
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
			if (!_fields.SequenceEqual(other._fields)) return false;
			if (!base.Equals(other)) return false;
			return true;
		}

		public override int GetHashCode()
		{
			// For this class we want a hash code based on the the object's reference so that we
			// can store and retrieve the object in the LiftLexEntryRepository. However, this is
			// not ideal and Microsoft warns: "Do not use the hash code as the key to retrieve an
			// object from a keyed collection."
			// https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=netframework-4.8#remarks
			return base.GetHashCode();
		}

		public override string ToString()
		{
			//hack
			return _lexicalForm != null ? _lexicalForm.GetFirstAlternative() : string.Empty;
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

		public new IEnumerable<string> PropertiesInUse => base.PropertiesInUse.Concat(Senses.SelectMany(sense => sense.PropertiesInUse));

		public override void SomethingWasModified(string propertyModified)
		{
			base.SomethingWasModified(propertyModified);
			ModificationTime = PreciseDateTime.UtcNow;
			IsDirty = true;
			//too soon to make id: this method is called after first keystroke
			//  GetOrCreateId(false);
		}

		public void Clean()
		{
			IsDirty = false;
		}

		public override void NotifyPropertyChanged(string propertyName)
		{
			if (!IsBeingDeleted)
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

		/// <remarks>The signature here is MultiText rather than LexicalFormMultiText because we want
		/// to hide this (hopefully temporary) performance implementation detail. </remarks>
		public MultiText LexicalForm => _lexicalForm;

		public DateTime CreationTime
		{
			get => GetSafeDateTime(_creationTime);
			set
			{
				Debug.Assert(value.Kind == DateTimeKind.Utc);
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
			get => GetSafeDateTime(_modificationTime);
			set
			{
				if (!ModifiedTimeIsLocked)
				{
					Debug.Assert(value.Kind == DateTimeKind.Utc);
					//converting time to LiftFormatResolution
					_modificationTime = new DateTime(value.Year,
													 value.Month,
													 value.Day,
													 value.Hour,
													 value.Minute,
													 value.Second,
													 value.Kind);
					IsDirty = true;
				}
			}
		}

		public IList<LexSense> Senses => _senses;

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, but SOLID does
		/// </summary>
		public IList<LexVariant> Variants => _variants;

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, as it only handles a single, typeless note and uses the well-known-properties approach
		/// </summary>
		public IList<LexNote> Notes => _notes;

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, but SOLID does
		/// </summary>
		public IList<LexPhonetic> Pronunciations => _pronunciations;

		/// <summary>
		/// NOTE: in oct 2010, wesay does not yet use this field, but SOLID does
		/// </summary>
		public IList<LexEtymology> Etymologies => _etymologies;

		public IList<LexField> Fields => _fields;

		/// <summary>
		/// Used to track this entry across programs, for the purpose of merging and such.
		/// </summary>
		public Guid Guid
		{
			get => _guid;
			set
			{
				if (_guid == value)
					return;

				_guid = value;
				NotifyPropertyChanged("GUID");
			}
		}

		public override bool IsEmpty => Senses.Count == 0 && LexicalForm.Empty && !HasProperties;

		/// <summary>
		/// This use for keeping track of the item when importing an then exporting again,
		/// like for merging. Also used for relations (e.g. superentry). we purposefully
		/// delay making one of these (if we aren't constructed with one) in order to give
		/// time to get a LexemeForm to make the id more readable.
		/// </summary>
		public string Id
		{
			get => GetOrCreateId(false);
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

		public override void CleanUpAfterEditing()
		{
			if (IsBeingDeleted)
			{
				return;
			}
			base.CleanUpAfterEditing();
			foreach (LexSense sense in _senses)
			{
				sense.CleanUpAfterEditing();
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

			for (int i = 0; i < _senses.Count; i++)
			{
				_senses[i].CleanUpEmptyObjects();
			}

			// remove any senses that are empty
			int count = _senses.Count;
			for (int i = count - 1; i >= 0; i--)
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
		/// can lead to update notifications that get the disposed entry back in the db, or in some cache
		/// </summary>
		public bool IsBeingDeleted { get; set; }

		/// <summary>
		/// used during import so we don't accidentally change the modified time while building up the entry
		/// </summary>
		[field: NonSerialized]
		public bool ModifiedTimeIsLocked { get; set; }

		public MultiText CitationForm => GetOrCreateProperty<MultiText>(WellKnownProperties.Citation);

		/// <summary>
		/// The name here is to remind us that our homograph number
		/// system doesn't know how to take this into account
		/// </summary>
		public int OrderForRoundTripping
		{
			get => _orderForRoundTripping;
			set
			{
				_orderForRoundTripping = value;
				NotifyPropertyChanged("order");
			}
		}

		public int OrderInFile
		{
			get => _orderInFile;
			set
			{
				_orderInFile = value;
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
			get;
			// Ideally, this wouldn't be needed, but in making the homograph merger, I (jh) found that adding a property (a citation form)
			// left _isDirty still false. I don't have the stomach to spend a day to figure out why, so I'm making this settable.
			set;
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
		/// <returns>string.empty if no headword</returns>
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
			string formForLogging;
			try
			{
				formForLogging = LexicalForm.GetFirstAlternative();
			}
			catch (Exception)
			{
				formForLogging = "(unknown)";
			}
			return formForLogging;
		}

		/// <summary>
		/// used by SILCAWL list
		/// </summary>
		public string GetSomeMeaningToUseInAbsenceOfHeadWord(string writingSystemId)
		{
			var s = Senses.FirstOrDefault();
			if (s == null)
				return "?NoMeaning?";
			var gloss = s.Gloss.GetExactAlternative(writingSystemId);
			if (string.IsNullOrEmpty(gloss))
			{
				var def = s.Definition.GetExactAlternative(writingSystemId);
				if (string.IsNullOrEmpty(def))
					return "?NoGlossOrDef?";
				return def;
			}
			return gloss;
		}
	}
}