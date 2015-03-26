using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class IetfLanguageTagHelperTests
	{
		#region ConcatenateVariantAndPrivateUse
		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantOnly_ReturnsVariant()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("1901", string.Empty);
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithxDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("1901", "x-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithoutxDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("1901", "audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithoutxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("", "audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("", "x-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("x-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_PrivateUseWithCapitalXDashOnly_ReturnsPrivateUseWithxDash()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("", "X-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("X-audio"));
		}

		[Test]
		public void ConcatenateVariantAndPrivateUse_VariantAndPrivateUseWithCapitalXDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("1901", "X-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("1901-X-audio"));
		}

		//this test shows that there is no checking involved as to wether your variants and private use are rfc/writingsystemdefinition conform. All the method does is glue two strings together while handling the "x-"
		[Test]
		public void ConcatenateVariantAndPrivateUse_BogusVariantBadprivateUse_HappilyGluesTheTwoTogether()
		{
			string concatenatedVariantAndPrivateUse = IetfLanguageTagHelper.ConcatenateVariantAndPrivateUse("bogusvariant", "etic-emic-audio");
			Assert.That(concatenatedVariantAndPrivateUse, Is.EqualTo("bogusvariant-x-etic-emic-audio"));
		}
		#endregion

		#region SplitVariantAndPrivateUse
		[Test]
		public void SplitVariantAndPrivateUse_VariantOnly_ReturnsVariant()
		{
			string variant;
			string privateUse;
			IetfLanguageTagHelper.SplitVariantAndPrivateUse("1901", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo(String.Empty));
		}

		[Test]
		public void SplitVariantAndPrivateUse_VariantAndPrivateUse_ReturnsVariantAndPrivateUse()
		{
			string variant;
			string privateUse;
			IetfLanguageTagHelper.SplitVariantAndPrivateUse("1901-x-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_NoxDash_ReturnsVariantOnly()
		{
			string variant;
			string privateUse;
			IetfLanguageTagHelper.SplitVariantAndPrivateUse("1901-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901-audio"));
			Assert.That(privateUse, Is.EqualTo(String.Empty));
		}

		[Test]
		public void SplitVariantAndPrivateUse_PrivateUseWithxDashOnly_ReturnsPrivateUseWithxDash()
		{
			string variant;
			string privateUse;
			IetfLanguageTagHelper.SplitVariantAndPrivateUse("x-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo(String.Empty));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_PrivateUseWithCapitalXDashOnly_ReturnsPrivateUseWithxDash()
		{
			string variant;
			string privateUse;
			IetfLanguageTagHelper.SplitVariantAndPrivateUse("X-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo(String.Empty));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		[Test]
		public void SplitVariantAndPrivateUse_VariantAndPrivateUseWithCapitalXDash_ReturnsConcatenatedVariantAndPrivateUse()
		{
			string variant;
			string privateUse;
			IetfLanguageTagHelper.SplitVariantAndPrivateUse("1901-X-audio", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("1901"));
			Assert.That(privateUse, Is.EqualTo("audio"));
		}

		//this test shows that there is no checking involved as to wether your variants and private use are rfc/writingsystemdefinition conform. All the method does is split on x-
		[Test]
		public void SplitVariantAndPrivateUse_BogusVariantBadPrivateUse_HappilysplitsOnxDash()
		{
			string variant;
			string privateUse;
			IetfLanguageTagHelper.SplitVariantAndPrivateUse("bogusVariant-X-audio-emic-etic", out variant, out privateUse);
			Assert.That(variant, Is.EqualTo("bogusVariant"));
			Assert.That(privateUse, Is.EqualTo("audio-emic-etic"));
		}
		#endregion

		#region ToIetfLanguageTag
		/// <summary>
		/// Tests the ToLanguageTag() method which converts an ICU locale to a language tag.
		/// </summary>
		[Test]
		public void ToIetfLanguageTag_EnglishIcuLocales_ReturnsEnglishLanguageTags()
		{
			// language
			Assert.AreEqual("en", IetfLanguageTagHelper.ToIetfLanguageTag("en"));
			// language, script
			Assert.AreEqual("en", IetfLanguageTagHelper.ToIetfLanguageTag("en_Latn"));
			// language, region
			Assert.AreEqual("en-US", IetfLanguageTagHelper.ToIetfLanguageTag("en_US"));
			// language, script, region, ICU variant
			Assert.AreEqual("en-US-fonipa-x-etic", IetfLanguageTagHelper.ToIetfLanguageTag("en_Latn_US_X_ETIC"));
			// language, ICU variant
			Assert.AreEqual("en-fonipa-x-emic", IetfLanguageTagHelper.ToIetfLanguageTag("en__X_EMIC"));
		}

		[Test]
		public void ToIetfLanguageTag_ChinesePinyinIcuLocale_ReturnsChineseLanguageTag()
		{
			// language, region, ICU variant
			Assert.AreEqual("zh-CN-pinyin", IetfLanguageTagHelper.ToIetfLanguageTag("zh_CN_X_PY"));
		}

		[Test]
		public void ToIetfLanguageTag_PrivateUseIcuLocales_ReturnsPrivateUseLanguageTags()
		{
			// private use language
			Assert.AreEqual("qaa-x-kal", IetfLanguageTagHelper.ToIetfLanguageTag("xkal"));
			// private use language, custom ICU variant
			Assert.AreEqual("qaa-fonipa-x-kal", IetfLanguageTagHelper.ToIetfLanguageTag("xkal__IPA"));
			// private use language, (standard) private use region
			Assert.AreEqual("qaa-XA-x-kal", IetfLanguageTagHelper.ToIetfLanguageTag("xkal_XA"));
			// private use language, (non-standard) private use script
			Assert.AreEqual("qaa-Qaaa-x-kal-Fake", IetfLanguageTagHelper.ToIetfLanguageTag("xkal_Fake"));
			// language, private use script
			Assert.AreEqual("en-Qaaa-x-Fake", IetfLanguageTagHelper.ToIetfLanguageTag("en_Fake"));
			// language, private use script, private use region
			Assert.AreEqual("en-Qaaa-QM-x-Fake-QD", IetfLanguageTagHelper.ToIetfLanguageTag("en_Fake_QD"));
			// private use language, script
			Assert.AreEqual("qaa-Latn-x-zzz", IetfLanguageTagHelper.ToIetfLanguageTag("zzz_Latn"));
		}

		[Test]
		public void ToIetfLanguageTag_FWIcuLocales_ReturnsLanguageTags()
		{
			// convert older FW language tags
			Assert.AreEqual("slu", IetfLanguageTagHelper.ToIetfLanguageTag("eslu"));
			// other possibilities from FW6.0.6
			Assert.AreEqual("qaa-x-bcd", IetfLanguageTagHelper.ToIetfLanguageTag("x123"));
			Assert.AreEqual("qaa-x-kac", IetfLanguageTagHelper.ToIetfLanguageTag("xka2"));
		}

		[Test]
		public void ToIetfLanguageTag_AlreadyLanguageTag_NoChange()
		{
			// following are already lang tags
			Assert.AreEqual("en-US", IetfLanguageTagHelper.ToIetfLanguageTag("en-US"));
			Assert.AreEqual("en-Latn-US-fonipa-x-etic", IetfLanguageTagHelper.ToIetfLanguageTag("en-Latn-US-fonipa-x-etic"));
		}
		#endregion

		#region CreateIetfLanguageTag
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void CreateIetfLanguageTag_InvalidTag_Throws()
		{
			IetfLanguageTagHelper.CreateIetfLanguageTag("a", "", "", string.Empty);
		}

		[Test]
		public void CreateIetfLanguageTag_InvalidTagValidateDisabled_DoesNotThrow()
		{
			Assert.That(IetfLanguageTagHelper.CreateIetfLanguageTag("a", "", "", string.Empty, false), Is.EqualTo("a"));
		}

		[Test]
		public void CreateIetfLanguageTag_ImplicitScript_SuppressesScript()
		{
			Assert.That(IetfLanguageTagHelper.CreateIetfLanguageTag("en", "Latn", "US", string.Empty), Is.EqualTo("en-US"));
			Assert.That(IetfLanguageTagHelper.CreateIetfLanguageTag("en", "Latn", "US", Enumerable.Empty<VariantSubtag>()), Is.EqualTo("en-US"));
		}

		[Test]
		public void CreateIetfLanguageTag_ValidTag_ReturnsTag()
		{
			Assert.That(IetfLanguageTagHelper.CreateIetfLanguageTag("en", "Arab", "GB", "1996"), Is.EqualTo("en-Arab-GB-1996"));
		}

		[Test]
		public void CreateIetfLanguageTag_ValidPrivateUseTag_ReturnsTag()
		{
			Assert.That(IetfLanguageTagHelper.CreateIetfLanguageTag("qaa", "Zxxx", "GB", "fonipa-x-emic-Jimmy"), Is.EqualTo("qaa-Zxxx-GB-fonipa-x-emic-Jimmy"));
		}
		#endregion

		#region Validate

		[Test]
		public void Validate_InvalidTag_ReturnsFalse()
		{
			string message;
			Assert.That(
				IetfLanguageTagHelper.Validate(new LanguageSubtag("a"), new ScriptSubtag(""), new RegionSubtag(""),
					new List<VariantSubtag>(), out message), Is.False);
		}

		[Test]
		public void Validate_ValidTag_ReturnsTrue()
		{
			string message;
			Assert.That(
				IetfLanguageTagHelper.Validate(new LanguageSubtag("en"), new ScriptSubtag("Latn"), new RegionSubtag("US"),
					new List<VariantSubtag>(), out message), Is.True);
		}
		#endregion

		#region Canonicalize
		[Test]
		public void Canonicalize_ImplicitScript_SuppressesScript()
		{
			Assert.That(IetfLanguageTagHelper.Canonicalize("en-Latn-US"), Is.EqualTo("en-US"));
		}

		[Test]
		public void Canonicalize_NonStandardCapitalization_StandardCapitalization()
		{
			Assert.That(IetfLanguageTagHelper.Canonicalize("zH-latn-cn-FonIpa-X-Etic"), Is.EqualTo("zh-Latn-CN-fonipa-x-etic"));
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
			Assert.AreEqual("en", IetfLanguageTagHelper.ToIcuLocale("en"));
			// language, script
			Assert.AreEqual("en", IetfLanguageTagHelper.ToIcuLocale("en-Latn"));
			// language, region
			Assert.AreEqual("en_US", IetfLanguageTagHelper.ToIcuLocale("en-US"));
			// language, script, region, ICU variant
			Assert.AreEqual("en_US_X_ETIC", IetfLanguageTagHelper.ToIcuLocale("en-Latn-US-fonipa-x-etic"));
			// language, ICU variant
			Assert.AreEqual("en__X_EMIC", IetfLanguageTagHelper.ToIcuLocale("en-fonipa-x-emic"));
		}

		[Test]
		public void ToIcuLocale_ChineseLanguageTag_ReturnsChineseIcuLocale()
		{
			// language, region, ICU variant
			Assert.AreEqual("zh_CN_X_PY", IetfLanguageTagHelper.ToIcuLocale("zh-CN-pinyin"));
		}

		[Test]
		public void ToIcuLocale_PrivateUseLanguageTags_ReturnsPrivateUseIcuLocales()
		{
			// private use language
			Assert.AreEqual("xkal", IetfLanguageTagHelper.ToIcuLocale("qaa-x-kal"));
			// private use language, ICU variant
			Assert.AreEqual("xkal__X_ETIC", IetfLanguageTagHelper.ToIcuLocale("qaa-fonipa-x-kal-etic"));
			// private use language, private use region
			Assert.AreEqual("xkal_XA", IetfLanguageTagHelper.ToIcuLocale("qaa-QM-x-kal-XA"));
			// private use language, private use script
			Assert.AreEqual("xkal_Fake", IetfLanguageTagHelper.ToIcuLocale("qaa-Qaaa-x-kal-Fake"));
		}

		[Test]
		public void ToIcuLocale_UnknownLanguageTags_ReturnsIcuLocales()
		{
			// language, private use script
			Assert.AreEqual("en_Fake", IetfLanguageTagHelper.ToIcuLocale("en-Qaaa-x-Fake"));
			// language, private use script, private use region
			Assert.AreEqual("en_Fake_QD", IetfLanguageTagHelper.ToIcuLocale("en-Qaaa-QM-x-Fake-QD"));
			// private use language, script
			Assert.AreEqual("xzzz_Latn", IetfLanguageTagHelper.ToIcuLocale("qaa-Latn-x-zzz"));
		}

		/// <summary>
		/// Tests the ToIcuLocale method with an invalid language tag.
		/// </summary>
		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ToIcuLocale_InvalidLangTag_Throws()
		{
			IetfLanguageTagHelper.ToIcuLocale("en_Latn_US_X_ETIC");
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
				IetfLanguageTagHelper.TryGetSubtags("en", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags),
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
				IetfLanguageTagHelper.TryGetSubtags("en-Latn", out languageSubtag, out scriptSubtag, out regionSubtag,
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
				IetfLanguageTagHelper.TryGetSubtags("en-US", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "US"));
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_FonipaXEtic_ReturnsFonipaEtic()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTagHelper.TryGetSubtags("en-Latn-US-fonipa-x-etic", out languageSubtag, out scriptSubtag,
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
				IetfLanguageTagHelper.TryGetSubtags("qaa-x-kal", out languageSubtag, out scriptSubtag, out regionSubtag,
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
				IetfLanguageTagHelper.TryGetSubtags("qaa-Qaaa-x-kal-Fake", out languageSubtag, out scriptSubtag, out regionSubtag,
					out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);
		}

		[Test]
		public void TryGetSubtags_XKalXA_ReturnsEmptyScriptSubtag()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(
				IetfLanguageTagHelper.TryGetSubtags("qaa-QM-x-kal-XA", out languageSubtag, out scriptSubtag, out regionSubtag,
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
				IetfLanguageTagHelper.TryGetSubtags("en-Qaaa-QM-x-Fake-QD", out languageSubtag, out scriptSubtag, out regionSubtag,
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
				IetfLanguageTagHelper.TryGetSubtags("en_Latn_US_X_ETIC", out languageSubtag, out scriptSubtag, out regionSubtag,
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
			Assert.That(IetfLanguageTagHelper.TryGetSubtags("qaa-x-dupl0", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "qaa"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] {"dupl0"}));
		}
		#endregion

		#region GetVariantCodes
		[Test]
		public void GetVariantCodes_EmptyVariants_ReturnsEmpty()
		{
			IEnumerable<VariantSubtag> variantSubtags = new VariantSubtag[] {};
			Assert.That(IetfLanguageTagHelper.GetVariantCodes(variantSubtags), Is.Null);
			
		}

		[Test]
		public void GetVariantCodes_VariantsSet_ReturnsString()
		{
			IEnumerable<VariantSubtag> variantSubtags = new VariantSubtag[] { "fonipa", "etic" };
			Assert.That(IetfLanguageTagHelper.GetVariantCodes(variantSubtags), Is.EqualTo("fonipa-x-etic"));
		}
		#endregion

		#region IsValidLanguageCode
		[Test]
		public void IsValidLanguageCode_InvalidLanguageCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("a"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("0"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("abcdefghi"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("123456789"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("abcdefgh-abc-def-ghi-jkl"), Is.False);
		}

		[Test]
		public void IsValidLanguageCode_ValidLanguageCode_ReturnsTrue()
		{
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("ab"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("abcdefgh"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("abcdefgh-abc"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("abcdefgh-abc-def"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidLanguageCode("abcdefgh-abc-def-ghi"), Is.True);

		}
		#endregion

		#region IsValidScriptCode
		[Test]
		public void IsValidScriptCode_InvalidScriptCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTagHelper.IsValidScriptCode("abc"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidScriptCode("abc1"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidScriptCode("abcde"), Is.False);
		}

		[Test]
		public void IsValidScriptCode_ValidScriptCode_ReturnsTrue()
		{
			Assert.That(IetfLanguageTagHelper.IsValidScriptCode("abcd"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidScriptCode("ABCD"), Is.True);
			
		}
		#endregion

		#region IsValidRegionCode
		[Test]
		public void IsValidRegionCode_InvlidRegionCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTagHelper.IsValidRegionCode("a"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidRegionCode("abc"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidRegionCode("12"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidRegionCode("1234"), Is.False);
		}

		[Test]
		public void IsValidRegionCode_ValidRegionCode_ReturnsTrue()
		{
			Assert.That(IetfLanguageTagHelper.IsValidRegionCode("ab"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidRegionCode("AB"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidRegionCode("123"), Is.True);
		}
		#endregion

		#region IsValidPrivateUseCode
		[Test]
		public void IsValidPrivateUseCode_InvalidPrivateUseCode_ReturnsFalse()
		{
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode(""), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode("abcdefghi"), Is.False);
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode("123456789"), Is.False);

		}

		[Test]
		public void IsValidPrivateUseCode_ValidPrivateUseCode_Returns_True()
		{
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode("a"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode("A"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode("1"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode("abcdefgh"), Is.True);
			Assert.That(IetfLanguageTagHelper.IsValidPrivateUseCode("12345678"), Is.True);
			
		}
		#endregion

		#region ToUniqueIetfLanguageTag
		[Test]
		public void ToUniqueIetfLanguageTag_IsAlreadyUnique_NothingChanges()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("de");
			ws.IetfLanguageTag = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(ws.IetfLanguageTag, existingTags);
			Assert.That(ws.IetfLanguageTag, Is.EqualTo("de"));
		}

		[Test]
		public void ToUniqueIetfLanguageTag_IsNotUnique_DuplicateMarkerIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			ws.IetfLanguageTag = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(ws.IetfLanguageTag, existingTags);
			Assert.That(ws.IetfLanguageTag, Is.EqualTo("en-Zxxx-x-audio-dupl0"));
		}

		[Test]
		public void ToUniqueIetfLanguageTag_ADuplicateAlreadyExists_DuplicatemarkerWithHigherNumberIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio", "en-Zxxx-x-audio-dupl0" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			ws.IetfLanguageTag = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(ws.IetfLanguageTag, existingTags);
			Assert.That(ws.IetfLanguageTag, Is.EqualTo("en-Zxxx-x-audio-dupl1"));
		}

		[Test]
		public void ToUniqueIetfLanguageTag_ADuplicatewithHigherNumberAlreadyExists_DuplicateMarkerWithLowNumberIsAppended()
		{
			var existingTags = new[] { "en-Zxxx-x-audio", "en-Zxxx-x-audio-dupl1" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-audio");
			ws.IetfLanguageTag = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(ws.IetfLanguageTag, existingTags);
			Assert.That(ws.IetfLanguageTag, Is.EqualTo("en-Zxxx-x-audio-dupl0"));
		}

		[Test]
		public void ToUniqueIetfLanguageTag_IdIsNull()
		{
			var existingTags = new[] { "en-Zxxx-x-audio" };
			var ws = new WritingSystemDefinition("de");
			ws.IetfLanguageTag = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(ws.IetfLanguageTag, existingTags);
			Assert.That(ws.Id, Is.Null);
		}

		[Test]
		public void ToUniqueIetfLanguageTag_IetfLanguageTagAlreadyContainsADuplicateMarker_DuplicateNumberIsMaintainedAndNewOneIsIntroduced()
		{
			var existingTags = new[] { "en-Zxxx-x-dupl0-audio", "en-Zxxx-x-audio-dupl1" };
			var ws = new WritingSystemDefinition("en-Zxxx-x-dupl0-audio");
			ws.IetfLanguageTag = IetfLanguageTagHelper.ToUniqueIetfLanguageTag(ws.IetfLanguageTag, existingTags);
			Assert.That(ws.IetfLanguageTag, Is.EqualTo("en-Zxxx-x-dupl0-audio-dupl1"));
		}
		#endregion
	}
}
