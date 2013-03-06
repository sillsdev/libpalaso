using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using NUnit.Framework;
using Palaso.Xml;

namespace Palaso.Tests.Xml
{
	/// <summary>
	/// Tests for the FastXmlElementSplitter class.
	/// </summary>
	[TestFixture]
	public class FastXmlElementSplitterTests
	{
		[Test]
		public void Null_Pathname_Throws()
		{
			Assert.Throws<ArgumentException>(() => new FastXmlElementSplitter(null));
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
			// review: I (CP) don't know that this is a sufficiently good method for determining the file - even in windows.
			// In mono I had to make this the absolute path. 2011-01
#if MONO
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file://", null)))
#else
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", null)))
#endif
			{
				// ToList is needed to make the enumerable evaluate.
				Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes(null).ToList());
			}
		}

		[Test]
		public void Empty_String_Parameter_Throws()
		{
			// review: I (CP) don't know that this is a sufficiently good method for determining the file - even in windows.
			// In mono I had to make this the absolute path. 2011-01
#if MONO
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file://", null)))
#else
			using (var reader = new FastXmlElementSplitter(Assembly.GetExecutingAssembly().CodeBase.Replace(@"file:///", null)))
#endif
			{
				// ToList is needed to make the enumeration evaluate.
				Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes("").ToList());
			}
		}

		[Test]
		public void No_Records_With_Children_Is_Fine()
		{
			const string noRecordsInput =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata>
</classdata>";
			var tempPathname = Path.GetTempFileName();
			var goodXmlPathname = Path.ChangeExtension(tempPathname, ".ClassData");
			File.Delete(tempPathname);
			try
			{
				File.WriteAllText(goodXmlPathname, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(goodXmlPathname))
				{
					Assert.AreEqual(0, reader.GetSecondLevelElementBytes("rt").Count());
				}
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void No_Records_Without_Children_Is_Fine()
		{
			const string noRecordsInput =
@"<?xml version='1.0' encoding='utf-8'?>
<classdata />";

			var tempPathname = Path.GetTempFileName();
			var goodXmlPathname = Path.ChangeExtension(tempPathname, ".ClassData");
			File.Delete(tempPathname);
			try
			{
				File.WriteAllText(goodXmlPathname, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(goodXmlPathname))
				{
					Assert.AreEqual(0, reader.GetSecondLevelElementBytes("rt").Count());
				}
			}
			finally
			{
				File.Delete(goodXmlPathname);
			}
		}

		[Test]
		public void Not_Xml_Throws()
		{
			const string noRecordsInput = "Some random text file.";
			var tempPathname = Path.GetTempFileName();
			var goodPathname = Path.ChangeExtension(tempPathname, ".txt");
			File.Delete(tempPathname);
			try
			{
				File.WriteAllText(goodPathname, noRecordsInput, Encoding.UTF8);
				using (var reader = new FastXmlElementSplitter(goodPathname))
				{
					// An earlier version was expected to throw XmlException. But we aren't parsing XML well enough to do that confidently.
					// Note: the ToList is needed to force the enumeration to enumerate.
					Assert.Throws<ArgumentException>(() => reader.GetSecondLevelElementBytes("rt").ToList());
				}
			}
			finally
			{
				File.Delete(goodPathname);
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
			// LIFT still has its optional header element, which coudl be used here instead,
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

		private static void CheckGoodFile(string hasRecordsInput, int expectedCount, string firstElementMarker, string recordMarker)
		{
			var goodPathname = Path.GetTempFileName();
			try
			{
				var enc = Encoding.UTF8;
				File.WriteAllText(goodPathname, hasRecordsInput, enc);
				using (var fastXmlElementSplitter = new FastXmlElementSplitter(goodPathname))
				{
					bool foundOptionalFirstElement;
					var elementBytes = fastXmlElementSplitter.GetSecondLevelElementBytes(firstElementMarker, recordMarker, out foundOptionalFirstElement).ToList();
					Assert.AreEqual(expectedCount, elementBytes.Count);
					var elementStrings = fastXmlElementSplitter.GetSecondLevelElementStrings(firstElementMarker, recordMarker, out foundOptionalFirstElement).ToList();
					Assert.AreEqual(expectedCount, elementStrings.Count);
					for (var i = 0; i < elementStrings.Count; ++i)
					{
						var currentStr = elementStrings[i];
						Assert.AreEqual(
							currentStr,
							enc.GetString(elementBytes[i]));
						var el = XElement.Parse(currentStr);
					}
				}
			}
			finally
			{
				File.Delete(goodPathname);
			}
		}
	}
}
