using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Palaso.Code;
using Palaso.IO;
using Palaso.Xml;

namespace Palaso.Lift
{
// TODO: multi-forms.
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
	/// as well as for humans who want to compare two or more versions of lift files.</para>
	/// </summary>
	public static class LiftSorter
	{
		private static readonly Encoding Utf8 = Encoding.UTF8;

		public static void SortLiftFile(string liftPathname)
		{
			Guard.AgainstNullOrEmptyString(liftPathname, "liftPathname");
			Guard.Against(Path.GetExtension(liftPathname).ToLowerInvariant() != ".lift", "Unexpected file extension");
			Guard.Against<FileNotFoundException>(!File.Exists(liftPathname), "Lift file does not exist.");

			using (var tempFile = new TempFile(File.ReadAllText(liftPathname), Utf8))
			{
				var sortedRootAttributes = SortRootElementAttributes(tempFile.Path);
				var sortedEntries = new SortedDictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
				XElement header = null;
				using (var splitter = new FastXmlElementSplitter(tempFile.Path))
				{
					bool hasHeader;
					foreach (var record in splitter.GetSecondLevelElementStrings("header", "entry", out hasHeader))
					{
						var element = XElement.Parse(record);
						SortAttributes(element);

						if (hasHeader)
						{
							hasHeader = false;
							header = element;
							SortHeader(header);
						}
						else
						{
							SortEntry(element);
							sortedEntries[element.Attribute("guid").Value] = Utf8.GetBytes(element.ToString());
						}
					}
				}
				using (var writer = XmlWriter.Create(tempFile.Path, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("lift");

					foreach (var rootAttributeKvp in sortedRootAttributes)
					{
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

		public static void SortLiftRangesFile(string liftRangesPathname)
		{
			Guard.AgainstNullOrEmptyString(liftRangesPathname, "liftRangesPathname");
			Guard.Against(Path.GetExtension(liftRangesPathname).ToLowerInvariant() != ".lift-ranges", "Unexpected file extension");
			Guard.Against<FileNotFoundException>(!File.Exists(liftRangesPathname), "Lift range file does not exist.");

			using (var tempFile = new TempFile(File.ReadAllText(liftRangesPathname), Utf8))
			{
				var sortedRanges = new SortedDictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
				using (var splitter = new FastXmlElementSplitter(tempFile.Path))
				{
					// lift-ranges
					bool hasHeader;
					foreach (var record in splitter.GetSecondLevelElementStrings(null, "range", out hasHeader))
					{
						var rangeElement = XElement.Parse(record);
						SortAttributes(rangeElement);
						SortRange(rangeElement);
						sortedRanges[rangeElement.Attribute("id").Value] = Utf8.GetBytes(rangeElement.ToString());
					}
				}
				using (var writer = XmlWriter.Create(tempFile.Path, CanonicalXmlSettings.CreateXmlWriterSettings()))
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("lift-ranges");

					foreach (var entryElement in sortedRanges.Values)
					{
						WriteElement(writer, entryElement);
					}

					writer.WriteEndElement();
					writer.WriteEndDocument();
				}
				File.Copy(tempFile.Path, liftRangesPathname, true);
			}
		}

		private static void SortRange(XElement rangeElement)
		{
			var description = rangeElement.Element("description");
			var label = rangeElement.Element("label");
			var abbrev = rangeElement.Element("abbrev");
			var sortedRanges = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var rangeElementElement in rangeElement.Elements("range-element"))
			{
				sortedRanges.Add(rangeElementElement.Attribute("id").Value, rangeElementElement);
			}

			rangeElement.RemoveNodes();
			if (description != null)
				rangeElement.Add(description);
			if (label != null)
				rangeElement.Add(label);
			if (abbrev != null)
				rangeElement.Add(abbrev);
			foreach (var rangeElementElement in sortedRanges.Values)
			{
				SortRangeElementElement(rangeElementElement);
				rangeElement.Add(rangeElementElement);
			}
		}

		private static void SortRangeElementElement(XElement rangeElementElement)
		{
			var description = rangeElementElement.Element("description");
			var label = rangeElementElement.Element("label");
			var abbrev = rangeElementElement.Element("abbrev");

			rangeElementElement.RemoveNodes();
			if (description != null)
				rangeElementElement.Add(description);
			if (label != null)
				rangeElementElement.Add(label);
			if (abbrev != null)
				rangeElementElement.Add(abbrev);
		}

		private static void WriteElement(XmlWriter writer, XElement element)
		{
			WriteElement(writer, element.ToString());
		}

		private static void WriteElement(XmlWriter writer, string element)
		{
			WriteElement(writer, Utf8.GetBytes(element));
		}

		private static void WriteElement(XmlWriter writer, byte[] element)
		{
			using (var nodeReader = XmlReader.Create(new MemoryStream(element, false), CanonicalXmlSettings.CreateXmlReaderSettings(ConformanceLevel.Fragment)))
				writer.WriteNode(nodeReader, true);
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
					sortedRootAttributes.Add(tempReader.LocalName, tempReader.Value);
				}
			}
			return sortedRootAttributes;
		}

		private static void SortHeader(XElement header)
		{
			// Has three optional elements: description, ranges, and fields.
			// Put them in this order, if present: description, ranges, and fields.
			var description = header.Element("description");
			var ranges = header.Element("ranges");
			var fields = header.Element("fields");
			header.RemoveNodes();
			if (description != null)
			{
				header.Add(description);
			}
			if (ranges != null)
			{
				SortRanges(ranges);
				header.Add(ranges);
			}
			if (fields != null)
			{
				SortFields(fields, "tag");
				header.Add(fields);
			}
		}

		private static void SortRanges(XElement rangesParent)
		{
			if (!rangesParent.HasElements)
				return;

			// Optional, multiple <range> elements. Unordered. Key attr: id
			var sortedRanges = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var rangeElement in rangesParent.Elements("range"))
			{
				sortedRanges.Add(rangeElement.Attribute("id").Value, rangeElement);
			}

			rangesParent.RemoveNodes();
			foreach (var rangeElement in sortedRanges.Values)
			{
				rangesParent.Add(rangeElement);
			}
		}

