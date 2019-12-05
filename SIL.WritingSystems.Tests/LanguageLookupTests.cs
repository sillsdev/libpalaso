using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

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
		public void GetLanguageFromCode_FindsCorrectPrimaryCountry(string code, string primaryCountry)
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
			Assert.True(languages.Take(50).All(l => l.PrimaryCountry == "United States"));
			Assert.That(languages, Has.Member(lookup.GetLanguageFromCode("es")));

			languages = lookup.SuggestLanguages("Fran"); // prefix of 'France'
			Assert.That(languages, Has.Member(lookup.GetLanguageFromCode("fr")));

			languages = lookup.SuggestLanguages("Russian");
			Assert.That(languages, Has.Member(lookup.GetLanguageFromCode("bua"))); // macro-language in Russian Federation

			languages = lookup.SuggestLanguages("?");
			Assert.That(languages, Has.No.Member(lookup.GetLanguageFromCode("qaa")));
			Assert.That(languages, Has.No.Member(lookup.GetLanguageFromCode("mn-Mong")));

			languages = lookup.SuggestLanguages("China");
			Assert.True(languages.Take(50).All(l => l.PrimaryCountry == "China"));
		}

		[Test]
		public void SuggestLanguages_ExactCountryMatch_SortsAboveFuzzyNames()
		{
			List<AllTagEntry> entries = new List<AllTagEntry>
			{
				new AllTagEntry { name = "Crimean Tatar", tag="crh-Cyrl-UA", region = "UA", regions = new List<string> {"BG", "KG", "US"}},
				new AllTagEntry { name = "English", tag = "en", region = "US"},
				new AllTagEntry {name = "French", tag = "fr", region = "FR", regions = new List<string> {"FR", "US"}} 
			};
			var lookup = new LanguageLookup(entries);
			var languages = lookup.SuggestLanguages("United States");
			Assert.That(languages.First(), Is.EqualTo(lookup.GetLanguageFromCode("en")));
		}

		/// <summary>
		/// With this data we were seeing that German sorted Greek and Georgian above German.
		/// </summary>
		[Test]
		public void SuggestLanguages_ExactIanaNameMatch_SortsAboveOther()
		{
			List<AllTagEntry> entries = new List<AllTagEntry>
			{
				new AllTagEntry
				{ iana = new List<string> { "German" }, name = "German, Standard", full="de-Latn-DE", tag="de", region = "DE",
					regions = new List<string> {"AE", "AR", "AT", "AU", "BA", "BE", "BG", "BO", "BR", "CA", "CH", "CL", "CY", "CZ", "DK", "EC", "EE", "FI", "FR", "GR", "HR", "HU", "IE", "IL", "IT", "KG", "KZ", "LI", "LT", "LU", "MT", "MZ", "NA", "NL", "NZ", "PH", "PL", "PR", "PT", "PY", "RO", "RU", "SE", "SI", "SK", "TJ", "UA", "US", "UY", "ZA"}},
				new AllTagEntry
				{ iana = new List<string> { "Georgian" }, name = "Georgian", names = new List<string>{ "Common Kartvelian", "Gorji", "Grunzinski yazyk", "Gruzin" }, tag = "ka", region = "GE",
					regions = new List<string> {"AM", "AZ", "DE", "IR", "KG", "KZ", "RU", "TJ", "TM", "TR", "UA", "UZ"}},
				new AllTagEntry
				{ iana = new List<string> { "Korean" }, name = "Korean", names = new List<string> {"Chaoxian", "Chaoxianyu", "Chaoyu", "Goryeomal", "Hangouyu", "Hanguohua", "Hanyu", "Koryomal", "Zanichi Korean"}, tag = "ko", region = "KR",
					regions = new List<string> {"AS", "AU", "AZ", "BH", "BN", "BR", "BY", "CA", "CN", "DE", "GU", "JP", "KG", "KP", "KZ", "LY", "MN", "MP", "MZ", "NZ", "PA", "PH", "PY", "RU", "SG", "SR", "TJ", "TM", "UA", "US", "UZ"}}
			};
			var lookup = new LanguageLookup(entries);
			var languages = lookup.SuggestLanguages("German");
			Assert.That(languages.First(), Is.EqualTo(lookup.GetLanguageFromCode("de")));
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
			Assert.True(languages.Any(l => l.Names.Contains("Degexit'an")));
			Assert.AreEqual(4, languages[0].Names.Count, "2 of the 6 names are pejorative and should not be listed");
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
			Assert.True(languages.Any(l => l.Names.Contains("Wolaytta"))); // Ethnologue name, and first ianna name (2019/05/02)
			Assert.True(languages.Any(l => l.Names.Contains("Wolaitta"))); // ianna second official name
			Assert.AreEqual(3, languages[0].Names.Count, "Should list only the ethnologue name, the official IANA names, and the local name.");
			languages = lookup.SuggestLanguages("Qimant").ToArray();
			Assert.True(languages.Any(l => l.Names.Contains("Qimant")));
			Assert.AreEqual(1, languages[0].Names.Count, "Should list only the ethnologue name, the official IANA names, and the local name.");
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
				var languages = lookup.SuggestLanguages(wayToSaySign);
				Assert.True(languages.Take(20).All(l => l.Names.Any(n => n.ToLowerInvariant().Contains("sign"))),
					"At least the top 20 results for each way of typing 'sign' should have 'sign' in one of the language names");
			}
		}

		[Test]
		public void EnsureDefaultTags_WorksAsExpected()
		{
			var entries = new List<AllTagEntry>
			{
				new AllTagEntry { full="abq-Cyrl-RU", iana=new List<string>{"Abaza"}, iso639_3="abq",
					name="Abaza", names=new List<string> { "Abazin", "Abazintsy", "Ahuwa", "Ashuwa" },
					region="RU", regionName="Russian Federation", regions=new List<string> { "TR" },
					tag="abq-Cyrl", tags=new List<string> { "abq", "abq-RU" }
					},
				new AllTagEntry { full="abq-Latn-TR", iana=new List<string>{"Abaza"}, iso639_3="abq",
					name="Abaza", names=new List<string> { "Abazin", "Abazintsy", "Ahuwa", "Ashuwa" },
					region="TR", regionName="Turkey", regions=new List<string> { "RU" },
					tag="abq-Latn", tags=new List<string> { "abq-TR" }
					},
				new AllTagEntry { full="aii-Cyrl-RU", iana=new List<string>{"Assyrian Neo-Aramaic"}, iso639_3="aii",
					name="Assyrian Neo-Aramaic",
					region="RU", regionName="Russian Federation",
					tag="aii-Cyrl", tags=new List<string> { "aii-RU" }
					},
				new AllTagEntry { full="aii-Syrc-IQ", iana=new List<string>{"Assyrian Neo-Aramaic"}, iso639_3="aii",
					localname="ܠܫܢܐ ܣܘܪܝܝܐ", name="Assyrian Neo-Aramaic", names=new List<string> { "Aisorski", "Assyrian", "Assyrianci", "Assyriski", "Aturaya Swadaya", "Lishana Aturaya", "Neo-Syriac", "Sooreth", "Suret", "Sureth", "Suryaya Swadaya", "Swadai", "Swadaya" },
					region="IQ", regionName="Iraq", regions=new List<string> { "AM", "AT", "AU", "AZ", "BE", "BR", "CA", "CH", "CY", "DE", "GB", "GE", "GR", "IR", "IT", "LB", "NL", "NZ", "RU", "SE", "SY", "TR", "US" },
					tag="aii-Syrc", tags=new List<string> { "aii-IQ" }
					},
				new AllTagEntry { full="aij-Hebr-IL", iana=new List<string>{"Lishanid Noshan"}, iso639_3="aij",
					name="Inter-Zab Jewish Neo-Aramaic", names=new List<string> { "Hulani", "Jbeli", "Kurdit", "Lishana Didán", "Lishanid Noshan" },
					region="IL", regionName="Israel",
					tag="aij", tags=new List<string> { "aij-Hebr", "aij-IL" }
					},
				new AllTagEntry { full="cmg-Soyo-MN", iana=new List<string>{"Classical Mongolian"}, iso639_3="cmg",
					name="Classical Mongolian",
					region="MN", regionName="Mongolia",
					tag="cmg-Soyo", tags=new List<string> { "cmg-MN" }
					},
				new AllTagEntry { full="crh-Cyrl-UA", iana=new List<string> { "Crimean Tatar", "Crimean Turkish" }, iso639_3="crh",
					name="Crimean Tatar", names=new List<string> { "Crimean", "Crimean Turkish", "Qirim", "Qirimtatar", "Qırımtatar tili", "Qırımtatarca" },
					region="UA", regionName="Ukraine", regions=new List<string> { "BG", "KG", "MD", "RO", "RU", "TR", "US", "UZ" },
					tag="crh", tags=new List<string> { "crh-Cyrl", "crh-UA" }
					},
				new AllTagEntry { full="crh-Arab-UZ", iana=new List<string> { "Crimean Tatar", "Crimean Turkish" }, iso639_3="crh",
					name="Crimean Tatar", names=new List<string> { "Crimean", "Crimean Turkish", "Qirim", "Qirimtatar", "Qırımtatar tili", "Qırımtatarca" },
					region="UZ", regionName="Uzbekistan", regions=new List<string> { "BG", "KG", "MD", "RO", "RU", "TR", "UA", "US" },
					tag="crh-Arab", tags=new List<string> { "crh-UZ" }
					},
				new AllTagEntry { full="crh-Latn-UA", iana=new List<string> { "Crimean Tatar", "Crimean Turkish" }, iso639_3="crh",
					name="Crimean Tatar", names=new List<string> { "Crimean", "Crimean Turkish", "Qirim", "Qirimtatar", "Qırımtatar tili", "Qırımtatarca" },
					region="UA", regionName="Ukraine", regions=new List<string> { "BG", "KG", "MD", "RO", "RU", "TR", "US", "UZ" },
					tag="crh-Latn",
					},
				new AllTagEntry { full="de-Latn-DE", iana=new List<string>{"German"}, iso639_3="deu",
					localname="Deutsch", name="German, Standard", names=new List<string> { "Alemán", "Alemão", "Allemand", "Deutsch", "Duits", "Däitsch", "German", "Germană", "Nemec", "Nemetskiy", "Niemiec", "Német", "Němec", "Tedesco", "Tudestg", "Tysk" },
					region="DE", regionName="Germany", regions=new List<string> { "AE", "AR", "AU", "BA", "BE", "BG", "BO", "BR", "CA", "CL", "CY", "CZ", "DK", "EC", "EE", "FI", "FR", "GR", "HR", "HU", "IE", "IL", "KG", "KZ", "LT", "MT", "MZ", "NA", "NL", "NZ", "PH", "PL", "PR", "PT", "PY", "RO", "RU", "SE", "SI", "SK", "TJ", "UA", "US", "UY", "ZA" },
					tag="de", tags=new List<string> { "de-DE", "de-Latn" },
				},
				new AllTagEntry { full="en-Latn-US", iana=new List<string>{"English"}, iso639_3="eng",
					localname="American English", name="English", names=new List<string> { "Anglais", "Angleščina", "Anglisy", "Angličtina", "Anglų", "Angol", "Angļu", "Engels", "Engelsk", "Engelska", "Engelski", "Englaisa", "Englanti", "Engleză", "Englisch", "Ingilizce", "Inglese", "Ingliż", "Inglés", "Inglês", "Język angielski", "Kiingereza" },
					region="US", regionName="United States", regions=new List<string> { "AD", "AR", "AS", "AW", "BD", "BG", "BH", "BL", "BN", "BQ", "BT", "CL", "CN", "CO", "CW", "CY", "CZ", "DO", "EC", "EE", "ES", "ET", "FM", "FR", "GQ", "GR", "HN", "HR", "HU", "ID", "IT", "JP", "KH", "KR", "LB", "LK", "LT", "LU", "LV", "LY", "MC", "MF", "MV", "MX", "NO", "NP", "OM", "PL", "PM", "PR", "PT", "RO", "RU", "SA", "SK", "SO", "SR", "ST", "TC", "TH", "UM", "VE", "VG", "VI" },
					tag="en", tags=new List<string> { "en-Latn", "en-US" },
					},
				new AllTagEntry {full="en-Brai-GB", iana=new List<string>{"English"}, iso639_3="eng",
					name="English", names=new List<string> { "Anglais", "Angleščina", "Anglisy", "Angličtina", "Anglų", "Angol", "Angļu", "Engels", "Engelsk", "Engelska", "Englaisa", "Englanti", "Engleză", "Englisch", "Ingilizce", "Inglese", "Ingliż", "Inglés", "Inglês", "Język angielski", "Kiingereza" },
					region="GB", regionName="United Kingdom", regions=new List<string> { "AD", "AE", "AG", "AI", "AS", "AT", "AU", "AW", "BB", "BD", "BE", "BG", "BH", "BL", "BM", "BN", "BQ", "BS", "BT", "BW", "BZ", "CA", "CH", "CK", "CL", "CM", "CW", "CY", "CZ", "DE", "DK", "DM", "DO", "EC", "EE", "ER", "ES", "ET", "FI", "FJ", "FM", "FR", "GD", "GG", "GH", "GI", "GM", "GQ", "GR", "GU", "GY", "HK", "HN", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IT", "JE", "JM", "JP", "KE", "KH", "KI", "KR", "KY", "LB", "LC", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MF", "MG", "MH", "MO", "MP", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "NA", "NF", "NG", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PT", "PW", "RO", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SI", "SK", "SL", "SO", "SR", "SS", "ST", "SX", "SZ", "TC", "TH", "TK", "TO", "TT", "TV", "TZ", "UG", "US", "VC", "VE", "VG", "VI", "VU", "WS", "ZA", "ZM", "ZW" },
					tag="en-Brai"
				},
				new AllTagEntry {full="en-Dsrt-US", iana=new List<string>{"English"}, iso639_3="eng",
					name="English", names=new List<string> { "Anglais", "Angleščina", "Anglisy", "Angličtina", "Anglų", "Angol", "Angļu", "Engels", "Engelsk", "Engelska", "Engelski", "Englaisa", "Englanti", "Engleză", "Englisch", "Ingilizce", "Inglese", "Ingliż", "Inglés", "Inglês", "Język angielski", "Kiingereza" },
					region="US", regionName="United States", regions=new List<string> { "AC", "AD", "AE", "AG", "AI", "AR", "AS", "AT", "AU", "AW", "BA", "BB", "BD", "BE", "BG", "BH", "BL", "BM", "BN", "BQ", "BR", "BS", "BT", "BW", "BZ", "CA", "CH", "CK", "CL", "CM", "CW", "CY", "CZ", "DE", "DK", "DM", "DO", "DZ", "EC", "EE", "EG", "ER", "ES", "ET", "FI", "FJ", "FM", "FR", "GB", "GD", "GG", "GH", "GI", "GM", "GQ", "GR", "GU", "GY", "HK", "HN", "HR", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IT", "JE", "JM", "JO", "JP", "KE", "KH", "KI", "KR", "KY", "KZ", "LB", "LC", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MF", "MG", "MH", "MO", "MP", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "NA", "NF", "NG", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PT", "PW", "RO", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SI", "SK", "SL", "SO", "SR", "SS", "ST", "SX", "SZ", "TA", "TC", "TH", "TK", "TO", "TR", "TT", "TV", "TZ", "UG", "VC", "VE", "VG", "VI", "VU", "WS", "YE", "ZA", "ZM", "ZW" },
					tag="en-Dsrt"
				},
				new AllTagEntry {full="en-Dupl-US", iana=new List<string>{"English"}, iso639_3="eng",
					name="English",
					region="US", regionName="United States",
					tag="en-Dupl"
				},
				new AllTagEntry {full="en-Shaw-GB", iana=new List<string>{"English"}, iso639_3="eng",
					name="English", names=new List<string> { "Anglais", "Angleščina", "Anglisy", "Angličtina", "Anglų", "Angol", "Angļu", "Engels", "Engelsk", "Engelska", "Engelski", "Englaisa", "Englanti", "Engleză", "Englisch", "Ingilizce", "Inglese", "Ingliż", "Inglés", "Inglês", "Język angielski", "Kiingereza" },
					region="GB", regionName="United Kingdom", regions=new List<string> { "AC", "AD", "AE", "AG", "AI", "AR", "AS", "AT", "AU", "AW", "BA", "BB", "BD", "BE", "BG", "BH", "BL", "BM", "BN", "BQ", "BR", "BS", "BT", "BW", "BZ", "CA", "CH", "CK", "CL", "CM", "CW", "CY", "CZ", "DE", "DK", "DM", "DO", "DZ", "EC", "EE", "EG", "ER", "ES", "ET", "FI", "FJ", "FM", "FR", "GD", "GG", "GH", "GI", "GM", "GQ", "GR", "GU", "GY", "HK", "HN", "HR", "HU", "ID", "IE", "IL", "IM", "IN", "IO", "IQ", "IT", "JE", "JM", "JO", "JP", "KE", "KH", "KI", "KR", "KY", "KZ", "LB", "LC", "LK", "LR", "LS", "LT", "LU", "LV", "LY", "MA", "MF", "MG", "MH", "MO", "MP", "MS", "MT", "MU", "MV", "MW", "MX", "MY", "NA", "NF", "NG", "NL", "NO", "NP", "NR", "NU", "NZ", "OM", "PG", "PH", "PK", "PL", "PM", "PN", "PR", "PT", "PW", "RO", "RW", "SA", "SB", "SC", "SD", "SE", "SG", "SI", "SK", "SL", "SO", "SR", "SS", "ST", "SX", "SZ", "TA", "TC", "TH", "TK", "TO", "TR", "TT", "TV", "TZ", "UG", "US", "VC", "VE", "VG", "VI", "VU", "WS", "YE", "ZA", "ZM", "ZW" },
					tag="en-Shaw"
				},
				new AllTagEntry { full="pi-Deva-IN", iana=new List<string>{"Pali"}, iso639_3="pli",
					name="Pali",
					region="IN", regionName="India", regions=new List<string> { "MM" },
					tag="pi-Deva"
					},
				new AllTagEntry { full="pi-Mymr-IN", iana=new List<string>{"Pali"}, iso639_3="pli",
					name="Pali",
					region="IN", regionName="India", regions=new List<string> { "MM" },
					tag="pi-Mymr"
					},
				new AllTagEntry { full="pi-Sinh-IN", iana=new List<string>{"Pali"}, iso639_3="pli",
					name="Pali",
					region="IN", regionName="India", regions=new List<string> { "MM" },
					tag="pi-Sinh"
					},
				new AllTagEntry { full="pi-Thai-IN", iana=new List<string>{"Pali"}, iso639_3="pli",
					name="Pali",
					region="IN", regionName="India", regions=new List<string> { "MM" },
					tag="pi-Thai"
					},
				new AllTagEntry { full="pia-Latn-MX", iana=new List<string>{"Pima Bajo"}, iso639_3="pia",
					name="Pima Bajo", names=new List<string> { "Lower Piman", "Mountain Pima", "Névome", "Oob No’ok" },
					region="MX", regionName="Mexico",
					tag="pia", tags=new List<string> { "pia-Latn", "pia-MX" }
					},
				new AllTagEntry { full="zrg-Orya-IN", iana=new List<string>{"Mirgan"}, iso639_3="zrg",
					name="Mirgan", names=new List<string> { "Mirgami", "Mirkan", "Panika", "Panka" },
					region="IN", regionName="India",
					tag="zrg-Orya"
					},
				new AllTagEntry { full="zrg-Telu-IN", iana=new List<string>{"Mirgan"}, iso639_3="zrg",
					name="Mirgan", names=new List<string> { "Mirgami", "Mirkan", "Panika", "Panka" },
					region="IN", regionName="India",
					tag="zrg-Telu"
					}
			};

			var lookup1 = new LanguageLookup(entries, false);
			//SUT
			var lookup2 = new LanguageLookup(entries, true);

			CheckLanguageTagCounts(lookup1, "abq", 2, 2, 0, 0);
			CheckLanguageTagCounts(lookup2, "abq", 2, 1, 1, 1);	// has one entry changed from original
			CheckModifiedEntry(lookup2, "abq", "abq", "Abaza", 5, "Russian Federation", 2);	// first possibility
			CheckUnmodifiedEntries(lookup2, "abq", new[] {"abq-Latn"});

			CheckLanguageTagCounts(lookup1, "aii", 2, 2, 0, 0);
			CheckLanguageTagCounts(lookup2, "aii", 2, 1, 1, 1);	// has one entry changed from original
			CheckModifiedEntry(lookup2, "aii", "aii", "Assyrian Neo-Aramaic", 15, "Iraq", 24);	// second possibility
			CheckUnmodifiedEntries(lookup2, "aii", new [] {"aii-Cyrl"});

			CheckLanguageTagCounts(lookup1, "crh", 3, 2, 1, 1);
			CheckLanguageTagCounts(lookup2, "crh", 3, 2, 1, 1);	// same as original

			CheckLanguageTagCounts(lookup1, "cmg", 1, 1, 0, 0);
			CheckLanguageTagCounts(lookup2, "cmg", 1, 0, 1, 1);	// has one and only entry changed from original
			CheckModifiedEntry(lookup2, "cmg", "cmg", "Classical Mongolian", 1, "Mongolia", 1);
			CheckUnmodifiedEntries(lookup2, "cmg", new string[0]);

			CheckLanguageTagCounts(lookup1, "de", 1, 0, 1, 1);
			CheckLanguageTagCounts(lookup2, "de", 1, 0, 1, 1);	// same as original

			CheckLanguageTagCounts(lookup1, "en", 5, 4, 1, 1);
			CheckLanguageTagCounts(lookup2, "en", 5, 4, 1, 1);	// same as original

			CheckLanguageTagCounts(lookup1, "pi", 5, 4, 0, 1);
			CheckLanguageTagCounts(lookup2, "pi", 5, 3, 1, 2);	// has one entry changed from original
			CheckModifiedEntry(lookup2, "pi", "pli", "Pali", 1, "India", 2);	// should be first possibility
			CheckUnmodifiedEntries(lookup2, "pi", new []{"pi-Mymr","pi-Sinh","pi-Thai"});
			var languages = lookup1.SuggestLanguages("pi").Where(li => li.LanguageTag == "pia").ToList();
			Assert.AreEqual(1, languages.Count);
			languages = lookup2.SuggestLanguages("pi").Where(li => li.LanguageTag == "pia").ToList();
			Assert.AreEqual(1, languages.Count);	// same as original

			CheckLanguageTagCounts(lookup1, "zrg", 2, 2, 0, 0);
			CheckLanguageTagCounts(lookup2, "zrg", 2, 1, 1, 1);
			CheckModifiedEntry(lookup2, "zrg", "zrg", "Mirgan", 5, "India", 1);	// should be first possibility
			CheckUnmodifiedEntries(lookup2, "zrg", new []{"zrg-Telu"});
		}

		private void CheckLanguageTagCounts(LanguageLookup lookup, string tag, int startWith, int startWithPlusHyphen,
			int equal, int startWithFiltered)
		{
			var languages = lookup.SuggestLanguages(tag).Where(li => li.LanguageTag.StartsWith(tag)).ToArray();
			Assert.AreEqual(startWith, languages.Length);
			languages = lookup.SuggestLanguages(tag).Where(li => li.LanguageTag.StartsWith(tag + "-")).ToArray();
			Assert.AreEqual(startWithPlusHyphen, languages.Length);
			languages = lookup.SuggestLanguages(tag).Where(li => li.LanguageTag == tag).ToArray();
			Assert.AreEqual(equal, languages.Length);
			languages = lookup.SuggestLanguages(tag).Where(li => li.LanguageTag.StartsWith(tag) && ScriptMarkerFilter(li)).ToArray();
			Assert.AreEqual(startWithFiltered, languages.Length);
		}

		private void CheckModifiedEntry(LanguageLookup lookup, string tag, string isoTag, string name, int nameCount, string country, int regionCount)
		{
			var languages = lookup.SuggestLanguages(tag).Where(li => li.LanguageTag == tag).ToArray();
			Assert.AreEqual(1, languages.Length);
			Assert.AreEqual(isoTag, languages[0].ThreeLetterTag);
			Assert.AreEqual(name, languages[0].DesiredName);
			Assert.AreEqual(nameCount, languages[0].Names.Count);
			Assert.AreEqual(country, languages[0].PrimaryCountry);
			Assert.AreEqual(regionCount, languages[0].Countries.Count);
		}

		private void CheckUnmodifiedEntries(LanguageLookup lookup, string tag, string[] unchangedTags)
		{
			var languages = lookup.SuggestLanguages(tag).Where(li => li.LanguageTag.StartsWith(tag + "-")).ToList();
			Assert.AreEqual(unchangedTags.Length, languages.Count);
			foreach (var unchanged in unchangedTags)
				Assert.IsTrue(languages.Count(li => li.LanguageTag == unchanged) == 1);
		}

		private bool ScriptMarkerFilter(LanguageInfo li)
		{
			string language;
			string script;
			string region;
			string variant;
			if (IetfLanguageTag.TryGetParts(li.LanguageTag, out language, out script, out region, out variant))
				return string.IsNullOrEmpty(script);	// OK only if no script.
			return true;	// Not a tag?  Don't filter it out.
		}

	}
}
