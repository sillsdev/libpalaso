using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.Extensions;

namespace SIL.Lift.Tests
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
			using (var tempFolder = new TempFolder("TempLiftProject" + Guid.NewGuid()))
			{
				Assert.Throws<FileNotFoundException>(() => LiftSorter.SortLiftFile(Path.Combine(tempFolder.Path, "bogus.lift")));
			}
		}

		[Test]
		public void NotLiftFileThrows()
		{
			using (var tempFile = SIL.IO.TempFile.WithFilename("bogus.txt"))
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
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);

				LiftSorter.SortLiftFile(liftFile.Path); // Called once, but it will sort both lift files in that one call.

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
		<lexical-unit>
			<form lang='b' >
				<text />
			</form>
			<form lang='a' >
				<text />
			</form>
		</lexical-unit>
		<etymology source='a' type='a' />
		<sense id='2' />
		<relation ref='a' type='a' />
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var entryChildren = doc.Root.Element("entry").Elements().ToList();
				Assert.IsTrue(entryChildren.Count == 21);

				var currentIdx = -1;
				var currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("lexical-unit", currentChild.Name.LocalName);
				var lexUnitChildren = currentChild.Elements("form").ToList();
				Assert.IsTrue(lexUnitChildren.Count == 2);
				currentChild = lexUnitChildren[0];
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = lexUnitChildren[1];
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("citation", currentChild.Name.LocalName);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("pronunciation", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("pronunciation", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("variant", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("variant", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("sense", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("id").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("sense", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("id").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("etymology", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("etymology", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = entryChildren[++currentIdx];
				Assert.AreEqual("leftover", currentChild.Name.LocalName);
			}
		}

		[Test]
		public void PronunciationContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
		<pronunciation>
			<trait name='a' value='a' />
			<field type='a' />
			<annotation name='a' value='a' />
			<form lang='b'>
				<text />
			</form>
			<media href='b' />
			<form lang='a'>
				<text />
			</form>
			<media href='a' >
				<label>
			<form lang='lb'>
				<text />
			</form>
			<form lang='la'>
				<text />
			</form>
				</label>
			</media>
		</pronunciation>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var pronunciation = doc.Root.Element("entry").Element("pronunciation");
				var pronunciationChildren = pronunciation.Elements().ToList();
				Assert.IsTrue(pronunciationChildren.Count == 7);

				var currentIdx = -1;
				var currentChild = pronunciationChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = pronunciationChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = pronunciationChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = pronunciationChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				currentChild = pronunciationChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);

				currentChild = pronunciationChildren[++currentIdx];
				Assert.AreEqual("a", currentChild.Attribute("href").Value);
				var currentChildChildren = currentChild.Elements().ToList();
				Assert.IsTrue(currentChildChildren.Count == 1);
				Assert.AreEqual("label", currentChildChildren[0].Name.LocalName);
				Assert.IsTrue(currentChildChildren[0].Elements().Count() == 2);
				Assert.AreEqual("la", currentChildChildren[0].Elements().ToList()[0].Attribute("lang").Value);
				Assert.AreEqual("lb", currentChildChildren[0].Elements().ToList()[1].Attribute("lang").Value);
			}
		}

		[Test]
		public void VariantContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
		<variant ref='1'>
			<trait name='a' value='a' />
			<field type='a' />
			<annotation name='a' value='a' />
			<form lang='b'>
				<text />
			</form>
			<relation ref='a' type='b' />
			<pronunciation o='1' />
			<form lang='a'>
				<text />
			</form>
			<relation ref='a' type='a' >
				<trait name='a' value='aa' />
				<field type='aa' />
				<usage>
					<form lang='b' >
						<text />
					</form>
					<form lang='a' >
						<text />
					</form>
				</usage>
				<annotation name='a' value='aa' />
			</relation>
			<pronunciation o='2' />
		</variant>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var variant = doc.Root.Element("entry").Element("variant");
				var variantChildren = variant.Elements().ToList();
				Assert.IsTrue(variantChildren.Count == 9);

				var currentIdx = -1;
				var currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);

				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("pronunciation", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("pronunciation", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);

				var currentChildChildren = currentChild.Elements().ToList();
				Assert.IsTrue(currentChildChildren.Count == 4);
				var currentGrandChild = currentChildChildren[0];
				Assert.AreEqual("annotation", currentGrandChild.Name.LocalName);
				currentGrandChild = currentChildChildren[1];
				Assert.AreEqual("trait", currentGrandChild.Name.LocalName);
				currentGrandChild = currentChildChildren[2];
				Assert.AreEqual("field", currentGrandChild.Name.LocalName);
				currentGrandChild = currentChildChildren[3];
				Assert.AreEqual("usage", currentGrandChild.Name.LocalName);
				Assert.IsTrue(currentGrandChild.Elements().Count() == 2);
				Assert.AreEqual("a", currentGrandChild.Elements().ToList()[0].Attribute("lang").Value);
				Assert.AreEqual("b", currentGrandChild.Elements().ToList()[1].Attribute("lang").Value);

				currentChild = variantChildren[++currentIdx];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);
			}
		}

		[Test]
		public void EntryNoteContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
		<note type='1'>
			<trait name='a' value='a' />
			<field type='a' />
			<annotation name='a' value='a' />
			<form lang='b'>
				<text />
			</form>
			<form lang='a'>
				<text />
			</form>
		</note>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var note = doc.Root.Element("entry").Element("note");
				var noteChildren = note.Elements().ToList();
				Assert.IsTrue(noteChildren.Count == 5);

				var currentIdx = -1;
				var currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
			}
		}

		[Test]
		public void EntryRelationContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
			<relation ref='a' type='a' >
				<trait name='a' value='aa' />
				<field type='aa' />
				<usage>
					<form lang='b' >
						<text />
					</form>
					<form lang='a' >
						<text />
					</form>
				</usage>
				<annotation name='a' value='aa' />
			</relation>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var relation = doc.Root.Element("entry").Element("relation");
				var relationChildren = relation.Elements().ToList();
				Assert.IsTrue(relationChildren.Count == 4);

				var currentIdx = -1;
				var currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);

				currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("usage", currentChild.Name.LocalName);
				var grandchildren = currentChild.Elements().ToList();
				Assert.IsTrue(grandchildren.Count == 2);
				Assert.AreEqual("a", grandchildren[0].Attribute("lang").Value);
				Assert.AreEqual("b", grandchildren[1].Attribute("lang").Value);
			}
		}

		[Test]
		public void EtymologyContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
			<etymology source='a' type='a' >
				<trait name='a' value='aa' />
				<gloss lang='b'>
					<text />
				</gloss>
				<field type='aa' />
				<form lang='b' >
					<text />
				</form>
				<form lang='a' >
					<text />
				</form>
				<gloss lang='a'>
					<text />
				</gloss>
				<annotation name='a' value='aa' />
			</etymology>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var etymology = doc.Root.Element("entry").Element("etymology");
				var etymologyChildren = etymology.Elements().ToList();
				Assert.IsTrue(etymologyChildren.Count == 7);

				var currentIdx = -1;
				var currentChild = etymologyChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = etymologyChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = etymologyChildren[++currentIdx];
				Assert.AreEqual("gloss", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = etymologyChildren[++currentIdx];
				Assert.AreEqual("gloss", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = etymologyChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = etymologyChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				currentChild = etymologyChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
			}
		}

		[Test]
		public void EntryAnnotationContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
			<annotation name='a' >
				<form lang='b' >
					<text />
				</form>
				<form lang='a' >
					<text />
				</form>
			</annotation>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var annotation = doc.Root.Element("entry").Element("annotation");
				var annotationChildren = annotation.Elements().ToList();
				Assert.IsTrue(annotationChildren.Count == 2);

				var currentIdx = -1;
				var currentChild = annotationChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = annotationChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);
			}
		}

		[Test]
		public void EntryTraitContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
			<trait name='a' value='a' >
				<annotation name='a' value='b' />
				<annotation name='a' value='a' />
			</trait>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var trait = doc.Root.Element("entry").Element("trait");
				var traitChildren = trait.Elements().ToList();
				Assert.IsTrue(traitChildren.Count == 2);

				var currentIdx = -1;
				var currentChild = traitChildren[++currentIdx];
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = traitChildren[++currentIdx];
				Assert.AreEqual("b", currentChild.Attribute("value").Value);
			}
		}

		[Test]
		public void EntryFieldContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
			<field type='a' >
				<annotation name='a' value='a' />
				<form lang='b' >
					<text />
				</form>
				<trait name='a' value='a' />
				<form lang='a' >
					<text />
				</form>
			</field>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var field = doc.Root.Element("entry").Element("field");
				var fieldChildren = field.Elements().ToList();
				Assert.IsTrue(fieldChildren.Count == 4);

				var currentIdx = -1;
				var currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);
				currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
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
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var senseChildren = doc.Root.Element("entry").Element("sense").Elements().ToList();
				Assert.IsTrue(senseChildren.Count == 26);

				var currentIdx = -1;
				var currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("definition", currentChild.Name.LocalName);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("gloss", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("gloss", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("grammatical-info", currentChild.Name.LocalName);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("example", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("example", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("reversal", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("reversal", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("subsense", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("id").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("subsense", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("id").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("relation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("illustration", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("href").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("illustration", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("href").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("type").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("type").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);

				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("leftover", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("o").Value);
				currentChild = senseChildren[++currentIdx];
				Assert.AreEqual("leftover", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("o").Value);
			}
		}

		[Test]
		public void DefinitionContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
<sense id='1' >
	<definition>
		<form lang='b' >
			<text />
		</form>
		<form lang='a' >
			<text />
		</form>
	</definition>
</sense>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var definition = doc.Root.Element("entry").Element("sense").Element("definition");
				var definitionChildren = definition.Elements().ToList();
				Assert.IsTrue(definitionChildren.Count == 2);

				var currentIdx = -1;
				var currentChild = definitionChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = definitionChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);
			}
		}

		[Test]
		public void GlossContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
	<entry guid='1' >
<sense id='1' >
	<gloss lang='a'>
		<form lang='a' >
			<annotation name='a' value='b' />
			<text />
			<annotation name='a' value='a' />
		</form>
	</gloss>
</sense>
	</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var gloss = doc.Root.Element("entry").Element("sense").Element("gloss");
				var glossChildren = gloss.Elements().ToList();
				Assert.IsTrue(glossChildren.Count == 1);

				var currentIdx = -1;
				var currentChild = glossChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);

				var grandChildren = currentChild.Elements().ToList();
				Assert.IsTrue(grandChildren.Count == 3);
				var grandChild = grandChildren[0];
				Assert.AreEqual("text", grandChild.Name.LocalName);
				grandChild = grandChildren[1];
				Assert.AreEqual("annotation", grandChild.Name.LocalName);
				Assert.AreEqual("a", grandChild.Attribute("value").Value);
				grandChild = grandChildren[2];
				Assert.AreEqual("annotation", grandChild.Name.LocalName);
				Assert.AreEqual("b", grandChild.Attribute("value").Value);
			}
		}

		[Test]
		public void GrammaticalInfoContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<grammatical-info>
		<trait name='a' value='b' />
		<trait name='a' value='a' />
	</grammatical-info>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var grammaticalInfo = doc.Root.Element("entry").Element("sense").Element("grammatical-info");
				var grammaticalInfoChildren = grammaticalInfo.Elements().ToList();
				Assert.IsTrue(grammaticalInfoChildren.Count == 2);

				var currentIdx = -1;
				var currentChild = grammaticalInfoChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = grammaticalInfoChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("value").Value);
			}
		}

		[Test]
		public void ExampleContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<example>
		<note type='2' />
		<field type='a' />
		<form lang='b' >
			<text />
		</form>
		<note type='1' />
		<trait name='a' value='a' />
		<translation type='b' />
		<form lang='a' >
			<text />
		</form>
		<annotation name='a' value='a' />
		<translation type='a' >
			<form lang='tb' >
				<text />
			</form>
			<form lang='ta' >
				<text />
			</form>
		</translation>
	</example>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var example = doc.Root.Element("entry").Element("sense").Element("example");
				var exampleChildren = example.Elements().ToList();
				Assert.IsTrue(exampleChildren.Count == 9);

				var currentIdx = -1;
				var currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("translation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
				var translationChildren = currentChild.Elements().ToList();
				Assert.IsTrue(translationChildren.Count == 2);
				var currentTransChild = translationChildren[0];
				Assert.AreEqual("form", currentTransChild.Name.LocalName);
				Assert.AreEqual("ta", currentTransChild.Attribute("lang").Value);
				currentTransChild = translationChildren[1];
				Assert.AreEqual("form", currentTransChild.Name.LocalName);
				Assert.AreEqual("tb", currentTransChild.Attribute("lang").Value);

				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("translation", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("type").Value);
				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("1", currentChild.Attribute("type").Value);
				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("note", currentChild.Name.LocalName);
				Assert.AreEqual("2", currentChild.Attribute("type").Value);
				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = exampleChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("type").Value);
			}
		}

		[Test]
		public void ReversalContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<reversal type='a'>
		<form lang='b' >
			<text />
		</form>
		<grammatical-info value='a' />
		<main>
			<grammatical-info value='ma' />
			<form lang='mb' >
				<text />
			</form>
			<main>
				<grammatical-info  value='innermaina' />
			</main>
			<form lang='ma' >
				<text />
			</form>
		</main>
		<form lang='a' >
			<text />
		</form>
	</reversal>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var reversal = doc.Root.Element("entry").Element("sense").Element("reversal");
				var reversalChildren = reversal.Elements().ToList();
				Assert.IsTrue(reversalChildren.Count == 4);

				var currentIdx = -1;
				var currentChild = reversalChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = reversalChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);
				currentChild = reversalChildren[++currentIdx];
				Assert.AreEqual("grammatical-info", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = reversalChildren[++currentIdx];
				Assert.AreEqual("main", currentChild.Name.LocalName);
				var grandchildren = currentChild.Elements().ToList();
				Assert.IsTrue(grandchildren.Count == 4);
				var currentGrandchild = grandchildren[0];
				Assert.AreEqual("form", currentGrandchild.Name.LocalName);
				Assert.AreEqual("ma", currentGrandchild.Attribute("lang").Value);
				currentGrandchild = grandchildren[1];
				Assert.AreEqual("form", currentGrandchild.Name.LocalName);
				Assert.AreEqual("mb", currentGrandchild.Attribute("lang").Value);
				currentGrandchild = grandchildren[2];
				Assert.AreEqual("grammatical-info", currentGrandchild.Name.LocalName);
				Assert.AreEqual("ma", currentGrandchild.Attribute("value").Value);
				currentGrandchild = grandchildren[3];
				Assert.AreEqual("main", currentGrandchild.Name.LocalName);
				var greatGrandChildren = currentGrandchild.Elements().ToList();
				Assert.IsTrue(greatGrandChildren.Count == 1);
				Assert.AreEqual("grammatical-info", greatGrandChildren[0].Name.LocalName);
				Assert.AreEqual("innermaina", greatGrandChildren[0].Attribute("value").Value);
			}
		}

		[Test]
		public void SenseRelationContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<relation ref='a' type='a' >
		<trait name='a' value='aa' />
		<field type='aa' />
		<usage>
			<form lang='b' >
				<text />
			</form>
			<form lang='a' >
				<text />
			</form>
		</usage>
		<annotation name='a' value='aa' />
	</relation>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var relation = doc.Root.Element("entry").Element("sense").Element("relation");
				var relationChildren = relation.Elements().ToList();
				Assert.IsTrue(relationChildren.Count == 4);

				var currentIdx = -1;
				var currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);

				currentChild = relationChildren[++currentIdx];
				Assert.AreEqual("usage", currentChild.Name.LocalName);
				var grandchildren = currentChild.Elements().ToList();
				Assert.IsTrue(grandchildren.Count == 2);
				Assert.AreEqual("a", grandchildren[0].Attribute("lang").Value);
				Assert.AreEqual("b", grandchildren[1].Attribute("lang").Value);
			}
		}

		[Test]
		public void IllustrationContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<illustration href='1' >
		<label>
			<form lang='b' >
				<text />
			</form>
			<form lang='a' >
				<text />
			</form>
		</label>
	</illustration>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var illustration = doc.Root.Element("entry").Element("sense").Element("illustration");
				var illustrationChildren = illustration.Elements().ToList();
				Assert.IsTrue(illustrationChildren.Count == 1);

				var labelChild = illustrationChildren[0];
				Assert.AreEqual("label", labelChild.Name.LocalName);
				var labelChildren = labelChild.Elements().ToList();
				Assert.IsTrue(labelChildren.Count == 2);

				var currentIdx = -1;
				var currentChild = labelChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = labelChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);
			}
		}

		[Test]
		public void SenseNoteContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<note type='1'>
		<trait name='a' value='a' />
		<field type='a' />
		<annotation name='a' value='a' />
		<form lang='b'>
			<text />
		</form>
		<form lang='a'>
			<text />
		</form>
	</note>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var note = doc.Root.Element("entry").Element("sense").Element("note");
				var noteChildren = note.Elements().ToList();
				Assert.IsTrue(noteChildren.Count == 5);

				var currentIdx = -1;
				var currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);

				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
				currentChild = noteChildren[++currentIdx];
				Assert.AreEqual("field", currentChild.Name.LocalName);
			}
		}

		[Test]
		public void SenseAnnotationContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<annotation name='a' >
		<form lang='b' >
			<text />
		</form>
		<form lang='a' >
			<text />
		</form>
	</annotation>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var annotation = doc.Root.Element("entry").Element("sense").Element("annotation");
				var annotationChildren = annotation.Elements().ToList();
				Assert.IsTrue(annotationChildren.Count == 2);

				var currentIdx = -1;
				var currentChild = annotationChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = annotationChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);
			}
		}

		[Test]
		public void SenseTraitContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<trait name='a' value='a' >
		<annotation name='a' value='b' />
		<annotation name='a' value='a' />
	</trait>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var trait = doc.Root.Element("entry").Element("sense").Element("trait");
				var traitChildren = trait.Elements().ToList();
				Assert.IsTrue(traitChildren.Count == 2);

				var currentIdx = -1;
				var currentChild = traitChildren[++currentIdx];
				Assert.AreEqual("a", currentChild.Attribute("value").Value);
				currentChild = traitChildren[++currentIdx];
				Assert.AreEqual("b", currentChild.Attribute("value").Value);
			}
		}

		[Test]
		public void SenseFieldContentsAreOrdered()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<entry guid='1' >
