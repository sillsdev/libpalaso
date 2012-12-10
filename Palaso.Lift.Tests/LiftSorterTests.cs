using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;

namespace Palaso.Lift.Tests
{
	[TestFixture]
	public class LiftSorterTests
	{
		[Test]
		public void NullPathnameThrows()
		{
			Assert.Throws<ArgumentNullException>(() =>  LiftSorter.SortLiftFile(null));
		}

		[Test]
		public void EmptyPathnameThrows()
		{
			Assert.Throws<ArgumentNullException>(() => LiftSorter.SortLiftFile(""));
		}

		[Test]
		public void NonexistantFileThrows()
		{
			Assert.Throws<FileNotFoundException>(() => LiftSorter.SortLiftFile("bogus.lift"));
		}

		[Test]
		public void NotLiftFileThrows()
		{
			using (var tempFile = IO.TempFile.WithFilename("bogus.txt"))
			{
				Assert.Throws<InvalidOperationException>(() => LiftSorter.SortLiftFile(tempFile.Path));
			}
		}

		[Test]
		public void EntriesAreSortedInGuidOrder()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='2' />
<entry guid='1' />
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var entries = doc.Root.Elements("entry").ToList();
				Assert.IsTrue(entries.Count == 2);
				Assert.IsTrue(entries[0].Attribute("guid").Value == "1");
				Assert.IsTrue(entries[1].Attribute("guid").Value == "2");
			}
		}

		[Test]
		public void EntryContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<leftover />
