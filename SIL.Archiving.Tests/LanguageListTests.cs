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
		}

		[Test]
		public void FindByISO3Code_CodeNotFoundTest()
		{
			var result = LanguageList.FindByISO3Code("tru");
			Assert.IsNullOrEmpty(result.EnglishName);
			Assert.AreEqual("tru", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:tru", result.Id);
		}

		[Test]
		public void FindByEnglishNameTest()
		{
			var result = LanguageList.FindByEnglishName("French");
			Assert.AreEqual("French", result.EnglishName);
			Assert.AreEqual("fra", result.Iso3Code);
			Assert.IsNullOrEmpty(result.OtherName);
			Assert.AreEqual("ISO639-3:fra", result.Id);
		}

		[Test]
		public void FindByEnglishName_NameNotFoundTest()
		{
			var result = LanguageList.FindByEnglishName("Turoyo");
			Assert.AreEqual("Turoyo", result.EnglishName);
			Assert.AreEqual("und", result.Iso3Code);
			Assert.AreEqual("Turoyo", result.OtherName);
			Assert.AreEqual("ISO639-3:und", result.Id);
		}
	}
}
