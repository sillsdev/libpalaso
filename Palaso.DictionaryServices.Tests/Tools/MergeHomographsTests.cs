using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Palaso.DictionaryServices.Tools;
using Palaso.Progress.LogBox;
using Palaso.TestUtilities;

namespace Palaso.DictionaryServices.Tests.Tools
{
	public class MergeHomographsTests
	{
		private TempFile _outputLift;
		private StringBuilderProgress _progress;
		private XmlDocument _resultDom;

		[SetUp]
		public void Setup()
		{
			_outputLift = new Palaso.TestUtilities.TempFile();
			_progress = new StringBuilderProgress();
			_resultDom = new XmlDocument();
		}

		[Test]
		public void Run_NoEntries_HeaderPreserved()
		{
			Run(@"<header>
					<ranges>
						<range href='file://C:/dev/temp/sena3/sena3.lift-ranges' id='dialect'></range>
						<range href='file://C:/dev/temp/sena3/sena3.lift-ranges' id='etymology'></range>
					</ranges>
				</header>");
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("lift/header/ranges/range", 2);

		}

		[Test, Ignore("Waiting for LIFT version which supports metada")]
		public void Run_NoEntries_MetadataInAnotherNameSpacePreserved()
		{
			Run(@"<header>
					<metadata>
						 <olac:olac xmlns:olac='http://www.language-archives.org/OLAC/1.1/'
							  xmlns:dc='http://purl.org/dc/elements/1.1/'
							  xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'
							  xsi:schemaLocation='http://www.language-archives.org/OLAC/1.1/
								 http://www.language-archives.org/OLAC/1.1/olac.xsd'
							  <dc:creatorBloomfield, Leonard</dc:creator>
							  <dc:date>1933</dc:date>
							  <dc:titleLanguage</dc:title>
							  <dc:publisherNew York: Holt</dc:publisher>
						   </olac:olac>
					</metadata>
				</header>");

			//todo:
			XmlNamespaceManager nameSpaceManager = new XmlNamespaceManager(new NameTable());
			nameSpaceManager.AddNamespace("olac", "http://www.language-archives.org/OLAC/1.1/");
			nameSpaceManager.AddNamespace("dc", "http://purl.org/dc/elements/1.1/");
			AssertThatXmlIn.Dom(_resultDom).HasAtLeastOneMatchForXpath("lift/header/metadata/olac:olac/dc:", nameSpaceManager);
		}

		[Test]
		public void Run_NoHomographs_OK()
		{
			var contents = @"
				<entry id='foo' GUID1>
					<lexical-unit>
						  <form lang='en'><text>hello</text></form>
					</lexical-unit>
				</entry>
				<entry GUID2>
					<lexical-unit>
						  <form lang='en'><text>bye</text></form>
					</lexical-unit>
				</entry>";
			Run(contents);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry", 2);
		}


		[Test]
		public void Run_DiffModifiedDates_NewerModifiedDateUsed()
		{
			var contents = @"
				<entry id='foo' GUID1 dateModified='2006-10-02T01:42:57Z'>
					<lexical-unit>
						  <form lang='en'><text>foo</text></form>
					</lexical-unit>
				</entry>
				<entry GUID2 dateModified='2009-10-02T01:42:57Z'>
					<lexical-unit>
						  <form lang='en'><text>foo</text></form>
					</lexical-unit>
				</entry>";
			Run(contents);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry", 1);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[@dateModified='2009-10-02T01:42:57Z']", 1);

			//now reverse them and it should not matter
			contents = @"
				<entry id='foo' GUID1 dateModified='2009-10-02T01:42:57Z'>
					<lexical-unit>
						  <form lang='en'><text>foo</text></form>
					</lexical-unit>
				</entry>
				<entry GUID2 dateModified='2006-10-02T01:42:57Z'>
					<lexical-unit>
						  <form lang='en'><text>foo</text></form>
					</lexical-unit>
				</entry>";
			Run(contents);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry", 1);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[@dateModified='2009-10-02T01:42:57Z']", 1);
		}


		[Test]
		public void Run_ComplicatedSense_FullSensesPreserved()
		{
			var contents = @"
				<entry id='foo1' guid='57009cdb-cd11-451f-8340-05dce62cc000'>
					<lexical-unit>
						  <form lang='en'><text>foo</text></form>
					</lexical-unit>
					<sense id='senseId1' order='4'>
						<grammatical-info value='Associativo'>
							<trait name='type' value='inflAffix'></trait>
							<trait name='Preposição-slot' value='assocncl'></trait>
						</grammatical-info>
						<gloss lang='en'><text>english1</text></gloss>
						<gloss lang='pt'><text>portugues1</text></gloss>
					</sense>
				</entry>
				<entry id='blah' guid='57009cdb-cd11-451f-8340-05dce62cc001'>
					<lexical-unit>
						  <form lang='en'><text>blah</text></form>
					</lexical-unit>
				</entry>
				<entry id='foo2' guid='57009cdb-cd11-451f-8340-05dce62cc002'>
					<lexical-unit>
						  <form lang='en'><text>foo</text></form>
					</lexical-unit>
				   <sense id='senseId2' order='4'>
						<grammatical-info value='Nombre'>
							<trait name='type' value='inflAffix'></trait>
							<trait name='Preposição-slot' value='assocncl'></trait>
						</grammatical-info>
						<gloss lang='en'><text>english2</text></gloss>
						<gloss lang='pt'><text>portugues2</text></gloss>
					</sense>
				</entry>";
			Run(contents);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//lexical-unit", 2);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//lexical-unit/form/text[text()='foo']", 1);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[lexical-unit/form/text[text()='foo']]/sense", 2);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[lexical-unit/form/text[text()='foo']]/sense[@id='senseId2']/grammatical-info/trait", 2);
			AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[lexical-unit/form/text[text()='foo']]/sense[@id='senseId2']/gloss", 2);
		}

		private void Run(string contents)
		{
			var m = new MergeHomographs();
			contents = contents.Replace("GUID1", "guid='"+Guid.NewGuid().ToString()+"'");
			contents = contents.Replace("GUID2", "guid='" + Guid.NewGuid().ToString() + "'");
			using (var input = new TempLiftFile(contents,"0.13"))
			{
				File.Delete(_outputLift.Path);
				m.Run(input.Path, _outputLift.Path, _progress);
			}

			_resultDom.Load(_outputLift.Path);

			//removing these tombstones simplifies our assertions, later
			foreach (XmlNode deletedEntry in _resultDom.SelectNodes("//entry[@dateDeleted]"))
			{
				deletedEntry.ParentNode.RemoveChild(deletedEntry);
			}
		}
	}
}
