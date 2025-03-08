using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;
using SIL.IO;
using SIL.Xml;

namespace SIL.Tests.Xml
{
	/// <summary>
	/// Tests for the FastXmlElementSplitter class.
	/// </summary>
	[TestFixture]
	public class FastXmlElementSplitterTests
	{
		private int _oldBufLen;
		[SetUp]
		public void Setup()
		{
			// Run these tests with a very small buffer, to give the AsyncFileReader a real workout.
			_oldBufLen = AsyncFileReader.kbufLen;
			AsyncFileReader.kbufLen = 10;
		}
		[TearDown]
		public void TearDown()
		{
			AsyncFileReader.kbufLen = _oldBufLen;
		}
		[Test]
		public void Null_Pathname_Throws()
		{
			Assert.Throws<ArgumentException>(() => new FastXmlElementSplitter((string)null));
		}

		[Test]
		public void Empty_String_Pathname_Throws()
		{
			Assert.Throws<ArgumentException>(() => new FastXmlElementSplitter(string.Empty));
		}

		[Test]
		public void File_Not_Found_Throws()
		{
			Assert.Throws<FileNotFoundException>(() => new FastXmlElementSplitter("Non-existent-file.xml"));
		}

		[Test]
		public void Null_Parameter_Throws()
		{
			using (var tempFile = new TempFile(""))
			{
				using (var reader = new FastXmlElementSplitter(tempFile.Path))
				{
					// ToList is needed to make the enumerable evaluate.
					Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes(null).ToList());
				}
			}
		}

		[Test]
		public void Empty_String_Parameter_Throws()
		{
			using (var tempFile = new TempFile(""))
			{
				using (var reader = new FastXmlElementSplitter(tempFile.Path))
				{
					// ToList is needed to make the enumeration evaluate.
					Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes("").ToList());
				}
			}
		}

		[Test]
		public void No_Records_With_Children_Is_Fine()
		{
			const string noRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
</classdata>";

			using (var tempFile = TempFile.WithExtension(".ClassData"))
			{
				File.WriteAllText(tempFile.Path, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(tempFile.Path))
				{
					Assert.AreEqual(0, reader.GetSecondLevelElementBytes("rt").Count());
				}
			}
		}

		[Test]
		public void No_Records_Without_Children_Is_Fine()
		{
			const string noRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata />";

			using (var tempFile = TempFile.WithExtension(".ClassData"))
			{
				File.WriteAllText(tempFile.Path, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(tempFile.Path))
				{
					Assert.AreEqual(0, reader.GetSecondLevelElementBytes("rt").Count());
				}
			}
		}

		[Test]
		public void Not_Xml_Throws()
		{
			const string noRecordsInput = "Some random text file.";

			using (var tempFile = TempFile.WithExtension(".txt"))
			{
				File.WriteAllText(tempFile.Path, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(tempFile.Path))
				{
					// An earlier version was expected to throw XmlException. But we aren't parsing XML well enough to do that confidently.
					// Note: the ToList is needed to force the enumeration to enumerate.
					Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes("rt").ToList());
				}
			}
		}

		[Test]
		public void MismatchedOptionalTagThrows()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<optional />
<rt guid='emptyElement1'/>
<rt guid='normalElement'>
	<randomElement />
</rt>
<rt guid='emptyElement2' />
</classdata>";

			// throws because we're telling it to parse a file with <rt> elements and an optional first <badfirsttag> element,
			// which means the actual first element <optional /> is unexpected.
			Assert.Throws<ArgumentException>(() => CheckGoodFile(hasRecordsInput, 5, "badfirsttag", "rt"));
		}

		[Test]
		public void MismatchedMainTagThrows()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='emptyElement1'/>
<rt guid='normalElement'>
	<randomElement />
</rt>
<rt guid='emptyElement2' />
</classdata>";

			// We're telling it the file should containg <notag> elements.
			Assert.Throws<ArgumentException>(() => CheckGoodFile(hasRecordsInput, 5, null, "notag"));
		}

		[Test]
		public void MismatchedOptionalAndMainTagThrows()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<optional />
<rt guid='emptyElement1'/>
<rt guid='normalElement'>
	<randomElement />
