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
		/// <param name="scanner">scanner</param>
		/// <returns>the match</returns>
		protected override ParserMatch ParseMain(Scanners.IScanner scanner)
		{
			long startOffset = scanner.Offset;

			ParserMatch m1 = FirstParser.Parse(scanner);
			long offset1 = scanner.Offset;

			scanner.Seek(startOffset);

			ParserMatch m2 = SecondParser.Parse(scanner);

			if (m1.Success && !m2.Success)
			{
				scanner.Seek(offset1);
				return m1;
			}
			if (m2.Success && !m1.Success)
			{
				return m2;
			}

			scanner.Seek(startOffset);
			return ParserMatch.CreateFailureMatch(scanner);
		}

	}

}
