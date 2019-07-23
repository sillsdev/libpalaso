using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using SIL.Code;
using SIL.IO;
using SIL.Xml;

namespace SIL.Lift
{
	/// <summary>
	/// <para>Given a lift or lift ranges pathname, sort it and write it back to the same file.</para>
	///
	/// <para>Sorting will include all attributes of all elements and any elements where the Lift specification allows for no ordering.
	/// Where possible, element names will be used for the ordering, but at times one or more attribute values will help.
	/// For instance, entry elements will be sorted by their guid attribute values. When the Lift specification does not allow for
	/// such sorting (e.g., sense elements) the order of that group of child elements will be retained, even though that group may be reordered,
	/// as a block among other sibling elements.</para>
	///
	/// <para>Besides being sorted, the resulting file will use the canonical xml writer settings defined elsewhere.
	/// The resulting file ought to make life easier for programs, such as Mercurial,
	/// as well as for humans who want to compare two, or more, versions of lift files.</para>
	/// </summary>
	public static class LiftSorter
	{
		private static readonly Encoding Utf8 = Encoding.UTF8;

		/// <summary>
		/// Sort the provided lift file into canonical order.
		///
		/// The resulting sorted file will be in a canonical order for the attributes and elements
		/// </summary>
		/// <param name="liftPathname">The assumed main lift file in a folder.</param>
		public static void SortLiftFile(string liftPathname)
		{
			Guard.AgainstNullOrEmptyString(liftPathname, "liftPathname");
			Guard.Against(Path.GetExtension(liftPathname).ToLowerInvariant() != ".lift", "Unexpected file extension");
			Guard.Against<FileNotFoundException>(!File.Exists(liftPathname), "Lift file does not exist.");

			using (var tempFile = new TempFile(File.ReadAllText(liftPathname), Utf8))
			{
				var sortedRootAttributes = SortRootElementAttributes(tempFile.Path);
				var sortedEntries = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
				XElement header = null;
				using (var splitter = new FastXmlElementSplitter(tempFile.Path))
				{
					bool hasHeader;
					foreach (var record in splitter.GetSecondLevelElementStrings("header", "entry", out hasHeader))
					{
						XElement element = FixBadTextElements(record);
						SortAttributes(element);

						if (hasHeader)
						{
							hasHeader = false;
							header = element;
							SortHeader(header);
						}
						else
						{
							var guidKey = element.Attribute("guid").Value.ToLowerInvariant();
							if (!sortedEntries.ContainsKey(guidKey))
							{
								SortEntry(element);
								sortedEntries.Add(GetUniqueKey(sortedEntries.Keys, guidKey), element);
							}
						}
					}
				}

				using (var writer = XmlWriter.Create(tempFile.Path, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("lift");

					foreach (var rootAttributeKvp in sortedRootAttributes)
					{
						var keyParts = rootAttributeKvp.Key.Split(':');
						if (keyParts.Length > 1)
							writer.WriteAttributeString(keyParts[0], keyParts[1], null, rootAttributeKvp.Value);
						else
							writer.WriteAttributeString(rootAttributeKvp.Key, rootAttributeKvp.Value);
					}

					if (header != null)
					{
						WriteElement(writer, header);
					}

					foreach (var entryElement in sortedEntries.Values)
					{
						WriteElement(writer, entryElement);
					}

					writer.WriteEndElement();
					writer.WriteEndDocument();
				}
				File.Copy(tempFile.Path, liftPathname, true);
			}
		}

		/// <summary>
		/// Sort zero or more lift range files in the folder that contains the given lift ranges file.
		///
		/// The resulting sorted file will be in a canonical order for the attributes and elements
		/// </summary>
		/// <param name="liftRangesPathname">The given lift ranges file (may or may not exist, which is fine)</param>
		public static void SortLiftRangesFiles(string liftRangesPathname)
		{
			Guard.AgainstNullOrEmptyString(liftRangesPathname, "liftRangesPathname");
			Guard.Against(Path.GetExtension(liftRangesPathname).ToLowerInvariant() != ".lift-ranges", "Unexpected file extension");
			var projectDir = Path.GetDirectoryName(liftRangesPathname);

			foreach (var liftRangesFile in Directory.GetFiles(projectDir, @"*.lift-ranges").OrderBy(filename => filename))
			{
				using (var tempFile = new TempFile(File.ReadAllText(liftRangesFile), Utf8))
				{
					var sortedRootAttributes = SortRootElementAttributes(tempFile.Path);

					var doc = XDocument.Load(liftRangesFile);
					FixBadTextElementsInXContainer(doc);

					SortAttributes(doc.Root);
					SortRanges(doc.Root);

					using (var writer = XmlWriter.Create(tempFile.Path, CanonicalXmlSettings.CreateXmlWriterSettings()))
					{
						writer.WriteStartDocument();
						writer.WriteStartElement(doc.Root.Name.LocalName);

						foreach (var rootAttributeKvp in sortedRootAttributes)
						{
							writer.WriteAttributeString(rootAttributeKvp.Key, rootAttributeKvp.Value);
						}

						foreach (var rangeElement in doc.Root.Elements())
						{
							WriteElement(writer, rangeElement);
						}

						writer.WriteEndElement();
						writer.WriteEndDocument();
					}

					File.Copy(tempFile.Path, liftRangesFile, true);
				}
			}
		}

		/// <summary>
		/// Return a set of the names of those elements in LIFT were we must not indent each child on a separate line.
		/// This is basically the elements which can contain both text and elements as children.
		/// In particular we must know about any parent where it is possible all children are elements, but where
		/// additional white space is significant.
		/// </summary>
		public static HashSet<string> LiftSuppressIndentingChildren
		{
			get
			{
				return new HashSet<string> {"text", "span"};
			}
		}

		private static void SortRange(XElement rangeElement)
		{
			var sortedChildRanges = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var childRangeElement in rangeElement.Elements("range-element").ToList())
			{
				SortBasicsOfRangeElement(childRangeElement);
				sortedChildRanges.Add(GetUniqueKey(sortedChildRanges.Keys, childRangeElement.Attribute("id").Value), childRangeElement);
				childRangeElement.Remove();
			}

			SortBasicsOfRangeElement(rangeElement);

			foreach (var childRangeElement in sortedChildRanges.Values)
			{
				rangeElement.Add(childRangeElement);
			}
		}

