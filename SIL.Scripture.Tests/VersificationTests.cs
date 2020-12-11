using System.IO;
using NUnit.Framework;

namespace SIL.Scripture.Tests
{
	[TestFixture]
	public class VersificationTests
	{
		[Test]
		public void Load_FromTextReaderWithValidContents_GetsAdHocScrVers()
		{
			var src = "# Versification  \"Test\"\r\n" +
				"# Version=1.9\r\n" +
				"GEN 1:31 2:25 3:24 4:26 5:32 6:22 7:24 8:22 9:29 10:32 11:32 12:20 13:18 14:24 15:21 16:16 17:27 18:33 19:38 20:18 21:34 22:24 23:20 24:67\r\n" +
				"MRK 1:45 2:28 3:35 4:41 5:44 6:56\r\n" +
				"MRK 5:44 = MRK 6:1";
			using (var reader = new StringReader(src))
			{
				var versification = Versification.Table.Implementation.Load(reader, "not a file");
				Assert.IsNotNull(versification);
				Assert.AreEqual(41, versification.GetLastBook());
				Assert.AreEqual(24, versification.GetLastChapter(1));
				Assert.AreEqual(6, versification.GetLastChapter(41));
				Assert.AreEqual(28, versification.GetLastVerse(41, 2));
				var reference = new VerseRef(41005044, versification);
				reference.ChangeVersification(ScrVers.Original);
				Assert.AreEqual(041006001, reference.BBBCCCVVV);
			}
		}

		[TestCase(null)]
		[TestCase("")]
		public void Load_FromTextReaderWithoutNameOrFallback_Throws(string fallback)
		{
			var src = "GEN 1:31 \r\n" +
				"MRK 1:45";
			using (var reader = new StringReader(src))
			{
				var exception = Assert.Throws<InvalidVersificationLineException>(() => Versification.Table.Implementation.Load(reader, "not a file", fallback));
				Assert.AreEqual(VersificationLoadErrorType.MissingName, exception.Type);
			}
		}

		[Test]
		public void Load_FromTextReaderWithoutBadDataLine_Throws()
		{
			var src = "GEN 1:31 MRK 1:-8MAT 5:44 = FFF6,1";
			using (var reader = new StringReader(src))
			{
				var exception = Assert.Throws<InvalidVersificationLineException>(() => Versification.Table.Implementation.Load(reader, "not a file", "bogosity"));
				Assert.AreEqual(VersificationLoadErrorType.InvalidSyntax, exception.Type);
			}
		}
	}
}
