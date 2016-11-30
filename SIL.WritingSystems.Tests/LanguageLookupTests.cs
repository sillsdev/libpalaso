using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class LanguageLookupTests
	{
		[Test]
		public void SuggestLanguages_EmptyString_ReturnsNothing()
		{
			var lookup = new LanguageLookup();
			Assert.That(lookup.SuggestLanguages("").Count(), Is.EqualTo(0));
		}

		[Test]
		public void SuggestLanguages_Asterisk_ReturnsEverything()
		{
			var lookup = new LanguageLookup();
			Assert.Greater(lookup.SuggestLanguages("*").Count(), 1000);
		}

		[Test]
		public void SuggestLanguages_LargeMispelling_StillFinds()
		{
			var lookup = new LanguageLookup();
			Assert.That(lookup.SuggestLanguages("angrish").Any(), Is.True);
		}

		[Test]
		public void SuggestLanguages_Thai_ReturnsLocalizedNameAsFirstChoiceLanguage()
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
			var lookup = new LanguageLookup();
			LanguageInfo thai = lookup.SuggestLanguages("thai").First();
			Assert.That(thai.Names[0], Is.EqualTo("ภาษาไทย"));
			Assert.That(thai.Names[1], Is.EqualTo("Thai"));
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
			var lookup = new LanguageLookup();
			LanguageInfo languageInfo = lookup.SuggestLanguages("thai").First();
			Assert.That(languageInfo.LanguageTag, Is.EqualTo("th"));
			Assert.That(languageInfo.Countries, Is.EqualTo(new[] {"Cambodia", "Thailand"}));
		}

		/// <summary>
		/// test that we're delivering 2 letter codes when available
		/// </summary>
		[Test]
		public void SuggestLanguages_Thai_CodeIsJustTwoLetters()
		{
			var lookup = new LanguageLookup();
			LanguageInfo languageInfo = lookup.SuggestLanguages("thai").First();
			Assert.That(languageInfo.LanguageTag, Is.EqualTo("th"));
		}

		/// <summary>
		/// languages are entered once for every country, so we need to be careful not to list "English, English, English" in the names
		/// </summary>
		[Test]
		public void SuggestLanguages_English_EnglishNotInTheAlternativeNames()
		{
			var lookup = new LanguageLookup();
			//messed up case is intentional
			Assert.That(lookup.SuggestLanguages("english").First().Names.Count(s => s == "English"), Is.EqualTo(1));
		}


		[Test]
		public void SuggestLanguages_GivenUnambigous3LetterCode_ReturnsLanguage()
		{
			var lookup = new LanguageLookup();
			//messed up case is intentional
			Assert.That(lookup.SuggestLanguages("eTR").First().Names.First().Contains("Edolo"), Is.True);
		}

		[Test]
		public void SuggestLanguages_GivenPNGLanguage_ReturnsPNGCountryName()
		{
			var lookup = new LanguageLookup();
			//messed up case is intentional
			Assert.That(lookup.SuggestLanguages("eTR").First().Countries, Is.EqualTo(new[] {"Papua New Guinea"}));
		}

		[Test]
		public void SuggestLanguages_StartOfName_ReturnsManyLanguages()
		{
			var lookup = new LanguageLookup();
			//messed up case is intentional
			Assert.Greater(1000, lookup.SuggestLanguages("eastern").Count());
		}


		[Test]
		public void SuggestLanguages_3LetterCode_ResultIncludesAlternateLanguageNames()
		{
			var lookup = new LanguageLookup();
			LanguageInfo[] languages = lookup.SuggestLanguages("ana").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "ana"));
			Assert.True(languages.Any(l => l.Names.Contains("Aguanunga")));
			Assert.True(languages.Any(l => l.Names.Contains("Andaki")));
			Assert.True(languages.Any(l => l.Names.Contains("Churuba")));
		}
		[Test]
		public void SuggestLanguages_NonPrimary_ResultIncludesAlternateLanguageNames()
		{
			var lookup = new LanguageLookup();
			//messed up case and extra spaces are is intentional
			LanguageInfo[] languages = lookup.SuggestLanguages("  ChuRUba  ").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "ana"));
			Assert.True(languages.Any(l => l.Names.Contains("Aguanunga")));
			Assert.True(languages.Any(l => l.Names.Contains("Andaki")));
			Assert.True(languages.Any(l => l.Names.Contains("Churuba")));
		}

		[Test]
		public void SuggestLanguages_Akan_DoesnotCrash()
		{
			var lookup = new LanguageLookup();
			LanguageInfo[] languages = lookup.SuggestLanguages("a").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "ak"));
			Assert.True(languages.Any(l => l.LanguageTag == "akq"));
			Assert.True(languages.Any(l => l.Names.Contains("Akuapem")));
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.True(languages.Any(l => l.Names.Contains("Akan")));
			Assert.True(languages.Any(l => l.Names.Contains("Fanti")));
			languages = lookup.SuggestLanguages("ak").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "ak"));
			Assert.True(languages.Any(l => l.LanguageTag == "akq"));
			Assert.True(languages.Any(l => l.Names.Contains("Asante")));
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.True(languages.Any(l => l.Names.Contains("Akan")));
			Assert.True(languages.Any(l => l.Names.Contains("Fanti")));
		}

		[Test]
		public void SuggestLanguages_LanguageHasPejorativeAlternativeNames_FilteredOut()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("Degexit’an").ToArray();
			Assert.AreEqual("ing", languages[0].LanguageTag);
			Assert.True(languages.Any(l => l.Names.Contains("Degexit’an")));
			Assert.True(languages.Any(l => l.Names.Contains("Deg Xinag")));
			Assert.True(languages.Any(l => l.Names.Contains("Deg Xit’an")));
			Assert.AreEqual(3, languages[0].Names.Count, "3 of the 5 names are pejorative and should not be listed");
		}
	}
}
