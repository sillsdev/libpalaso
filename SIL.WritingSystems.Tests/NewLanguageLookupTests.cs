using System.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace SIL.WritingSystems.Tests
{
    [TestFixture]
    public class NewLanguageLookupTests
    {
        [Test]
        public void SuggestLanguages_EmptyString_ReturnsNothing()
        {
            var lookup = new NewLanguageLookup();
            Assert.That(lookup.SuggestLanguages("").Count(), Is.EqualTo(0));
        }

        [Test]
        public void SuggestLanguages_Asterisk_ReturnsEverything()
        {
            var lookup = new NewLanguageLookup();
            Assert.Greater(lookup.SuggestLanguages("*").Count(), 1000);
        }

        [Test]
        public void SuggestLanguages_LargeMispelling_StillFinds()
        {
            var lookup = new NewLanguageLookup();
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
            var lookup = new NewLanguageLookup();
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
            var lookup = new NewLanguageLookup();
            LanguageInfo languageInfo = lookup.SuggestLanguages("thai").First();
            Assert.That(languageInfo.LanguageTag, Is.EqualTo("th"));
            Assert.That(languageInfo.Countries, Is.EqualTo(new[] { "Cambodia", "Thailand" }));
        }

        /// <summary>
        /// test that we're delivering 2 letter codes when available
        /// </summary>
        [Test]
        public void SuggestLanguages_Thai_CodeIsJustTwoLetters()
        {
            var lookup = new NewLanguageLookup();
            LanguageInfo languageInfo = lookup.SuggestLanguages("thai").First();
            Assert.That(languageInfo.LanguageTag, Is.EqualTo("th"));
        }

        /// <summary>
        /// languages are entered once for every country, so we need to be careful not to list "English, English, English" in the names
        /// </summary>
        [Test]
        public void SuggestLanguages_English_EnglishNotInTheAlternativeNames()
        {
            var lookup = new NewLanguageLookup();
            //messed up case is intentional
            Assert.That(lookup.SuggestLanguages("english").First().Names.Count(s => s == "English"), Is.EqualTo(1));
        }


        [Test]
        public void SuggestLanguages_GivenUnambigous3LetterCode_ReturnsLanguage()
        {
            var lookup = new NewLanguageLookup();
            //messed up case is intentional
            Assert.That(lookup.SuggestLanguages("eTR").First().Names.First().Contains("Edolo"), Is.True);
        }

        [Test]
        public void SuggestLanguages_GivenPNGLanguage_ReturnsPNGCountryName()
        {
            var lookup = new NewLanguageLookup();
            //messed up case is intentional
            Assert.That(lookup.SuggestLanguages("eTR").First().Countries, Is.EqualTo(new[] { "Papua New Guinea" }));
        }

        [Test]
        public void SuggestLanguages_StartOfName_ReturnsManyLanguages()
        {
            var lookup = new NewLanguageLookup();
            //messed up case is intentional
            Assert.Greater(1000, lookup.SuggestLanguages("eastern").Count());
        }


        [Test]
        public void SuggestLanguages_3LetterCode_ResultIncludesAlternateLanguageNames()
        {
            var lookup = new NewLanguageLookup();
            LanguageInfo[] languages = lookup.SuggestLanguages("ana").ToArray();
            Assert.True(languages.Any(l => l.LanguageTag == "ana"));
            Assert.True(languages.Any(l => l.Names.Contains("Aguanunga")));
            Assert.True(languages.Any(l => l.Names.Contains("Andaki")));
            Assert.True(languages.Any(l => l.Names.Contains("Churuba")));
        }
        [Test]
        public void SuggestLanguages_NonPrimary_ResultIncludesAlternateLanguageNames()
        {
            var lookup = new NewLanguageLookup();
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
            var lookup = new NewLanguageLookup();
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
            var lookup = new NewLanguageLookup();
            var languages = lookup.SuggestLanguages("Degexit’an").ToArray();
            Assert.AreEqual("ing", languages[0].LanguageTag);
            Assert.True(languages.Any(l => l.Names.Contains("Degexit’an")));
            Assert.True(languages.Any(l => l.Names.Contains("Deg Xinag")));
            Assert.True(languages.Any(l => l.Names.Contains("Deg Xit’an")));
            Assert.AreEqual(3, languages[0].Names.Count, "3 of the 5 names are pejorative and should not be listed");
        }

        /// <summary>
        /// This is a result of finding that some of the alternative names, in Nov 2016, were *not* marked as pejorative but actually were.
        /// These may be fixed in the Ethnologue over time, but it was requested that we just remove all alternative names for now.
        /// </summary>
        [Test]
        public void SuggestLanguages_LanguageIsInEthiopia_ShowOnlyOfficialNames()
        {
            var lookup = new NewLanguageLookup();
            var languages = lookup.SuggestLanguages("Wolaytta").ToArray();
            Assert.True(languages.Any(l => l.Names.Contains("Wolaytta")));
            Assert.AreEqual(1, languages[0].Names.Count, "Should only list a single name for Ethiopian languages.");
        }

        /// <summary>
        /// We have been asked to temporarily suppress these three codes for Ethiopia, until the Ethologue is changed.
        /// </summary>
        [Test]
        public void SuggestLanguages_LanguageIsOromo_DoNotShowRelatedLanguages()
        {
            var lookup = new NewLanguageLookup();
            var languages = lookup.SuggestLanguages("Oromo").ToArray();
            Assert.True(languages.All(l => l.DesiredName == "Oromo"));
            Assert.True(languages.All(l => l.LanguageTag.StartsWith("om")), "We should be suppressing gat, hae, gaz");
        }

        /// <summary>
        /// We should not suggest macro languages.
        /// </summary>
        [Test]
        public void SuggestLanguages_DoesNotSuggestMacroLanguages()
        {
            var lookup = new NewLanguageLookup();
            Assert.That(lookup.SuggestLanguages("macrolanguage").Count(), Is.EqualTo(0));
            var languages = lookup.SuggestLanguages("zza").ToArray();
            Assert.False(languages.Any(l => l.LanguageTag == "zza"));
        }

        /// <summary>
        /// We should not suggest deprecated tags for languages.
        /// </summary>
        [Test]
        public void SuggestLanguages_DoesNotSuggestDeprecatedTags()
        {
            var lookup = new NewLanguageLookup();
			var languages = lookup.SuggestLanguages("dzd").ToArray();
			Assert.False(languages.Any(l => l.LanguageTag == "dzd"));
			languages = lookup.SuggestLanguages("yiy").ToArray();
			Assert.False(languages.Any(l => l.LanguageTag == "yiy"));
        }

        /// <summary>
        /// We should now be able to search for 3 letter codes e.g. nld for languages that have 2 letter codes e.g. nl
        /// </summary>
        [Test]
        public void SuggestLanguages_CanFind3LetterCodesForLanguagesWith2LetterCodes()
        {
            var lookup = new NewLanguageLookup();
            var languages = lookup.SuggestLanguages("nld").ToArray();
            Assert.True(languages.Any(l => l.DesiredName == "Dutch"));
        }

        /// <summary>
        /// Check that new language lookup is same as old language lookup as far as we can check.
        /// Note that if Sldr alltags.txt file is cached or online sldr used then results will be 
        /// different when creating LanguageDataIndex.txt
        /// </summary>
        [Test]
        public void NewLanguageLookup_SameCountAsOld()
        {
            Console.WriteLine("StandardSubtags has {0} languages, {1} regions, {2} scripts, {3} variants",
                StandardSubtags.RegisteredLanguages.Count,
                StandardSubtags.RegisteredRegions.Count,
                StandardSubtags.RegisteredScripts.Count,
                StandardSubtags.RegisteredVariants.Count
            );

            var lookup = new NewLanguageLookup();
            var oldlookup = new LanguageLookup();
            int newcount = lookup.SuggestLanguages("*").Count();
            int oldcount = oldlookup.SuggestLanguages("*").Count();
            Assert.AreEqual(oldcount, newcount);
        }

        /// <summary>
        /// Check that new language lookup is same as old language lookup as far as we can check.
        /// </summary>
        [Test]
        public void NewLanguageLookup_SameValuesAsOld()
        {
            var lookup = new NewLanguageLookup();
            var oldlookup = new LanguageLookup();

            Dictionary<string, LanguageInfo> newlanguages = lookup.SuggestLanguages("*").ToDictionary(p => p.LanguageTag);
            Dictionary<string, LanguageInfo> oldlanguages = oldlookup.SuggestLanguages("*").ToDictionary(p => p.LanguageTag);
            int newcount = newlanguages.Count();
            int oldcount = oldlanguages.Count();

            foreach (var language in oldlanguages)
            {        
                if (!newlanguages.ContainsKey(language.Key))
                {
                    Console.WriteLine("NewLookup does not contain {0}:{1}", language.Key, language.Value.DesiredName);
                    Assert.True(newlanguages.ContainsKey(language.Key));
                }
            }

            foreach (var language in newlanguages)
            {
                if (!oldlanguages.ContainsKey(language.Key))
                {
                    Console.WriteLine("OldLookup does not contain {0}:{1}", language.Key, language.Value.DesiredName);
                    Assert.True(oldlanguages.ContainsKey(language.Key));
                }
            }

            Assert.AreEqual(oldcount, newcount);
        }

    }
}
