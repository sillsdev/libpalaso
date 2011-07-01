using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class StandardTagsTests
	{
		private class TestEvironment : IDisposable
		{
			public TestEvironment()
			{
				// I guess we don't need this right now
			}
			public void Dispose()
			{
			}
		}

		[Test]
		public void ValidIso639LanguageCodes_HasEnglish_True()
		{
			var codes = StandardTags.ValidIso639LanguageCodes;
			Assert.IsTrue(codes.Any(code => code.Code == "en"));
		}

		[Test]
		public void ValidIso639LanguageCodes_HasISO3CodeForEnglish()
		{
			var codes = StandardTags.ValidIso639LanguageCodes;
			var english = codes.Where(code => code.Code == "en").First();
			Assert.That(english.ISO3Code, Is.EqualTo("eng"));
		}

		[Test]
		public void ValidIso639LanguageCodes_HasFonipa_False()
		{
			var codes = StandardTags.ValidIso639LanguageCodes;
			Assert.IsFalse(codes.Any(code => code.Code == "fonipa"));
		}

		[Test]
		public void ValidIso15924Scripts_HasLatn_True()
		{
			var codes = StandardTags.ValidIso15924Scripts;
			Assert.IsTrue(codes.Any(code => code.Code == "Latn"));
		}

		[Test]
		public void ValidIso15924Scripts_HasOldItalic_True()
		{
			var codes = StandardTags.ValidIso15924Scripts;
			Assert.IsTrue(codes.Any(code => code.Label == "Old Italic (Etruscan, Oscan, etc.)"));
		}

		[Test]
		public void SubTagComponentDescription_HasBeginningParenthesis_RemovesParens()
		{
			Assert.AreEqual(
				"Hiragana + Katakana",
				StandardTags.SubTagComponentDescription("Description: (Hiragana + Katakana)")
			);
		}

		[Test]
		public void SubTagComponentDescription_HasForwardSlash_ReplacesWithPipe()
		{
			Assert.AreEqual(
				"=|Kx'au||'ein",
				StandardTags.SubTagComponentDescription("Description: =/Kx'au//'ein")
			);
		}


		[Test]
		public void SubTagComponentDescription_HasAliasFor_RemovesAliasFor()
		{
			Assert.AreEqual(
				"Japanese (Han + Hiragana + Katakana)",
				StandardTags.SubTagComponentDescription("Description: Japanese (alias for Han + Hiragana + Katakana)")
			);
		}

		[Test]
		public void ValidIso15924Scripts_HasLatinFraktur_True()
		{
			var codes = StandardTags.ValidIso15924Scripts;
			Assert.IsTrue(codes.Any(code => code.Label == "Latin (Fraktur variant)"));
		}

		[Test]
		public void ValidIso15924Scripts_HasHiraganaKatakana_True()
		{
			var codes = StandardTags.ValidIso15924Scripts;
			Assert.IsTrue(codes.Any(code => code.Label == "Hiragana + Katakana"));
		}

		[Test]
		public void ValidIso15924Scripts_HasFonipa_False()
		{
			var codes = StandardTags.ValidIso15924Scripts;
			Assert.IsFalse(codes.Any(code => code.Code == "fonipa"));
		}

		[Test]
		public void ValidIso15924Scripts_HasSome()
		{
			var codes = StandardTags.ValidIso15924Scripts;
			Assert.Greater(codes.Count, 4);
		}

		[Test]
		public void ValidIso3166Regions_HasUS_True()
		{
			var codes = StandardTags.ValidIso3166Regions;
			Assert.IsTrue(codes.Any(code => code.Subtag == "US"));

		}
		[Test]
		public void ValidIso3166Regions_HasFonipa_False()
		{
			var codes = StandardTags.ValidIso3166Regions;
			Assert.IsFalse(codes.Any(code => code.Subtag == "fonipa"));

		}

		[Test]
		public void ValidRegisteredVariants_HasFonipa_True()
		{
			var codes = StandardTags.ValidRegisteredVariants;
			Assert.IsTrue(codes.Any(code => code.Subtag == "fonipa"));

		}

		[Test]
		public void ValidRegisteredVariants_HasBiske_True()
		{
			var codes = StandardTags.ValidRegisteredVariants;
			Assert.IsTrue(codes.Any(code => code.Subtag == "biske"));

		}

		[Test]
		public void ValidRegisteredVariants_HasEn_False()
		{
			var codes = StandardTags.ValidRegisteredVariants;
			Assert.IsFalse(codes.Any(code => code.Subtag == "en"));
		}

		[Test]
		public void IsValidIso639LanguageCode_en_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsValidIso639LanguageCode("en"));
		}

		[Test]
		public void IsValidIso639LanguageCode_fonipa_ReturnFalse()
		{
			Assert.IsFalse(StandardTags.IsValidIso639LanguageCode("fonipa"));
		}

		[Test]
		public void IsValidIso639LanguageCode_one_ReturnTrue()
		{
			// Yes it's true
			Assert.IsTrue(StandardTags.IsValidIso639LanguageCode("one"));
		}

		[Test]
		public void IsValidIso639LanguageCode_two_ReturnTrue()
		{
			// Yes it's true
			Assert.IsTrue(StandardTags.IsValidIso639LanguageCode("two"));
		}

		[Test]
		public void IsValidIso15924ScriptCode_Latn_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsValidIso15924ScriptCode("Latn"));
		}

		[Test]
		public void IsValidIso15924ScriptCode_Qaaa_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsValidIso15924ScriptCode("Qaaa"));
		}
		[Test]
		public void IsStandardIso15924ScriptCode_Qaaa_ReturnsFalse()
		{
			Assert.IsFalse(StandardTags.IsStandardIso15924ScriptCode("Qaaa"));
		}

		[Test]
		public void IsStandardIso15924ScriptCode_Latn_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsStandardIso15924ScriptCode("Latn"));
		}

		[Test]
		public void IsValidIso15924ScriptCode_fonipa_ReturnsFalse()
		{
			Assert.IsFalse(StandardTags.IsValidIso15924ScriptCode("fonipa"));
		}

		[Test]
		public void IsValidIso3166Region_US_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsValidIso3166Region("US"));
		}

		[Test]
		public void IsStandardIso3166Region_US_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsStandardIso3166Region("US"));
		}

		[Test]
		public void IsValidIso3166Region_QM_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsValidIso3166Region("QM"));
			Assert.IsTrue(StandardTags.IsValidIso3166Region("qm"));
		}

		[Test]
		public void IsStandardIso3166Region_QM_ReturnsFalse()
		{
			Assert.IsFalse(StandardTags.IsStandardIso3166Region("QM"));
		}

		[Test]
		public void IsValidIso3166Region_fonipa_ReturnsFalse()
		{
			Assert.IsFalse(StandardTags.IsValidIso3166Region("fonipa"));
		}

		[Test]
		public void IsValidRegisteredVariant_fonipa_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsValidRegisteredVariant("fonipa"));
		}
		[Test]
		public void IsValidRegisteredVariant_en_ReturnsFalse()
		{
			Assert.IsFalse(StandardTags.IsValidRegisteredVariant("en"));
		}

		[Test]
		public void IsValidIso639LanguageCode_qaa_ReturnsTrue()
		{
			Assert.IsTrue(StandardTags.IsValidIso639LanguageCode("qaa"));
		}

		[Test]
		public void ValidIso639LanguageCodes_DoesNotContainCodeRanges()
		{
			Assert.AreEqual(0, StandardTags.ValidIso639LanguageCodes.Where(iso639 => iso639.Code.Contains("..")).Count());
		}
	}
}