<sense id='1' >
	<field type='a' >
		<annotation name='a' value='a' />
		<form lang='b' >
			<text />
		</form>
		<trait name='a' value='a' />
		<form lang='a' >
			<text />
		</form>
	</field>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var field = doc.Root.Element("entry").Element("sense").Element("field");
				var fieldChildren = field.Elements().ToList();
				Assert.IsTrue(fieldChildren.Count == 4);

				var currentIdx = -1;
				var currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("a", currentChild.Attribute("lang").Value);
				currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("b", currentChild.Attribute("lang").Value);
				currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("annotation", currentChild.Name.LocalName);
				currentChild = fieldChildren[++currentIdx];
				Assert.AreEqual("trait", currentChild.Name.LocalName);
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
<subsense id='2' >
	<leftover o='ss1' />
	<definition />
</subsense>
</sense>
</entry>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
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
				Assert.AreEqual("ss1", currentChild.Attribute("o").Value);
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
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
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
<range id='1' >
	<label>
		<form lang='1lb'>
			<text />
		</form>
		<form lang='1la'>
			<text />
		</form>
	</label>
	<abbrev>
		<form lang='1ab'>
			<text />
		</form>
		<form lang='1aa'>
			<text />
		</form>
	</abbrev>
	<description>
		<form lang='1db'>
			<text />
		</form>
		<form lang='1da'>
			<text />
		</form>
	</description>
	<range-element id='1b' />
	<range-element id='1a' >
		<label>
			<form lang='1alb'>
				<text />
			</form>
			<form lang='1ala'>
				<text />
			</form>
		</label>
		<abbrev>
			<form lang='1aab'>
				<text />
			</form>
			<form lang='1aaa'>
				<text />
			</form>
		</abbrev>
		<description>
			<form lang='1adb'>
				<text />
			</form>
			<form lang='1ada'>
				<text />
			</form>
		</description>
	</range-element>
