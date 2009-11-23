using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using LiftIO.Validation;
using Palaso.Annotations;
using Palaso.Lift;
using Palaso.Lift.Model;
using Palaso.Lift.Options;
using Palaso.Text;

namespace WeSay.LexicalModel
{
	public class WeSayLiftWriter : ILiftWriter<LexEntry>
	{
		public const string LiftDateTimeFormat = "yyyy-MM-ddThh:mm:ssZ";
		private readonly XmlWriter _writer;
		private readonly Dictionary<string, int> _allIdsExportedSoFar;

		#if DEBUG
		[CLSCompliant(false)]
		protected StackTrace _constructionStack;
		#endif

		private WeSayLiftWriter()
		{
			#if DEBUG
			_constructionStack = new StackTrace();
			#endif

			_allIdsExportedSoFar = new Dictionary<string, int>();
		}

		public WeSayLiftWriter(string path): this()
		{
			_disposed = true; // Just in case we throw in the constructor
			_writer = XmlWriter.Create(path, PrepareSettings(false));
			Start();
			_disposed = false;
		}

		public WeSayLiftWriter(StringBuilder builder, bool produceFragmentOnly): this()
		{
			_writer = XmlWriter.Create(builder, PrepareSettings(produceFragmentOnly));
			if (!produceFragmentOnly)
			{
				Start();
			}
		}

		private static XmlWriterSettings PrepareSettings(bool produceFragmentOnly)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			if (produceFragmentOnly)
			{
				settings.ConformanceLevel = ConformanceLevel.Fragment;
				settings.Indent = false; //helps with tests that just do a string compare
			}
			else
			{
				settings.Indent = true;
			}
			// this will give you a bom, which messes up princexml :settings.Encoding = Encoding.UTF8;
			Encoding utf8NoBom = new UTF8Encoding(false);
			settings.Encoding = utf8NoBom;
			settings.NewLineOnAttributes = false;
			settings.CloseOutput = true;
			return settings;
		}

		private void Start()
		{
			Writer.WriteStartDocument();
			Writer.WriteStartElement("lift");
			Writer.WriteAttributeString("version", Validator.LiftVersion);
			Writer.WriteAttributeString("producer", ProducerString);
			// _writer.WriteAttributeString("xmlns", "flex", null, "http://fieldworks.sil.org");
		}

		public static string ProducerString
		{
			get { return "WeSay " + Assembly.GetExecutingAssembly().GetName().Version; }
		}

		protected XmlWriter Writer
		{
			get { return _writer; }
		}

		public void End()
		{
			if (Writer.Settings.ConformanceLevel != ConformanceLevel.Fragment)
			{
				Writer.WriteEndElement(); //lift
				Writer.WriteEndDocument();
			}
			Writer.Flush();
			Writer.Close();
		}

		public virtual void Add(LexEntry entry)
		{
			Add(entry, 0);
		}

		public void Add(LexEntry entry, int order)
		{
			List<string> propertiesAlreadyOutput = new List<string>();

			Writer.WriteStartElement("entry");
			Writer.WriteAttributeString("id", GetHumanReadableId(entry, _allIdsExportedSoFar));

			if (order > 0)
			{
				Writer.WriteAttributeString("order", order.ToString());
			}

			Debug.Assert(entry.CreationTime.Kind == DateTimeKind.Utc);
			Writer.WriteAttributeString("dateCreated",
										entry.CreationTime.ToString(LiftDateTimeFormat));
			Debug.Assert(entry.ModificationTime.Kind == DateTimeKind.Utc);
			Writer.WriteAttributeString("dateModified",
										entry.ModificationTime.ToString(LiftDateTimeFormat));
			Writer.WriteAttributeString("guid", entry.Guid.ToString());
			// _writer.WriteAttributeString("flex", "id", "http://fieldworks.sil.org", entry.Guid.ToString());
			WriteMultiWithWrapperIfNonEmpty(LexEntry.WellKnownProperties.LexicalUnit,
											"lexical-unit",
											entry.LexicalForm);

			WriteHeadword(entry);
			WriteWellKnownCustomMultiText(entry,
										  LexEntry.WellKnownProperties.Citation,
										  propertiesAlreadyOutput);
			WriteWellKnownCustomMultiText(entry,
										  PalasoDataObject.WellKnownProperties.Note,
										  propertiesAlreadyOutput);
			WriteCustomProperties(entry, propertiesAlreadyOutput);
			InsertPronunciationIfNeeded(entry, propertiesAlreadyOutput);

			foreach (LexSense sense in entry.Senses)
			{
				Add(sense);
			}
			Writer.WriteEndElement();
		}

