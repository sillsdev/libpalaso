using System.Linq;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class EthnologueLookupTests
	{
		private EthnologueLookup _ethnologue;

		[SetUp]
		public void Setup()
		{
			_ethnologue = new EthnologueLookup();
		}

		[Test]
		public void SuggestLanguages_EmptyString_ReturnsNothing()
		{
			Assert.AreEqual(0,_ethnologue.SuggestLanguages("").Count());
		}

		[Test]
		public void SuggestLanguages_Asterisk_ReturnsNothing()
		{
			Assert.Greater(_ethnologue.SuggestLanguages("*").Count(), 1000);
		}

		[Test]
		public void SuggestLanguages_LargeMispelling_StillFinds()
		{
			Assert.AreEqual(_ethnologue.SuggestLanguages("angrish").Count(), 1);
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
			Assert.AreEqual("Thai",_ethnologue.SuggestLanguages("thai").First().Names[0]);
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
			var languageInfo = _ethnologue.SuggestLanguages("thai").First();
			Assert.AreEqual("th", languageInfo.Code);
			Assert.AreEqual("Cambodia, Thailand", languageInfo.Country);
		}

		/// <summary>
		/// test that we're delivering 2 letter codes when available
		/// </summary>
		[Test]
		public void SuggestLanguages_Thai_CodeIsJustTwoLetters()
		{
			var languageInfo = _ethnologue.SuggestLanguages("thai").First();
			Assert.AreEqual("th", languageInfo.Code);
		}

		/// <summary>
		/// languages are entered once for every country, so we need to be careful not to list "English, English, English" in the names
		/// </summary>
		[Test]
		public void SuggestLanguages_English_EnglishNotInTheAlternativeNames()
		{
			//messed up case is intentional
			Assert.AreEqual(1,_ethnologue.SuggestLanguages("english").First().Names.Where(s=>s=="English").Count());
		}


		[Test]
		public void SuggestLanguages_GivenUnambigous3LetterCode_ReturnsLanguage()
		{
			//messed up case is intentional
			Assert.True(_ethnologue.SuggestLanguages("eTR").First().Names.First().Contains("Edolo"));
		}

		[Test]
		public void SuggestLanguages_GivenPNGLanguage_ReturnsPNGCountryName()
		{
			//messed up case is intentional
			Assert.AreEqual("Papua New Guinea",_ethnologue.SuggestLanguages("eTR").First().Country);
		}

		[Test]
		public void SuggestLanguages_StartOfName_ReturnsManyLanguages()
		{
			//messed up case is intentional
			Assert.Greater(1000, _ethnologue.SuggestLanguages("eastern").Count());
		}

		[Test]
		public void SuggestLanguages_MacroLanguageNotFound()
		{
			var languages = _ethnologue.SuggestLanguages("zza");
			Assert.False(languages.Any(l => l.Code == "zza"));
			var arabic = _ethnologue.SuggestLanguages("ar");
			Assert.False(arabic.Any(l => l.Code == "ar"));
		}

		[Test]
		public void SuggestLanguages_NorwegianFindsNynorskAndBokmal()
		{
			var languages = _ethnologue.SuggestLanguages("norwegian");
			Assert.True(languages.Any(l => l.Code == "nb"));
			Assert.True(languages.Any(l => l.Code == "nn"));
		}

		[Test]
		public void SuggestLanguages_3LetterCode_ResultIncludesAlternateLanguageNames()
		{
			var languages = _ethnologue.SuggestLanguages("ana");
			Assert.True(languages.Any(l => l.Code == "ana"));
			Assert.True(languages.Any(l => l.Names.Contains("Aguanunga")));
			Assert.True(languages.Any(l => l.Names.Contains("Andaki")));
			Assert.True(languages.Any(l => l.Names.Contains("Churuba")));
		}
		[Test]
		public void SuggestLanguages_NonPrimary_ResultIncludesAlternateLanguageNames()
		{
			//messed up case and extra spaces are is intentional
			var languages = _ethnologue.SuggestLanguages("  ChuRUba  ");
			Assert.True(languages.Any(l => l.Code == "ana"));
			Assert.True(languages.Any(l => l.Names.Contains("Aguanunga")));
			Assert.True(languages.Any(l => l.Names.Contains("Andaki")));
			Assert.True(languages.Any(l => l.Names.Contains("Churuba")));
		}

		[Test]
		// Akan is a macrolanguage so won't be found
		public void SuggestLanguages_Akan_DoesnotCrash()
		{
			var languages = _ethnologue.SuggestLanguages("a");
			Assert.False(languages.Any(l => l.Code == "ak"));
			Assert.True(languages.Any(l => l.Code == "akq"));
			Assert.False(languages.Any(l => l.Names.Contains("Akuapem")));
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.False(languages.Any(l => l.Names.Contains("Akan")));
			Assert.False(languages.Any(l => l.Names.Contains("Fanti")));
			languages = _ethnologue.SuggestLanguages("ak");
			Assert.False(languages.Any(l => l.Code == "ak"));
			Assert.True(languages.Any(l => l.Code == "akq"));
			Assert.False(languages.Any(l => l.Names.Contains("Asante")));
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.False(languages.Any(l => l.Names.Contains("Akan")));
			Assert.False(languages.Any(l => l.Names.Contains("Fanti")));
		}


		[Test]
		public void SuggestLanguages_LanguageHasPejorativeAlternativeNames_FilteredOut()
		{
			var languages = _ethnologue.SuggestLanguages("Degexit’an").ToArray();
			Assert.AreEqual("ing", languages[0].Code);
			Assert.True(languages.Any(l => l.Names.Contains("Degexit'an")));
			Assert.True(languages.Any(l => l.Names.Contains("Deg Xinag")));
			Assert.True(languages.Any(l => l.Names.Contains("Deg Xit'an")));
			Assert.False(languages.Any(l => l.Names.Contains("Ingalik")));
			Assert.False(languages.Any(l => l.Names.Contains("Ingalit")));
			Assert.AreEqual(3, languages[0].Names.Count, "2 of the 5 names are pejorative and should not be listed");
		}

		/// <summary> 
		/// This is a result of finding that some of the alternative names, in Nov 2016, were *not* marked as pejorative but actually were. 
		/// These may be fixed in the Ethnologue over time, but it was requested that we just remove all alternative names for now. 
		/// </summary> 
		[Test]
		public void SuggestLanguages_LanguageIsInEthiopia_ShowOnlyOfficialNames()
		{
			var languages = _ethnologue.SuggestLanguages("Wolaytta").ToArray();
			Assert.True(languages.Any(l => l.Names.Contains("Wolaytta")));
			Assert.AreEqual(1, languages[0].Names.Count, "Should only list a single name for Ethiopian languages.");
		}

		/// <summary> 
		/// We have been asked to temporarily suppress these three codes for Ethiopia, until the Ethologue is changed. 
		/// Oromo is a macrolanguage so it won't be found
		/// </summary> 
		[Test]
		public void SuggestLanguages_LanguageIsOromo_DoNotShowRelatedLanguages()
		{
			var languages = _ethnologue.SuggestLanguages("Oromo").ToArray();
			Assert.False(languages.All(l => l.DesiredName == "Oromo"));
			Assert.False(languages.All(l => l.Code.StartsWith("gax")), "suppress gax code");
			Assert.False(languages.All(l => l.Code.StartsWith("gaz")), "suppress gaz code");
			Assert.False(languages.All(l => l.Code.StartsWith("hae")), "suppress hae code");
			Assert.False(languages.All(l => l.Code.StartsWith("om")), "We should be suppressing gax, hae, gaz");
		}
	}
}
