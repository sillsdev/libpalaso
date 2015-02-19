using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class StandardSubtagsTests
	{
		[Test]
		public void ValidIso639LanguageCodes_HasEnglish_True()
		{
			Assert.That(StandardSubtags.Iso639Languages.Contains("en"), Is.True);
		}

		[Test]
		public void CodeFaHasIso3Pes()
		{
			LanguageSubtag target = StandardSubtags.Iso639Languages["fa"];
			Assert.That(target.Iso3Code, Is.EqualTo("pes"));
		}

		[Test]
		public void CodeAqtHasIso3Angaite() // Updated to the 2014 version of the subtag registry
		{
			Assert.That(StandardSubtags.Iso639Languages.Contains("aqt"), Is.True);
		}

		[Test]
		public void Iso639LanguageCodes_HasIso3CodeForEnglish()
		{
			LanguageSubtag english = StandardSubtags.Iso639Languages["en"];
			Assert.That(english.Iso3Code, Is.EqualTo("eng"));
		}

		[Test]
		public void Iso639LanguageCodes_HasImplicitScriptCodeForEnglish()
		{
			LanguageSubtag english = StandardSubtags.Iso639Languages["en"];
			Assert.That(english.ImplicitScriptCode, Is.EqualTo("Latn"));
		}

		[Test]
		public void Iso639LanguageCodes_HasFonipa_False()
		{
			Assert.That(StandardSubtags.Iso639Languages.Contains("fonipa"), Is.False);
		}

		[Test]
		public void Iso15924Scripts_HasLatn_True()
		{
			Assert.That(StandardSubtags.Iso15924Scripts.Contains("Latn"), Is.True);
		}

        [Test]
		public void Iso15924Scripts_HasOldItalic_True()
		{
			Assert.That(StandardSubtags.Iso15924Scripts.Any(code => code.Name == "Old Italic (Etruscan, Oscan, etc.)"), Is.True);
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
		public void Iso15924Scripts_HasLatinFraktur_True()
		{
			Assert.That(StandardSubtags.Iso15924Scripts.Any(code => code.Name == "Latin (Fraktur variant)"), Is.True);
		}

		[Test]
		public void Iso15924Scripts_HasHiraganaKatakana_True()
		{
            Assert.That(StandardSubtags.Iso15924Scripts.Any(code => code.Name == "Japanese syllabaries (Hiragana + Katakana)"), Is.True);
		}

		[Test]
		public void Iso15924Scripts_HasFonipa_False()
		{
			Assert.That(StandardSubtags.Iso15924Scripts.Contains("fonipa"), Is.False);
		}

		[Test]
		public void Iso15924Scripts_HasSome()
		{
			Assert.That(StandardSubtags.Iso15924Scripts.Count, Is.GreaterThan(4));
		}

		[Test]
		public void Iso3166Regions_HasUS_True()
		{
			Assert.That(StandardSubtags.Iso3166Regions.Contains("US"), Is.True);
		}

		[Test]
		public void Iso3166Regions_HasFonipa_False()
		{
			Assert.That(StandardSubtags.Iso3166Regions.Contains("fonipa"), Is.False);
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
			Assert.That(StandardSubtags.Iso639Languages.Where(iso639 => iso639.Code.Contains("..")), Is.Empty);
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
	}
}
