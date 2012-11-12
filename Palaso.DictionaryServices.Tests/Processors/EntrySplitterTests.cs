using System;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Palaso.DictionaryServices.Processors;
using Palaso.Progress;
using Palaso.TestUtilities;

namespace Palaso.DictionaryServices.Tests.Merging
{
	[TestFixture]
	public class EntrySplitterTests
	{
		private StringBuilderProgress _progress;
		private XmlDocument _resultDom;

		[SetUp]
		public void Setup()
		{
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
				</header>",
						  ()=>
							{
								AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("lift/header/ranges/range", 2);
							});

		}
		[Test]
		public void Run_Has2CAWLSenses_SplitIntoToEntries()
		{
			Run(
				@"<entry id='foo' guid='5e67f88d-f0f0-42d1-a10e-6a7abaac05a4'>
						<lexical-unit>
			<form
				lang='abc'>
				<text>ai</text>
			</form>
		</lexical-unit>
		<sense
			id='eye_5e67f88d-f0f0-42d1-a10e-6a7abaac05a4'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>eye</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>œil</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>eye</text>
				</form>
				<form
					lang='fr'>
					<text>œil</text>
				</form>
			</definition>
			<field
				type='SILCAWL'>
				<form
					lang='en'>
					<text>0006</text>
				</form>
			</field>
			<trait
				name='semantic-domain-ddp4'
				value='2.1.1.1 Eye' />
		</sense>
		<sense
			id='eyelid_19d4a1a6-a714-42f3-964b-2e35e972b636'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>eyelid</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>paupière</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>eyelid</text>
				</form>
				<form
					lang='fr'>
					<text>paupière</text>
				</form>
			</definition>
			<field
				type='SILCAWL'>
				<form
					lang='en'>
					<text>0008</text>
				</form>
			</field>
			<trait
				name='semantic-domain-ddp4'
				value='2.1.1.1 Eye' />
				</sense>
				</entry>",
				() =>
				{
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry", 2);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//sense", 2);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[count(sense)=1]", 2);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry/lexical-unit/form[@lang='abc']/text[text()='ai']", 2);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry/sense/field/form/text[text()='0006']", 1);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry/sense/field/form/text[text()='0008']", 1);
				});
		}
		[Test]
		public void Run_Has1NonCAWLSense_DoesNothing()
		{
			Run(
				@"<entry id='foo' guid='5e67f88d-f0f0-42d1-a10e-6a7abaac05a4'>
						<lexical-unit>
			<form
				lang='abc'>
				<text>ai</text>
			</form>
		</lexical-unit>
		<sense
			id='eye_5e67f88d-f0f0-42d1-a10e-6a7abaac05a4'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>eye</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>œil</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>eye</text>
				</form>
				<form
					lang='fr'>
					<text>œil</text>
				</form>
			</definition>
		</sense>
				</entry>",
				() =>
				{
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry", 1);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//sense", 1);
				});
		}
		[Test]
		public void Run_Has2CAWLSensesAndOneOther_LeavesNonCawlSenseAlone()
		{
			Run(
				@"<entry id='foo' guid='5e67f88d-f0f0-42d1-a10e-6a7abaac05a4'>
						<lexical-unit>
			<form
				lang='abc'>
				<text>ai</text>
			</form>
		</lexical-unit>
		<sense
			id='eye_5e67f88d-f0f0-42d1-a10e-6a7abaac05a4'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>eye</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>œil</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>eye</text>
				</form>
				<form
					lang='fr'>
					<text>œil</text>
				</form>
			</definition>
			<field
				type='SILCAWL'>
				<form
					lang='en'>
					<text>0006</text>
				</form>
			</field>
			<trait
				name='semantic-domain-ddp4'
				value='2.1.1.1 Eye' />
		</sense>
		<sense
			id='leaveMeAlone'>
			<definition>
				<form
					lang='en'>
					<text>blah</text>
				</form>
			</definition>
		</sense>
		<sense
			id='eyelid_19d4a1a6-a714-42f3-964b-2e35e972b636'>
			<grammatical-info
				value='Noun' />
			<gloss
				lang='en'>
				<text>eyelid</text>
			</gloss>
			<gloss
				lang='fr'>
				<text>paupière</text>
			</gloss>
			<definition>
				<form
					lang='en'>
					<text>eyelid</text>
				</form>
				<form
					lang='fr'>
					<text>paupière</text>
				</form>
			</definition>
			<field
				type='SILCAWL'>
				<form
					lang='en'>
					<text>0008</text>
				</form>
			</field>
			<trait
				name='semantic-domain-ddp4'
				value='2.1.1.1 Eye' />
				</sense>
				</entry>",
				() =>
				{
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry", 2);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//sense", 3);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[count(sense)=2]", 1);
					AssertThatXmlIn.Dom(_resultDom).HasSpecifiedNumberOfMatchesForXpath("//entry[count(sense)=1]", 1);
				});
		}
		private void Run(string contents, Action test)
		{
			using (var input = new TempLiftFile(contents, "0.13"))
			{
				using (var repo = new LiftLexEntryRepository(input.Path))
				{
					EntrySplitter.Run(repo, _progress);
				}

				_resultDom.Load(input.Path);


				var bakPathname = input.Path + ".bak";
				if (File.Exists(bakPathname))
					File.Delete(bakPathname);
			}

			test();
		}
	}
}
