using NUnit.Framework;
using SIL.Archiving.IMDI.Lists;
using SIL.TestUtilities;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[OfflineSldr]
	[Category("Archiving")]
	public class LanguageListTests
	{
		[TestCase("fra", "French")]
		[TestCase("tru", "Turoyo")]
		[TestCase("eng", "English")]
		public void FindByISO3Code_ExistingLanguageWithNoOtherName_GetsExpectedNameCodeAndId(string iso3Code, string expectedEnglishName)
		{
			var result = LanguageList.FindByISO3Code(iso3Code);
			Assert.AreEqual(expectedEnglishName, result.EnglishName);
			Assert.AreEqual(iso3Code, result.Iso3Code);
			Assert.That(result.OtherName, Is.Null.Or.Empty);
			Assert.AreEqual($"ISO639-3:{iso3Code}", result.Id);
		}

		[Test]
		public void FindByISO3Code_NoneExistentCode_ResultHasIsoCodeButNoName()
		{
			// This works until qqq is assigned as an ISO639-3 language code, which
			// should never happen.
			var result = LanguageList.FindByISO3Code("qqq");
			Assert.That(result.EnglishName, Is.Null.Or.Empty);
			Assert.AreEqual("qqq", result.Iso3Code);
			Assert.That(result.OtherName, Is.Null.Or.Empty);
			Assert.AreEqual("ISO639-3:qqq", result.Id);
		}

		[TestCase("French", "fra")]
		[TestCase("français", "fra")] // This works because even though "French" is the default (English) name associated with "fra", the Ethnologue data also contains "français"
		[TestCase("Turoyo", "tru")]
		[TestCase("English", "eng")]
		// [TestCase("Eng", "gss", "Greek Sign Language")] // because these tests use the offline Sldr and there is an outdated langtags.json in our resources, "Eng" does not resolve to gss.
		public void FindByEnglishName_ExistingLanguage_GetsExpectedNameCodeAndId(string name, string expectedIso3Code, string defaultName = null)
		{
			var result = LanguageList.FindByEnglishName(name);
			Assert.AreEqual(defaultName ?? name, result.EnglishName);
			Assert.AreEqual(expectedIso3Code, result.Iso3Code);
			Assert.That(result.OtherName, Is.Null.Or.Empty);
			Assert.AreEqual($"ISO639-3:{expectedIso3Code}", result.Id);
		}

		[Test]
		public void FindByEnglishName_NonexistentLanguage_ResultHasGivenLanguageNameAndUndeterminedIsoCode()
		{
			var result = LanguageList.FindByEnglishName("Fake Language");
			Assert.AreEqual("Fake Language", result.EnglishName);
			Assert.AreEqual("und", result.Iso3Code);
			Assert.AreEqual("Fake Language", result.OtherName);
			Assert.AreEqual("ISO639-3:und", result.Id);
		}
	}
}