		private static void SortBasicsOfRangeElement(XElement rangeElement)
		{
			var description = rangeElement.Element("description");
			if (description != null)
			{
				SortMultiformElement(description);
				description.Remove();
			}
			var label = rangeElement.Element("label");
			if (label != null)
			{
				SortMultiformElement(label);
				label.Remove();
			}
			var abbrev = rangeElement.Element("abbrev");
			if (abbrev != null)
			{
				SortMultiformElement(abbrev);
				abbrev.Remove();
			}
			var leftovers = rangeElement.Elements().ToList();

			rangeElement.RemoveNodes();

			if (description != null)
				rangeElement.Add(description);
			if (label != null)
				rangeElement.Add(label);
			if (abbrev != null)
				rangeElement.Add(abbrev);
			foreach (var leftover in leftovers)
			{
				rangeElement.Add(leftover);
			}
		}

		private static void WriteElement(XmlWriter writer, XElement element)
		{
			XmlUtils.WriteNode(writer, element, LiftSuppressIndentingChildren);
		}

		private static SortedDictionary<string, string> SortRootElementAttributes(string pathname)
		{
			var sortedRootAttributes = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
			using (var tempReader = XmlReader.Create(pathname, CanonicalXmlSettings.CreateXmlReaderSettings()))
			{
				tempReader.MoveToContent();
				for (var i = 0; i < tempReader.AttributeCount; ++i)
				{
					tempReader.MoveToAttribute(i);
					sortedRootAttributes.Add(GetUniqueKey(sortedRootAttributes.Keys, tempReader.Name), tempReader.Value);
				}
			}
			return sortedRootAttributes;
		}