<etymology source='a' type='b' />
<annotation name='a' value='b' />
<variant o='1' />
<relation ref='a' type='b' />
<note o='1' />
<trait name='a' value='b' />
<field type='b' />
<sense id='1' />
<annotation name='a' value='a' />
<field type='a' />
<pronunciation o='1' />
<note o='2' />
<variant o='2' />
<citation />
<trait name='a' value='a' />
<pronunciation o='2' />
<lexical-unit />
<etymology source='a' type='a' />
<sense id='2' />
<relation ref='a' type='a' />
</entry>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var entryChildren = doc.Root.Element("entry").Elements().ToList();
				Assert.IsTrue(entryChildren.Count == 21);

				var currentChild = entryChildren[0];
				Assert.AreEqual("lexical-unit", currentChild.Name.LocalName);

				currentChild = entryChildren[1];
				Assert.AreEqual("citation", currentChild.Name.LocalName);

				currentChild = entryChildren[2];
				Assert.AreEqual("pronunciation", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = entryChildren[3];
				Assert.AreEqual("pronunciation", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = entryChildren[4];
				Assert.AreEqual("variant", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = entryChildren[5];
				Assert.AreEqual("variant", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = entryChildren[6];
				Assert.AreEqual("sense", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("id").Value);
				currentChild = entryChildren[7];
				Assert.AreEqual("sense", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("id").Value);

				currentChild = entryChildren[8];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = entryChildren[9];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = entryChildren[10];
				Assert.AreEqual("etymology", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = entryChildren[11];
				Assert.AreEqual("etymology", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = entryChildren[12];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = entryChildren[13];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = entryChildren[14];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = entryChildren[15];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = entryChildren[16];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = entryChildren[17];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = entryChildren[18];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = entryChildren[19];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = entryChildren[20];
				Assert.AreEqual("leftover", currentChild.Name.LocalName);
			}
		}

		[Test]
		public void SenseContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
<trait name='a' value='b' />
<illustration href='2' />
<field type='b' />
<subsense id='1' />
<leftover o='1' />
<relation ref='a' type='b' />
<annotation name='a' value='b' />
<reversal o='1' />
<example o='1' />
<subsense id='2' />
<annotation name='a' value='a' />
<example o='2' />
<grammatical-info />
<trait name='a' value='a' />
<field type='a' />
<note o='1' />
<gloss lang='b' />
<illustration href='1' />
<note type='2' />
<reversal o='2' />
<leftover o='2' />
<gloss lang='a' />
<note type='1' />
<relation ref='a' type='a' />
<definition />
<note o='2' />
</sense>
</entry>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var senseChildren = doc.Root.Element("entry").Element("sense").Elements().ToList();
				Assert.IsTrue(senseChildren.Count == 26);

				var currentChild = senseChildren[0];
				Assert.AreEqual("definition", currentChild.Name.LocalName);

				currentChild = senseChildren[1];
				Assert.AreEqual("gloss", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = senseChildren[2];
				Assert.AreEqual("gloss", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = senseChildren[3];
				Assert.AreEqual("grammatical-info", currentChild.Name.LocalName);

				currentChild = senseChildren[4];
				Assert.AreEqual("example", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[5];
				Assert.AreEqual("example", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = senseChildren[6];
				Assert.AreEqual("reversal", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[7];
				Assert.AreEqual("reversal", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = senseChildren[8];
				Assert.AreEqual("subsense", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("id").Value);
				currentChild = senseChildren[9];
				Assert.AreEqual("subsense", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("id").Value);

				currentChild = senseChildren[10];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = senseChildren[11];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = senseChildren[12];
				Assert.AreEqual("illustration", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("href").Value);
				currentChild = senseChildren[13];
				Assert.AreEqual("illustration", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("href").Value);

				currentChild = senseChildren[14];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[15];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);
				currentChild = senseChildren[16];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("type").Value);
				currentChild = senseChildren[17];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("type").Value);

				currentChild = senseChildren[18];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = senseChildren[19];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = senseChildren[20];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = senseChildren[21];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = senseChildren[22];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = senseChildren[23];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = senseChildren[24];
				Assert.AreEqual("leftover", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[25];
				Assert.AreEqual("leftover", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);
			}
		}

		[Test]
		public void SubsenseContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
<subsense id='1' >
<leftover o='1' />
<definition />
</subsense>
</sense>
</entry>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var subsenseChildren = doc.Root.Element("entry").Element("sense").Element("subsense").Elements().ToList();
				Assert.IsTrue(subsenseChildren.Count == 2);

				var currentChild = subsenseChildren[0];
				Assert.AreEqual("definition", currentChild.Name.LocalName);

				currentChild = subsenseChildren[1];
				Assert.AreEqual("leftover", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
			}
		}

		[Test]
		public void HeaderIsSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<header>
<fields />
<ranges />
<description />
</header>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var header = doc.Root.Elements("header");
				var childElements = header.Elements().ToList();
				Assert.IsTrue(childElements.Count == 3);
				Assert.IsTrue(childElements[0].Name.LocalName == "description");
				Assert.IsTrue(childElements[1].Name.LocalName == "ranges");
				Assert.IsTrue(childElements[2].Name.LocalName == "fields");
			}
		}

		[Test]
		public void HeaderRangesAreSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<header>
<ranges>
<range id='2' />
<range id='1' />
</ranges>
</header>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var rangeElements = doc.Root.Elements("header").Elements("ranges").Elements("range").ToList();
				Assert.IsTrue(rangeElements.Count == 2);
				Assert.IsTrue(rangeElements[0].Attribute("id").Value == "1");
				Assert.IsTrue(rangeElements[1].Attribute("id").Value == "2");
			}
		}

		[Test]
		public void HeaderFieldsAreSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<header>
<fields>
<field tag='2' />
<field tag='1' />
</fields>
</header>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var rangeElements = doc.Root.Elements("header").Elements("fields").Elements("field").ToList();
				Assert.IsTrue(rangeElements.Count == 2);
				Assert.IsTrue(rangeElements[0].Attribute("tag").Value == "1");
				Assert.IsTrue(rangeElements[1].Attribute("tag").Value == "2");
			}
		}

		[Test]
		public void AttributesAreSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift version='0.13' producer='SIL.FLEx 3.0.0.40042'>
