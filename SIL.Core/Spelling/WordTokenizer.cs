using System;
using System.Collections.Generic;

namespace SIL.Spelling
{
	public class WordTokenizer
	{
		public class Token
		{
			private readonly string _value;
			private readonly int _offset;
			private readonly int _length;

			internal Token(string value, int offset, int length)
			{
				_value = value;
				_offset = offset;
				_length = length;
			}

			public string Value
			{
				get { return _value; }
			}

			public int Offset
			{
				get { return _offset; }
			}

			public int Length
			{
				get { return _length; }
			}
		}

		private static bool IsWhiteSpaceCharacter(char c)
		{
			// zwsp not considered whitespace by IsWhiteSpace
			// http://blogs.msdn.com/michkap/archive/2007/01/07/1430714.aspx
			return c == 0x200b || char.IsWhiteSpace(c);
		}

		public static IEnumerable<Token> TokenizeText(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException();
			}

			int i = 0;
			while (i != text.Length)
			{
				//word starts at first non-whitespace, non-punctuation character
				//... move forward while punctuation or space to get to start of word
				while (i != text.Length
					   && (IsWhiteSpaceCharacter(text[i])
						   || char.IsPunctuation(text, i)))
				{
					++i;
				}

				int wordStart = i;

				//word ends at last non-punctuation character before whitespace or end of stream
				//... inside word move forward while not space
				while (i != text.Length
					   && !IsWhiteSpaceCharacter(text[i]))
				{
					++i;
				}

				int wordEnd = i;

				//... backup while previous character is punctuation
				while (wordEnd > wordStart && char.IsPunctuation(text, wordEnd - 1))
				{
					--wordEnd;
				}

				if (wordEnd > wordStart)
				{
					int length = wordEnd - wordStart;
					yield return new Token(text.Substring(wordStart, length), wordStart, length);
				}
			}
		}
	}
}