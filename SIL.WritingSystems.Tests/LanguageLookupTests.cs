using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LanguageLookupTests
	{
		private LanguageLookup _language;

		[SetUp]
		public void Setup()
		{
			_language = new LanguageLookup();
		}

		[Test]
		public void SuggestLanguages_EmptyString_ReturnsNothing()
		{
			Assert.AreEqual(0,_language.SuggestLanguages("").Count());
		}

		[Test]
		public void SuggestLanguages_Asterisk_ReturnsEverything()
		{
			Assert.Greater(_language.SuggestLanguages("*").Count(), 1000);
		}

		[Test]
		public void SuggestLanguages_LargeMispelling_StillFinds()
		{
			Assert.That(_language.SuggestLanguages("angrish").Any(), Is.True);
		}

		[Test]
		public void SuggestLanguages_Thai_ReturnsThaiAsFirstChoiceLanguage()
		{
			/*	tha	KH 	D	Thai Koh Kong
				tha	KH 	D	Thai Norkor Raja
				tha	KH 	DA	Siam Nokor
				tha	KH 	DA	Siam Trang
				tha	KH 	L	Thai
				tha	TH 	D	Khorat Thai
				tha	TH 	DA	Korat
				tha	TH 	DA	Thaikorat
				tha	TH 	L	Thai
				tha	TH 	LA	Central Tai
				tha	TH 	LA	Siamese
				tha	TH 	LA	Standard Thai
				tha	TH 	LA	Thaiklang
			*/
			Assert.AreEqual("Thai",_language.SuggestLanguages("thai").First().Names[0]);
		}

		[Test]
		public void SuggestLanguages_Thai_TwoCountries()
		{
			/*	tha	KH 	D	Thai Koh Kong
				tha	KH 	D	Thai Norkor Raja
				tha	KH 	DA	Siam Nokor
				tha	KH 	DA	Siam Trang
				tha	KH 	L	Thai
				tha	TH 	D	Khorat Thai
				tha	TH 	DA	Korat
				tha	TH 	DA	Thaikorat
				tha	TH 	L	Thai
				tha	TH 	LA	Central Tai
				tha	TH 	LA	Siamese
				tha	TH 	LA	Standard Thai
				tha	TH 	LA	Thaiklang
			*/
			var languageInfo = _language.SuggestLanguages("thai").First();
			Assert.AreEqual("th", languageInfo.LanguageTag);
			Assert.That(languageInfo.Countries, Is.EqualTo(new[] {"Cambodia", "Thailand"}));
		}

		/// <summary>
		/// test that we're delivering 2 letter codes when available
		/// </summary>
		[Test]
		public void SuggestLanguages_Thai_CodeIsJustTwoLetters()
		{
			var languageInfo = _language.SuggestLanguages("thai").First();
			Assert.AreEqual("th", languageInfo.LanguageTag);
		}

		/// <summary>
		/// languages are entered once for every country, so we need to be careful not to list "English, English, English" in the names
		/// </summary>
		[Test]
		public void SuggestLanguages_English_EnglishNotInTheAlternativeNames()
		{
			//messed up case is intentional
			Assert.AreEqual(1,_language.SuggestLanguages("english").First().Names.Where(s => s == "English").Count());
		}


		[Test]
		public void SuggestLanguages_GivenUnambigous3LetterCode_ReturnsLanguage()
		{
			//messed up case is intentional
			Assert.True(_language.SuggestLanguages("eTR").First().Names.First().Contains("Edolo"));
		}

		[Test]
		public void SuggestLanguages_GivenPNGLanguage_ReturnsPNGCountryName()
		{
			//messed up case is intentional
			Assert.That(_language.SuggestLanguages("eTR").First().Countries, Is.EqualTo(new [] {"Papua New Guinea"}));
		}

		[Test]
		public void SuggestLanguages_StartOfName_ReturnsManyLanguages()
		{
			//messed up case is intentional
			Assert.Greater(1000, _language.SuggestLanguages("eastern").Count());
		}


		[Test]
		public void SuggestLanguages_3LetterCode_ResultIncludesAlternateLanguageNames()
		{
			var languages = _language.SuggestLanguages("ana");
			Assert.True(languages.Any(l => l.LanguageTag == "ana"));
			Assert.True(languages.Any(l => l.Names.Contains("Aguanunga")));
			Assert.True(languages.Any(l => l.Names.Contains("Andaki")));
			Assert.True(languages.Any(l => l.Names.Contains("Churuba")));
		}
		[Test]
		public void SuggestLanguages_NonPrimary_ResultIncludesAlternateLanguageNames()
		{
			//messed up case and extra spaces are is intentional
			var languages = _language.SuggestLanguages("  ChuRUba  ");
			Assert.True(languages.Any(l => l.LanguageTag == "ana"));
			Assert.True(languages.Any(l => l.Names.Contains("Aguanunga")));
			Assert.True(languages.Any(l => l.Names.Contains("Andaki")));
			Assert.True(languages.Any(l => l.Names.Contains("Churuba")));
		}

		[Test]
		public void SuggestLanguages_Akan_DoesnotCrash()
		{
			var languages = _language.SuggestLanguages("a");
			Assert.True(languages.Any(l => l.LanguageTag == "ak"));
			Assert.True(languages.Any(l => l.LanguageTag == "akq"));
			Assert.True(languages.Any(l => l.Names.Contains("Akuapem")));
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.True(languages.Any(l => l.Names.Contains("Akan")));
			Assert.True(languages.Any(l => l.Names.Contains("Fanti")));
			languages = _language.SuggestLanguages("ak");
			Assert.True(languages.Any(l => l.LanguageTag == "ak"));
			Assert.True(languages.Any(l => l.LanguageTag == "akq"));
			Assert.True(languages.Any(l => l.Names.Contains("Asante")));
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.True(languages.Any(l => l.Names.Contains("Akan")));
			Assert.True(languages.Any(l => l.Names.Contains("Fanti")));
		}
	}
}