		private static void SortFields(XElement fieldsParent, string keyAttribute)
		{
			if (!fieldsParent.HasElements)
				return;

			// Optional, multiple <field> elements. Unordered. Key attr: tag
			var sortedFields = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			foreach (var fieldElement in fieldsParent.Elements("field"))
			{
				sortedFields.Add(fieldElement.Attribute(keyAttribute).Value, fieldElement);
			}

			fieldsParent.RemoveNodes();
			foreach (var rangeElement in sortedFields.Values)
			{
				fieldsParent.Add(rangeElement);
			}
		}

		private static void SortEntry(XElement entry)
		{
			/*
<optional>
	<element name="lexical-unit"> Singleton
		<ref name="multitext-content"/>
	</element>
</optional>
			 */
			var lexicalUnit = entry.Element("lexical-unit");
			if (lexicalUnit != null)
				lexicalUnit.Remove();
			/*
<optional>
	<element name="citation"> Singleton
		<ref name="multitext-content"/>
	</element>
</optional>
			 */
			var citation = entry.Element("citation");
			if (citation != null)
				citation.Remove();
			 /*
<zeroOrMore> Leave "as is", but together
	<element name="pronunciation">
		<ref name="pronunciation-content"/>
	</element>
</zeroOrMore>
			 */
			var pronunciations = entry.Elements("pronunciation").ToList();
			foreach (var pronunciation in pronunciations)
				pronunciation.Remove();
			/*
<zeroOrMore> Leave "as is", but together
	<element name="variant">
		<ref name="variant-content"/>
	</element>
</zeroOrMore>
			 */
			var variants = entry.Elements("variant").ToList();
			foreach (var variant in variants)
				variant.Remove();
			 /*
<zeroOrMore> Leave "as is", but together
	<element name="sense">
		<ref name="sense-content"/>
	</element>
</zeroOrMore>
			 */
			var senses = entry.Elements("sense").ToList();
			foreach (var sense in senses)
			{
				SortSense(sense);
				sense.Remove();
			}
			 /*
<zeroOrMore>
	<element name="annotation">
		combined key: name+value
	</element>
</zeroOrMore>
			 */
			var sortedAnnotations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var annotations = entry.Elements("annotation").ToList();
			foreach (var annotation in annotations)
			{
				sortedAnnotations.Add(annotation.Attribute("name").Value + annotation.Attribute("value").Value, annotation);
				annotation.Remove();
			}
			/*
<zeroOrMore>
   <element name="etymology">
	   combined key: type+source
   </element>
</zeroOrMore>
			 */
			var sortedEtymologies = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var etymologies = entry.Elements("etymology").ToList();
			foreach (var etymology in etymologies)
			{
				sortedEtymologies.Add(etymology.Attribute("source").Value + etymology.Attribute("type").Value, etymology);
				etymology.Remove();
			}
			/*
<zeroOrMore>
   <element name="field">
	   // key attr: type
   </element>
</zeroOrMore>
			 */
			var sortedFields = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var fields = entry.Elements("field").ToList();
			foreach (var field in fields)
			{
				sortedFields.Add(field.Attribute("type").Value, field);
				field.Remove();
			}
			/*
<zeroOrMore> Leave "as is", but together
   <element name="note">
	   <ref name="note-content"/>
   </element>
</zeroOrMore>
			 */
			var notes = entry.Elements("note").ToList();
			foreach (var note in notes)
				note.Remove();
			/*
<zeroOrMore>
   <element name="relation">
	   combined key: type+ref
   </element>
</zeroOrMore>
			 */
			var sortedRelations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var relations = entry.Elements("relation").ToList();
			foreach (var relation in relations)
			{
				sortedRelations.Add(relation.Attribute("ref").Value + relation.Attribute("type").Value, relation);
				relation.Remove();
			}
			/*
<zeroOrMore>
   <element name="trait">">
	   combined key: name+value
   </element>
</zeroOrMore>
		   */
			var sortedTraits = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var traits = entry.Elements("trait").ToList();
			foreach (var trait in traits)
			{
				sortedTraits.Add(trait.Attribute("name").Value + trait.Attribute("value").Value, trait);
				trait.Remove();
			}

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
			foreach (var sense in senses)
				entry.Add(sense);
			foreach (var annotation in sortedAnnotations.Values)
				entry.Add(annotation);
			foreach (var etymology in sortedEtymologies.Values)
				entry.Add(etymology);
			foreach (var field in sortedFields.Values)
				entry.Add(field);
			foreach (var note in notes)
				entry.Add(note);
			foreach (var relation in sortedRelations.Values)
				entry.Add(relation);
			foreach (var trait in sortedTraits.Values)
				entry.Add(trait);
			foreach (var leftover in leftovers)
				entry.Add(leftover);
		}

