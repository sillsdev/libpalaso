using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class IetfLanguageTagHelperTests
	{
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

		//Split
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

		/// <summary>
		/// Tests the ToLanguageTag() method which converts an ICU locale to a language tag.
		/// </summary>
		[Test]
		public void ToIetfLanguageTag()
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
			// language, region, ICU variant
			Assert.AreEqual("zh-CN-pinyin", IetfLanguageTagHelper.ToIetfLanguageTag("zh_CN_X_PY"));
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
			// convert older FW language tags
			Assert.AreEqual("slu", IetfLanguageTagHelper.ToIetfLanguageTag("eslu"));
			// other possibilities from FW6.0.6
			Assert.AreEqual("qaa-x-bcd", IetfLanguageTagHelper.ToIetfLanguageTag("x123"));
			Assert.AreEqual("qaa-x-kac", IetfLanguageTagHelper.ToIetfLanguageTag("xka2"));

			// following are already lang tags
			Assert.AreEqual("en-US", IetfLanguageTagHelper.ToIetfLanguageTag("en-US"));
			Assert.AreEqual("en-Latn-US-fonipa-x-etic", IetfLanguageTagHelper.ToIetfLanguageTag("en-Latn-US-fonipa-x-etic"));
		}

		[Test]
		public void CreateIetfLanguageTag_ImplicitScript_SuppressesScript()
		{
			Assert.That(IetfLanguageTagHelper.CreateIetfLanguageTag("en", "Latn", "US", string.Empty), Is.EqualTo("en-US"));
			Assert.That(IetfLanguageTagHelper.CreateIetfLanguageTag("en", "Latn", "US", Enumerable.Empty<VariantSubtag>()), Is.EqualTo("en-US"));
		}

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

		/// <summary>
		/// Tests the ToIcuLocale() method which converts a language tag to an ICU locale.
		/// </summary>
		[Test]
		public void ToIcuLocale()
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
			// language, region, ICU variant
			Assert.AreEqual("zh_CN_X_PY", IetfLanguageTagHelper.ToIcuLocale("zh-CN-pinyin"));
			// private use language
			Assert.AreEqual("xkal", IetfLanguageTagHelper.ToIcuLocale("qaa-x-kal"));
			// private use language, ICU variant
			Assert.AreEqual("xkal__X_ETIC", IetfLanguageTagHelper.ToIcuLocale("qaa-fonipa-x-kal-etic"));
			// private use language, private use region
			Assert.AreEqual("xkal_XA", IetfLanguageTagHelper.ToIcuLocale("qaa-QM-x-kal-XA"));
			// private use language, private use script
			Assert.AreEqual("xkal_Fake", IetfLanguageTagHelper.ToIcuLocale("qaa-Qaaa-x-kal-Fake"));
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
		public void ToIcuLocale_InvalidLangTag()
		{
			IetfLanguageTagHelper.ToIcuLocale("en_Latn_US_X_ETIC");
		}

		/// <summary>
		/// Tests the TryGetSubtags() method.
		/// </summary>
		[Test]
		public void TryGetSubtags()
		{
			LanguageSubtag languageSubtag;
			ScriptSubtag scriptSubtag;
			RegionSubtag regionSubtag;
			IEnumerable<VariantSubtag> variantSubtags;
			Assert.That(IetfLanguageTagHelper.TryGetSubtags("en", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("en-Latn", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("en-US", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "US"));
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("en-Latn-US-fonipa-x-etic", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "US"));
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] {"fonipa", "etic"}));

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("qaa-x-kal", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("qaa-Qaaa-x-kal-Fake", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("qaa-QM-x-kal-XA", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "XA"));
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("en-Qaaa-QM-x-Fake-QD", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "QD"));
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTagHelper.TryGetSubtags("en_Latn_US_X_ETIC", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.False);

			// Although dupl0 is in a position where it would normally be interpreted as a private language code, since it isn't a valid one,
			// we instead interpret it as simply a variant of qaa, the unknown language.
			Assert.That(IetfLanguageTagHelper.TryGetSubtags("qaa-x-dupl0", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "qaa"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] {"dupl0"}));
		}
	}
}
