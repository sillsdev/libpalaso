using System;

namespace Spart
{
	public class ParserErrorException : SystemException
	{
		private ParserError _parserError;

		public ParserErrorException(ParserError parserError)
		{
			ParserError = parserError;
		}

		public override string Message
		{
			get { return ParserError.ToString(); }
		}

		public ParserError ParserError
		{
			get { return _parserError; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException();
				}
				_parserError = value;
			}
		}
	}
}