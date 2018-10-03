using NUnit.Framework;
using SIL.Archiving.IMDI.Lists;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	public class LanguageListTests
	{
		[Test]
		public void FindByISO3CodeTest()
		{
			var result = LanguageList.FindByISO3Code("fra");
			Assert.AreEqual("French", result.EnglishName);
			Assert.AreEqual("fra", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:fra", result.Id);

			result = LanguageList.FindByISO3Code("tru");
			Assert.AreEqual("Turoyo", result.EnglishName);
			Assert.AreEqual("tru", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:tru", result.Id);
		}

		[Test]
		public void FindByISO3Code_CodeNotFoundTest()
		{
			// This works until qqq is assigned as an ISO639-3 language code, which
			// should never happen.
			var result = LanguageList.FindByISO3Code("qqq");
			Assert.IsNullOrEmpty(result.EnglishName);
			Assert.AreEqual("qqq", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:qqq", result.Id);
		}

		[Test]
		public void FindByEnglishNameTest()
		{
			var result = LanguageList.FindByEnglishName("French");
			Assert.AreEqual("French", result.EnglishName);
			Assert.AreEqual("fra", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:fra", result.Id);

			result = LanguageList.FindByEnglishName("français");
			Assert.AreEqual("français", result.EnglishName);
			Assert.AreEqual("fra", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:fra", result.Id);

			result = LanguageList.FindByEnglishName("Turoyo");
			Assert.AreEqual("Turoyo", result.EnglishName);
			Assert.AreEqual("tru", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:tru", result.Id);

		}

		[Test]
		public void FindByEnglishName_NameNotFoundTest()
		{
			var result = LanguageList.FindByEnglishName("Fake Language");
			Assert.AreEqual("Fake Language", result.EnglishName);
			Assert.AreEqual("und", result.Iso3Code);
			Assert.AreEqual("Fake Language", result.OtherName);
			Assert.AreEqual("ISO639-3:und", result.Id);
		}
	}
}