</rt>
<rt guid='emptyElement2' />
</classdata>";

			// The file is expected to contain an option <badfirsttag> followed by <notag> elements.
			Assert.Throws<ArgumentException>(() => CheckGoodFile(hasRecordsInput, 5, "badfirsttag", "notag"));
		}

		[Test]
		public void Can_Find_Good_FieldWorks_Records()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<rt guid='emptyElement1'/>
<rt guid='normalElement'>
	<randomElement />
</rt>
<rt
	guid='atterOnNextLine'>
</rt>
<rt		guid='tabAfterOpenTag'>
</rt>
<rt guid='emptyElement2' />
</classdata>";

			CheckGoodFile(hasRecordsInput, 5, null, "rt");
			CheckGoodFile(hasRecordsInput, 5, null, "<rt");
		}

		[Test]
		public void Can_Find_Single_Good_Element_With_No_White_Space()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata><rt guid='emptyElement1'/></classdata>";

			CheckGoodFile(hasRecordsInput, 1, null, "rt");
		}

		[Test]
		public void Can_Find_Single_Complex_Element_With_No_White_Space()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata><rt guid='emptyElement1'><rt guid='trash'/></rt></classdata>";

			CheckGoodFile(hasRecordsInput, 1, null, "rt");
		}

		[Test]
		public void Bad_Final_Marker_Is_Detected()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata><marker guid='emptyElement1'><marker guid='trash'/></marke></classdata>";

			Assert.Throws<ArgumentException>(() => CheckGoodFile(hasRecordsInput, 1, null, "marker"));
		}

		// We detect at least a top-level mismatched /something.
		// We won't currently detect a mismatch nested inside a good pair.
		[Test]
		public void Invalid_Nesting_Is_Detected()
		{
			const string hasNestedRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<ReversalIndex>
</ReversalIndex>
<ReversalIndexEntry guid='elementWithNesting1'>
	<something>
		<ReversalIndexEntry guid='nestedElement1'/>
	</ReversalIndexEntry>
</something>
<ReversalIndexEntry guid='elementWithNesting2'>
	<ReversalIndexEntry guid='nestedElement2'>
		<morestuff />
	</ReversalIndexEntry>
</ReversalIndexEntry>
</Reversal>";

			Assert.Throws<ArgumentException>(() => CheckGoodFile(hasNestedRecordsInput, 3, "ReversalIndex", "ReversalIndexEntry"));
		}

		[Test]
		public void FindsMainRecordsWhenTheyContainNestedElementsOfSameElementName()
		{
			const string hasNestedRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<Reversal>
<ReversalIndex>
</ReversalIndex>
<ReversalIndexEntry guid='elementWithNesting1'>
	<ReversalIndexEntry guid='nestedElement1' />
</ReversalIndexEntry>
<ReversalIndexEntry guid='elementWithNesting2'>
	<ReversalIndexEntry guid='nestedElement2'>
		<morestuff />
	</ReversalIndexEntry>
</ReversalIndexEntry>
</Reversal>";

			CheckGoodFile(hasNestedRecordsInput, 3, "ReversalIndex", "ReversalIndexEntry");
		}

		[Test]
		public void Can_Find_Obsolete_Custom_FieldWorks_Element()
		{
			// FW no longer has the AdditionalFields element in the main file,
			// but it is still a good test for the fast splitter, which does support optional first elements.
			// LIFT still has its optional header element, which could be used here instead,
			// but it is not worth it (to me [RandyR]) to switch it to a LIFT sample.
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
<AdditionalFields>
<CustomField name='Certified' class='WfiWordform' type='Boolean' />
</AdditionalFields>
<rt guid='emptyElement1'/>
<rt guid='normalElement'>
	<randomElement />
</rt>
<rt
	guid='atterOnNextLine'>
</rt>
<rt		guid='tabAfterOpenTag'>
</rt>
<rt guid='emptyElement2' />
</classdata>";

			CheckGoodFile(hasRecordsInput, 6, "AdditionalFields", "rt");
		}

		[Test]
		public void Can_Find_Good_Lift_Records()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
					   <entry id='sameInBoth'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='themOnly'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUs'/>

						<entry
							id='brewingConflict'>
							<sense>
								 <gloss lang='a'>
									<text>them</text>
								 </gloss>
							 </sense>
						</entry>

					</lift>";

			CheckGoodFile(hasRecordsInput, 4, "header", "entry");
		}

		[Test]
		public void Handles_Missing_End_Element()
		{
			const string hasIncompleteInput =
			@"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.10' producer='WeSay 1.0.0.0'>
					   <entry id='sameInBoth'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='themOnly'>
							<lexical-unit>
								<form lang='b'>
									<text>form b</text>
								</form>
							</lexical-unit>
						 </entry>
						<entry id='doomedByUs'/>

						<entry
							id='brewingConflict'>
							<sense>
								 <gloss lang='a'>
									<text>them</text>
								 </gloss>
							 </sense>

					</lift>";

			Assert.Throws<ArgumentException>(() => CheckGoodFile(hasIncompleteInput, 0, "header", "entry"));
		}

		[Test]
		public void Can_Find_Good_ChorusNotes_Records_With_CDATA()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
