using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class IetfLanguageTagTests
	{
		#region ConcatenateVariantAndPrivateUse
		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantOnly_ReturnsVariant()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("1901", string.Empty);
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithxDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("1901", "x-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithoutxDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("1901", "audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithoutxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("", "audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("", "x-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithCapitalXDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("", "X-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("X-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithCapitalXDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("1901", "X-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-X-audio"));
		}

		//this test shows that there is no checking involved as to wether your variants and private use are rfc/writingsystemdefinition conform. All the method does is glue two strings together while handling the "x-"
		[Test]
		public void ConcatenateVariantAndPrivateUse_BogusVariantBadprivateUse_HappilyGluesTheTwoTogether()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTag.ConcatenateVariantAndPrivateUse("bogusvariant", "etic-emic-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("bogusvariant-x-etic-emic-audio"));
		}
		#endregion

		#region SplitVariantAndPrivateUse
		[Test]
		public void SplitVariantAndPrivateUse_VariantOnly_ReturnsVariant()
		{
			string variant;
			string privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse("1901", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo(String.Empty));
		}

		[Test]
		public void SplitVariantAndPrivateUse_VariantAndPrivateUse_ReturnsVariantAndPrivateUse()
		{
			string variant;
			string privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse("1901-x-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_NoxDash_ReturnsVariantOnly()
		{
			string variant;
			string privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse("1901-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901-audio"));
			Assert.That(privateUse, Is.EqualTo(String.Empty));
		}

		[Test]
		public void SplitVariantAndPrivateUse_PrivateUseWithxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string variant;
			string privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse("x-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo(String.Empty));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_PrivateUseWithCapitalXDashOnly_ReturnsPrivateUseWithxDash()
		{
			string variant;
			string privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse("X-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo(String.Empty));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_VariantAndPrivateUseWithCapitalXDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string variant;
			string privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse("1901-X-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		//this test shows that there is no checking involved as to wether your variants and private use are rfc/writingsystemdefinition conform. All the method does is split on x-
		[Test]
		public void SplitVariantAndPrivateUse_BogusVariantBadPrivateUse_HappilysplitsOnxDash()
		{
			string variant;
			string privateUse;
			IetfLanguageTag.SplitVariantAndPrivateUse("bogusVariant-X-audio-emic-etic", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("bogusVariant"));
			Assert.That(privateUse, Is.EqualTo("audio-emic-etic"));
		}
		#endregion

		#region ToLanguageTag
		/// <summary>
		/// Tests the ToLanguageTag() method which converts an ICU locale to a language tag.
		/// </summary>
		[Test]
		public void ToLanguageTag_EnglishIcuLocales_ReturnsEnglishLanguageTags()
		{
			// language
			Assert.AreEqual("en", IetfLanguageTag.ToLanguageTag("en"));
			// language, script
			Assert.AreEqual("en", IetfLanguageTag.ToLanguageTag("en_Latn"));
			// language, region
			Assert.AreEqual("en-US", IetfLanguageTag.ToLanguageTag("en_US"));
			// language, script, region, ICU variant
			Assert.AreEqual("en-US-fonipa-x-etic", IetfLanguageTag.ToLanguageTag("en_Latn_US_X_ETIC"));
			// language, ICU variant
			Assert.AreEqual("en-fonipa-x-emic", IetfLanguageTag.ToLanguageTag("en__X_EMIC"));
		}

		[Test]
		public void ToLanguageTag_ChinesePinyinIcuLocale_ReturnsChineseLanguageTag()
		{
			// language, region, ICU variant
			Assert.AreEqual("zh-CN-pinyin", IetfLanguageTag.ToLanguageTag("zh_CN_X_PY"));
		}

		[Test]
		public void ToLanguageTag_PrivateUseIcuLocales_ReturnsPrivateUseLanguageTags()
		{
			// private use language
			Assert.AreEqual("qaa-x-kal", IetfLanguageTag.ToLanguageTag("xkal"));
			// private use language, custom ICU variant
			Assert.AreEqual("qaa-fonipa-x-kal", IetfLanguageTag.ToLanguageTag("xkal__IPA"));
			// private use language, (standard) private use region
			Assert.AreEqual("qaa-XA-x-kal", IetfLanguageTag.ToLanguageTag("xkal_XA"));
			// private use language, (non-standard) private use script
			Assert.AreEqual("qaa-Qaaa-x-kal-Fake", IetfLanguageTag.ToLanguageTag("xkal_Fake"));
			// language, private use script
			Assert.AreEqual("en-Qaaa-x-Fake", IetfLanguageTag.ToLanguageTag("en_Fake"));
			// language, private use script, private use region
			Assert.AreEqual("en-Qaaa-QM-x-Fake-QD", IetfLanguageTag.ToLanguageTag("en_Fake_QD"));
			// private use language, script
			Assert.AreEqual("qaa-Latn-x-zzz", IetfLanguageTag.ToLanguageTag("zzz_Latn"));
		}

		[Test]
		public void ToLanguageTag_FWIcuLocales_ReturnsLanguageTags()
		{
			// convert older FW language tags
			Assert.AreEqual("slu", IetfLanguageTag.ToLanguageTag("eslu"));
			// other possibilities from FW6.0.6
			Assert.AreEqual("qaa-x-bcd", IetfLanguageTag.ToLanguageTag("x123"));
			Assert.AreEqual("qaa-x-kac", IetfLanguageTag.ToLanguageTag("xka2"));
		}

		[Test]
		public void ToLanguageTag_AlreadyLanguageTag_NoChange()
		{
			// following are already lang tags
			Assert.AreEqual("en-US", IetfLanguageTag.ToLanguageTag("en-US"));
			Assert.AreEqual("en-Latn-US-fonipa-x-etic", IetfLanguageTag.ToLanguageTag("en-Latn-US-fonipa-x-etic"));
		}
		#endregion

		#region Create
		[Test]
		public void Create_InvalidLanguageTag_Throws()
		{
			Assert.That(()=>IetfLanguageTag.Create("a", "", "", string.Empty),
				Throws.ArgumentException.With.Message.Contains("code [a] is invalid"));
		}

		[Test]
		public void Create_InvalidScriptTag_Throws()
		{
			Assert.That(() => IetfLanguageTag.Create("aa", "scripty", "", string.Empty),
				Throws.ArgumentException.With.Message.Contains("code [scripty] is invalid"));
		}

		[Test]
		public void Create_InvalidRegionTag_Throws()
		{
			Assert.That(() => IetfLanguageTag.Create("aa", "latn", "region", string.Empty),
				Throws.ArgumentException.With.Message.Contains("code [region] is invalid"));
		}

		[Test]
		public void Create_InvalidTagValidateDisabled_DoesNotThrow()
		{
			Assert.That(IetfLanguageTag.Create("a", "", "", string.Empty, false), Is.EqualTo("a"));
		}

		[Test]
		public void Create_ImplicitScript_SuppressesScript()
		{
			Assert.That(IetfLanguageTag.Create("en", "Latn", "US", string.Empty), Is.EqualTo("en-US"));
			Assert.That(IetfLanguageTag.Create("en", "Latn", "US", Enumerable.Empty<VariantSubtag>()), Is.EqualTo("en-US"));
		}

		[Test]
		public void Create_SimplifiedChinese_SuppressesScript()
		{
			Assert.That(IetfLanguageTag.Create("zh", "Hans", "CN", string.Empty),
				Is.EqualTo(WellKnownSubtags.ChineseSimplifiedTag));
			Assert.That(IetfLanguageTag.Create("zh", "Hans", "CN", Enumerable.Empty<VariantSubtag>()),
				Is.EqualTo(WellKnownSubtags.ChineseSimplifiedTag));
		}

		[Test]
		public void Create_ValidTag_ReturnsTag()
		{
			Assert.That(IetfLanguageTag.Create("en", "Arab", "GB", "1996"), Is.EqualTo("en-Arab-GB-1996"));
		}

		[Test]
		public void Create_ValidPrivateUseTag_ReturnsTag()
		{
			Assert.That(IetfLanguageTag.Create("qaa", "Zxxx", "GB", "fonipa-x-emic-Jimmy"),
				Is.EqualTo("qaa-Zxxx-GB-fonipa-x-emic-Jimmy"));
		}
		#endregion

		#region Validate

		[Test]
		public void Validate_InvalidTag_ReturnsFalse()
		{
			string message;
			Assert.That(
				IetfLanguageTag.Validate(new LanguageSubtag("a"), new ScriptSubtag(""), new RegionSubtag(""),
					new List<VariantSubtag>(), out message), Is.False);
		}

		[Test]
		public void Validate_ValidTag_ReturnsTrue()
		{
			string message;
			Assert.That(
				IetfLanguageTag.Validate(new LanguageSubtag("en"), new ScriptSubtag("Latn"), new RegionSubtag("US"),
					new List<VariantSubtag>(), out message), Is.True);
		}
		#endregion

		#region Canonicalize
		[Test]
		public void Canonicalize_ImplicitScript_SuppressesScript()
		{
			Assert.That(IetfLanguageTag.Canonicalize("en-Latn-US"), Is.EqualTo("en-US"));
			Assert.That(IetfLanguageTag.Canonicalize("zh-hans-Cn-x-stuff"), Is.EqualTo("zh-CN-x-stuff"));
			Assert.That(IetfLanguageTag.Canonicalize("zH-hans-Cn"),
				Is.EqualTo(WellKnownSubtags.ChineseSimplifiedTag));
			Assert.That(IetfLanguageTag.Canonicalize("zH-Hant-tW-x-stuff"), Is.EqualTo("zh-TW-x-stuff"));
			Assert.That(IetfLanguageTag.Canonicalize("Zh-hant-Tw"), Is.EqualTo("zh-TW"));
			Assert.That(IetfLanguageTag.Canonicalize("oro-Latn"), Is.EqualTo("oro"));
		}

		[Test]
		public void Canonicalize_NonStandardCapitalization_StandardCapitalization()
		{
			Assert.That(IetfLanguageTag.Canonicalize("zH-latn-cn-FonIpa-X-Etic"), Is.EqualTo("zh-Latn-CN-fonipa-x-etic"));
		}

		[Test]
		public void Canonicalize_NonImplicitScript_DoesNotSuppressScript()
		{
			Assert.That(IetfLanguageTag.Canonicalize("en-Cyrl-US"), Is.EqualTo("en-Cyrl-US"));
			Assert.That(IetfLanguageTag.Canonicalize("sr-Latn"), Is.EqualTo("sr-Latn"));
		}

		#endregion

		#region ToICuLocale
		/// <summary>
		/// Tests the ToIcuLocale() method which converts a language tag to an ICU locale.
		/// </summary>
		[Test]
		public void ToIcuLocale_EnglishLanguageTags_ReturnsEnglishIcuLocales()
		{
			// language
			Assert.AreEqual("en", IetfLanguageTag.ToIcuLocale("en"));
			// language, script
			Assert.AreEqual("en", IetfLanguageTag.ToIcuLocale("en-Latn"));
			// language, region
			Assert.AreEqual("en_US", IetfLanguageTag.ToIcuLocale("en-US"));
			// language, script, region, ICU variant
			Assert.AreEqual("en_US_X_ETIC", IetfLanguageTag.ToIcuLocale("en-Latn-US-fonipa-x-etic"));
			// language, ICU variant
			Assert.AreEqual("en__X_EMIC", IetfLanguageTag.ToIcuLocale("en-fonipa-x-emic"));
		}

		[Test]
		public void ToIcuLocale_ChineseLanguageTag_ReturnsChineseIcuLocale()
		{
			// language, region, ICU variant
			Assert.AreEqual("zh_Latn_CN_X_PY", IetfLanguageTag.ToIcuLocale("zh-Latn-CN-pinyin"));
		}

		[Test]
		public void ToIcuLocale_PrivateUseLanguageTags_ReturnsPrivateUseIcuLocales()
		{
			// private use language
			Assert.AreEqual("xkal", IetfLanguageTag.ToIcuLocale("qaa-x-kal"));
			// private use language, ICU variant
			Assert.AreEqual("xkal__X_ETIC", IetfLanguageTag.ToIcuLocale("qaa-fonipa-x-kal-etic"));
			// private use language, private use region
			Assert.AreEqual("xkal_XA", IetfLanguageTag.ToIcuLocale("qaa-QM-x-kal-XA"));
			// private use language, private use script
			Assert.AreEqual("xkal_Fake", IetfLanguageTag.ToIcuLocale("qaa-Qaaa-x-kal-Fake"));
		}

		[Test]
		public void ToIcuLocale_UnknownLanguageTags_ReturnsIcuLocales()
		{
			// language, private use script
			Assert.AreEqual("en_Fake", IetfLanguageTag.ToIcuLocale("en-Qaaa-x-Fake"));
			// language, private use script, private use region
			Assert.AreEqual("en_Fake_QD", IetfLanguageTag.ToIcuLocale("en-Qaaa-QM-x-Fake-QD"));
			// private use language, script
			Assert.AreEqual("xzzz_Latn", IetfLanguageTag.ToIcuLocale("qaa-Latn-x-zzz"));
		}

		/// <summary>
		/// Tests the ToIcuLocale method with an invalid language tag.
		/// </summary>
		[Test]
		public void ToIcuLocale_InvalidLangTag_Throws()
		{
			Assert.Throws<ArgumentException>(()=>IetfLanguageTag.ToIcuLocale("en_Latn_US_X_ETIC"));
		}
		#endregion

		#region TryGetParts

		[TestCase("en", true, "en", null, null, null)]
		[TestCase("tpi-AR", true, "tpi", null, "AR", null)]
		[TestCase("tpi-Lepc-BR-fonipa-x-blah", true, "tpi", "Lepc", "BR", "fonipa-x-blah")]
		[TestCase("a", false, null, null, null, null)]
		[TestCase("qaa", true, "qaa", null, null, null)]
		[TestCase("qed", true, "qed", null, null, null)]
		[TestCase("qed-Lepc-x-rubbish", true, "qed", "Lepc", null, "x-rubbish")]
		public void TryGetParts_ReturnsExpectedResults(string tag, bool valid, string expectedLanguage, string expectedScript, string expectedRegion, string expectedVariant)
		{
			string language, script, region, variant;
			var result = IetfLanguageTag.TryGetParts(tag, out language, out script, out region, out variant);
			Assert.That(result, Is.EqualTo(valid));
			Assert.That(language, Is.EqualTo(expectedLanguage), "parsing " + tag + " produced unexpected language " + language + " instead of " + expectedLanguage);
			Assert.That(script, Is.EqualTo(expectedScript), "parsing " + tag + " produced unexpected script " + script + " instead of " + expectedScript);
			Assert.That(region, Is.EqualTo(expectedRegion), "parsing " + tag + " produced unexpected region " + region + " instead of " + expectedRegion);
			Assert.That(variant, Is.EqualTo(expectedVariant), "parsing " + tag + " produced unexpected variant " + variant + " instead of " + expectedVariant);
		}

		#endregion

		#region TryGetSubtags

		/// <summary>
		/// Tests the TryGetSubtags() method.
		/// </summary>
		[Test]
		public void TryGetSubtags_En_ReturnsEnLatn()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("en", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags),
				Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_EmptyVariant1_ReturnsEmpty()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("en-Latn", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_EmptyVariant2_ReturnsEmpty()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("en-US", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "US"));
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetVariantSubtags_ReturnsTwoVariantSubTags()
		{
			IEnumerable<VariantSubtag> variantSubtags;
			IetfLanguageTag.TryGetVariantSubtags("x-code1-code2", out variantSubtags, "x-name1,name2");
			Assert.AreEqual(2, variantSubtags.Count());
			var index = 0;
			foreach (VariantSubtag variantSubtag in variantSubtags)
			{
				if (index == 0) //For first VariantSubTag
				{
					Assert.That("code1", Is.EqualTo(variantSubtag.Code));
					Assert.That("x-name1", Is.EqualTo(variantSubtag.Name));
				}
				else //For second VariantSubTag
				{
					Assert.That("code2", Is.EqualTo(variantSubtag.Code));
					Assert.That("name2", Is.EqualTo(variantSubtag.Name));
				}
				index++;
			}
		}

		[TestCase("x-a#b")] // non alpha numeric
		[TestCase("x-thisiswaytoolong")] // > 15 characters (should be 8 but langtags.json has up to 15)
		public void TryGetVariantSubtags_PrivateUseRulesHonored(string privateUseVariantTag)
		{
			IEnumerable<VariantSubtag> variantSubtags;
			// Test both with and without custom name
			Assert.That(IetfLanguageTag.TryGetVariantSubtags(privateUseVariantTag, out variantSubtags, "Bad Variant"), Is.False);
			Assert.That(IetfLanguageTag.TryGetVariantSubtags(privateUseVariantTag, out variantSubtags), Is.False);
		}

		[Test]
		public void TryGetSubtags_FonipaXEtic_ReturnsFonipaEtic()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("en-Latn-US-fonipa-x-etic", out languageSubtag, out scriptSubtag,
					out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "US"));
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] {"fonipa", "etic"}));
		}

		[Test]
		public void TryGetSubtags_XKal_ReturnsKal()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("qaa-x-kal", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_XKalFake_ReturnsKalFake()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("qaa-Qaaa-x-kal-Fake", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_FullPUAConventionWithVariantAndPUAVariant_ReturnsAllParts()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(IetfLanguageTag.TryGetSubtags("qaa-Qaaa-QM-fonipa-x-kal-Fake-RG-extravar",
				out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			// SUT
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "RG"));
			CollectionAssert.IsNotEmpty(variantSubtags);
			CollectionAssert.AreEquivalent(new[] {"International Phonetic Alphabet", "extravar"},
				variantSubtags.Select(x => x.ToString()));
		}

		[Test]
		public void TryGetSubtags_InvalidConventionForScript_ReturnsPrivateUseScript()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(IetfLanguageTag.TryGetSubtags("en-Qaaa-x-toolong",
				out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.IsNull(regionSubtag);
			// SUT
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Qaaa"));
			Assert.IsTrue(scriptSubtag.IsPrivateUse);
		}

		[Test]
		public void TryGetSubtags_XKalXA_ReturnsEmptyScriptSubtag()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("qaa-QM-x-kal-XA", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "XA"));
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_XFakeQD_ReturnsEmptyVariantSubtags()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("en-Qaaa-QM-x-Fake-QD", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "QD"));
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_XETIC_ReturnsFalse()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("en_Latn_US_X_ETIC", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.False);
		}

		[Test]
		public void TryGetSubtags_XDupl0_ReturnsDupl0VariantSubtag()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			// Although dupl0 is in a position where it would normally be interpreted as a private language code, since it isn't a valid one,
			// we instead interpret it as simply a variant of qaa, the unknown language.
			Assert.That(IetfLanguageTag.TryGetSubtags("qaa-x-dupl0", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "qaa"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] {"dupl0"}));
		}

		[Test]
		public void TryGetSubtags_CompatibleForm_ReturnsScript()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("zh-cN-fonipa-x-etic", out languageSubtag, out scriptSubtag,
					out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag)"zh"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag)"Hans"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag)"CN"));
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] { "fonipa", "etic" }));
		}

		[Test]
		public void TryGetSubtags_SimplePrivateUseLanguage_ReturnsValidResults()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("qtz", out languageSubtag, out scriptSubtag,
					out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag)"qtz"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[0]));
		}

		[Test]
		public void TryGetSubtags_ComplexPrivateLanguageCode_ReturnsValidResults()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTag.TryGetSubtags("qed-Lepc-cN-fonipa-x-etic", out languageSubtag, out scriptSubtag,
					out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag)"qed"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag)"Lepc"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag)"CN"));
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] { "fonipa", "etic" }));
		}
		#endregion

		#region Get__Part
		[TestCase("en", ExpectedResult = "en")]
		[TestCase("tpi-AR", ExpectedResult = "tpi")]
		[TestCase("tpi-Lepc-BR-fonipa-x-blah", ExpectedResult = "tpi")]
		[TestCase("qaa", ExpectedResult = "qaa")]
		[TestCase("qed", ExpectedResult = "qed")]
		[TestCase("qed-Lepc-x-rubbish", ExpectedResult = "qed")]
		public string GetLanguagePart_ValidIetfTag_ReturnsLanguage(string tag)
		{
			return IetfLanguageTag.GetLanguagePart(tag);
		}

		[TestCase("a")]
		[TestCase("-")]
		public void GetLanguagePart_InvalidIetfTag_ThrowsArgumentException(string tag)
		{
			Assert.That(() => IetfLanguageTag.GetLanguagePart(tag), Throws.ArgumentException);
		}
		
		[TestCase("en", ExpectedResult = null)]
		[TestCase("tpi-AR", ExpectedResult = null)]
		[TestCase("tpi-Lepc-BR-fonipa-x-blah", ExpectedResult = "Lepc")]
		[TestCase("am-Ethi", ExpectedResult = "Ethi")]
		[TestCase("am-Latn", ExpectedResult = "Latn")]
		[TestCase("qed-Lepc-x-rubbish", ExpectedResult = "Lepc")]
		public string GetScriptPart_ValidIetfTag_ReturnsScript(string tag)
		{
			return IetfLanguageTag.GetScriptPart(tag);
		}

		[TestCase("a")]
		[TestCase("-")]
		[TestCase("qed-?~")]
		public void GetScriptPart_InvalidIetfTag_ThrowsArgumentException(string tag)
		{
			Assert.That(() => IetfLanguageTag.GetScriptPart(tag), Throws.ArgumentException);
		}
		
		[TestCase("en", ExpectedResult = null)]
		[TestCase("tpi-AR", ExpectedResult = "AR")]
		[TestCase("tpi-Lepc-BR-fonipa-x-blah", ExpectedResult = "BR")]
		[TestCase("am-Ethi", ExpectedResult = null)]
		[TestCase("qed-Lepc-x-rubbish", ExpectedResult = null)]
		public string GetRegionPart_ValidIetfTag_ReturnsRegion(string tag)
		{
			return IetfLanguageTag.GetRegionPart(tag);
		}

		[TestCase("--")]
		[TestCase("qed-?~")]
		public void GetRegionPart_InvalidIetfTag_ThrowsArgumentException(string tag)
		{
			Assert.That(() => IetfLanguageTag.GetRegionPart(tag), Throws.ArgumentException);
		}
		
		[TestCase("en", ExpectedResult = null)]
		[TestCase("tpi-AR", ExpectedResult = null)]
		[TestCase("tpi-Lepc-BR-fonipa-x-blah", ExpectedResult = "fonipa-x-blah")]
		[TestCase("am-Ethi", ExpectedResult = null)]
		[TestCase("qed-Lepc-x-rubbish", ExpectedResult = "x-rubbish")]
		public string GetVariantPart_ValidIetfTag_ReturnsVariant(string tag)
		{
			return IetfLanguageTag.GetVariantPart(tag);
		}

		[TestCase("---")]
		[TestCase("qed-?~")]
		public void GetVariantPart_InvalidIetfTag_ThrowsArgumentException(string tag)
		{
			Assert.That(() => IetfLanguageTag.GetVariantPart(tag), Throws.ArgumentException);
		}
		#endregion

		#region GetVariantCodes
		[Test]
		public void GetVariantCodes_EmptyVariants_ReturnsEmpty()
		{
			IEnumerable<VariantSubtag> variantSubtags = new VariantSubtag[] {};
			Assert.That(IetfLanguageTag.GetVariantCodes(variantSubtags), Is.Null);
			
		}

		[Test]
		public void GetVariantCodes_VariantsSet_ReturnsString()
		{
			IEnumerable<VariantSubtag> variantSubtags = new VariantSubtag[] { "fonipa", "etic" };
			Assert.That(IetfLanguageTag.GetVariantCodes(variantSubtags), Is.EqualTo("fonipa-x-etic"));
		}
		#endregion

		#region IsValidLanguageCode
		[Test]
		public void IsValidLanguageCode_InvalidLanguageCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTag.IsValidLanguageCode("a"), Is.False);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("0"), Is.False);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("abcdefghi"), Is.False);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("123456789"), Is.False);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("abcdefgh-abc-def-ghi-jkl"), Is.False);
		}

		[Test]
		public void IsValidLanguageCode_ValidLanguageCode_ReturnsTrue()
		{
			Assert.That(IetfLanguageTag.IsValidLanguageCode("ab"), Is.True);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("abcdefgh"), Is.True);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("abcdefgh-abc"), Is.True);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("abcdefgh-abc-def"), Is.True);
			Assert.That(IetfLanguageTag.IsValidLanguageCode("abcdefgh-abc-def-ghi"), Is.True);

		}
		#endregion

		#region IsValidScriptCode
		[Test]
		public void IsValidScriptCode_InvalidScriptCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTag.IsValidScriptCode("abc"), Is.False);
			Assert.That(IetfLanguageTag.IsValidScriptCode("abc1"), Is.False);
			Assert.That(IetfLanguageTag.IsValidScriptCode("abcde"), Is.False);
		}

		[Test]
		public void IsValidScriptCode_ValidScriptCode_ReturnsTrue()
		{
			Assert.That(IetfLanguageTag.IsValidScriptCode("abcd"), Is.True);
			Assert.That(IetfLanguageTag.IsValidScriptCode("ABCD"), Is.True);
			
		}
		#endregion

		#region IsValidRegionCode
		[Test]
		public void IsValidRegionCode_InvlidRegionCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTag.IsValidRegionCode("a"), Is.False);
			Assert.That(IetfLanguageTag.IsValidRegionCode("abc"), Is.False);
			Assert.That(IetfLanguageTag.IsValidRegionCode("12"), Is.False);
			Assert.That(IetfLanguageTag.IsValidRegionCode("1234"), Is.False);
		}

		[Test]
		public void IsValidRegionCode_ValidRegionCode_ReturnsTrue()
		{
			Assert.That(IetfLanguageTag.IsValidRegionCode("ab"), Is.True);
			Assert.That(IetfLanguageTag.IsValidRegionCode("AB"), Is.True);
			Assert.That(IetfLanguageTag.IsValidRegionCode("123"), Is.True);
		}
		#endregion

		#region IsValidPrivateUseCode
		[Test]
		public void IsValidPrivateUseCode_InvalidPrivateUseCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode(""), Is.False);
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode("abcdefghijklmnop"), Is.False); // temporarily not conforming to standard in the data
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode("1234567890123456"), Is.False);

		}

		[Test]
		public void IsValidPrivateUseCode_ValidPrivateUseCode_Returns_True()
		{
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode("a"), Is.True);
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode("A"), Is.True);
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode("1"), Is.True);
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode("abcdefgh"), Is.True);
			Assert.That(IetfLanguageTag.IsValidPrivateUseCode("12345678"), Is.True);
			
		}
		#endregion

		#region ToUniqueLanguageTag
		[Test]
		public void ToUniqueLanguageTag_IsAlreadyUnique_NothingChanges()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("de");
			ws.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(ws.LanguageTag, existingTags);
			Assert.That(ws.LanguageTag, Is.EqualTo("de"));
		}

		[Test]
		public void ToUniqueLanguageTag_IsNotUnique_DuplicateMarkerIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			ws.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(ws.LanguageTag, existingTags);
			Assert.That(ws.LanguageTag, Is.EqualTo("en-Zxxx-x-audio-dupl0"));
		}

		[Test]
		public void ToUniqueLanguageTag_ADuplicateAlreadyExists_DuplicatemarkerWithHigherNumberIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio", "en-Zxxx-x-audio-dupl0" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			ws.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(ws.LanguageTag, existingTags);
			Assert.That(ws.LanguageTag, Is.EqualTo("en-Zxxx-x-audio-dupl1"));
		}

		[Test]
		public void ToUniqueLanguageTag_ADuplicatewithHigherNumberAlreadyExists_DuplicateMarkerWithLowNumberIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio", "en-Zxxx-x-audio-dupl1" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			ws.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(ws.LanguageTag, existingTags);
			Assert.That(ws.LanguageTag, Is.EqualTo("en-Zxxx-x-audio-dupl0"));
		}

		[Test]
		public void ToUniqueLanguageTag_IdIsNull()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("de");
			ws.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(ws.LanguageTag, existingTags);
			Assert.That(ws.Id, Is.Null);
		}

		[Test]
		public void ToUniqueLanguageTag_LanguageTagAlreadyContainsADuplicateMarker_DuplicateNumberIsMaintainedAndNewOneIsIntroduced()
		{
			var existingTags = new[] { "en-Zxxx-x-dupl0-audio", "en-Zxxx-x-audio-dupl1" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-dupl0-audio");
			ws.LanguageTag = IetfLanguageTag.ToUniqueLanguageTag(ws.LanguageTag, existingTags);
			Assert.That(ws.LanguageTag, Is.EqualTo("en-Zxxx-x-dupl0-audio-dupl1"));
		}
		#endregion

		[TestCase("en", true)]
		[TestCase("en-Latn", true)]
		[TestCase("en-Arab", false)]
		[TestCase("en-Latn-US", true)]
		public void IsScriptImplied_ReturnsExpectedResults(string tag, bool expectedResult)
		{
			Assert.That(IetfLanguageTag.IsScriptImplied(tag), Is.EqualTo(expectedResult));
		}

		#region GetLocalizedLanguageName
		[TestCase("en", "en", ExpectedResult = "English")]
		[TestCase("en-Latn-US", "en", ExpectedResult = "English")]
		[TestCase("en-x-etic", "en-GB", ExpectedResult = "English")]
		[TestCase("en-Ethi", "en-GB", ExpectedResult = "English")]
		[TestCase("en-US", "en-US", ExpectedResult = "English")]
		[TestCase("en-US", "es", "English", ExpectedResult = "inglés")]
		[TestCase("en-x-etic", "fr-FR-x-unknown", "English", ExpectedResult = "anglais")]
		[TestCase("fr", "fr", ExpectedResult = "français")]
		[TestCase("es-419", "es-ES", ExpectedResult = "español")]
		[TestCase("zh-CN", "en", ExpectedResult = "Chinese (Simplified)")]
		[TestCase("pbu", "pbu", ExpectedResult = "پښتو")]
		[TestCase("pbu", "tpi", ExpectedResult = "پښتو")]
		[TestCase("prs", "en", ExpectedResult = "Dari")]
		[TestCase("tpi", "tpi", ExpectedResult = "Tok Pisin")]
		[TestCase("es", "tpi", ExpectedResult = "español")]
		[TestCase("es", "fr", "español", ExpectedResult = "espagnol")]
		[TestCase("es", "de", "español", ExpectedResult = "Spanisch")]
		[TestCase("pt-BR", "en-US", ExpectedResult = "Portuguese")]
		[TestCase("pt", "pt-PT", ExpectedResult = "português")]
		[TestCase("qaa-x-kal", "en", ExpectedResult = "Language Not Listed (qaa-x-kal)")]
		[TestCase("qaa-x-kal", "pbu", ExpectedResult = "Language Not Listed (qaa-x-kal)")]
		[TestCase("noh", "en", ExpectedResult = "Nomu")]
		[TestCase("noh", "noh", ExpectedResult = "Nomu")]
		// See REVIEW comment in IEtfLanguageTag.FixBotchedNativeName
		[TestCase("zh-CN", "zh-CN", "中文(中华人民共和国)", ExpectedResult = "中文(中国)")]
		// Not sure if this fallback for Dari is truly acceptable. Although it looks the same and
		// is what Windows supplies as the "Native Name", Wiktionary lists it as a different word
		// and says that the word we have hardcoded in IetfLanguageTags.GetNativeNameIfKnown is the
		// correct name. Wikipedia seems to agree. Interestingly, Google brings up the Wikipedia
		// info for Dari when you search for either one, even though the presumably incorrect
		// version does not actually appear on that Wikipedia page.
		[TestCase("prs", "prs", /*"درى",*/ ExpectedResult = "دری")]
		[TestCase("prs", "fr", /*"درى",*/ ExpectedResult = "دری")]
		public string GetLocalizedLanguageName_Valid_GetsNameInTargetLanguageOrNativeAsFallback(
			string tag, string uiTag, string acceptableFallback = null)
		{
			var uiLanguageCode = IetfLanguageTag.GetLanguagePart(uiTag);
			var resetUiCultureTo =
				CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != uiLanguageCode &&
				CultureInfo.CurrentUICulture.ThreeLetterISOLanguageName != uiLanguageCode ?
					CultureInfo.CurrentUICulture : null;
			if (resetUiCultureTo != null)
			{
				try
				{
					CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(uiLanguageCode);
				}
				catch (CultureNotFoundException)
				{
					if (PlatformUtilities.Platform.IsPreWindows10)
						Assert.Ignore(
							"This TestCase requires a culture which is not supported prior to Windows 10.");
					if (PlatformUtilities.Platform.IsLinux)
						Assert.Ignore(
							"This TestCase requires a culture which is not supported on this version of Linux.");
					throw;
				}
			}

			try
			{
				var result = IetfLanguageTag.GetLocalizedLanguageName(tag, uiTag);
				if (result == acceptableFallback)
					Assert.Ignore("Got an acceptable fallback name, but not what we really wanted.");
				if (tag == WellKnownSubtags.ChineseSimplifiedTag && uiTag == WellKnownSubtags.ChineseSimplifiedTag)
				{
					// In ICU, the parentheses are thin "Chinese" ones with included spacing.
					// That's fine.
					result = result.Replace("（", "(").Replace("）", ")");
				}
				return result;
			}
			finally
			{
				if (resetUiCultureTo != null)
					CultureInfo.CurrentUICulture = resetUiCultureTo;
			}
		}
		#endregion
		
		#region GetNativeLanguageNameWithEnglishSubtitle

		[TestCase("en", ExpectedResult = "English")]
		[TestCase("en-Latn-US", ExpectedResult = "English")]
		[TestCase("en-x-etic", ExpectedResult = "English")]
		[TestCase("en-Ethi", ExpectedResult = "English")]
		[TestCase("en-US", ExpectedResult = "English")]
		[TestCase("en-GB", ExpectedResult = "English")]
		[TestCase("en-x-etic", ExpectedResult = "English")]
		[TestCase("fr", ExpectedResult = "français")]
		[TestCase("es", ExpectedResult = "español")]
		[TestCase("es-419", ExpectedResult = "español")]
		[TestCase("pbu", ExpectedResult = "پښتو (Pashto)")]
		[TestCase("prs", ExpectedResult = "دری (Dari)")]
		[TestCase("tpi", ExpectedResult = "Tok Pisin")]
		[TestCase("pt", ExpectedResult = "português")]
		[TestCase("pt-BR", ExpectedResult = "português")]
		[TestCase("qaa-x-kal", ExpectedResult = "Language Not Listed (qaa-x-kal)")]
		[TestCase("noh", ExpectedResult = "noh (Nomu)")]
		public string GetNativeLanguageNameWithEnglishSubtitle_Valid_GetsNativeNamePlusEnglishAsNeeded(
			string tag)
		{
			return IetfLanguageTag.GetNativeLanguageNameWithEnglishSubtitle(tag);
		}

		[TestCase("zh-CN", "中文(中华人民共和国) (Chinese (Simplified))", "中文 (中国) (Chinese (Simplified))", ExpectedResult = "中文(中国) (Chinese (Simplified))")]
		[TestCase("zh-TW", "中文(中华人民共和国) (Chinese (Traditional))", "中文 (台灣) (Chinese (Traditional))", ExpectedResult = "中文(台灣) (Chinese (Traditional))")]
		public string GetNativeLanguageNameWithEnglishSubtitle_China_GetsNativeNamePlusEnglishAsNeeded(
			string tag, string acceptableResultPreWindows10, string acceptableResultLinux)
		{
			var result = IetfLanguageTag.GetNativeLanguageNameWithEnglishSubtitle(tag);
			if (result == acceptableResultPreWindows10 && PlatformUtilities.Platform.IsPreWindows10)
				Assert.Pass("Acceptable on older versions of the OS");
			if (result == acceptableResultLinux && PlatformUtilities.Platform.IsLinux)
				Assert.Pass("Extra space is acceptable");
			return result;
		}
		#endregion

		#region GetBestLanguageName
		/// <summary>
		/// No match at all.
		/// </summary>
		[Test]
		public void GetBestLanguageName_ForXYZ_FindsXYZ()
		{
			Assert.That(IetfLanguageTag.GetBestLanguageName("xyz", out var name), Is.False);
			Assert.That(name, Is.EqualTo("xyz"));
		}

		/// <summary>
		/// In this test, StandardSubtags.RegisteredLanguages has some, but none have the exact right code.
		/// </summary>
		[Test]
		public void GetBestLanguageName_ForArab_FindsArab()
		{
			Assert.That(IetfLanguageTag.GetBestLanguageName("arab", out var name), Is.False);
			Assert.That(name, Is.EqualTo("arab"));
		}

		/// <summary>
		/// We test various 2 and 3-letter codes to make sure they get the expected language name.
		/// We also make sure that various tags get stripped off when searching for Best Name.
		/// The method for getting the "General code" (w/o Script/Region/Variant codes) has an exception for Chinese.
		/// But this method doesn't trigger it (which is okay at this point).
		/// </summary>
		[Test]
		[TestCase("ara", "Arabic")] // classic 3-letter code
		[TestCase("fr", "French")] // classic 2-letter code
		[TestCase("nsk", "Naskapi")]
		[TestCase("nsk-Latn", "Naskapi")]
		[TestCase("nsk-Latn-x-Quebec", "Naskapi")]
		[TestCase("nsk-Latn-CA-x-Quebec", "Naskapi")]
		[TestCase("nsk-misc-garbage", "Naskapi")]
		[TestCase("shu", "Chadian Arabic")]
		[TestCase("shu-arab", "Chadian Arabic")]
		[TestCase("shu-latn", "Chadian Arabic")]
		[TestCase("sok", "Sokoro")] // Should not be required to have a '-Latn' tag.
		[TestCase("zh-CN", "Chinese")]
		[TestCase("zho", "Chinese")]
		public void GetBestLanguageName_ForLotsOfVariants_FindsExpectedName(string codeVariant, string expectedResult)
		{
			Assert.That(IetfLanguageTag.GetBestLanguageName(codeVariant, out var name), Is.True);
			Assert.That(name, Is.EqualTo(expectedResult));
		}
		#endregion
	}
}
