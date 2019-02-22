using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class StandardSubtagsTests
	{
		[Test]
		public void RegisteredLanguages_HasEnglish_True()
		{
			Assert.That(StandardSubtags.RegisteredLanguages.Contains("en"), Is.True);
		}

		[Test]
		public void RegisteredLanguages_CodeFaHasIso3Fas()
		{
			LanguageSubtag target = StandardSubtags.RegisteredLanguages["fa"];
			Assert.That(target.Iso3Code, Is.EqualTo("fas"));
		}

		[Test]
		public void RegisteredLanguages_CodeAqtHasIso3Angaite() // Updated to the 2014 version of the subtag registry
		{
			Assert.That(StandardSubtags.RegisteredLanguages.Contains("aqt"), Is.True);
		}

		[Test]
		public void RegisteredLanguages_HasIso3CodeForEnglish()
		{
			LanguageSubtag english = StandardSubtags.RegisteredLanguages["en"];
			Assert.That(english.Iso3Code, Is.EqualTo("eng"));
		}

		[Test]
		public void RegisteredScripts_HasLatn_True()
		{
			Assert.That(StandardSubtags.RegisteredScripts.Contains("Latn"), Is.True);
		}

		[Test]
		public void RegisteredScripts_HasOldItalic_True()
		{
			Assert.That(StandardSubtags.RegisteredScripts.Any(code => code.Name == "Old Italic (Etruscan, Oscan, etc.)"), Is.True);
		}

		[Test]
		public void SubTagComponentDescription_HasBeginningParenthesis_RemovesParens()
		{
			Assert.AreEqual(
				"Hiragana + Katakana",
				StandardSubtags.SubTagComponentDescription("Description: (Hiragana + Katakana)")
			);
		}

		[Test]
		public void SubTagComponentDescription_HasForwardSlash_ReplacesWithPipe()
		{
			Assert.AreEqual(
				"=|Kx'au||'ein",
				StandardSubtags.SubTagComponentDescription("Description: =/Kx'au//'ein")
			);
		}


		[Test]
		public void SubTagComponentDescription_HasAliasFor_RemovesAliasFor()
		{
			Assert.AreEqual(
				"Japanese (Han + Hiragana + Katakana)",
				StandardSubtags.SubTagComponentDescription("Description: Japanese (alias for Han + Hiragana + Katakana)")
			);
		}

		[Test]
		public void SubTagComponentDescription_HasIndividualLanguage_RemovesIt()
		{
			Assert.AreEqual(
				"Malay",
				StandardSubtags.SubTagComponentDescription("Description: Malay (individual language)")
			);
		}

		[Test]
		public void RegisteredScripts_HasLatinFraktur_True()
		{
			Assert.That(StandardSubtags.RegisteredScripts.Any(code => code.Name == "Latin (Fraktur variant)"), Is.True);
		}

		[Test]
		public void RegisteredScripts_HasHiraganaKatakana_True()
		{
			Assert.That(StandardSubtags.RegisteredScripts.Any(code => code.Name == "Japanese syllabaries (Hiragana + Katakana)"), Is.True);
		}

		[Test]
		public void RegisteredScripts_HasFonipa_False()
		{
			Assert.That(StandardSubtags.RegisteredScripts.Contains("fonipa"), Is.False);
		}

		[Test]
		public void RegisteredScripts_HasSome()
		{
			Assert.That(StandardSubtags.RegisteredScripts.Count, Is.GreaterThan(4));
		}

		[Test]
		public void RegisteredRegions_HasUS_True()
		{
			Assert.That(StandardSubtags.RegisteredRegions.Contains("US"), Is.True);
		}

		[Test]
		public void RegisteredRegions_HasFonipa_False()
		{
			Assert.That(StandardSubtags.RegisteredRegions.Contains("fonipa"), Is.False);
		}

		[Test]
		public void RegisteredVariants_HasFonipa_True()
		{
			Assert.That(StandardSubtags.RegisteredVariants.Contains("fonipa"), Is.True);
		}

		[Test]
		public void RegisteredVariants_HasBiske_True()
		{
			Assert.That(StandardSubtags.RegisteredVariants.Contains("biske"), Is.True);
		}

		[Test]
		public void RegisteredVariants_HasEn_False()
		{
			Assert.That(StandardSubtags.RegisteredVariants.Contains("en"), Is.False);
		}

		[Test]
		public void IsValidIso639LanguageCode_en_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsValidIso639LanguageCode("en"), Is.True);
		}

		[Test]
		public void IsValidIso639LanguageCode_fonipa_ReturnFalse()
		{
			Assert.That(StandardSubtags.IsValidIso639LanguageCode("fonipa"), Is.False);
		}

		[Test]
		public void IsValidIso639LanguageCode_one_ReturnTrue()
		{
			// Yes it's true
			Assert.That(StandardSubtags.IsValidIso639LanguageCode("one"), Is.True);
		}

		[Test]
		public void IsValidIso639LanguageCode_two_ReturnTrue()
		{
			// Yes it's true
			Assert.That(StandardSubtags.IsValidIso639LanguageCode("two"), Is.True);
		}

		[Test]
		public void IsValidIso15924ScriptCode_Latn_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsValidIso15924ScriptCode("Latn"), Is.True);
		}

		[Test]
		public void IsValidIso15924ScriptCode_Qaaa_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsValidIso15924ScriptCode("Qaaa"), Is.True);
		}
		[Test]
		public void IsPrivateUseScriptCode_Qaaa_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsPrivateUseScriptCode("Qaaa"), Is.True);
		}

		[Test]
		public void IsValidIso15924ScriptCode_fonipa_ReturnsFalse()
		{
			Assert.That(StandardSubtags.IsValidIso15924ScriptCode("fonipa"), Is.False);
		}

		[Test]
		public void IsValidIso3166Region_US_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsValidIso3166RegionCode("US"), Is.True);
		}

		[Test]
		public void IsValidIso3166Region_QM_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsValidIso3166RegionCode("QM"), Is.True);
			Assert.That(StandardSubtags.IsValidIso3166RegionCode("qm"), Is.True);
		}

		[Test]
		public void IsPrivateUseRegionCode_QM_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsPrivateUseRegionCode("QM"), Is.True);
		}

		[Test]
		public void IsValidIso3166Region_fonipa_ReturnsFalse()
		{
			Assert.That(StandardSubtags.IsValidIso3166RegionCode("fonipa"), Is.False);
		}

		[Test]
		public void IsValidRegisteredVariant_fonipa_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsValidRegisteredVariantCode("fonipa"), Is.True);
		}
		[Test]
		public void IsValidRegisteredVariant_en_ReturnsFalse()
		{
			Assert.That(StandardSubtags.IsValidRegisteredVariantCode("en"), Is.False);
		}

		[Test]
		public void IsValidIso639LanguageCode_qaa_ReturnsTrue()
		{
			Assert.That(StandardSubtags.IsValidIso639LanguageCode("qaa"), Is.True);
		}

		[Test]
		public void Iso639LanguageCodes_DoesNotContainCodeRanges()
		{
			Assert.That(StandardSubtags.RegisteredLanguages.Where(iso639 => iso639.Code.Contains("..")), Is.Empty);
		}

		[Test]
		public void VariantTagPrefixes()
		{
			string[] monotonPrefixes = StandardSubtags.RegisteredVariants["monoton"].Prefixes.ToArray();
			Assert.That(monotonPrefixes, Has.Length.EqualTo(1));
			Assert.That(monotonPrefixes, Has.Member("el"));

			string[] pinyinPrefixes = StandardSubtags.RegisteredVariants["pinyin"].Prefixes.ToArray();
			Assert.That(pinyinPrefixes, Has.Length.EqualTo(2));
			Assert.That(pinyinPrefixes, Has.Member("zh-Latn"));
			Assert.That(pinyinPrefixes, Has.Member("bo-Latn"));
		}

		[TestCase("Qaaa", true)]
		[TestCase("Qabx", true)]
		[TestCase("Qaby", false)] // Valid script code that will is unlikely to be registered
		[TestCase("A1B2", false)] // Valid script code that will is unlikely to be registered
		public void VerifyAddedPrivateUseScriptsMarkedProperly(string scriptCode, bool expectedValue)
		{
			StandardSubtags.RegisteredScripts.Remove(scriptCode);
			StandardSubtags.AddScript(scriptCode, "description");
			Assert.AreEqual(StandardSubtags.RegisteredScripts[scriptCode].IsPrivateUse, expectedValue);
			StandardSubtags.RegisteredScripts.Remove(scriptCode);
		}
	}
}