<notes
	version='0'>
	<annotation
		class='mergeConflict'
		ref='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
		guid='1cb66d60-90d5-4367-95b1-b7b41eb8986d'>
		<message
			author='merger'
			status='open'
			guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
			date='2012-07-20T14:18:35Z'>Entry 'pintu': user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.<![CDATA[<conflict
	typeGuid='3d9ba4ac-4a25-11df-9879-0800200c9a66'
	class='Chorus.merge.xml.generic.EditedVsRemovedElementConflict'
	relativeFilePath='Linguistics\Lexicon\Lexicon.lexdb'
	type='Removed Vs Edited Element Conflict'
	guid='ef89b532-5441-48a8-aea9-065b6ab5cfbd'
	date='2012-07-20T14:18:35Z'
	whoWon='user57'
	htmlDetails='&lt;head&gt;&lt;style type='text/css'&gt;&lt;/style&gt;&lt;/head&gt;&lt;body&gt;&lt;div class='description'&gt;Entry &quot;pintu&quot;: user57@tpad2 deleted this element, while user57 edited it. The automated merger kept the change made by user57.&lt;/div&gt;&lt;div class='alternative'&gt;user57's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 &lt;span style=&quot;text-decoration: line-through; color: red&quot;&gt;13:46:3.625&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;14:14:20.218&lt;/span&gt;&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;AStr&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;Run&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;ws=&quot;en&quot;&gt;a&lt;/span&gt; &lt;span style=&quot;background: Yellow&quot;&gt;door&lt;/span&gt;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Run&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/AStr&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&lt;span style=&quot;background: Yellow&quot;&gt;&amp;lt;/Definition&gt;&lt;/span&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='alternative'&gt;user57@tpad2's changes: &amp;lt;LexEntry guid=&quot;bab7776e-531b-4ce1-997f-fa638c09e381&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateCreated val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DateModified val=&quot;2012-7-20 13:46:3.625&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;DoNotUseForParsing val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;HomographNumber val=&quot;0&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemAllomorph guid=&quot;556f6e08-0fb2-4171-82e0-6dcdddf9490b&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;id&quot;&gt;pintu&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Form&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;IsAbstract val=&quot;False&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;d7f713e8-e8cf-11d3-9764-00c04f186933&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphType&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MoStemAllomorph&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/LexemeForm&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MoStemMsa guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalyses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;Senses&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;ownseq class=&quot;LexSense&quot; guid=&quot;dad069de-dfad-45f6-a5d2-449265adbc3a&quot;&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;AUni ws=&quot;en&quot;&gt;door&amp;lt;/AUni&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/Gloss&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;objsur guid=&quot;f63e03f0-ac9d-4b1b-980f-316bbb741f70&quot; t=&quot;r&quot; /&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/MorphoSyntaxAnalysis&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;nbsp;&amp;lt;/ownseq&gt;&lt;br/&gt;&amp;nbsp;&amp;nbsp;&amp;lt;/Senses&gt;&lt;br/&gt;&amp;lt;/LexEntry&gt;&lt;/div&gt;&lt;div class='mergechoice'&gt;The merger kept the change made by user57&lt;/div&gt;&lt;/body&gt;'
	contextPath='silfw://localhost/link?app=flex&amp;database=current&amp;server=&amp;tool=default&amp;guid=bab7776e-531b-4ce1-997f-fa638c09e381&amp;tag=&amp;label=Entry &quot;pintu&quot;'
	contextDataLabel='Entry &quot;pintu&quot;'>
	<MergeSituation
		alphaUserId='user57'
		betaUserId='user57@tpad2'
		alphaUserRevision='306520fcc148'
		betaUserRevision='5aa248710fbc'
		path='Linguistics\Lexicon\Lexicon.lexdb'
		conflictHandlingMode='WeWin' />
</conflict>]]></message>
		<message
			author='user57'
			status='closed'
			date='2012-07-20T22:49:03Z'
			guid='bf43783e-eca1-4b0f-bacd-6fe168d7d616'></message>
	</annotation>
