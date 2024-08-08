using System;

namespace SIL.Archiving.IMDI.Lists
{
	public class InvalidLanguageCodeException : ArgumentException
	{
		public string Code { get; }

		public InvalidLanguageCodeException(string code, string paramName = null, Exception innerException = null) :
			base ("Invalid language code", paramName, innerException)
		{
			Code = code;
		}
	}
}
