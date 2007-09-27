using System;
using Spart.Parsers;
using Spart.Parsers.Primitives;
using Spart.Scanners;

namespace Spart.Parsers.NonTerminal
{
	/// <summary>
	/// An AssertiveParser is a parser holder that throws an exception
	///      in response to a parsing failure.
	/// </summary>
	/// <remarks>The parsing failure is determined by the AssertDelegate which defaults
	/// to asserting on match failure.</remarks>
	public class AssertiveParser: NonTerminalParser
	{
		public delegate bool AssertDelegate(ParserMatch m);

		private Parser _parser;
		private string _message;
		private AssertDelegate _assertDelegate;

		public AssertiveParser(string id, string message) :this(new NothingParser(), id, message){}

		public AssertiveParser(Parser parser, string id, string message):
			this(AssertMatchSuccess, parser, id, message)
		{
		}

		public AssertiveParser( AssertDelegate assertDelegate, Parser parser, string id, string message)
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
			Parser = parser;
			Message = message;
			ID = id;
			Assert = assertDelegate;

		}

		static private bool AssertMatchSuccess(ParserMatch match)
		{
			return match.Success;
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

		/// <summary>
		/// validator
		/// </summary>
		public AssertDelegate Assert
		{
			get
			{
				return _assertDelegate;
			}
			set
			{
				if (value == null)
				{
					_assertDelegate = AssertMatchSuccess;
				}
				else
				{
					if (_assertDelegate != value)
					{
						_assertDelegate = value;
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
			if(Assert(m))
			{
				return m;
			}

			throw new ParserErrorException(new ParserError(m, ID, Message));
		}
	}
}