<entry guid='1' id='x_1' dateModified='2010-11-17T08:14:31Z' dateCreated='2008-08-09T05:17:10Z' >
<trait value='stem' name='morph-type' />
</entry>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var attributes = doc.Root.Attributes().ToList();
				Assert.IsTrue(attributes.Count == 2);
				Assert.IsTrue(attributes[0].Name == "producer");
				Assert.IsTrue(attributes[0].Value == "SIL.FLEx 3.0.0.40042");
				Assert.IsTrue(attributes[1].Name == "version");
				Assert.IsTrue(attributes[1].Value == "0.13");

				var element = doc.Root.Element("entry");
				attributes = element.Attributes().ToList();
				Assert.IsTrue(attributes.Count == 4);
				Assert.IsTrue(attributes[0].Name == "dateCreated");
				Assert.IsTrue(attributes[0].Value == "2008-08-09T05:17:10Z");
				Assert.IsTrue(attributes[1].Name == "dateModified");
				Assert.IsTrue(attributes[1].Value == "2010-11-17T08:14:31Z");
				Assert.IsTrue(attributes[2].Name == "guid");
				Assert.IsTrue(attributes[2].Value == "1");
				Assert.IsTrue(attributes[3].Name == "id");
				Assert.IsTrue(attributes[3].Value == "x_1");

				element = element.Element("trait");
				attributes = element.Attributes().ToList();
				Assert.IsTrue(attributes.Count == 2);
				Assert.IsTrue(attributes[0].Name == "name");
				Assert.IsTrue(attributes[0].Value == "morph-type");
				Assert.IsTrue(attributes[1].Name == "value");
				Assert.IsTrue(attributes[1].Value == "stem");
			}
		}

		[Test]
		public void NullPathnameForLiftRangesThrows()
		{
			Assert.Throws<ArgumentNullException>(() => LiftSorter.SortLiftRangesFile(null));
		}

		[Test]
		public void EmptyPathnameForLiftRangesThrows()
		{
			Assert.Throws<ArgumentNullException>(() => LiftSorter.SortLiftRangesFile(""));
		}

		[Test]
		public void NonexistantFileForLiftRangesThrows()
		{
			Assert.Throws<FileNotFoundException>(() => LiftSorter.SortLiftRangesFile("bogus.lift-ranges"));
		}

		[Test]
		public void NotForLiftRangesFileThrows()
		{
			using (var tempFile = IO.TempFile.WithFilename("bogus.txt"))
			{
				Assert.Throws<InvalidOperationException>(() => LiftSorter.SortLiftRangesFile(tempFile.Path));
			}
		}

		[Test]
		public void RangesAreSortedInIdOrder()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='2' />
<range id='1' />
</lift-ranges>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift-ranges"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftRangesFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var ranges = doc.Root.Elements("range").ToList();
				Assert.IsTrue(ranges.Count == 2);
				Assert.IsTrue(ranges[0].Attribute("id").Value == "1");
				Assert.IsTrue(ranges[1].Attribute("id").Value == "2");
			}
		}

		[Test]
		public void RangeElementsAreSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='1' >
<range-element id='2' />
<range-element id='1' />
<abbrev />
<label />
<description />
</range>
</lift-ranges>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift-ranges"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftRangesFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var range = doc.Root.Element("range");
				var children = range.Elements().ToList();
				Assert.IsTrue(children[0].Name.LocalName == "description");
				Assert.IsTrue(children[1].Name.LocalName == "label");
				Assert.IsTrue(children[2].Name.LocalName == "abbrev");
				Assert.IsTrue(children[3].Attribute("id").Value == "1");
				Assert.IsTrue(children[4].Attribute("id").Value == "2");
			}
		}

		[Test]
		public void Range_ElementsAreSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='1' >
<range-element id='1' >
<abbrev />
<label />
<description />
</range-element>
</range>
</lift-ranges>";
			/*
				<interleave>
				  <optional>
					<element name="description">
					  <ref name="multitext-content"/>
					</element>
				  </optional>
				  <optional>
					<element name="label">
					  <ref name="multitext-content"/>
					</element>
				  </optional>
				  <optional>
					<element name="abbrev">
					  <ref name="multitext-content"/>
					</element>
				  </optional>
				</interleave>
			*/
			using (var liftFile = IO.TempFile.WithFilename("good.lift-ranges"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftRangesFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var range_element = doc.Root.Element("range").Element("range-element");
				var children = range_element.Elements().ToList();
				Assert.IsTrue(children[0].Name.LocalName == "description");
				Assert.IsTrue(children[1].Name.LocalName == "label");
				Assert.IsTrue(children[2].Name.LocalName == "abbrev");
			}
		}
	}
}