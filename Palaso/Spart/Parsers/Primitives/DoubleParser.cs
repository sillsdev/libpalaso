using System;

namespace Spart.Parsers.Primitives
{
	using Spart.Scanners;

	public class DoubleParser : TerminalParser
	{
		public override ParserMatch ParseMain(IScanner scan)
		{
			long offset = scan.Offset;
			return null;
		}
	}
}
