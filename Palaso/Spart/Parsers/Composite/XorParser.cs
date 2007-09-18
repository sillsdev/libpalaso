namespace Spart.Parsers.Composite
{
	/// <summary>
	/// Recognizes anything which is found in the first parser or the second, but not both
	/// </summary>
	public class XorParser : BinaryTerminalParser
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="left">match left</param>
		/// <param name="right">don't match right</param>
		public XorParser(Parser left, Parser right)
			: base(left, right)
		{
		}

		/// <summary>
		/// Inner parse method
		/// match (first but not second) or (second but not first)
		/// </summary>
		/// <param name="scan">scanner</param>
		/// <returns>the match</returns>
		protected override ParserMatch ParseMain(Scanners.IScanner scan)
		{
			long startOffset = scan.Offset;

			ParserMatch m1 = FirstParser.Parse(scan);
			long offset1 = scan.Offset;

			scan.Seek(startOffset);

			ParserMatch m2 = SecondParser.Parse(scan);

			if (m1.Success && !m2.Success)
			{
				scan.Seek(offset1);
				return m1;
			}
			if (m2.Success && !m1.Success)
			{
				return m2;
			}


			scan.Seek(startOffset);
			return scan.NoMatch;
		}

	}

}