		private static void SortHeader(XElement header)
		{
			// Has three optional elements: description, ranges, and fields.
			// Put them in this order, if present: description, ranges, and fields.
			var description = header.Element("description");
			if (description != null)
			{
				description.Remove();
			}
			var ranges = header.Element("ranges");
			if (ranges != null)
			{
				ranges.Remove();
			}
			var fields = header.Element("fields");
			if (fields != null)
			{
				fields.Remove();
			}
			var leftovers = header.Elements().ToList();

			header.RemoveNodes();

			if (description != null)
			{
				SortMultiformElement(description);
				header.Add(description);
			}
			if (ranges != null)
			{
				SortRanges(ranges);
				header.Add(ranges);
			}
			if (fields != null)
			{
				var sortedFields = SortFields(fields, "tag");
				header.Add(fields);
				foreach (var field in sortedFields.Values)
				{
					fields.Add(field);
				}
			}

			foreach (var leftover in leftovers)
			{
				header.Add(leftover);
			}
		}

		private static void SortRanges(XElement rangesParent)
		{
			if (!rangesParent.HasElements)
				return;

			// Optional, multiple <range> elements. Unordered. Key attr: id
			var sortedRanges = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var rangeElement in rangesParent.Elements("range").ToList())
			{
				SortRange(rangeElement);
				sortedRanges.Add(GetUniqueKey(sortedRanges.Keys, rangeElement.Attribute("id").Value), rangeElement);
				rangeElement.Remove();
			}
			var leftovers = rangesParent.Elements().ToList();

			rangesParent.RemoveNodes();

			foreach (var rangeElement in sortedRanges.Values)
			{
				rangesParent.Add(rangeElement);
			}
			foreach (var leftover in leftovers)
			{
				rangesParent.Add(leftover);
			}
		}

		private static SortedDictionary<string, XElement> SortFields(XElement fieldsParent, string keyFieldAttribute)
		{
			var sortedFields = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);

			if (!fieldsParent.HasElements)
				return sortedFields;

			// Optional, multiple <field> elements. Unordered. Key attr: tag
			foreach (var fieldElement in fieldsParent.Elements("field").ToList())
			{
				var sortedForms = SortMultiformElementCore(fieldElement);
				var sortedExtensibleSansFields = SortExtensibleSansField(fieldElement);

				sortedFields.Add(GetUniqueKey(sortedFields.Keys, fieldElement.Attribute(keyFieldAttribute).Value), fieldElement);

				fieldElement.Remove();

				foreach (var form in sortedForms.Values)
				{
					fieldElement.Add(form);
				}
				RestoreSortedExtensibles(sortedExtensibleSansFields, fieldElement);
			}