</range>
</ranges>
</header>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var rangeElements = doc.Root.Elements("header").Elements("ranges").Elements("range").ToList();
				Assert.IsTrue(rangeElements.Count == 2);

				var currentRange = rangeElements[0];
				Assert.IsTrue(currentRange.Attribute("id").Value == "1");
				var rangeElementChildren = currentRange.Elements().ToList();
				Assert.IsTrue(rangeElementChildren.Count == 5);
				var currentRangeChild = rangeElementChildren[0];
				Assert.IsTrue(currentRangeChild.Name.LocalName == "description");
				var currentFormChidren = currentRangeChild.Elements().ToList();
				Assert.IsTrue(currentFormChidren.Count == 2);
				Assert.IsTrue(currentFormChidren[0].Attribute("lang").Value == "1da");
				Assert.IsTrue(currentFormChidren[1].Attribute("lang").Value == "1db");
				currentRangeChild = rangeElementChildren[1];
				Assert.IsTrue(currentRangeChild.Name.LocalName == "label");
				currentFormChidren = currentRangeChild.Elements().ToList();
				Assert.IsTrue(currentFormChidren.Count == 2);
				Assert.IsTrue(currentFormChidren[0].Attribute("lang").Value == "1la");
				Assert.IsTrue(currentFormChidren[1].Attribute("lang").Value == "1lb");
				currentRangeChild = rangeElementChildren[2];
				Assert.IsTrue(currentRangeChild.Name.LocalName == "abbrev");
				currentFormChidren = currentRangeChild.Elements().ToList();
				Assert.IsTrue(currentFormChidren.Count == 2);
				Assert.IsTrue(currentFormChidren[0].Attribute("lang").Value == "1aa");
				Assert.IsTrue(currentFormChidren[1].Attribute("lang").Value == "1ab");
				currentRangeChild = rangeElementChildren[3];
				Assert.IsTrue(currentRangeChild.Attribute("id").Value == "1a");
				var currentRangeChildren = currentRangeChild.Elements().ToList();
				Assert.IsTrue(currentRangeChildren.Count == 3);
				currentRangeChild = currentRangeChildren[0];
				Assert.IsTrue(currentRangeChild.Name.LocalName == "description");
				currentFormChidren = currentRangeChild.Elements().ToList();
				Assert.IsTrue(currentFormChidren.Count == 2);
				Assert.IsTrue(currentFormChidren[0].Attribute("lang").Value == "1ada");
				Assert.IsTrue(currentFormChidren[1].Attribute("lang").Value == "1adb");
				currentRangeChild = currentRangeChildren[1];
				Assert.IsTrue(currentRangeChild.Name.LocalName == "label");
				currentFormChidren = currentRangeChild.Elements().ToList();
				Assert.IsTrue(currentFormChidren.Count == 2);
				Assert.IsTrue(currentFormChidren[0].Attribute("lang").Value == "1ala");
				Assert.IsTrue(currentFormChidren[1].Attribute("lang").Value == "1alb");
				currentRangeChild = currentRangeChildren[2];
				Assert.IsTrue(currentRangeChild.Name.LocalName == "abbrev");
				currentFormChidren = currentRangeChild.Elements().ToList();
				Assert.IsTrue(currentFormChidren.Count == 2);
				Assert.IsTrue(currentFormChidren[0].Attribute("lang").Value == "1aaa");
				Assert.IsTrue(currentFormChidren[1].Attribute("lang").Value == "1aab");

				currentRangeChild = rangeElementChildren[4];
				Assert.IsTrue(currentRangeChild.Attribute("id").Value == "1b");
				currentRange = rangeElements[1];
				Assert.IsTrue(currentRange.Attribute("id").Value == "2");
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
		<field tag='1' >
			<form lang='1b'>
				<text />
			</form>
			<form lang='1a'>
				<text />
			</form>
		</field>
	</fields>
