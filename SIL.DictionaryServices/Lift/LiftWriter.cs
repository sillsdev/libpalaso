using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using SIL.Annotations;
using SIL.DictionaryServices.Model;
using SIL.Extensions;
using SIL.Lift;
using SIL.Lift.Options;
using SIL.Lift.Validation;
using SIL.PlatformUtilities;
using SIL.Text;
using SIL.Xml;

namespace SIL.DictionaryServices.Lift
{
	public class LiftWriter : ILiftWriter<LexEntry>
	{
		private readonly XmlWriter _writer;

#if DEBUG
		private StackTrace _constructionStack;
#endif

		private LiftWriter()
		{
#if DEBUG
			_constructionStack = new StackTrace();
#endif
		}

		public enum ByteOrderStyle
		{
			BOM,
			NoBOM
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="path"></param>
		/// <param name="byteOrderStyle">PrinceXML (at least v7 chokes if given a BOM, Lexique Pro chokes without it) </param>
		public LiftWriter(string path, ByteOrderStyle byteOrderStyle) : this()
		{
			Disposed = true; // Just in case we throw in the constructor
			var settings = CanonicalXmlSettings.CreateXmlWriterSettings();
			settings.Encoding = new UTF8Encoding(byteOrderStyle == ByteOrderStyle.BOM);
			_writer = XmlWriter.Create(path, settings);
			Start();
			Disposed = false;
		}

		public LiftWriter(StringBuilder builder, bool produceFragmentOnly) : this()
		{
			_writer = XmlWriter.Create(builder, CanonicalXmlSettings.CreateXmlWriterSettings(
				produceFragmentOnly ? ConformanceLevel.Fragment : ConformanceLevel.Document, NewLineHandling.None)
			);
			if (!produceFragmentOnly)
			{
				Start();
			}
		}

		private void Start()
		{
			Writer.WriteStartDocument();
			Writer.WriteStartElement("lift");
			Writer.WriteAttributeString("version", Validator.LiftVersion);
			Writer.WriteAttributeString("producer", ProducerString);
			// _writer.WriteAttributeString("xmlns", "flex", null, "http://fieldworks.sil.org");
		}

		public void WriteHeader(string headerContentsNotIncludingHeaderElement)
		{
			Writer.WriteStartElement("header");
			Writer.WriteRaw(headerContentsNotIncludingHeaderElement);
			Writer.WriteEndElement();
		}

		public static string ProducerString
		{
			get { return "SIL.DictionaryServices.LiftWriter " + Assembly.GetExecutingAssembly().GetName().Version; }
		}

		protected XmlWriter Writer
		{
			get { return _writer; }
		}

#if DEBUG
		protected StackTrace ConstructionStack
		{
			get { return _constructionStack; }
			set { _constructionStack = value; }
		}
#endif

		public void End()
		{
			if (Writer.Settings.ConformanceLevel != ConformanceLevel.Fragment)
			{
				if (!Platform.IsWindows)
				{
					// If there are no open elements and you try to WriteEndElement then mono throws a
					// InvalidOperationException: There is no more open element
					// WriteEndDocument will close any open elements anyway
					//
					// If you try to WriteEndDocument on a closed writer then mono throws a
					// InvalidOperationException: This XmlWriter does not accept EndDocument at this state Closed
					if (Writer.WriteState != WriteState.Closed)
						Writer.WriteEndDocument();
				}
				else
				{
					Writer.WriteEndElement(); //lift
					Writer.WriteEndDocument();
				}
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
			Writer.WriteAttributeString("id",
										GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry));

			if (order > 0)
			{
				Writer.WriteAttributeString("order", order.ToString());
			}

			Debug.Assert(entry.CreationTime.Kind == DateTimeKind.Utc);
			Writer.WriteAttributeString("dateCreated",
										entry.CreationTime.ToLiftDateTimeFormat());
			Debug.Assert(entry.ModificationTime.Kind == DateTimeKind.Utc);
			Writer.WriteAttributeString("dateModified",
										entry.ModificationTime.ToLiftDateTimeFormat());
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
			foreach (var variant in entry.Variants)
			{
				AddVariant(variant);
			}
			foreach (var phonetic in entry.Pronunciations)
			{
				AddPronunciation(phonetic);
			}
			foreach (var etymology in entry.Etymologies)
			{
				AddEtymology(etymology);
			}
			foreach (var field in entry.Fields)
			{
				AddField(field);
			}
			foreach (var note in entry.Notes)
			{
				AddNote(note);
			}
			Writer.WriteEndElement();
		}

