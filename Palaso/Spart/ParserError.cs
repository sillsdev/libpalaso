using System;
using Spart.Parsers;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Spart
{
	public class ParserError
	{
		private string _fileName;
		private long _line;
		private long _column;

		private string _errorId;
		private string _errorText;

		public ParserError(IScanner scanner, string errorId, string errorMessage)
		{
		  if (errorId == null)
		  {
			throw new ArgumentNullException("errorId");
		  }

		  if (errorMessage == null)
		  {
			throw new ArgumentNullException("errorMessage");
		  }

		  long originalOffset = scanner.Offset;
		  long lastLineOffset = 0;
			scanner.Offset = 0;
			Parser eol = Prims.Eol;
			Parser notEol = new CharParser(delegate(Char c)
					{
						return c != '\r' && c != '\n';
					});
			_line=1; // 1 based not 0 based
			while (!scanner.AtEnd)
			{
				notEol.Parse(scanner);
				if (scanner.AtEnd)
				{
				  break;
				}
				ParserMatch match = eol.Parse(scanner);
				if(scanner.Offset > originalOffset)
				{
					break;
				}
				if (match.Success)
				{
					++_line;
					lastLineOffset = scanner.Offset;
				}
			}

			_column = originalOffset - lastLineOffset + 1; // 1 based not 0 based
			scanner.Offset = originalOffset;
			_errorText = errorMessage;
		  _errorId = errorId;
		}


		public ParserError(long line, long column, string errorId, string errorMessage)
		{
			if(line < 0)
			{
				throw new ArgumentOutOfRangeException("line",line, "Must be greater than 0");
			}
			if (column < 0)
			{
				throw new ArgumentOutOfRangeException("column", column, "Must be greater than 0");
			}

			if (errorId == null)
			{
				throw new ArgumentNullException("errorId");
			}

			if (errorMessage == null)
			{
				throw new ArgumentNullException("errorMessage");
			}

			_line = line;
			_column = column;
			_errorId = errorId;
			_errorText = errorMessage;

		}


		public ParserError(string fileName, long line, long column, string errorId, string errorMessage) : this(line, column, errorId, errorMessage)
		{
			if(fileName == null)
			{
				throw new ArgumentNullException("fileName");
			}

			_fileName = fileName;
		}

		public string FileName
		{
			get
			{
				return this._fileName ?? string.Empty;
			}
		}

		public long Line
		{
			get { return this._line; }
		}

		public long Column
		{
			get { return this._column; }
		}

		public string ErrorId
		{
			get { return this._errorId; }
		}

		public string ErrorText
		{
			get { return this._errorText; }
		}

		public override string ToString()
		{
			return string.Format("{0}({1}:{2}): {3}: {4}", FileName, Line, Column, ErrorId, ErrorText);
		}
	}
}