</header>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var fieldElements = doc.Root.Elements("header").Elements("fields").Elements("field").ToList();
				Assert.IsTrue(fieldElements.Count == 2);
				var currentField = fieldElements[0];
				Assert.IsTrue(currentField.Attribute("tag").Value == "1");
				var fieldElementChildren = currentField.Elements().ToList();
				Assert.IsTrue(fieldElementChildren.Count == 2);
				var currentFieldChild = fieldElementChildren[0];
				Assert.IsTrue(currentFieldChild.Attribute("lang").Value == "1a");
				currentFieldChild = fieldElementChildren[1];
				Assert.IsTrue(currentFieldChild.Attribute("lang").Value == "1b");
				currentField = fieldElements[1];
				Assert.IsTrue(currentField.Attribute("tag").Value == "2");
			}
		}

		[Test]
		public void HeaderDescriptionIsSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift producer='SIL.FLEx 3.0.0.40042' version='0.13'>
<header>
<description>
<form lang='b' >
	<text />
</form>
<form lang='a' >
	<text />
	<annotation name='a' value='b' />
	<annotation name='a' value='a' >
		<form lang='b' >
			<text />
		</form>
		<form lang='a' >
			<text />
		</form>
	</annotation>
</form>
</description>
</header>
</lift>";
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var descriptionFormElements = doc.Root.Element("header").Element("description").Elements("form").ToList();
				Assert.IsTrue(descriptionFormElements.Count == 2);
				Assert.IsTrue(descriptionFormElements[0].Attribute("lang").Value == "a");

				var formChildren = descriptionFormElements[0].Elements().ToList();
				Assert.IsTrue(formChildren.Count == 3);
				var formChild = formChildren[0];
				Assert.IsTrue(formChild.Name.LocalName == "text");
				formChild = formChildren[1];
				Assert.IsTrue(formChild.Name.LocalName == "annotation");
				Assert.IsTrue(formChild.Attribute("value").Value == "a");
				var annotationChildren = formChild.Elements("form").ToList();
				Assert.IsTrue(annotationChildren[0].Attribute("lang").Value == "a");
				Assert.IsTrue(annotationChildren[1].Attribute("lang").Value == "b");
				formChild = formChildren[2];
				Assert.IsTrue(formChild.Name.LocalName == "annotation");
				Assert.IsTrue(formChild.Attribute("value").Value == "b");

				Assert.IsTrue(descriptionFormElements[1].Attribute("lang").Value == "b");
			}
		}

		[Test]
		public void AttributesAreSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift xmlns:flex='http://fieldworks.sil.org' version='0.13' producer='SIL.FLEx 3.0.0.40042'>