		private void AddEtymology(LexEtymology etymology)
		{
			Writer.WriteStartElement("etymology");
			// Type is required, so add the attribute even if it's empty.
			Writer.WriteAttributeString("type", etymology.Type.Trim());
			// Source is required, so add the attribute even if it's empty.
			Writer.WriteAttributeString("source", etymology.Source.Trim());
			AddMultitextGlosses(string.Empty, etymology.Gloss);
			// Ok if MultiTextBase.IsEmpty(etymology) is true.
			WriteCustomMultiTextField("comment", etymology.Comment);
			AddMultitextForms(string.Empty, etymology);
			Writer.WriteEndElement();
		}

		private void AddField(LexField field)
		{
			Writer.WriteStartElement("field");
			// Type is required, so add the attribute even if it's empty.
			Writer.WriteAttributeString("type", field.Type.Trim());
			foreach (var trait in field.Traits)
			{
				WriteTrait(trait);
			}
			// Ok if MultiTextBase.IsEmpty(field) is true.
			AddMultitextForms(string.Empty, field);
			Writer.WriteEndElement();
		}

		private void AddPronunciation(LexPhonetic phonetic)
		{
			WriteMultiWithWrapperIfNonEmpty(string.Empty, "pronunciation", phonetic);
		}

		public void AddVariant(LexVariant variant)
		{
			WriteMultiWithWrapperIfNonEmpty(string.Empty, "variant", variant);
		}

		public void AddNote(LexNote note)
		{
			if (!MultiTextBase.IsEmpty(note))
			{
				Writer.WriteStartElement("note");
				if (!string.IsNullOrEmpty(note.Type))
				{
					Writer.WriteAttributeString("type", note.Type.Trim());
				}
				AddMultitextForms(string.Empty, note);
				Writer.WriteEndElement();
			}
		}

		public void AddReversal(LexReversal reversal)
		{
			if (!MultiTextBase.IsEmpty(reversal))
			{
				Writer.WriteStartElement("reversal");
				if (!string.IsNullOrEmpty(reversal.Type))
				{
					Writer.WriteAttributeString("type", reversal.Type.Trim());
				}
				AddMultitextForms(string.Empty, reversal);
				Writer.WriteEndElement();
			}
		}

		/// <summary>
		/// in the plift subclass, we add a pronunciation if we have an audio writing system alternative on the lexical unit
		/// </summary>
		protected virtual void InsertPronunciationIfNeeded(
			LexEntry entry, List<string> propertiesAlreadyOutput)
		{
		}

		protected virtual void WriteHeadword(LexEntry entry) { }

		/// <summary>
		/// Get a human readable identifier for an entry.
		/// </summary>
		public static string GetHumanReadableIdWithAnyIllegalUnicodeEscaped(LexEntry entry)
		{
			string id = entry.GetOrCreateId(true);
			return id.EscapeAnyUnicodeCharactersIllegalInXml();
		}

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
		[Obsolete("Use GetHumanReadableIdWithAnyIllegalUnicodeEscaped(LexEntry) without the second parameter.")]
		public static string GetHumanReadableIdWithAnyIllegalUnicodeEscaped(LexEntry entry, Dictionary<string, int> idsAndCounts)
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
			return id.EscapeAnyUnicodeCharactersIllegalInXml();
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
				// review: I (cp) don't think this has the same checking for round tripping that AddMultiText... methods have.
				WriteGlossOneElementPerFormIfNonEmpty(sense.Gloss);
				propertiesAlreadyOutput.Add(LexSense.WellKnownProperties.Gloss);
			}