			return sortedFields;
		}

		private static void SortEntry(XElement entry)
		{
			var lexicalUnit = entry.Element("lexical-unit");
			if (lexicalUnit != null)
			{
				SortMultiformElement(lexicalUnit);
				lexicalUnit.Remove();
			}
			var citation = entry.Element("citation");
			if (citation != null)
			{
				SortMultiformElement(citation);
				citation.Remove();
			}

			var pronunciations = SortPronunciations(entry);
			var variants = SortVariantContents(entry);
			var sensesInFileOrder = SortSenseContent(entry, "sense");
			var sortedEtymologies = SortedEtymologies(entry);
			var notes = SortedNotes(entry);
			var sortedRelations = SortedRelations(entry);
			var sortedExtensibles = SortExtensibleWithField(entry);
			var leftovers = entry.Elements().ToList();

			entry.RemoveNodes();

			if (lexicalUnit != null)
				entry.Add(lexicalUnit);
			if (citation != null)
				entry.Add(citation);
			foreach (var pronunciation in pronunciations)
				entry.Add(pronunciation);
			foreach (var variant in variants)
				entry.Add(variant);
			foreach (var sense in sensesInFileOrder)
				entry.Add(sense);
			foreach (var note in notes.Values)
				entry.Add(note);
			foreach (var relation in sortedRelations.Values)
				entry.Add(relation);
			foreach (var etymology in sortedEtymologies.Values)
				entry.Add(etymology);
			RestoreSortedExtensibles(sortedExtensibles, entry);
			foreach (var leftover in leftovers)
				entry.Add(leftover);
		}

		private static SortedDictionary<string, XElement> SortedNotes(XElement notesParent)
		{
			var sortedNotes = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var fakeKeyPart = 1;
			var tempGuid = Guid.NewGuid();
			foreach (var note in notesParent.Elements("note").ToList())
			{
				var sortedForms = SortMultiformElementCore(note);
				var typeAttr = note.Attribute("type");
				var key = typeAttr == null ? "@@@@@@" + tempGuid.ToString() + fakeKeyPart : typeAttr.Value;
				sortedNotes.Add(GetUniqueKey(sortedNotes.Keys, key), note);
				if (typeAttr == null)
					fakeKeyPart++;

				var sortedExtensibles = SortExtensibleWithField(note);

				note.Remove();

				foreach (var form in sortedForms.Values)
				{
					note.Add(form);
				}
				RestoreSortedExtensibles(sortedExtensibles, note);
			}
			return sortedNotes;
		}

		private static SortedDictionary<string, XElement> SortedEtymologies(XElement entry)
		{
			var sortedEtymologies = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);

			foreach (var etymology in entry.Elements("etymology").ToList())
			{
				var sortedForms = SortMultiformElementCore(etymology);
				var sortedGlosses = new SortedDictionary<string, XElement>();
				foreach (var gloss in etymology.Elements("gloss").ToList())
				{
					sortedGlosses.Add(GetUniqueKey(sortedGlosses.Keys, gloss.Attribute("lang").Value), gloss);
					gloss.Remove();
				}
				var sortedExtensibles = SortExtensibleWithField(etymology);

				sortedEtymologies.Add(GetUniqueKey(sortedEtymologies.Keys, etymology.Attribute("source").Value + etymology.Attribute("type").Value), etymology);
				etymology.Remove();

				foreach (var form in sortedForms.Values)
				{
					etymology.Add(form);
				}
				foreach (var gloss in sortedGlosses.Values)
				{
					etymology.Add(gloss);
				}
				RestoreSortedExtensibles(sortedExtensibles, etymology);
			}
			return sortedEtymologies;
		}

		private static SortedDictionary<string, XElement> SortedRelations(XElement relationParent)
		{
			var sortedRelations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var relations = relationParent.Elements("relation").ToList();
			foreach (var relation in relations)
			{
				var usage = relation.Element("usage");
				if (usage != null)
				{
					SortMultiformElement(usage);
					usage.Remove();
				}
				var sortedExtensible = SortExtensibleWithField(relation);
				sortedRelations.Add(GetUniqueKey(sortedRelations.Keys, relation.Attribute("ref").Value + relation.Attribute("type").Value), relation);

				relation.Remove();

				RestoreSortedExtensibles(sortedExtensible, relation);
				if (usage != null)
				{
					relation.Add(usage);
				}
			}
			return sortedRelations;
		}

		private static IEnumerable<XElement> SortVariantContents(XElement entry)
		{
			var variants = entry.Elements("variant").ToList();
			foreach (var variant in variants)
			{
				var sortedForms = SortMultiformElementCore(variant);
				var sortedExtensible = SortExtensibleWithField(variant);
				var sortedPronunciations = SortPronunciations(variant);
				var sortedRelations = SortedRelations(variant);

				variant.Remove();

				foreach (var form in sortedForms.Values)
				{
					variant.Add(form);
				}
				RestoreSortedExtensibles(sortedExtensible, variant);
				foreach (var pronunciation in sortedPronunciations)
				{
					variant.Add(pronunciation);
				}
				foreach (var relation in sortedRelations.Values)
				{
					variant.Add(relation);
				}
			}
			return variants;
		}

		private static IEnumerable<XElement> SortPronunciations(XElement entry)
		{
			/*
		  <ref name="multitext-content"/>
		  <ref name="extensible-content"/>
		  <zeroOrMore>
			<element name="media">
			  <ref name="URLRef-content"/>
			</element>
		  </zeroOrMore>
			*/
			var pronunciations = entry.Elements("pronunciation").ToList();
			foreach (var pronunciation in pronunciations)
			{
				var sortedForms = SortMultiformElementCore(pronunciation);
				var sortedExtensible = SortExtensibleWithField(pronunciation);
				var sortedMedia = SortedMedia(pronunciation);

				pronunciation.Remove();

				foreach (var form in sortedForms.Values)
				{
					pronunciation.Add(form);
				}
				RestoreSortedExtensibles(sortedExtensible, pronunciation);
				foreach (var media in sortedMedia.Values)
				{
					pronunciation.Add(media);
				}
			}
			return pronunciations;
		}

		private static void RestoreSortedExtensibles(Dictionary<string, SortedDictionary<string, XElement>> sortedExtensible, XElement parent)
		{
			var current = sortedExtensible["annotations"];
			foreach (var annotation in current.Values)
			{
				parent.Add(annotation);
			}

			current = sortedExtensible["traits"];
			foreach (var trait in current.Values)
			{
				parent.Add(trait);
			}

			if (!sortedExtensible.TryGetValue("fields", out current))
				return;

			foreach (var field in current.Values)
			{
				parent.Add(field);
			}
		}

		private static SortedDictionary<string, XElement> SortedMedia(XElement mediaParent)
		{
			var sortedMedia = new SortedDictionary<string, XElement>();

			foreach (var media in mediaParent.Elements("media").ToList())
			{
				var label = media.Element("label");
				if (label != null)
				{
					var sortedForms = SortMultiformElementCore(label);

					foreach (var form in sortedForms.Values)
					{
						label.Add(form);
					}
				}
				sortedMedia.Add(GetUniqueKey(sortedMedia.Keys, media.Attribute("href").Value), media);
				media.Remove();
			}

			return sortedMedia;
		}

		private static IEnumerable<XElement> SortSenseContent(XElement senseParent, string senseElementName)
		{
			var sensesInFileOrder = senseParent.Elements(senseElementName).ToList();

			foreach (var sense in sensesInFileOrder)
			{
				var definition = sense.Element("definition");
				if (definition != null)
				{
					SortMultiformElement(definition);
					definition.Remove();
				}
				var sortedGlosses = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
				var glosses = sense.Elements("gloss").ToList();
				foreach (var gloss in glosses)
				{
					var form = gloss.Element("form");
					if (form != null)
						SortFormContent(form);
					sortedGlosses.Add(GetUniqueKey(sortedGlosses.Keys, gloss.Attribute("lang").Value), gloss);
					gloss.Remove();
				}
				var grammaticalInfo = SortGrammaticalInfoContent(sense);
				var examples = SortedExamplesContents(sense);
				var reversals = SortedReversals(sense);
				var subsenses = SortSenseContent(sense, "subsense");
				var sortedRelations = SortedRelations(sense);
				var sortedIllustrations = SortedIllustrations(sense);
				var sortedNotes = SortedNotes(sense);
				var sortedExtensible = SortExtensibleWithField(sense);
				var leftovers = sense.Elements().ToList();

				sense.RemoveNodes();

				if (definition != null)
					sense.Add(definition);
				foreach (var gloss in sortedGlosses.Values)
					sense.Add(gloss);
				if (grammaticalInfo != null)
					sense.Add(grammaticalInfo);
				foreach (var example in examples)
					sense.Add(example);
				foreach (var reversal in reversals.Values)
					sense.Add(reversal);
				foreach (var subsense in subsenses)
					sense.Add(subsense);
				foreach (var relation in sortedRelations.Values)
					sense.Add(relation);
				foreach (var illustration in sortedIllustrations.Values)
					sense.Add(illustration);
				foreach (var note in sortedNotes.Values)
					sense.Add(note);
				RestoreSortedExtensibles(sortedExtensible, sense);
				foreach (var leftover in leftovers)
					sense.Add(leftover);
				sense.Remove();
			}
			return sensesInFileOrder;
		}

		private static SortedDictionary<string, XElement> SortedIllustrations(XElement sense)
		{
			var sortedIllustrations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var illustrations = sense.Elements("illustration").ToList();
			foreach (var illustration in illustrations)
			{
				var label = illustration.Element("label");
				if (label != null)
					SortMultiformElement(label);
				sortedIllustrations.Add(GetUniqueKey(sortedIllustrations.Keys, illustration.Attribute("href").Value), illustration);
				illustration.Remove();
			}
			return sortedIllustrations;
		}

		private static SortedDictionary<string, XElement> SortedReversals(XElement reversalParent)
		{
			var sortedReversals = new SortedDictionary<string, XElement>();
			var fakeKeyPart = 1;
			var tempGuid = Guid.NewGuid();
			foreach (var reversal in reversalParent.Elements("reversal").ToList())
			{
				SortReversalContents(reversal);

				var typeAttr = reversal.Attribute("type");
				var key = typeAttr == null ? "@@@@@@" + tempGuid.ToString() + fakeKeyPart : typeAttr.Value;
				sortedReversals.Add(GetUniqueKey(sortedReversals.Keys, key), reversal);
				if (typeAttr == null)
					fakeKeyPart++;
			}

			return sortedReversals;
		}

		private static void SortReversalContents(XElement reversal)
		{
			var sortedForms = SortMultiformElementCore(reversal);
			var grammaticalInfo = SortGrammaticalInfoContent(reversal);

			var sortedNestedReversal = reversal.Element("main");
			if (sortedNestedReversal != null)
				SortReversalContents(sortedNestedReversal);

			reversal.Remove();

			foreach (var form in sortedForms.Values)
			{
				reversal.Add(form);
			}
			if (grammaticalInfo != null)
				reversal.Add(grammaticalInfo);
			if (sortedNestedReversal != null)
				reversal.Add(sortedNestedReversal);
		}

		private static IEnumerable<XElement> SortedExamplesContents(XElement sense)
		{
			var examples = sense.Elements("example").ToList();
			foreach (var example in examples)
			{
				var sortedForms = SortMultiformElementCore(example);
				var sortedTranslations = SortedTranslations(example);
				var sortedNotes = SortedNotes(example);
				var sortedExtensibles = SortExtensibleWithField(example);

				example.Remove();

				foreach (var form in sortedForms.Values)
				{
					example.Add(form);
				}
				foreach (var translation in sortedTranslations.Values)
				{
					example.Add(translation);
				}
				foreach (var note in sortedNotes.Values)
				{
					example.Add(note);
				}
				RestoreSortedExtensibles(sortedExtensibles, example);
			}
			return examples;
		}

		private static SortedDictionary<string, XElement> SortedTranslations(XElement translationParent)
		{
			var sortedTranslations = new SortedDictionary<string, XElement>();
			var fakeKeyPart = 1;
			var tempGuid = Guid.NewGuid();
			foreach (var translation in translationParent.Elements("translation").ToList())
			{
				var sortedForms = SortMultiformElementCore(translation);
				var typeAttr = translation.Attribute("type");
				var key = typeAttr == null ? "@@@@@@" + tempGuid.ToString() + fakeKeyPart : typeAttr.Value;
				sortedTranslations.Add(GetUniqueKey(sortedTranslations.Keys, key), translation);
				if (typeAttr == null)
					fakeKeyPart++;

				translation.Remove();

				foreach (var form in sortedForms.Values)
				{
					translation.Add(form);
				}
			}
			return sortedTranslations;
		}

		private static XElement SortGrammaticalInfoContent(XElement grammaticalInfoParent)
		{
			var grammaticalInfo = grammaticalInfoParent.Element("grammatical-info");
			if (grammaticalInfo != null)
			{
				var sortedTraits = SortedTraits(grammaticalInfo);

				grammaticalInfo.Remove();

				foreach (var trait in sortedTraits.Values)
				{
					grammaticalInfo.Add(trait);
				}
			}
			return grammaticalInfo;
		}

		private static Dictionary<string, SortedDictionary<string, XElement>> SortExtensibleSansField(XElement extensibleParent)
		{
			return new Dictionary<string, SortedDictionary<string, XElement>>(2, StringComparer.InvariantCultureIgnoreCase)
				{
					{"annotations", SortedAnnotations(extensibleParent)},
					{"traits", SortedTraits(extensibleParent)}
				};
		}

		private static Dictionary<string, SortedDictionary<string, XElement>> SortExtensibleWithField(XElement extensibleParent)
		{
			return new Dictionary<string, SortedDictionary<string, XElement>>(2, StringComparer.InvariantCultureIgnoreCase)
				{
					{"annotations", SortedAnnotations(extensibleParent)},
					{"traits", SortedTraits(extensibleParent)},
					{"fields", SortFields(extensibleParent, "type")}
				};
		}

		private static SortedDictionary<string, XElement> SortedAnnotations(XElement annotationParent)
		{
			var sortedAnnotations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var fakeKeyPart = 1;
			var tempGuid = Guid.NewGuid();
			foreach (var annotation in annotationParent.Elements("annotation").ToList())
			{
				var sortedForms = SortMultiformElementCore(annotation);
				var valueAttr = annotation.Attribute("value");
				var valuePartOfKey = valueAttr == null ? "@@@@@@" + tempGuid.ToString() + fakeKeyPart : valueAttr.Value;
				sortedAnnotations.Add(GetUniqueKey(sortedAnnotations.Keys, annotation.Attribute("name").Value + valuePartOfKey), annotation);
				if (valueAttr == null)
					fakeKeyPart++;
				foreach (var form in sortedForms.Values)
				{
					annotation.Add(form);
				}
				annotation.Remove();
			}
			return sortedAnnotations;
		}

		private static SortedDictionary<string, XElement> SortedTraits(XElement traitParent)
		{
			var sortedTraits = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var trait in traitParent.Elements("trait").ToList())
			{
				var sortedAnnotations = SortedAnnotations(trait);
				sortedTraits.Add(GetUniqueKey(sortedTraits.Keys, trait.Attribute("name").Value + trait.Attribute("value").Value), trait);
				trait.Remove();

				foreach (var annotation in sortedAnnotations.Values)
				{
					trait.Add(annotation);
				}
			}
			return sortedTraits;
		}

		private static void SortMultiformElement(XElement multiformElementParent)
		{
			if (multiformElementParent == null)
				return;

			var sortedForms = SortMultiformElementCore(multiformElementParent);

			foreach (var form in sortedForms.Values)
				multiformElementParent.Add(form);
		}

		private static SortedDictionary<string, XElement> SortMultiformElementCore(XElement multiformElementParent)
		{
			var sortedForms = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			if (multiformElementParent == null)
				return sortedForms;

			foreach (var form in multiformElementParent.Elements("form").ToList())
			{
				SortFormContent(form);
				sortedForms.Add(GetUniqueKey(sortedForms.Keys, form.Attribute("lang").Value), form);
				form.Remove();
			}
			return sortedForms;
		}

		private static void SortFormContent(XElement formElement)
		{
			// Don't even think of messing with the <text> innards.
			var textElement = formElement.Element("text");
			textElement.Remove();
			var sortedAnnotations = SortedAnnotations(formElement);

			formElement.Add(textElement);
			foreach (var annotation in sortedAnnotations.Values)
			{
				formElement.Add(annotation);
			}
		}

		private static void SortAttributes(XElement element)
		{
			if (element.HasElements)
			{
				// Recurse all the way down
				foreach (var childElement in element.Elements())
					SortAttributes(childElement);
			}

			if (element.Attributes().Count() < 2)
				return;

			var sortedAttributes = new SortedDictionary<string, XAttribute>();
			foreach (var attr in element.Attributes())
			{
				sortedAttributes.Add(GetUniqueKey(sortedAttributes.Keys, attr.Name.LocalName), attr);
			}

			element.Attributes().Remove();
			foreach (var sortedAttrKvp in sortedAttributes)
				element.Add(sortedAttrKvp.Value);
		}

		internal static string GetUniqueKey(ICollection<string> keys, string keyCandidate)
		{
			var keyWithSuffix = keyCandidate;
			var suffix = 1;
			while (true)
			{
				if (!keys.Contains(keyWithSuffix))
					return keyWithSuffix;
				keyWithSuffix = keyCandidate + suffix++;
			}
		}

		public static XElement FixBadTextElements(string parentEntry)
		{
			var parentElement = XElement.Parse(parentEntry);
			FixBadTextElementsInXContainer(parentElement);
			return parentElement;
		}

		public static void FixBadTextElementsInXContainer(XContainer parentElement)
		{
			foreach (var misnamedElement in parentElement.Descendants("element"))
			{
				misnamedElement.Name = "text";
				misnamedElement.Attribute("name").Remove();
			}
		}
	}
}