		private static void SortSense(XElement sense)
		{
			/*
							  <optional>
								<element name="definition">
								  <ref name="multitext-content"/>
								</element>
							  </optional>
			 */
			var definition = sense.Element("definition");
			if (definition != null)
				definition.Remove();
			/*
							  <zeroOrMore>
								<element name="gloss">
								  <ref name="form-content"/>
								</element>
							  </zeroOrMore>
			 */
			var sortedGlosses = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var glosses = sense.Elements("gloss").ToList();
			foreach (var gloss in glosses)
			{
				sortedGlosses.Add(gloss.Attribute("lang").Value, gloss);
				gloss.Remove();
			}
			/*
							  <optional>
								<element name="grammatical-info">
								  <ref name="grammatical-info-content"/>
								</element>
							  </optional>
			 */
			var grammaticalInfo = sense.Element("grammatical-info");
			if (grammaticalInfo != null)
				grammaticalInfo.Remove();
			/*
							  <zeroOrMore>
								<element name="example">
								  <ref name="example-content"/>
								</element>
							  </zeroOrMore>
			 */
			var examples = sense.Elements("example").ToList();
			foreach (var example in examples)
				example.Remove();
			/*
							  <zeroOrMore>
								<element name="reversal">
								  <ref name="reversal-content"/>
								</element>
							  </zeroOrMore>
			 */
			var reversals = sense.Elements("reversal").ToList();
			foreach (var reversal in reversals)
				reversal.Remove();
			/*
							<zeroOrMore>
								<element name="subsense">
								  <ref name="sense-content"/>
								</element>
							  </zeroOrMore>
			 */
			var subsenses = sense.Elements("subsense").ToList();
			foreach (var subsense in subsenses)
			{
				SortSense(subsense);
				subsense.Remove();
			}
			/*
							<zeroOrMore>
								<element name="relation">
								  <ref name="relation-content"/>
								</element>
							  </zeroOrMore>
			 */
			var sortedRelations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var relations = sense.Elements("relation").ToList();
			foreach (var relation in relations)
			{
				sortedRelations.Add(relation.Attribute("ref").Value + relation.Attribute("type").Value, relation);
				relation.Remove();
			}
			/*
							  <zeroOrMore>
								<element name="illustration">
								  <ref name="URLRef-content"/>
								</element>
							  </zeroOrMore>
			 */
			var sortedIllustrations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var illustrations = sense.Elements("illustration").ToList();
			foreach (var illustration in illustrations)
			{
				sortedIllustrations.Add(illustration.Attribute("href").Value, illustration);
				illustration.Remove();
			}
			/*
							  <zeroOrMore>
								<element name="note">
								  <ref name="note-content"/>
								</element>
							  </zeroOrMore>
			 */
			var sortedNotes = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var notes = sense.Elements("note").ToList();
			int fakeNoteKey = 1;
			foreach (var note in notes)
			{
				var typeAttr = note.Attribute("type");
				var key = typeAttr == null ? "@@@@@" + fakeNoteKey : typeAttr.Value;
				sortedNotes.Add(key, note);
				if (typeAttr == null)
					fakeNoteKey++;
				note.Remove();
			}
			/*
	  <zeroOrMore>
		<element name="annotation">
		  <ref name="annotation-content"/>
		</element>
	  </zeroOrMore>
			 */
			var sortedAnnotations = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var annotations = sense.Elements("annotation").ToList();
			foreach (var annotation in annotations)
			{
				sortedAnnotations.Add(annotation.Attribute("name").Value + annotation.Attribute("value").Value, annotation);
				annotation.Remove();
			}
			/*
				  <zeroOrMore>
					<element name="field">
					  <ref name="field-content"/>
					</element>
				  </zeroOrMore>
			 */
			var sortedFields = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var fields = sense.Elements("field").ToList();
			foreach (var field in fields)
			{
				sortedFields.Add(field.Attribute("type").Value, field);
				field.Remove();
			}
			/*
	  <zeroOrMore>
		<element name="trait">
		  <ref name="trait-content"/>
		</element>
	  </zeroOrMore>
			*/
			var sortedTraits = new SortedDictionary<string, XElement>(StringComparer.InvariantCultureIgnoreCase);
			var traits = sense.Elements("trait").ToList();
			foreach (var trait in traits)
			{
				sortedTraits.Add(trait.Attribute("name").Value + trait.Attribute("value").Value, trait);
				trait.Remove();
			}

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
			foreach (var reversal in reversals)
				sense.Add(reversal);
			foreach (var subsense in subsenses)
				sense.Add(subsense);
			foreach (var relation in sortedRelations.Values)
				sense.Add(relation);
			foreach (var illustration in sortedIllustrations.Values)
				sense.Add(illustration);
			foreach (var note in sortedNotes.Values)
				sense.Add(note);
			foreach (var annotation in sortedAnnotations.Values)
				sense.Add(annotation);
			foreach (var field in sortedFields.Values)
				sense.Add(field);
			foreach (var trait in sortedTraits.Values)
				sense.Add(trait);
			foreach (var leftover in leftovers)
				sense.Add(leftover);
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
				sortedAttributes.Add(attr.Name.LocalName, attr);

			element.Attributes().Remove();
			foreach (var sortedAttrKvp in sortedAttributes)
				element.Add(sortedAttrKvp.Value);
		}
	}
}
