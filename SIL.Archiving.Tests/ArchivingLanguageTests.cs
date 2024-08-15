using NUnit.Framework;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	public class ArchivingLanguageTests
	{
		[Test]
		public void Compare_SameCodeAndLanguageName_ReturnsZero()
		{
			Assert.That(ArchivingLanguage.Compare(new ArchivingLanguage("eng", "English"),
				new ArchivingLanguage("ENG", "English")), Is.EqualTo(0));
		}

		[TestCase("eng", "English", "frg", "Froggish", ExpectedResult = false)]
		[TestCase("zzz", "Zyzzyvian", "zzz", "Scrabbilian", ExpectedResult = true)]
		[TestCase("som", "Some Language", "unk", "Some Language", ExpectedResult = false)]
		public bool Compare_DifferentCodeOrLanguageName_ReturnsNonZero(string code1, string name1,
			string code2, string name2)
		{
			var val = ArchivingLanguage.Compare(new ArchivingLanguage(code1, name1),
				new ArchivingLanguage(code2, name2));
			Assert.That(val, Is.Not.EqualTo(0));
			return val > 0;
		}
	}
}