</notes>
}";

			CheckGoodFile(hasRecordsInput, 1, null, "annotation");
		}

		[Test]
		public void Can_Find_Good_Records_With_Second_Level_Comment()
		{
			const string hasRecordsInput =
				@"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
					   <entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'/>
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />

						<!-- everything above this line was being merged, but not this -->
						<entry id='lostBoy' guid='bbed1f95-e382-11de-8a39-0800200c9a66' />
					</lift>";

			CheckGoodFile(hasRecordsInput, 4, null, "entry");
		}

		[Test]
		public void SplitterHandlesEmptyRootOnASCIIAndUTF8Files()
		{
			const string emptyRoot =
@"<notes version='0'/>";
			CheckGoodFile(emptyRoot, 0, null, "annotation", Encoding.ASCII);
			CheckGoodFile(emptyRoot, 0, null, "annotation", Encoding.UTF8);
		}

		[Test]
		public void SplitterFailsInvalidRootTagOnASCIIAndUTF8Files()
		{
			const string emptyRoot =
@"notes version='0'/>";
			Assert.Throws<ArgumentException>(() => CheckGoodFile(emptyRoot, 0, null, "annotation", Encoding.ASCII), @"Failed to detect bad ASCII file");
			Assert.Throws<ArgumentException>(() => CheckGoodFile(emptyRoot, 0, null, "annotation", Encoding.UTF8), @"Failed to detect bad UTF8 file");
		}

		[Test]
		public void SplitterHandlesComments()
		{
			const string hasRecordsInput =
	@"<?xml version='1.0' encoding='utf-8'?>
					<lift version='0.13' producer='WeSay 1.0.0.0'>
						<header>
							<range
								id='translation-type'
								href='file://C:ranges' />
								<!-- The parts of speech are duplicated -->
							<range
								id='from-part-of-speech'
								href='file://C:/Users' />
						</header>
						<!-- First pesky comment -->
						<entry id='everybody' guid='dded1f95-e382-11de-8a39-0800200c9add'/>
						<!-- Another pesky comment -->
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />
						<entry id='duplicate' guid='c1ed1f95-e382-11de-8a39-0800200c9a66' />

						<!-- everything above this line was being merged, but not this -->
						<entry id='lostBoy' guid='bbed1f95-e382-11de-8a39-0800200c9a66' />
					</lift>";

			CheckGoodFile(hasRecordsInput, 5, "header", "entry");
		}

		[Test]
		public void SplitterHandlesMultipleEntryTypes()
		{
			const string simulatedLexicalFile =
				@"<Lexicon>
  <Language>Takwane</Language>
  <FontName>Tahoma</FontName>
  <FontSize>9</FontSize>
  <Analyses>
	<item>
	  <string>ahibathiziwa</string>
	  <ArrayOfLexeme>
		<Lexeme Type='Prefix' Form='ahi' Homograph='1' />
		<Lexeme Type='Stem' Form='bathiz' Homograph='1' />
		<Lexeme Type='Suffix' Form='iwa' Homograph='1' />
	  </ArrayOfLexeme>
	</item>
	<item>
	  <string>dhahaleele</string>
	  <ArrayOfLexeme>
		<Lexeme Type='Prefix' Form='dha' Homograph='1' />
		<Lexeme Type='Stem' Form='haleel' Homograph='1' />
		<Lexeme Type='Suffix' Form='e' Homograph='1' />
	  </ArrayOfLexeme>
	</item>
 </Analyses>
 <Entries>
	<item>
	  <Lexeme Type='Word' Form='eehu' Homograph='1' />
	  <Entry>
		<Sense Id='AnuoxmE2'>
		  <Gloss Language='English'>our</Gloss>
		</Sense>
		<Sense Id='LgBvPFTX'>
		  <Gloss Language='Portuguese'>nosso</Gloss>
		</Sense>
	  </Entry>
	</item>
	<item>
	  <Lexeme Type='Word' Form='yesu' Homograph='1' />
	  <Entry>
		<Sense Id='BBY1Mk/1'>
		  <Gloss Language='English'>jesus</Gloss>
		</Sense>
		<Sense Id='zV8exv2K'>
		  <Gloss Language='Portuguese'>jesus</Gloss>
		</Sense>
	  </Entry>
	</item>
  </Entries>
</Lexicon>";
			using (var tempFile = new TempFile())
			{
				File.WriteAllText(tempFile.Path, simulatedLexicalFile, Encoding.UTF8);
				using (var fastXmlElementSplitter = new FastXmlElementSplitter(tempFile.Path))
				{
					ProcessMultipleEntryContent(fastXmlElementSplitter);
				}
				using (var fastXmlElementSplitter = new FastXmlElementSplitter(File.ReadAllBytes(tempFile.Path)))
				{
					ProcessMultipleEntryContent(fastXmlElementSplitter);
				}
			}
		}

		private static void ProcessMultipleEntryContent(FastXmlElementSplitter fastXmlElementSplitter)
		{
			string[] expectedNames = new string[] { "Language", "FontName", "FontSize", "Analyses", "Entries" };
			var elements = fastXmlElementSplitter.GetSecondLevelElements().ToList();
			Assert.AreEqual(5, elements.Count);
			for (var i = 0; i < elements.Count; ++i)
			{
				var curElement = elements[i];
				Assert.AreEqual(curElement.Name, expectedNames[i]);
				XElement.Parse(curElement.BytesAsString);
				if (i >= 3)  // parse the sublist for Analyses and Entries
				{
					using (var splitter = new FastXmlElementSplitter(curElement.Bytes))
					{
						var subElements = splitter.GetSecondLevelElements().ToList();
						Assert.AreEqual(2, subElements.Count);
						Assert.IsFalse(subElements.Any(t => t.Name != "item"));
					}
				}
			}
		}

		// This test may be uncommented to try the splitter on some particular file which causes problems.
		//[Test]
		//public void SplitterParsesProblemFile()
		//{
		//	using (var fastXmlElementSplitter = new FastXmlElementSplitter(@"D:\DownLoads\y.lift"))
		//	{
		//		bool foundOptionalFirstElement;
		//		fastXmlElementSplitter.GetSecondLevelElementBytes("header", "entry", out foundOptionalFirstElement)
		//									  .ToList();
		//	}
		//}

		private static void CheckGoodFile(string hasRecordsInput, int expectedCount, string firstElementMarker,
			string recordMarker, Encoding enc = null)
		{
			var goodPathname = Path.GetTempFileName();
			try
			{
				if (enc == null)
					enc = Encoding.UTF8;
				File.WriteAllText(goodPathname, hasRecordsInput, enc);
				using (var fastXmlElementSplitter = new FastXmlElementSplitter(goodPathname))
				{
					ProcessContent(fastXmlElementSplitter, expectedCount, firstElementMarker, recordMarker, enc);
				}
				using (var fastXmlElementSplitter = new FastXmlElementSplitter(File.ReadAllBytes(goodPathname)))
				{
					ProcessContent(fastXmlElementSplitter, expectedCount, firstElementMarker, recordMarker, enc);
				}
			}
			finally
			{
				File.Delete(goodPathname);
			}
		}

		private static void ProcessContent(FastXmlElementSplitter fastXmlElementSplitter, int expectedCount, string firstElementMarker,
			string recordMarker, Encoding enc)
		{
			var elementBytes = fastXmlElementSplitter.GetSecondLevelElementBytes(
				firstElementMarker, recordMarker, out _).ToList();
			Assert.AreEqual(expectedCount, elementBytes.Count);
			var elementStrings = fastXmlElementSplitter.GetSecondLevelElementStrings(
				firstElementMarker, recordMarker, out _).ToList();
			Assert.AreEqual(expectedCount, elementStrings.Count);
			for (var i = 0; i < elementStrings.Count; ++i)
			{
				var currentStr = elementStrings[i];
				Assert.AreEqual(
					currentStr,
					enc.GetString(elementBytes[i]));
				XElement.Parse(currentStr);
			}
		}

	}
}
