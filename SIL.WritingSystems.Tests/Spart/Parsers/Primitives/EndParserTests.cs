using NUnit.Framework;
using Spart.Parsers;
using Spart.Scanners;

namespace SIL.WritingSystems.Tests.Spart.Parsers.Primitives
{
	[TestFixture]
	public class EndParserTests
	{
		private StringScanner scanner;
		[SetUp]
		public void Setup()
		{
			scanner = new StringScanner("abc");
		}

		[Test]
		public void NotAtEnd_Fails()
		{
			Assert.IsFalse(Prims.End.Parse(scanner).Success);
		}

		[Test]
		public void AtEnd_Succeeds()
		{
			while(!scanner.AtEnd)
			{
				scanner.Read();
			}
			ParserMatch match = Prims.End.Parse(this.scanner);
			Assert.IsTrue(match.Success);
		}

		[Test]
		public void Match_Position_AtEnd()
		{
			while (!scanner.AtEnd)
			{
				scanner.Read();
			}
			ParserMatch match = Prims.End.Parse(this.scanner);
			Assert.AreEqual(scanner.Offset,match.Offset);
		}

		[Test]
		public void Match_Length_0()
		{
			while (!scanner.AtEnd)
			{
				scanner.Read();
			}
			ParserMatch match = Prims.End.Parse(this.scanner);
			Assert.AreEqual(0, match.Length);
		}

	}

}