		/// <summary>
		/// in the plift subclass, we add a pronounciation if we have an audio writing system alternative on the lexical unit
		/// </summary>
		 protected virtual void InsertPronunciationIfNeeded(LexEntry entry, List<string> propertiesAlreadyOutput)
		{

		}

		protected virtual void WriteHeadword(LexEntry entry) {}

		/// <summary>
		/// Get a human readable identifier for this entry taking into account all the rest of the
		/// identifiers that this has seen
		/// </summary>
		/// <param name="entry">the entry to </param>
		/// <param name="idsAndCounts">the base ids that have been used so far and how many times</param>
		/// <remarks>This function alters the idsAndCounts and thus is not stable if the entry
		/// does not already have an id and the same idsAndCounts dictionary is provided.
		/// A second call to this function with the same entry that lacks an id and the same
		/// idsAndCounts will produce different results each time it runs
		/// </remarks>
		/// <returns>A base id composed with its count</returns>
		public static string GetHumanReadableId(LexEntry entry, Dictionary<string, int> idsAndCounts)
		{
			string id = entry.GetOrCreateId(true);
			/*         if (id == null || id.Length == 0)       // if the entry doesn't claim to have an id
			{
				id = entry.LexicalForm.GetFirstAlternative().Trim().Normalize(NormalizationForm.FormD); // use the first form as an id
				if (id == "")
				{
					id = "NoForm"; //review
				}
			}
			id = id.Replace('\r', ' ').Replace('\n', ' ').Replace('\t', ' ');
			//make this id unique
			int count;
			if (idsAndCounts.TryGetValue(id, out count))
			{
				++count;
				idsAndCounts.Remove(id);
				idsAndCounts.Add(id, count);
				id = string.Format("{0}_{1}", id, count);
			}
			else
			{
				idsAndCounts.Add(id, 1);
			}
			*/
			return id;
		}

		public void Add(LexSense sense)
		{
			List<string> propertiesAlreadyOutput = new List<string>();

			Writer.WriteStartElement("sense");
			Writer.WriteAttributeString("id", sense.GetOrCreateId());

			if (ShouldOutputProperty(LexSense.WellKnownProperties.PartOfSpeech))
			{
				WriteGrammi(sense);
				propertiesAlreadyOutput.Add(LexSense.WellKnownProperties.PartOfSpeech);
			}
			if (ShouldOutputProperty(LexSense.WellKnownProperties.Gloss))
			{
				WriteOneElementPerFormIfNonEmpty(LexSense.WellKnownProperties.Gloss,
												 "gloss",
												 sense.Gloss,
												 ';');
				propertiesAlreadyOutput.Add(LexSense.WellKnownProperties.Gloss);
			}


			WriteWellKnownCustomMultiText(sense,
										  LexSense.WellKnownProperties.Definition,
										  propertiesAlreadyOutput);
			foreach (LexExampleSentence example in sense.ExampleSentences)
			{
				Add(example);
			}
			WriteWellKnownCustomMultiText(sense,
										  PalasoDataObject.WellKnownProperties.Note,
										  propertiesAlreadyOutput);
			//   WriteWellKnownUnimplementedProperty(sense, LexSense.WellKnownProperties.Note, propertiesAlreadyOutput);
			WriteCustomProperties(sense, propertiesAlreadyOutput);
			Writer.WriteEndElement();
		}

		private void WriteGrammi(LexSense sense)
		{
			if (!ShouldOutputProperty(LexSense.WellKnownProperties.PartOfSpeech))
			{
				return;
			}

			OptionRef pos = sense.GetProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);

