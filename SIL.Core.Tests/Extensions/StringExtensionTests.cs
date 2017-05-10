using NUnit.Framework;
using SIL.Extensions;

namespace SIL.Tests.Extensions
{
	[TestFixture]
	public class StringExtensionTests
	{
		[Test]
		public void SplitTrimmed_StringHasSpacesOnly_GivesEmptyList()
		{
			Assert.AreEqual(0, "  ".SplitTrimmed(',').Count);
		}

		[Test]
		public void SplitTrimmed_StringIsEmpty_GivesEmptyList()
		{
			Assert.AreEqual(0, string.Empty.SplitTrimmed(',').Count);
		}

		[Test]
		public void SplitTrimmed_SingleItem_GivesSingleTrimmedItem()
		{
			var list = " hello ".SplitTrimmed(',');
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("hello", list[0]);
		}

		[Test]
		public void SplitTrimmed_StartsWithSeparator_JustSkipsFirstItem()
		{
			var list = " , first".SplitTrimmed(',');
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("first", list[0]);
		}

		[Test]
		public void SplitTrimmed_HasEmptyItems_JustSkipsThem()
		{
			var list = "one,, ,,two".SplitTrimmed(',');
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("one", list[0]);
			Assert.AreEqual("two", list[1]);
		}

		[Test]
		public void SplitTrimmed_Various_GivesEmptyList()
		{
			var list = " hello , bye ".SplitTrimmed(',');
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("hello", list[0]);
			Assert.AreEqual("bye", list[1]);
		}

		[Test]
		public void EscapeAnyUnicodeCharactersIllegalInXml_HasMixOfXmlAndIllegalChars_GivesXmlWithEscapedChars()
		{
			var s  ="This <span href=\"reference\">is well \u001F formed</span> XML!";
			Assert.AreEqual("This <span href=\"reference\">is well &#x1F; formed</span> XML!",
				s.EscapeAnyUnicodeCharactersIllegalInXml());
		}

		[Test]
		public void EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml_HasMixOfXmlAndIllegalChars_GivesPureTextEscapedChars()
		{
			var s = "This <span href=\"reference\">is not really \u001F xml.";
			Assert.AreEqual("This &lt;span href=\"reference\"&gt;is not really &#x1F; xml.",
				s.EscapeSoXmlSeesAsPureTextAndEscapeCharactersIllegalInXml());
//
//			XmlDocument x = new XmlDocument();
//			x.LoadXml("<hello>&#x1f;</hello>");
//			var writer = XmlWriter.Create(Path.GetTempFileName());
//			writer.WriteElementString("x", x.InnerText);
//
//			writer.WriteNode(z);
		 }

		[Test]
		public void Format_NormalSafeString_GivesSameAsStringFormat()
		{
			Assert.That("1={0}".FormatWithErrorStringInsteadOfException("1").Equals("1=1"));
		}


		[Test]
		public void Format_StringWithNoArgs_GivesErrorString()
		{
			Assert.That("{node}".FormatWithErrorStringInsteadOfException().Equals("{node}"));
		}
		[Test]
		public void Format_UnSafeString_GivesErrorString()
		{
			Assert.That("{foo}".FormatWithErrorStringInsteadOfException("blah").Contains("Error"));
		}

		[Test]
		public void ToUpperFirstLetter_Empty_EmptyString()
		{
			Assert.AreEqual("","".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_OneCharacter_UpperCase()
		{
			Assert.AreEqual("X", "x".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_Digit_ReturnsSame()
		{
			Assert.AreEqual("1abc", "1abc".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_AlreadyUpper_ReturnsSame()
		{
			Assert.AreEqual("Abc", "Abc".ToUpperFirstLetter());
		}
		[Test]
		public void ToUpperFirstLetter_typical_MakesUppercase()
		{
			Assert.AreEqual("Abc", "abc".ToUpperFirstLetter());
		}

		[Test]
		public void ToUpperFirstLetter_ALLUPPERCASE_MakesOnlyFirstUppercase()
		{
			Assert.AreEqual("Abc def", "ABC DEF".ToUpperFirstLetter());
		}

		[Test]
		public void ToIntArray_EmptyString_ReturnsEmptyArray()
		{
			Assert.AreEqual(0, string.Empty.ToIntArray().Length);
		}

		[Test]
		public void ToIntArray_StringWithCommaSeparatedIntegers_ReturnsArrayOfIntegers()
		{
			var result = "1, 2, 3, 8".ToIntArray();
			Assert.AreEqual(4, result.Length);
			Assert.AreEqual(1, result[0]);
			Assert.AreEqual(2, result[1]);
			Assert.AreEqual(3, result[2]);
			Assert.AreEqual(8, result[3]);
		}

		[Test]
		public void ToIntArray_StringWithEmptySpot_ReturnsArrayOfIntegers()
		{
			var result = "1, 2, 3,".ToIntArray();
			Assert.AreEqual(3, result.Length);
			Assert.AreEqual(1, result[0]);
			Assert.AreEqual(2, result[1]);
			Assert.AreEqual(3, result[2]);
		}

		[Test]
		public void ToIntArray_StringWithFloatingPointNumbers_ReturnsEmptyArray()
		{
			var result = "1.3, 2.5, 3.6,".ToIntArray();
			Assert.AreEqual(0, result.Length);
		}

		[TestCase("hello\u0300 world", "hello world")]
		[TestCase("hello world", "hello world")]
		public void RemoveDiacritics_ExpectedResults(string str, string expectedResult)
		{
			Assert.AreEqual(expectedResult, str.RemoveDiacritics());
		}
	}
}