<entry guid='1' id='x_1' dateModified='2010-11-17T08:14:31Z' dateCreated='2008-08-09T05:17:10Z' >
<trait value='stem' name='morph-type' />
</entry>
</lift>";
			using (var liftFile = IO.TempFile.WithFilename("good.lift"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftFile(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				Assert.That(doc.Root.Attributes().Select(s => s.ToStringMonoWorkaround()), Is.EqualTo(new[]
				{
					"producer=\"SIL.FLEx 3.0.0.40042\"",
					"version=\"0.13\"",
					"xmlns:flex=\"http://fieldworks.sil.org\""
				}));

				var element = doc.Root.Element("entry");
				Assert.That(element, Is.Not.Null);
				Assert.That(element.Attributes().Select(s => s.ToStringMonoWorkaround()), Is.EqualTo(new[]
				{
					"dateCreated=\"2008-08-09T05:17:10Z\"",
					"dateModified=\"2010-11-17T08:14:31Z\"",
					"guid=\"1\"",
					"id=\"x_1\""
				}));

				element = element.Element("trait");
				Assert.That(element, Is.Not.Null);
				Assert.That(element.Attributes().Select(s => s.ToStringMonoWorkaround()), Is.EqualTo(new[]
				{
					"name=\"morph-type\"",
					"value=\"stem\""
				}));
			}
		}

		[Test]
		public void NullPathnameForLiftRangesThrows()
		{
			Assert.Throws<ArgumentNullException>(() => LiftSorter.SortLiftRangesFiles(null));
		}

		[Test]
		public void EmptyPathnameForLiftRangesThrows()
		{
			Assert.Throws<ArgumentNullException>(() => LiftSorter.SortLiftRangesFiles(""));
		}

		[Test]
		public void NonexistantFileForLiftRangesDoesNotThrow()
		{
			using (var tempFolder = new TempFolder("TempLiftProject" + Guid.NewGuid()))
			{
				Assert.DoesNotThrow(() => LiftSorter.SortLiftRangesFiles(Path.Combine(tempFolder.Path, "bogus.lift-ranges")));
			}
		}

		[Test]
		public void NotForLiftRangesFileThrows()
		{
			using (var tempFile = SIL.IO.TempFile.WithFilename("bogus.txt"))
			{
				Assert.Throws<InvalidOperationException>(() => LiftSorter.SortLiftRangesFiles(tempFile.Path));
			}
		}

		[Test]
		public void RangesAreSortedInIdOrderInLiftRangesFile()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='2' />
<range id='1' />
</lift-ranges>";
			using (var liftRangesFile = SIL.IO.TempFile.WithFilename("good.lift-ranges"))
			using (var secondLiftRangesFile = SIL.IO.TempFile.WithFilename("good-dup.lift-ranges"))
			{
				File.WriteAllText(liftRangesFile.Path, liftData);
				File.WriteAllText(secondLiftRangesFile.Path, liftData);

				LiftSorter.SortLiftRangesFiles(liftRangesFile.Path); // Called once, but it will sort both lift ranges files in that one call.

				var doc = XDocument.Load(liftRangesFile.Path);
				Assert.AreEqual("lift-ranges", doc.Root.Name.LocalName);
				var ranges = doc.Root.Elements("range").ToList();
				Assert.IsTrue(ranges.Count == 2);
				Assert.IsTrue(ranges[0].Attribute("id").Value == "1");
				Assert.IsTrue(ranges[1].Attribute("id").Value == "2");

				doc = XDocument.Load(secondLiftRangesFile.Path);
				Assert.AreEqual("lift-ranges", doc.Root.Name.LocalName);
				ranges = doc.Root.Elements("range").ToList();
				Assert.IsTrue(ranges.Count == 2);
				Assert.IsTrue(ranges[0].Attribute("id").Value == "1");
				Assert.IsTrue(ranges[1].Attribute("id").Value == "2");
			}
		}

		[Test]
		public void RangeElementsInRangeAreSorted()
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
			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift-ranges"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftRangesFiles(liftFile.Path);
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
		public void RangeElementsContentsAreSorted()
		{
			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='1' >
<range-element id='1' >
<abbrev>
		<form lang='ab' >
			<text />
		</form>
		<form lang='aa' >
			<text />
		</form>
</abbrev>
<label>
		<form lang='lb' >
			<text />
		</form>
		<form lang='la' >
			<text />
		</form>
</label>
<description>
		<form lang='db' >
			<text />
		</form>
		<form lang='da' >
			<text />
		</form>
</description>
</range-element>
</range>
</lift-ranges>";

			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift-ranges"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftRangesFiles(liftFile.Path);
				var doc = XDocument.Load(liftFile.Path);
				var rangeElement = doc.Root.Element("range").Element("range-element");
				var children = rangeElement.Elements().ToList();
				Assert.IsTrue(children.Count == 3);

				var currentChild = children[0];
				Assert.IsTrue(currentChild.Name.LocalName == "description");
				var formChildren = currentChild.Elements().ToList();
				Assert.IsTrue(formChildren.Count == 2);
				var currentFormChild = formChildren[0];
				Assert.AreEqual("form", currentFormChild.Name.LocalName);
				Assert.AreEqual("da", currentFormChild.Attribute("lang").Value);
				currentFormChild = formChildren[1];
				Assert.AreEqual("form", currentFormChild.Name.LocalName);
				Assert.AreEqual("db", currentFormChild.Attribute("lang").Value);

				currentChild = children[1];
				Assert.IsTrue(currentChild.Name.LocalName == "label");
				formChildren = currentChild.Elements().ToList();
				Assert.IsTrue(formChildren.Count == 2);
				currentFormChild = formChildren[0];
				Assert.AreEqual("form", currentFormChild.Name.LocalName);
				Assert.AreEqual("la", currentFormChild.Attribute("lang").Value);
				currentFormChild = formChildren[1];
				Assert.AreEqual("form", currentFormChild.Name.LocalName);
				Assert.AreEqual("lb", currentFormChild.Attribute("lang").Value);

				currentChild = children[2];
				Assert.IsTrue(currentChild.Name.LocalName == "abbrev");
				formChildren = currentChild.Elements().ToList();
				Assert.IsTrue(formChildren.Count == 2);
				currentFormChild = formChildren[0];
				Assert.AreEqual("form", currentFormChild.Name.LocalName);
				Assert.AreEqual("aa", currentFormChild.Attribute("lang").Value);
				currentFormChild = formChildren[1];
				Assert.AreEqual("form", currentFormChild.Name.LocalName);
				Assert.AreEqual("ab", currentFormChild.Attribute("lang").Value);
			}
		}

		[Test]
		public void BadTextElementsAreFixedBeforeSorting()
		{
			// badly formatted entry: <element name='text'>string</element>
			// replaced with proper entry: <text>string</text>

			const string liftData =
@"<?xml version='1.0' encoding='utf-8'?>
<lift-ranges>
<range id='1' >
<range-element id='1' >
<label>
<form lang='en'>
<element name='text'>aa</element>
</form>
</label>
<description>
<form lang='fr'>
<element name='text'>ab</element>
</form>
</description>
</range-element>
</range>
</lift-ranges>";

			using (var liftFile = SIL.IO.TempFile.WithFilename("good.lift-ranges"))
			{
				File.WriteAllText(liftFile.Path, liftData);
				LiftSorter.SortLiftRangesFiles(liftFile.Path);
				var doc = XDocument.Load (liftFile.Path);
				var rangeElement = doc.Root.Element("range").Element ("range-element");
				var children = rangeElement.Elements().ToList();
				Assert.IsTrue(children.Count == 2);

				var currentChild = children[0];
				Assert.AreEqual("description", currentChild.Name.LocalName);
				var formChild = currentChild.Elements().ToList();
				Assert.IsTrue(formChild.Count == 1);
				currentChild = formChild[0];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("fr", currentChild.Attribute("lang").Value);
				var textChild = currentChild.Elements().ToList();
				Assert.IsTrue(textChild.Count == 1);
				currentChild = textChild[0];
				Assert.AreEqual("text", currentChild.Name.LocalName);
				Assert.AreEqual("ab", currentChild.Value);

				currentChild = children[1];
				Assert.AreEqual("label", currentChild.Name.LocalName);
				formChild = currentChild.Elements().ToList();
				Assert.IsTrue(formChild.Count == 1);
				currentChild = formChild[0];
				Assert.AreEqual("form", currentChild.Name.LocalName);
				Assert.AreEqual("en", currentChild.Attribute("lang").Value);
				textChild = currentChild.Elements().ToList();
				Assert.IsTrue(textChild.Count == 1);
				currentChild = textChild[0];
				Assert.AreEqual("text", currentChild.Name.LocalName);
				Assert.AreEqual("aa", currentChild.Value);
			}
		}

		[Test]
		public void GetUniqueKeyForDictionary()
		{
			var sortedDictionary = new SortedDictionary<string, string>
				{
					{"key", "value"}
				};
			var uniqueKey = LiftSorter.GetUniqueKey(sortedDictionary.Keys, "key");
			Assert.AreEqual("key1", uniqueKey);
			sortedDictionary.Add(uniqueKey, "value1");
			uniqueKey = LiftSorter.GetUniqueKey(sortedDictionary.Keys, "key");
			Assert.AreEqual("key2", uniqueKey);
		}

		[Test]
		public void GetUniqueKeyForSortedDictionary()
		{
			var dictionary = new Dictionary<string, string>
				{
					{"key", "value"}
				};
			var uniqueKey = LiftSorter.GetUniqueKey(dictionary.Keys, "key");
			Assert.AreEqual("key1", uniqueKey);
			dictionary.Add(uniqueKey, "value1");
			uniqueKey = LiftSorter.GetUniqueKey(dictionary.Keys, "key");
			Assert.AreEqual("key2", uniqueKey);
		}
	}
}