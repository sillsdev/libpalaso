using System.Linq;
using NUnit.Framework;
using SIL.Extensions;

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
			Assert.That(thai.Names[0], Is.EqualTo("Thai"));
			Assert.That(thai.Names[1], Is.EqualTo("ไทย"));
		}

		[Test]
		[Ignore("This test is not longer valid because additional regions come from CLDR and there are many for Thailand")] // 2018-11-01
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
			var selection = lookup.SuggestLanguages("thai");
			LanguageInfo languageInfo = selection.First();
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
			var search = lookup.SuggestLanguages("english");
			Assert.That(search.First().Names.Count(s => s == "English"), Is.EqualTo(1));
		}

		[TestCase("en", "United States")] // a typical result
		[TestCase("ro", "Romania")] // even more typical (and different from langInfo.Countries.First()).
		[TestCase("zrp", "France")] // a three-letter code that has a region
		[TestCase("xak", "Venezuela")] // two special cases, the countries currently without regions and with >1 country
		[TestCase("itd", "Indonesia")]
		[TestCase("fuv-Arab", "Nigeria")] // language code with script with country
		[TestCase("zh-Hans", "China")] // TODO want an example language code with script without country
		[TestCase("qaa", "")] // unknown language, no country
		[TestCase("bua", "Russian Federation")] // no region, but does have a unique country
		public void FindsCorrectPrimaryCountry(string code, string primaryCountry)
		{
			var lookup = new LanguageLookup();
			var lang = lookup.GetLanguageFromCode(code);
			Assert.That(lang.PrimaryCountry, Is.EqualTo(primaryCountry));
		}

		/// <summary>
		/// JT: At the time I wrote these tests, only the indicated two languages had more than one
		/// country and lacked a region specification to disambigute the primary country.
		/// This test is designed to catch a change in that situation when the language data
		/// tables are updated.
		///
		/// DG: LanguageData.exe will say which languages don't have a unique primary country
		/// see LanguageDataIndex()
		/// </summary>
		[Test]
		public void AllExpectedLanguagesHaveUniquePrimaryCountries()
		{
			var languagesWithoutRegions = new LanguageLookup().LanguagesWithoutRegions();
			var languagesWithAmbiguousPrimaryCountry =
				languagesWithoutRegions.Where(l => l.Countries.Count() > 1);
			Assert.IsEmpty(languagesWithAmbiguousPrimaryCountry);
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
		// Akan is a macrolanguage so make sure we know that
		public void SuggestLanguages_Akan_DoesnotCrash()
		{
			var lookup = new LanguageLookup();
			LanguageInfo[] languages = lookup.SuggestLanguages("a").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "ak"));
			Assert.True(languages.Any(l => l.LanguageTag == "akq"));
			//Assert.True(languages.Any(l => l.Names.Contains("Akuapem"))); // 2018-10-26 Dialect name so not found any more
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.True(languages.Any(l => l.Names.Contains("Akan")));
			// Assert.True(languages.Any(l => l.Names.Contains("Fanti"))); // 2018-10-26 Dialect name so not found any more
			languages = lookup.SuggestLanguages("ak").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "ak"));
			Assert.True(languages.Any(l => l.LanguageTag == "akq"));
			//Assert.True(languages.Any(l => l.Names.Contains("Asante"))); // 2018-10-26 Dialect name so not found any more
			Assert.True(languages.Any(l => l.Names.Contains("Ak")));
			Assert.True(languages.Any(l => l.Names.Contains("Akan")));
			//Assert.True(languages.Any(l => l.Names.Contains("Fanti"))); // 2018-10-26 Dialect name so not found any more
		}

		[Test]
		public void SuggestLanguages_ByCountry_Matches()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("United States");
			Assert.That(languages, Has.Member(lookup.GetLanguageFromCode("en")));
			Assert.That(languages, Has.Member(lookup.GetLanguageFromCode("es")));

			languages = lookup.SuggestLanguages("Fran"); // prefix of 'France'
			Assert.That(languages, Has.Member(lookup.GetLanguageFromCode("fr")));

			languages = lookup.SuggestLanguages("Russian");
			Assert.That(languages, Has.Member(lookup.GetLanguageFromCode("bua"))); // macro-language in Russian Federation

			languages = lookup.SuggestLanguages("?");
			Assert.That(languages, Has.No.Member(lookup.GetLanguageFromCode("qaa")));
			Assert.That(languages, Has.No.Member(lookup.GetLanguageFromCode("mn-Mong")));
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
			Assert.AreEqual(3, languages[0].Names.Count, "2 of the 5 names are pejorative and should not be listed");
		}

		/// <summary>
		/// This is a result of finding that some of the alternative names, in Nov 2016, were *not* marked as pejorative but actually were.
		/// These may be fixed in the Ethnologue over time, but it was requested that we just remove all alternative names for now.
		/// </summary>
		[Test]
		public void SuggestLanguages_LanguageIsInEthiopia_ShowOnlyOfficialNames()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("Wolaytta").ToArray();
			Assert.True(languages.Any(l => l.Names.Contains("ወላይታቱ")));
			Assert.True(languages.Any(l => l.Names.Contains("Wolaytta")));
			Assert.AreEqual(2, languages[0].Names.Count, "Should list only the first name in the IANA subtag registry for Ethiopian languages, plus local name.");
			languages = lookup.SuggestLanguages("Qimant").ToArray();
			Assert.True(languages.Any(l => l.Names.Contains("Qimant")));
			Assert.AreEqual(1, languages[0].Names.Count, "Should only list the first name in the IANA subtag registry for Ethiopian languages.");
		}

		/// <summary>
		/// The Ethnologue online reports these as pejoratives, but they were still not marked as pejorative in the data. WSTech is suppressing
		/// them but we still want to verify none of them are appearing.
		/// </summary>
		[Test]
		public void SuggestLanguages_LanguageIsOromo_HasNoPejorativeLanguageNames()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("Oromo").ToArray();
			// Verify that no related languages for "Oromo" list known pejorative names
			Assert.False(languages.Any(l => l.Names.Contains("Gall")));
			Assert.False(languages.Any(l => l.Names.Contains("ottu")));
			Assert.False(languages.Any(l => l.Names.Contains("Qotu")));
			// Specifically check three codes that historically returned pejorative names
			languages = lookup.SuggestLanguages("gax").Union(lookup.SuggestLanguages("gaz")).ToArray();
			// “Galla” (pej.), “Galligna” (pej.), “Gallinya” (pej.)
			Assert.False(languages.Any(l => l.Names.Contains("Gall")));
			languages = lookup.SuggestLanguages("hae").ToArray();
			// “Kwottu” (pej.), “Qottu” (pej.), “Quottu” (pej.), “Qwottu” (pej.)
			Assert.False(languages.Any(l => l.Names.Contains("ottu")));
			// “Qotu Oromo” (pej.), 
			Assert.False(languages.Any(l => l.Names.Contains("Qotu")));
		}

		/// <summary>
		/// We should not find old code for Kataang (deprecated Jan 2017)
		/// but find new ones instead
		/// </summary>
		[Test]
		public void SuggestLanguages_KataangNewCodes()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("Kataang").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "ncq"));
			Assert.True(languages.Any(l => l.LanguageTag == "sct"));
			Assert.False(languages.Any(l => l.LanguageTag == "kgd"));
		}

		/// <summary>
		/// We should not suggest macro languages unless they are marked as such so that they can be filtered out.
		/// </summary>
		[Test]
		[Ignore("Macrolanguages not used now")] // 2018-10-26
		public void SuggestLanguages_CanFilterMacroLanguages()
		{
			var lookup = new LanguageLookup();
			Assert.That(lookup.SuggestLanguages("macrolanguage").Count(), Is.EqualTo(0));
			var languages = lookup.SuggestLanguages("zza").ToArray();
			Assert.True(languages.Any(l => l.LanguageTag == "zza" && l.IsMacroLanguage));
		}

		/// <summary>
		/// We should not suggest deprecated tags for languages.
		/// </summary>
		[Test]
		public void SuggestLanguages_DoesNotSuggestDeprecatedTags()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("dzd").ToArray();
			Assert.False(languages.Any(l => l.LanguageTag == "dzd"));
			languages = lookup.SuggestLanguages("yiy").ToArray();
			Assert.False(languages.Any(l => l.LanguageTag == "yiy"));
			languages = lookup.SuggestLanguages("jeg").ToArray();
			Assert.False(languages.Any(l => l.LanguageTag == "jeg"));
		}

		/// <summary>
		/// We should not suggest language collections.
		/// </summary>
		[Test]
		public void SuggestLanguages_DoesNotSuggestLanguageCollections()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("urj").ToArray();
			Assert.False(languages.Any(l => l.LanguageTag == "urj"));
			languages = lookup.SuggestLanguages("aav").ToArray();
			Assert.False(languages.Any(l => l.LanguageTag == "aav"));
		}

		/// <summary>
		/// We should now be able to search for 3 letter codes e.g. nld for languages that have 2 letter codes e.g. nl
		/// </summary>
		[Test]
		public void SuggestLanguages_CanFind3LetterCodesForLanguagesWith2LetterCodes()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("nld").ToArray();
			Assert.True(languages.Any(l => l.DesiredName == "Dutch"));
		}

		/// <summary>
		/// We should now be able to find codes that are in iana registry but not Ethnologue
		/// </summary>
		[Test]
		[Ignore("Dialects are not included in the language data any more so this will not work until they are included again")] // 2018-10-26
		public void SuggestLanguages_CanFindValidTagsThatAreNotInEthnologue()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("fat").ToArray();
			Assert.True(languages.Any(l => l.DesiredName == "Fanti"));
			languages = lookup.SuggestLanguages("twi").ToArray();
			Assert.True(languages.Any(l => l.DesiredName == "Twi"));
		}

		[Test]
		public void GetLanguageFromCode_CodeNotFound_ReturnsNull()
		{
			var lookup = new LanguageLookup();
			Assert.IsNull(lookup.GetLanguageFromCode("foobar"));
		}

		[Test]
		public void GetLanguageFromCode_CodeFound_ReturnsMatchingLanguageInfo()
		{
			var lookup = new LanguageLookup();
			Assert.AreEqual("etr", lookup.GetLanguageFromCode("etr").LanguageTag);
			Assert.AreEqual("English", lookup.GetLanguageFromCode("en").DesiredName);
		}

		[Test]
		public void SuggestLanguages_HandlesApostrophe()
		{
			var lookup = new LanguageLookup();
			var languages = lookup.SuggestLanguages("K\u2019i").ToArray();
			Assert.True(languages.Any(l => l.DesiredName == "K\u2019iche\u2019"));
			languages = lookup.SuggestLanguages("K'i").ToArray();
			Assert.True(languages.Any(l => l.DesiredName == "K\u2019iche\u2019"));
		}

		/// <summary>
		/// We have a special case where we want to return all sign languages when you type "sign", or something like that.
		/// </summary>
		[Test]
		public void SuggestLanguages_SearchingForSign_ReturnsSignLanguages()
		{
			var lookup = new LanguageLookup();

			foreach (var wayToSaySign in new[] {"SiGn", "SIGNES   ", "señas", "sign language " })
			{
				var languages = lookup.SuggestLanguages(wayToSaySign).ToArray();
				var countOfPossibleSignLangsYouWouldGetWithThatSearchTerm = languages.Select(li =>
					li.Names.AsQueryable().Any(n => n.ToLowerInvariant().Contains("sign"))
				).Count();
				Assert.GreaterOrEqual(countOfPossibleSignLangsYouWouldGetWithThatSearchTerm,
					140); // was 145 in October 2018. So if it's giving us at least 140, it's probably doing the right thing
			}
		}
	}
}