			WriteWellKnownCustomMultiText(sense,
										  LexSense.WellKnownProperties.Definition,
										  propertiesAlreadyOutput);
			foreach (LexExampleSentence example in sense.ExampleSentences)
			{
				Add(example);
			}
			foreach (var reversal in sense.Reversals)
			{
				AddReversal(reversal);
			}
			foreach (var note in sense.Notes)
			{
				AddNote(note);
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
			foreach (string rawXml in pos.EmbeddedXmlElements)
			{
				Writer.WriteRaw(rawXml);
			}
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
		/// this base implementation is for when we're just exporting to lift, and don't want to filter or order.
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
			foreach (KeyValuePair<string, IPalasoDataObjectProperty> pair in item.Properties)
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
					WriteIllustrationElement(pictureRef);
					continue;
				}
				throw new ApplicationException(
						string.Format(
								"The LIFT exporter was surprised to find a property '{0}' of type: {1}",
								pair.Key,
								pair.Value.GetType()));
			}
		}

		protected virtual void WriteIllustrationElement(PictureRef pictureRef)
		{
			WriteURLRef("illustration", pictureRef.Value, pictureRef.Caption);
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

		protected void WriteURLRef(string key, string href, MultiText caption)
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
				if (string.IsNullOrEmpty(relation.Key))
					continue;
				if (!EntryDoesExist(relation.TargetId))
					continue;

				Writer.WriteStartElement("relation");
				Writer.WriteAttributeString("type", GetOutputRelationName(relation));
				Writer.WriteAttributeString("ref", relation.Key);
				WriteRelationTarget(relation);

				WriteExtensible(relation);

				foreach (string rawXml in relation.EmbeddedXmlElements)
				{
					Writer.WriteRaw(rawXml);
				}

				Writer.WriteEndElement();
			}
		}

		protected virtual bool EntryDoesExist(string id)
		{
			return true;// real implementations would check
		}


		protected virtual string GetOutputRelationName(LexRelation relation)
		{
			return relation.FieldId;
		}

		/// <summary>
		/// allows subclass to output a dereferenced target name, e.g., for plift
		/// </summary>
		protected virtual void WriteRelationTarget(LexRelation relation) { }

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

		private void WriteCustomMultiTextField(string type, MultiText text)  // review cp see WriteEmbeddedXmlCollection
		{
			if (!MultiTextBase.IsEmpty(text))
			{
				Writer.WriteStartElement("field");

				Writer.WriteAttributeString("type", type);
				WriteMultiTextNoWrapper(type, text);
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

				foreach (string rawXml in optionRef.EmbeddedXmlElements)
				{
					Writer.WriteRaw(rawXml);
				}

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
			propertiesAlreadyOutput.Add(LexExampleSentence.WellKnownProperties.ExampleSentence);
			//  WriteMultiWithWrapperIfNonEmpty(LexExampleSentence.WellKnownProperties.Translation, "translation", example.Translation);

			if (!MultiTextBase.IsEmpty(example.Translation))
			{
				Writer.WriteStartElement("translation");

				if (!string.IsNullOrEmpty(example.TranslationType))
				{
					Writer.WriteAttributeString("type", example.TranslationType);
					propertiesAlreadyOutput.Add("type");
				}

				AddMultitextForms(LexExampleSentence.WellKnownProperties.Translation, example.Translation);
				Writer.WriteEndElement();
				propertiesAlreadyOutput.Add(LexExampleSentence.WellKnownProperties.Translation);
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

		public void AddMultitextGlosses(string propertyName, MultiText text) // review cp see WriteEmbeddedXmlCollection
		{
			WriteLanguageFormsInWrapper(GetOrderedAndFilteredForms(text, propertyName), "gloss", false);
			WriteFormsThatNeedToBeTheirOwnFields(text, propertyName);
			WriteEmbeddedXmlCollection(text);
		}

		public void AddMultitextForms(string propertyName, MultiText text) // review cp see WriteEmbeddedXmlCollection
		{
			WriteLanguageFormsInWrapper(GetOrderedAndFilteredForms(text, propertyName), "form", false);
			WriteFormsThatNeedToBeTheirOwnFields(text, propertyName);
			WriteEmbeddedXmlCollection(text);
		}

		private void WriteExtensible(IExtensible extensible)
		{
			foreach (var trait in extensible.Traits)
			{
				WriteTrait(trait);
			}
			foreach (var field in extensible.Fields)
			{
				Writer.WriteStartElement("field");
				Writer.WriteAttributeString("type", field.Type);
				WriteMultiTextNoWrapper(string.Empty /*what's this for*/ , field);
				foreach (var trait in field.Traits)
				{
					WriteTrait(trait);
				}
				Writer.WriteEndElement();
			}
		}

		private void WriteTrait(LexTrait trait)
		{
			Writer.WriteStartElement("trait");
			Writer.WriteAttributeString("name", trait.Name);
			Writer.WriteAttributeString("value", trait.Value.Trim());
			Writer.WriteEndElement();
		}

		private void WriteEmbeddedXmlCollection(MultiText text)
		{
			foreach (string rawXml in text.EmbeddedXmlElements) // todo cp Promote roundtripping to SIL.Lift / SIL.Data also then can use MultiTextBase here (or a better interface).
			{
				Writer.WriteRaw(rawXml);
			}
		}

		protected virtual void WriteFormsThatNeedToBeTheirOwnFields(MultiText text, string name) // review cp For PLiftExporter GetAudioForms
		{
		}

		protected void WriteLanguageFormsInWrapper(IEnumerable<LanguageForm> forms, string wrapper, bool doMarkTheFirst)
		{
			foreach (LanguageForm form in forms)
			{
				var spans = form.Spans;
				Writer.WriteStartElement(wrapper);
				Writer.WriteAttributeString("lang", form.WritingSystemId);
				if (doMarkTheFirst)
				{
					doMarkTheFirst = false;
					Writer.WriteAttributeString("first", "true"); //useful for headword
				}
				//string wrappedTextToExport = "<text>" + form.Form + "</text>";
				//string wrappedTextToExport = form.Form;
				XmlReaderSettings fragmentReaderSettings = new XmlReaderSettings();
				fragmentReaderSettings.ConformanceLevel = ConformanceLevel.Fragment;

				string scaryUnicodeEscaped = form.Form.EscapeAnyUnicodeCharactersIllegalInXml();
				string safeFromScaryUnicodeSoItStaysEscaped = scaryUnicodeEscaped.Replace("&#x", "");
				XmlReader testerForWellFormedness = XmlReader.Create(new StringReader("<temp>" + safeFromScaryUnicodeSoItStaysEscaped + "</temp>"));

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

				if (isTextWellFormedXml)
				{
					Writer.WriteStartElement("text");
					if (spans.Count > 0)
					{
						// Trick writer into knowing this is "mixed mode".
						Writer.WriteString("");
						int index = 0;
						int count;
						foreach (var span in spans)
						{
							// User edits may have effectively deleted the text of this span.
							if (span.Length <= 0)
								continue;
							if (index < span.Index)
							{
								count = span.Index - index;
								string txtBefore = String.Empty;
								if (index + count <= form.Form.Length)
									txtBefore = form.Form.Substring(index, count);
								else if (index < form.Form.Length)
									txtBefore = form.Form.Substring(index);
								Writer.WriteRaw(txtBefore.EscapeAnyUnicodeCharactersIllegalInXml());
							}
							var txtInner = WriteSpanStartElementAndGetText(form, span);
							Writer.WriteRaw(txtInner.EscapeAnyUnicodeCharactersIllegalInXml());
							Writer.WriteEndElement();
							index = span.Index + span.Length;
						}
						if (index < form.Form.Length)
						{
							var txtAfter = form.Form.Substring(index);
							Writer.WriteRaw(txtAfter.EscapeAnyUnicodeCharactersIllegalInXml());
						}
					}
					else
					{
						Writer.WriteRaw(form.Form.EscapeAnyUnicodeCharactersIllegalInXml());
					}
					Writer.WriteEndElement();
				}
				else
				{
					Writer.WriteStartElement("text");
					if (spans.Count > 0)
					{
						// Trick writer into knowing this is "mixed mode".
						Writer.WriteString("");
						int index = 0;
						int count;
						foreach (var span in spans)
						{
							// User edits may have effectively deleted the text of this span.
							if (span.Length <= 0)
								continue;
							if (index < span.Index)
							{
								count = span.Index - index;
								string txtBefore = String.Empty;
								if (index + count <= form.Form.Length)
									txtBefore = form.Form.Substring(index, count);
								else if (index < form.Form.Length)
									txtBefore = form.Form.Substring(index);
								Writer.WriteRaw(txtBefore.EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml());
							}
							var txtInner = WriteSpanStartElementAndGetText(form, span);
							Writer.WriteRaw(txtInner.EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml());
							Writer.WriteEndElement();
							index = span.Index + span.Length;
						}
						if (index < form.Form.Length)
						{
							var txtAfter = form.Form.Substring(index);
							Writer.WriteRaw(txtAfter.EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml());
						}
					}
					else
					{
						Writer.WriteRaw(form.Form.EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml());
					}
					Writer.WriteEndElement();
				}
				WriteFlags(form);
				Writer.WriteEndElement();
			}
		}

		string WriteSpanStartElementAndGetText(LanguageForm form, LanguageForm.FormatSpan span)
		{
			Writer.WriteStartElement("span");
			if (!String.IsNullOrEmpty(span.Lang))
				Writer.WriteAttributeString("lang", span.Lang);
			if (!String.IsNullOrEmpty(span.Class))
				Writer.WriteAttributeString("class", span.Class);
			if (!String.IsNullOrEmpty(span.LinkURL))
				Writer.WriteAttributeString("href", span.LinkURL);
			if (span.Index + span.Length <= form.Form.Length)
				return form.Form.Substring(span.Index, span.Length);
			else if (span.Index < form.Form.Length)
				return form.Form.Substring(span.Index);
			else
				return String.Empty;
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
				AddMultitextForms(propertyName, text);
			}
		}

		private void WriteGlossOneElementPerFormIfNonEmpty(MultiTextBase text)
		{
			if (MultiTextBase.IsEmpty(text))
			{
				return;
			}
			foreach (var form in GetOrderedAndFilteredForms(text, LexSense.WellKnownProperties.Gloss))
			{
				if (string.IsNullOrEmpty(form.Form))
				{
					continue;
				}
				Writer.WriteStartElement("gloss");
				Writer.WriteAttributeString("lang", form.WritingSystemId);
				Writer.WriteStartElement("text");
				Writer.WriteString(form.Form);
				Writer.WriteEndElement();
				WriteFlags(form);
				Writer.WriteEndElement();
			}
		}

		// review cp see WriteEmbeddedXmlCollection
		private bool WriteMultiWithWrapperIfNonEmpty(
			string propertyName, string wrapperName, MultiText text)
		{
			if (!MultiTextBase.IsEmpty(text))
			{
				Writer.WriteStartElement(wrapperName);
				// review cp see WriteEmbeddedXmlCollection
				AddMultitextForms(propertyName, text);

				if (text is IExtensible)
				{
					WriteExtensible((IExtensible)text);
				}
				Writer.WriteEndElement();
				return true;
			}
			return false;
		}

		public void AddNewEntry(LexEntry entry)
		{
			Writer.WriteStartElement("entry");
			Writer.WriteAttributeString("dateCreated",
										entry.CreationTime.ToLiftDateTimeFormat());
			Writer.WriteAttributeString("dateModified",
										entry.ModificationTime.ToLiftDateTimeFormat());
			Writer.WriteAttributeString("guid", entry.Guid.ToString());
			Writer.WriteEndElement();
		}

		public void AddDeletedEntry(LexEntry entry)
		{
			Writer.WriteStartElement("entry");
			Writer.WriteAttributeString("id",
										GetHumanReadableIdWithAnyIllegalUnicodeEscaped(entry));
			Writer.WriteAttributeString("dateCreated",
										entry.CreationTime.ToLiftDateTimeFormat());
			Writer.WriteAttributeString("dateModified",
										entry.ModificationTime.ToLiftDateTimeFormat());
			Writer.WriteAttributeString("guid", entry.Guid.ToString());
			Writer.WriteAttributeString("dateDeleted", DateTime.UtcNow.ToLiftDateTimeFormat());

			Writer.WriteEndElement();
		}

		#region IDisposable Members

#if DEBUG
		~LiftWriter()
		{
			if (!Disposed)
			{
				throw new ApplicationException("Disposed not explicitly called on LiftWriter." + "\n" + _constructionStack);
			}
		}
#endif

		protected bool Disposed { get; set; }

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!Disposed)
			{
				if (disposing)
				{
					// dispose-only, i.e. non-finalizable logic
					_writer?.Close();
				}

				// shared (dispose and finalizable) cleanup logic
				Disposed = true;
			}
		}

		protected void VerifyNotDisposed()
		{
			if (!Disposed)
			{
				throw new ObjectDisposedException("WeSayLiftWriter");
			}
		}

		#endregion
	}
}