			if (pos != null && !pos.IsEmpty)
			{
				WritePosCore(pos);
			}
		}

		protected virtual void WritePosCore(OptionRef pos)
		{
			Writer.WriteStartElement("grammatical-info");
			Writer.WriteAttributeString("value", pos.Value);
			WriteFlags(pos);
			Writer.WriteEndElement();
		}

		private void WriteWellKnownCustomMultiText(PalasoDataObject item,
												   string property,
												   ICollection<string> propertiesAlreadyOutput)
		{
			if (ShouldOutputProperty(property))
			{
				MultiText m = item.GetProperty<MultiText>(property);
				if (WriteMultiWithWrapperIfNonEmpty(property, property, m))
				{
					propertiesAlreadyOutput.Add(property);
				}
			}
		}

		/// <summary>
		/// this base implementationg is for when we're just exporting to lift, and dont' want to filter or order.
		/// It is overridden in a child class for writing presentation-ready lift, when
		/// we do want to filter and order
		/// </summary>
		/// <param name="text"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		protected virtual LanguageForm[] GetOrderedAndFilteredForms(MultiTextBase text,
																	string propertyName)
		{
			return text.Forms;
		}

		private void WriteCustomProperties(PalasoDataObject item,
										   ICollection<string> propertiesAlreadyOutput)
		{
			foreach (KeyValuePair<string, object> pair in item.Properties)
			{
				if (propertiesAlreadyOutput.Contains(pair.Key))
				{
					continue;
				}
				if (!ShouldOutputProperty(pair.Key))
				{
					continue;
				}
				if (pair.Value is EmbeddedXmlCollection)
				{
					WriteEmbeddedXmlCollection(pair.Value as EmbeddedXmlCollection);
					continue;
				}
				if (pair.Value is MultiText)
				{
					WriteCustomMultiTextField(pair.Key, pair.Value as MultiText);
					continue;
				}
				if (pair.Value is OptionRef)
				{
					WriteOptionRef(pair.Key, pair.Value as OptionRef);
					continue;
				}
				if (pair.Value is OptionRefCollection)
				{
					WriteOptionRefCollection(pair.Key, pair.Value as OptionRefCollection);
					continue;
				}
				if (pair.Value is LexRelationCollection)
				{
					WriteRelationCollection(pair.Key, pair.Value as LexRelationCollection);
					continue;
				}
				if (pair.Value is FlagState)
				{
					WriteFlagState(pair.Key, pair.Value as FlagState);
					continue;
				}
				PictureRef pictureRef = pair.Value as PictureRef;
				if (pictureRef != null)
				{
					WriteURLRef("illustration", pictureRef.Value, pictureRef.Caption);
					continue;
				}
				throw new ApplicationException(
						string.Format(
								"The LIFT exporter was surprised to find a property '{0}' of type: {1}",
								pair.Key,
								pair.Value.GetType()));
			}
		}

		protected virtual bool ShouldOutputProperty(string key)
		{
			return true;
		}

		private void WriteEmbeddedXmlCollection(EmbeddedXmlCollection collection)
		{
			foreach (string rawXml in collection.Values)
			{
				Writer.WriteRaw(rawXml);
			}
		}

		private void WriteURLRef(string key, string href, MultiText caption)
		{
			if (!string.IsNullOrEmpty(href))
			{
				Writer.WriteStartElement(key);
				Writer.WriteAttributeString("href", href);
				WriteMultiWithWrapperIfNonEmpty(key, "label", caption);
				Writer.WriteEndElement();
			}
		}

		private void WriteFlagState(string key, FlagState state)
		{
			if (state.Value) //skip it if it's not set
			{
				Writer.WriteStartElement("trait");
				Writer.WriteAttributeString("name", key);
				Writer.WriteAttributeString("value", "set");
				//this attr required by lift schema, though we don't use it
				Writer.WriteEndElement();
			}
		}

		private void WriteRelationCollection(string key, LexRelationCollection collection)
		{
			if (!ShouldOutputProperty(key))
			{
				return;
			}

			foreach (LexRelation relation in collection.Relations)
			{
				if(string.IsNullOrEmpty(relation.Key))
					continue;

				Writer.WriteStartElement("relation");
				Writer.WriteAttributeString("type", GetOutputRelationName(relation));
				Writer.WriteAttributeString("ref", relation.Key);
				WriteRelationTarget(relation);
				Writer.WriteEndElement();
			}
		}

		protected virtual string GetOutputRelationName(LexRelation relation)
		{
			return relation.FieldId;
		}

		/// <summary>
		/// allows subclass to output a dereferenced target name, e.g., for plift
		/// </summary>
		protected virtual void WriteRelationTarget(LexRelation relation) {}

		private void WriteOptionRefCollection(string traitName, OptionRefCollection collection)
		{
			if (!ShouldOutputProperty(traitName))
			{
				return;
			}
			foreach (string key in collection.Keys)
			{
				Writer.WriteStartElement("trait");
				Writer.WriteAttributeString("name", traitName);
				Writer.WriteAttributeString("value", key); //yes, the 'value' here is an option key
				Writer.WriteEndElement();
			}
		}

		private void WriteCustomMultiTextField(string tag, MultiText text)  // review cp see WriteEmbeddedXmlCollection
		{
			if (!MultiTextBase.IsEmpty(text))
			{
				Writer.WriteStartElement("field");

				Writer.WriteAttributeString("type", tag);
				WriteMultiTextNoWrapper(tag, text);
				Writer.WriteEndElement();
			}
		}

		protected virtual void WriteOptionRef(string key, OptionRef optionRef)
		{
			if (optionRef.Value.Length > 0)
			{
				Writer.WriteStartElement("trait");
				Writer.WriteAttributeString("name", key);
				Writer.WriteAttributeString("value", optionRef.Value);
				Writer.WriteEndElement();
			}
		}

		public void Add(LexExampleSentence example)
		{
			if (!ShouldOutputProperty(LexExampleSentence.WellKnownProperties.ExampleSentence))
			{
				return;
			}

			List<string> propertiesAlreadyOutput = new List<string>();
			Writer.WriteStartElement("example");

			OptionRef source =
					example.GetProperty<OptionRef>(LexExampleSentence.WellKnownProperties.Source);

			if (source != null && source.Value.Length > 0)
			{
				if (ShouldOutputProperty(LexExampleSentence.WellKnownProperties.Source))
				{
					Writer.WriteAttributeString("source", source.Value);
					propertiesAlreadyOutput.Add("source");
				}
			}

			WriteMultiTextNoWrapper(LexExampleSentence.WellKnownProperties.ExampleSentence,
									example.Sentence);
			//  WriteMultiWithWrapperIfNonEmpty(LexExampleSentence.WellKnownProperties.Translation, "translation", example.Translation);

			if (!MultiTextBase.IsEmpty(example.Translation))
			{
				Writer.WriteStartElement("translation");

				if (!string.IsNullOrEmpty(example.TranslationType))
				{
					Writer.WriteAttributeString("type", example.TranslationType);
					propertiesAlreadyOutput.Add("type");
				}

				Add(LexExampleSentence.WellKnownProperties.Translation, example.Translation);
				Writer.WriteEndElement();
			}

			if (ShouldOutputProperty(LexExampleSentence.WellKnownProperties.ExampleSentence))
			{
				WriteWellKnownCustomMultiText(example,
											  PalasoDataObject.WellKnownProperties.Note,
											  propertiesAlreadyOutput);
			}

			WriteCustomProperties(example, propertiesAlreadyOutput);
			Writer.WriteEndElement();
		}

		public void Add(string propertyName, MultiText text) // review cp see WriteEmbeddedXmlCollection
		{
			Add(GetOrderedAndFilteredForms(text, propertyName), false);
			WriteFormsThatNeedToBeTheirOwnFields(text, propertyName);
			WriteEmbeddedXmlCollection(text);
		}
		private void WriteEmbeddedXmlCollection(MultiText text)
		{
			foreach (string rawXml in text.EmbeddedXmlElements) // todo cp Promote roundtripping to Palaso.Lift / Palaso.Data also then can use MultiTextBase here (or a better interface).
			{
				Writer.WriteRaw(rawXml);
			}
		}

		protected virtual void WriteFormsThatNeedToBeTheirOwnFields(MultiText text, string name) // review cp For PLiftExporter GetAudioForms
		{
		}

		protected void Add(IEnumerable<LanguageForm> forms, bool doMarkTheFirst)
		{
			foreach (LanguageForm form in forms)
			{
				Writer.WriteStartElement("form");
				Writer.WriteAttributeString("lang", form.WritingSystemId);
				if (doMarkTheFirst)
				{
					doMarkTheFirst = false;
					Writer.WriteAttributeString("first", "true"); //useful for headword
				}
				string wrappedTextToExport = "<text>" + form.Form + "</text>";
				XmlReaderSettings fragmentReaderSettings = new XmlReaderSettings();
				fragmentReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;
				XmlReader testerForWellFormedness = XmlReader.Create(new StringReader(wrappedTextToExport));

				bool isTextWellFormedXml = true;
				try
				{
					while (testerForWellFormedness.Read())
					{
						//Just checking for well formed XML
					}
				}
				catch
				{
					isTextWellFormedXml = false;
				}

				if(isTextWellFormedXml)
				{
					Writer.WriteRaw(wrappedTextToExport);
				}
				else
				{
					Writer.WriteStartElement("text");
					Writer.WriteString(form.Form);
					Writer.WriteEndElement();
				}
				WriteFlags(form);
				Writer.WriteEndElement();
			}
		}

		private void WriteFlags(IAnnotatable thing)
		{
			if (thing.IsStarred)
			{
				Writer.WriteStartElement("annotation");
				Writer.WriteAttributeString("name", "flag");
				Writer.WriteAttributeString("value", "1");
				Writer.WriteEndElement();
			}
		}

		private void WriteMultiTextNoWrapper(string propertyName, MultiText text) // review cp see WriteEmbeddedXmlCollection
		{
			if (!MultiTextBase.IsEmpty(text))
			{
				Add(propertyName, text);
			}
		}

		private void WriteOneElementPerFormIfNonEmpty(string propertyName,
													  string wrapperName,
													  MultiTextBase text,
													  char delimeter)
		{
			if (!MultiTextBase.IsEmpty(text))
			{
				foreach (LanguageForm alternative in GetOrderedAndFilteredForms(text, propertyName))
				{
					foreach (string part in alternative.Form.Split(new char[] {delimeter}))
					{
						string trimmed = part.Trim();
						if (part != string.Empty)
						{
							Writer.WriteStartElement(wrapperName);
							Writer.WriteAttributeString("lang", alternative.WritingSystemId);
							Writer.WriteStartElement("text");
							Writer.WriteString(trimmed);
							Writer.WriteEndElement();
							WriteFlags(alternative);
							Writer.WriteEndElement();
						}
					}
				}
			}
		}

		private bool WriteMultiWithWrapperIfNonEmpty(string propertyName,
													 string wrapperName,
													 MultiText text)  // review cp see WriteEmbeddedXmlCollection
		{
			if (!MultiTextBase.IsEmpty(text))
			{
				Writer.WriteStartElement(wrapperName);
				Add(propertyName, text);  // review cp see WriteEmbeddedXmlCollection
				Writer.WriteEndElement();
				return true;
			}
			return false;
		}

		public void AddNewEntry(LexEntry entry)
		{
			Writer.WriteStartElement("entry");
			Writer.WriteAttributeString("dateCreated",
										entry.CreationTime.ToString(LiftDateTimeFormat));
			Writer.WriteAttributeString("dateModified",
										entry.ModificationTime.ToString(LiftDateTimeFormat));
			Writer.WriteAttributeString("guid", entry.Guid.ToString());
			Writer.WriteEndElement();
		}

		public void AddDeletedEntry(LexEntry entry)
		{
			Writer.WriteStartElement("entry");
			Writer.WriteAttributeString("id", GetHumanReadableId(entry, _allIdsExportedSoFar));
			Writer.WriteAttributeString("dateCreated",
										entry.CreationTime.ToString(LiftDateTimeFormat));
			Writer.WriteAttributeString("dateModified",
										entry.ModificationTime.ToString(LiftDateTimeFormat));
			Writer.WriteAttributeString("guid", entry.Guid.ToString());
			Writer.WriteAttributeString("dateDeleted", DateTime.UtcNow.ToString(LiftDateTimeFormat));

			Writer.WriteEndElement();
		}

		#region IDisposable Members

#if DEBUG
		~WeSayLiftWriter()
		{
			if (!_disposed)
			{
				throw new ApplicationException("Disposed not explicitly called on WeSayLiftWriter." + "\n" + _constructionStack);
			}
		}
#endif

		[CLSCompliantAttribute(false)]
		protected bool _disposed;

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
				}

				// shared (dispose and finalizable) cleanup logic
				_disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (!_disposed)
			{
				throw new ObjectDisposedException("WeSayLiftWriter");
			}
		}

		#endregion

	}
}