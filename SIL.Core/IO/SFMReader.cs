using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace SIL.IO
{
	public class SFMReader
	{
		#region ParseMode enum

		public enum ParseMode
		{
			Default,
			Shoebox,
			Usfm
		}

		#endregion

		private readonly TextReader buffer;

		private ParseMode _parseMode;
		private State _parseState = State.Init;
		private string _currentTag = string.Empty;
		private int _offset = 0;

		/// <summary>
		/// Construct a new SFMReader with filename
		/// </summary>
		/// <param name="fname"></param>
		public SFMReader(string fname)
		{
			buffer = new StreamReader(fname);
		}

		/// <summary>
		/// Construct a new SFMReader with stream
		/// </summary>
		/// <param name="stream"></param>
		public SFMReader(Stream stream)
		{
			buffer = new StreamReader(stream);
		}

		public SFMReader(TextReader reader)
		{
			buffer = reader;
		}

		public ParseMode Mode
		{
			get { return _parseMode; }
			set { _parseMode = value; }
		}

		public int Offset
		{
			get { return _offset; }
		}

		/// <summary>
		/// Read next tag and return the name only (exclude backslash
		/// and space).
		/// </summary>
		/// <returns>next tag name</returns>
		public string ReadNextTag()
		{
			switch (_parseState)
			{
				case State.Init:
					ReadInitialText();
					break;
				case State.Text:
					ReadNextText();
					break;
			}
			if (_parseState == State.Finished)
			{
				return null;
			}
			Debug.Assert(_parseState == State.Tag);

			int c = buffer.Read(); // advance input stream over the initial \
			Debug.Assert(c == '\\' || c == -1);
			_offset++;

			return AlterStateAndReturnNextTag();
		}

		private string AlterStateAndReturnNextTag()
		{
			bool hasReadNextChar = false;
			_currentTag = ProcessNextTokenAccordingToMode(ref hasReadNextChar);

			if (_currentTag == null)
			{
				_parseState = State.Finished;
				return null;
			}
			if (buffer.Peek() != '\\' && !hasReadNextChar)
			{
				int c = buffer.Read();
				if(c != -1)
				{
					_offset++;
				}
				Debug.Assert(c == -1 || char.IsWhiteSpace((char) c));
			}
			_parseState = State.Text;
			return _currentTag;
		}

		private string ProcessNextTokenAccordingToMode(ref bool hasReadNextChar)
		{
			string tag;
			if (Mode == ParseMode.Usfm)
			{
				tag = GetNextToken(delegate(char ch) { return Char.IsWhiteSpace(ch) || ch == '\\' || ch == '*'; });
				if (buffer.Peek() == '*')
				{
					tag += (char) buffer.Read();
					_offset++;
					hasReadNextChar = true;
				}
			}
			else
			{
				tag = GetNextToken(delegate(char ch) { return Char.IsWhiteSpace(ch) || ch == '\\'; });
			}
			return tag;
		}

		private string GetNextToken(Predicate<char> isTokenTerminator)
		{
			StringBuilder token = new StringBuilder();
			for (;;)
			{
				int peekedChar = buffer.Peek();
				if (peekedChar == -1) //end of stream
				{
					if (token.Length == 0)
						return null;
					break;
				}

				if (isTokenTerminator((char) peekedChar))
					break;

				char read = (char)peekedChar;
				buffer.Read();
				_offset++;
				if (read == '\n' || read == '\r')
				{
					if (read == '\r' && ((char)buffer.Peek()) == '\n')
					{
						buffer.Read(); // eat it
						_offset++;
					}
				}
				token.Append(read);
			}
			return token.ToString();
		}

		/// <summary>
		/// Read next text block from stream
		/// </summary>
		/// <returns>Next text</returns>
		public string ReadNextText()
		{
			if (_parseState == State.Init)
			{
				ReadInitialText();
			}
			if (_parseState == State.Tag)
			{
				ReadNextTag();
			}
			return ReadText();
		}

		public string ReadInitialText()
		{
			if (_parseState != State.Init)
				throw new InvalidOperationException("ReadInitialText must be called before ReadNextText or ReadNextTag");

			return ReadText() ?? "";
		}

		private string ReadText()
		{
			if (_parseState == State.Finished)
				return null;
			string text = ReadTextAccordingToMode();
			if (text == null)
			{
				_parseState = State.Finished;
				return "";
			}
			_parseState = State.Tag;
			return text;
		}

		private string ReadTextAccordingToMode()
		{
			string text = string.Empty;
			do
			{
				string token = GetNextToken(delegate(char c) { return c == '\\'; });
				text += token;
				if (Mode == ParseMode.Shoebox)
				{
					if (token == null)
						break;
					if (text.Length > 0 && text[text.Length - 1] != '\n' && buffer.Peek() != -1)
					{
						text += (char) buffer.Read();
						_offset++;
					}
					else break;
				}
			} while (Mode == ParseMode.Shoebox);
			return text;
		}

		#region Nested type: State

		private enum State
		{
			Init,
			Tag,
			Text,
			Finished
		} ;

		#endregion
	}
}