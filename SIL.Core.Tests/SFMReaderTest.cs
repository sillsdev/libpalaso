using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using SIL.IO;

namespace SIL.Tests
{
	[TestFixture]
	public class SFMReaderTest
	{
		[Test]
		public void ReadNextText_MultilineInitialTextStateFollowedByMedialTag_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text first\ntext \\tag"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual(string.Empty, test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineInitialTextStateFollowedByInitialTagThenText_2ndLineText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text first\n\\tag more text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("more text", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineInitialTextState_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text first\nmore text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text first\nmore text", test.ReadInitialText());
			Assert.IsNull(test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineInitialTextStateFollowedByTagOnly_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text first\n\\tag"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual(string.Empty, test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineInitialTextStateFollowedByEmpty_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text first\n"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text first\n", test.ReadInitialText());
			Assert.IsNull(test.ReadNextText());
		}
		// Multiline tag state
		[Test]
		public void ReadNextText_MultilineTagStateFollowedByMedialTag_1stPartOf2ndLineText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\ntext \\tag more text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text ", test.ReadNextText());
			Assert.AreEqual("more text", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineTagStateFollowedByTextOnly_2ndLineText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\nmore text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("more text", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineTagStateFollowedByTagOnly_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\n\\tag"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual(string.Empty, test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineTagStateFollowedByEmpty_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\n"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual(string.Empty, test.ReadNextText());
		}
		// Multiline Text state
		[Test]
		public void ReadNextText_MultilineTextStateFollowedByMedialTag_1stLineTextAnd2ndLineBeginningText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text first\ntext \\tag more text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text first\ntext ", test.ReadNextText());
			Assert.AreEqual("more text", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineTextStateFollowedByInitialTag_1stLineTextThen2ndLineText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text first\n\\tag more text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text first\n", test.ReadNextText());
			Assert.AreEqual("more text", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineTextStateFollowedByTextOnly_1stAnd2ndLineText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text first\nmore text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text first\nmore text", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineTextStateFollowedByTagOnly_1stLineText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text first\n\\tag"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text first\n", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineTextStateFollowedByEmpty_1stAnd2ndLineText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text first\n"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("text first\n", test.ReadNextText());
		}
		// Multiline Next Text, Empty state
		[Test]
		public void ReadNextText_MultilineEmptyFollowedByEmpty_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\n\n"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("\n", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineEmptyFollowedByTextOnly_2ndLine()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\n\ntext"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("\ntext", test.ReadNextText());
		}
		[Test]
		public void ReadNextText_MultilineEmptyFollowedByTag_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\n\n\\tag"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("\n", test.ReadNextText());
		}
		//----
		[Test]
		public void ReadNextText_TextOnly_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"text first"));
			SFMReader test = new SFMReader(stream);

			Assert.IsNull(test.ReadNextText());
		}
		[Test]
		public void ReadNextText_TagOnly_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\one"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextText();
			Assert.AreEqual("", token);
		}
		[Test]
		public void ReadNextText_InitialText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"text first\bf text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextText();
			Assert.AreEqual("text", token);
		}
		[Test]
		public void ReadNextText_MedialText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\one text first\bf"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextText();
			Assert.AreEqual("text first", token);
		}
		[Test]
		public void ReadNextText_TagLast_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\one text first\two"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextText();
			string token = test.ReadNextText();
			Assert.AreEqual(string.Empty, token);
		}
		[Test]
		public void ReadNextText_TextLast_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\one text first"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextText();
			string token = test.ReadNextText();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextText_NoText_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@""));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextText();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextText_TwoTagsInARow_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag1\tag2"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextText();
			Assert.AreEqual(string.Empty, token);
		}
		[Test]
		public void ReadInitialText_InitialText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"text first\bf"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("text first", token);
		}
		[Test]
		public void ReadInitialText_NoInitialText_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\bf some text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual(string.Empty, token);
		}
		[Test]
		public void ReadInitialText_TagOnly_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\bf"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual(string.Empty, token);
		}
		[Test]
		public void ReadInitialText_TextOnly()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"some text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("some text", token);
		}
		[Test]
		public void ReadInitialText_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@""));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("", token);
		}
		// Initial Text, mulitiline
		[Test]
		public void ReadInitialText_MultiLineEmptyFollowedByEmpty_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\n\n"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("\n\n", token);
		}
		[Test]
		public void ReadInitialText_MultiLineEmptyFollowedByText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\ntext"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("\ntext", token);
		}
		[Test]
		public void ReadInitialText_MultiLineEmptyFollowedByTag_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\n\\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("\n", token);
		}
		[Test]
		public void ReadInitialText_MultiLineEmptyFollowedByTextAndTag_2ndLineBeginningText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\ntext \\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("\ntext ", token);
		}
		[Test]
		public void ReadInitialText_MultiLineTextFollowedByEmpty_1stLine()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text\n"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("text\n", token);
		}
		[Test]
		public void ReadInitialText_MultiLineTextFollowedByText_2Lines()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text\nmore text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("text\nmore text", token);
		}
		[Test]
		public void ReadInitialText_MultiLineTextFollowedByTag_1stLine()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text\n\\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("text\n", token);
		}
		[Test]
		public void ReadInitialText_MultiLineTextFollowedByTextAndTag_1stLineAndBeginning2ndLine()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"text\nmore text\\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual("text\nmore text", token);
		}
		[Test]
		public void ReadInitialText_MultiLineTagFollowedByEmpty_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\n"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual(string.Empty, token);
		}
		[Test]
		public void ReadInitialText_MultiLineTagFollowedByText_Empty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag\ntext"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadInitialText();
			Assert.AreEqual(string.Empty, token);
		}
		[Test]
		public void ReadNextTag_MedialTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag text first\bf more text"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.AreEqual("bf", token);
		}
		[Test]
		public void ReadNextTag_InitialText_FirstTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"text first\bf"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("bf", token);
		}
		[Test]
		public void ReadNextTag_InitialTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\bf some text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("bf", token);
		}
		[Test]
		public void ReadNextTag_FinalTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag text first\bf"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.AreEqual("bf", token);
		}
		[Test]
		public void ReadNextTag_AfterFinalText_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\bf text"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_TagOnly()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\bf"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("bf", token);
		}
		[Test]
		public void ReadNextTag_TextOnly_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"text first\bf"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("bf", token);
		}
		[Test]
		public void ReadNextTag_EmptyTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\ some text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual(string.Empty, token);
		}
		// Next tag, multiline, from empty
		[Test]
		public void ReadNextTag_MultilineEmptyFollowedByEmpty_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\ntext"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineEmptyFollowedByText_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\ntext"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineEmptyFollowedByTextThenTag_Tag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\ntext\\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);
		}
		[Test]
		public void ReadNextTag_MultilineEmptyFollowedByTagOnly()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\n\\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);
		}
		[Test]
		public void ReadNextTag_MultilineEmptyFollowedByTagThenText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\n\\tag text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);
		}
		// Multiline Initial Text
		[Test]
		public void ReadNextTag_MultilineInitialTextFollowedByEmpty()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"initial text\n"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineInitialTextFollowedByText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"initial text\nsome text"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineInitialTextFollowedByTextThenTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"initial text\ntext\\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);
		}
		[Test]
		public void ReadNextTag_MultilineInitialTextFollowedByTagOnly()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"initial text\n\\tag"));
			SFMReader test = new SFMReader(stream);

			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);
		}
		// Multiline text mode
		[Test]
		public void ReadNextTag_MultilineTextModeFollowedByEmpty_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag initial text\n"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineTextModeFollowedByTextOnly_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag initial text\ntext"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineTextModeFollowedByTextThenTag_2ndLineTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\taginitial text\ntext\\tag2"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.AreEqual("tag2", token);
		}
		[Test]
		public void ReadNextTag_MultilineTextModeFollowedByTagOnly_2ndLineTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\taginitial text\n\\tag2"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.AreEqual("tag2", token);
		}
		[Test]
		public void ReadNextTag_MultilineTagModeFollowedByEmpty_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\taginitial\n"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineTagModeFollowedByTextOnly_Null()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\taginitial\nsome text"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void ReadNextTag_MultilineTagModeFollowedByTextThenTag_2ndLineTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\taginitial\nsome text\\tag"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);
		}
		[Test]
		public void ReadNextTag_MultilineTagModeFollowedByTagOnly_2ndLineTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\taginitial\n\\tag"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			string token = test.ReadNextTag();
			Assert.AreEqual("tag", token);
		}
		[Test]
		public void ReadNextTag_TwoTagsInARow()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag1\tag2"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("tag1", test.ReadNextTag());
			Assert.AreEqual("tag2", test.ReadNextTag());
		}
		[Test]
		public void ReadNextTagThenReadNextText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag1 text"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextTag();
			Assert.AreEqual("text", test.ReadNextText());
		}
		[Test]
		public void ReadInitialThenReadNextTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"text \tag1"));
			SFMReader test = new SFMReader(stream);

			test.ReadInitialText();
			Assert.AreEqual("tag1", test.ReadNextTag());
		}
		[Test]
		public void ReadNextTextThenReadNextTag()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag1 some text\tag2"));
			SFMReader test = new SFMReader(stream);

			test.ReadNextText();
			Assert.AreEqual("tag2", test.ReadNextTag());
		}

		[Test]
		public void ReadNextTextThenReadInitialText_Throw()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(@"\tag1 some text\tag2"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextText();

			Assert.Throws<InvalidOperationException>(
				() => test.ReadInitialText());
		}
		[Test]
		public void ReadNextTagThenReadInitialText_Throw()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(@"\tag1 some text\tag2"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			Assert.Throws<InvalidOperationException>(
				() => test.ReadInitialText());
		}
		[Test]
		public void UsfmMode_TagTerminatedByAsterisk()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag1*some text"));
			SFMReader test = new SFMReader(stream);
			test.Mode = SFMReader.ParseMode.Usfm;

			Assert.AreEqual("tag1*", test.ReadNextTag());
			Assert.AreEqual("some text", test.ReadNextText());
		}
		[Test]
		public void DefaultMode_TagTerminatedByAsterisk()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag1*some text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual("tag1*some", test.ReadNextTag());
			Assert.AreEqual("text", test.ReadNextText());
		}
		[Test]
		public void Mode_InitializedToDefault()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag text first\bf"));
			SFMReader test = new SFMReader(stream);
			Assert.AreEqual(SFMReader.ParseMode.Default, test.Mode);
		}
		[Test]
		public void ShoeboxMode_TagsWithoutNewline_TreatedAsText()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					@"\tag text first\bf"));
			SFMReader test = new SFMReader(stream);
			test.Mode = SFMReader.ParseMode.Shoebox;
			Assert.AreEqual("text first\\bf", test.ReadNextText());
			string token = test.ReadNextTag();
			Assert.IsNull(token);
		}
		[Test]
		public void Offset_BeforeAnyTextIsRead_0()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text more text"));
			SFMReader test = new SFMReader(stream);

			Assert.AreEqual(0, test.Offset);
		}
		[Test]
		public void Offset_After3LetterTagThenSpace_5()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag "));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();

			Assert.AreEqual(5, test.Offset);
		}
		[Test]
		public void Offset_After3LetterTagAnd4LetterWord_9()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			test.ReadNextText();

			Assert.AreEqual(9, test.Offset);
		}
		[Test]
		public void Offset_After3LetterTagAnd4LetterWordAnd4LetterTag_14()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text\\tag2"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			test.ReadNextText();
			test.ReadNextTag();

			Assert.AreEqual(14, test.Offset);
		}
		[Test]
		public void Offset_After3LetterTagAnd4LetterWordASpaceThen4LetterTag_15()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text \\tag2"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			test.ReadNextText();
			test.ReadNextTag();

			Assert.AreEqual(15, test.Offset);
		}
		[Test]
		public void Offset_After5CharactersOfInitialText_5()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"hello"));
			SFMReader test = new SFMReader(stream);
			test.ReadInitialText();

			Assert.AreEqual(5, test.Offset);
		}
		[Test]
		public void Offset_After3LetterTag2SpacesAnd4LetterWord_10()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag  text"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			test.ReadNextText();

			Assert.AreEqual(10, test.Offset);
		}
		[Test]
		public void Offset_After3LetterTag4LetterWordAndASpace_10()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag text "));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			test.ReadNextText();

			Assert.AreEqual(10, test.Offset);
		}
		[Test]
		public void Offset_After3LetterTagAfterEOF_4()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag"));
			SFMReader test = new SFMReader(stream);
			test.ReadNextTag();
			test.ReadNextText();

			Assert.AreEqual(4, test.Offset);
		}
		[Test]
		public void Offset_ShoeboxModeAfter4LetterTagWithStarAfterEOF_5()
		{
			Stream stream = new MemoryStream(Encoding.ASCII.GetBytes(
					"\\tag*"));
			SFMReader test = new SFMReader(stream);
			test.Mode = SFMReader.ParseMode.Shoebox;
			test.ReadNextTag();
			test.ReadNextText();

			Assert.AreEqual(5, test.Offset);
		}
	}
}
