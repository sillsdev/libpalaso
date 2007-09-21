using System;
using System.IO;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Spart.Parsers.NonTerminal
{
	/// <summary>
	/// An AssertiveParser is a parser holder that throws an exception
	///      in response to a parsing failure.
	/// </summary>
	public class AssertiveParser: NonTerminalParser
	{
		private Parser _parser;
		private string _message;

		public AssertiveParser(string id, string message) :this(new NothingParser(), id, message){}

		public AssertiveParser(Parser parser, string id, string message)
		{
			if (parser == null)
			{
				throw new ArgumentNullException("parser");
			}

			if (id == null)
			{
			  throw new ArgumentNullException("id");
			}

			if (id == string.Empty)
			{
				throw new ArgumentException("id cannot be empty");
			}
			_parser = parser;
			Message = message;
			ID = id;
		  }


		/// <summary>
		/// Wrapped parser
		/// </summary>
		public Parser Parser
		{
			get
			{
				return _parser;
			}
			set
			{
				if (value == null && !(_parser is NothingParser))
				{
					_parser = new NothingParser();
				}
				else
				{
					if (_parser != value)
					{
						_parser = value;
					}
				}
			}
		}

		public string Message
		{
			get
			{
				return this._message;
			}
			set
			{
			  if (value == null)
			  {
				throw new ArgumentNullException();
			  }
			  if (value == string.Empty)
			  {
				throw new ArgumentException("Message cannot be empty");
			  }
			  this._message = value;
			}
		}

		protected override ParserMatch ParseMain(IScanner scanner)
		{
			if(scanner == null)
			{
				throw new ArgumentNullException();
			}
			ParserMatch m = Parser.Parse(scanner);
			if(m.Success)
			{
				return m;
			}

			throw new ParserErrorException(new ParserError(m, ID, Message));
		}
	}
}
