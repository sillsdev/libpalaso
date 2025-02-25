using System;
using System.Collections.Generic;
using NUnit.Framework;
using SIL.Spelling;

namespace SIL.Tests.Spelling
{
	[TestFixture]
	public class SpellingWordTokenizerTests
	{
		[Test]
		public void PassNULL_Throws()
		{
			Assert.Throws<ArgumentNullException>(
				() =>
				 {
					 foreach (var _ in WordTokenizer.TokenizeText(null))
					 { }
				 }
			 );
		}

		[Test]
		public void PassEmptyWord_ReturnWord()
		{
			string text = "";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(0, list.Count);
		}

		[Test]
		public void PassSimpleWord_ReturnWord()
		{
			string text = "Hello";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(text, list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(text.Length, list[0].Length);
		}

		[Test]
		public void PassWordWithWordInternalApostrophe_ReturnWord()
		{
			string text = "It's";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(text, list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(text.Length, list[0].Length);
		}

		[Test]
		public void PassWordWithWordFinalApostrophe_ReturnWord()
		{
			string text = "Jesus'";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("Jesus", list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(text.Length - 1, list[0].Length);
		}

		[Test]
		public void PassWordWithWordInternalHyphen_ReturnWord()
		{
			string text = "black-bird";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(text, list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(text.Length, list[0].Length);
		}

		[Test]
		public void PassWordWithWordPreviousInternalFinalPunctuation_ReturnWord()
		{
			string text = "?black,bird.flew!Home?";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual("black,bird.flew!Home", list[0].Value);
			Assert.AreEqual(1, list[0].Offset);
			Assert.AreEqual(text.Length - 2, list[0].Length); //drop word-initial and word-final punctuation
		}

		[Test]
		public void PassWordWithUnicodeSuperscript_ReturnWord()
		{
			string text = "fiance\u0301";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(text, list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(text.Length, list[0].Length);
		}

		[Test]
		public void PassGibberishWithoutWordWithoutSpace_ReturnWord()
		{
			string text = "1a2b3c";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(text, list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(text.Length, list[0].Length);
		}


		[Test]
		public void PassMultipleSimpleWords_ReturnMultipleWord()
		{
			string text = "Jesus wept";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("Jesus", list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(5, list[0].Length);

			Assert.AreEqual("wept", list[1].Value);
			Assert.AreEqual(6, list[1].Offset);
			Assert.AreEqual(4, list[1].Length);
		}


		[Test]
		public void PassMultipleSimpleWordsSeparatedByZeroWidthSpace_ReturnMultipleWords()
		{
			string text = "Jesus\u200bwept";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(2, list.Count);
			Assert.AreEqual("Jesus", list[0].Value);
			Assert.AreEqual(0, list[0].Offset);
			Assert.AreEqual(5, list[0].Length);

			Assert.AreEqual("wept", list[1].Value);
			Assert.AreEqual(6, list[1].Offset);
			Assert.AreEqual(4, list[1].Length);
		}

		[Test]
		public void PassAllPunctuation_ReturnWord()
		{
			string text = "??!!?!";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(0, list.Count);
		}

		[Test]
		public void PassMultipleDifferentWordsAndSpaces_ReturnWord()
		{
			string text = "Jesus' abc123s rejoiced,when black-bird's fiance\u0301\u200b flew.";
			List<WordTokenizer.Token> list = new List<WordTokenizer.Token>(WordTokenizer.TokenizeText(text));
			Assert.AreEqual(6, list.Count);

			Assert.AreEqual("Jesus", list[0].Value);
			Assert.AreEqual(5, list[0].Length);
			Assert.AreEqual(0, list[0].Offset);

			Assert.AreEqual("abc123s", list[1].Value);
			Assert.AreEqual(7, list[1].Length);
			Assert.AreEqual(7, list[1].Offset);

			Assert.AreEqual("rejoiced,when", list[2].Value);
			Assert.AreEqual(13, list[2].Length);
			Assert.AreEqual(15, list[2].Offset);

			Assert.AreEqual("black-bird's", list[3].Value);
			Assert.AreEqual(12, list[3].Length);
			Assert.AreEqual(29, list[3].Offset);

			Assert.AreEqual("fiance\u0301", list[4].Value);
			Assert.AreEqual(7, list[4].Length);
			Assert.AreEqual(42, list[4].Offset);

			Assert.AreEqual("flew", list[5].Value);
			Assert.AreEqual(4, list[5].Length);
			Assert.AreEqual(51, list[5].Offset);
		}
	}
}