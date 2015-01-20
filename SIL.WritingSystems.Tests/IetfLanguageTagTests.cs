using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class IetfLanguageTagTests
	{
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

		//Split
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

		/// <summary>
		/// Tests the ToLanguageTag() method which converts an ICU locale to a language tag.
		/// </summary>
		[Test]
		public void ToLanguageTag()
		{
			// language
			Assert.AreEqual("en", IetfLanguageTag.ToLanguageTag("en"));
			// language, script
			Assert.AreEqual("en-Latn", IetfLanguageTag.ToLanguageTag("en_Latn"));
			// language, region
			Assert.AreEqual("en-US", IetfLanguageTag.ToLanguageTag("en_US"));
			// language, script, region, ICU variant
			Assert.AreEqual("en-Latn-US-fonipa-x-etic", IetfLanguageTag.ToLanguageTag("en_Latn_US_X_ETIC"));
			// language, ICU variant
			Assert.AreEqual("en-fonipa-x-emic", IetfLanguageTag.ToLanguageTag("en__X_EMIC"));
			// language, region, ICU variant
			Assert.AreEqual("zh-CN-pinyin", IetfLanguageTag.ToLanguageTag("zh_CN_X_PY"));
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
			// convert older FW language tags
			Assert.AreEqual("slu", IetfLanguageTag.ToLanguageTag("eslu"));
			// other possibilities from FW6.0.6
			Assert.AreEqual("qaa-x-bcd", IetfLanguageTag.ToLanguageTag("x123"));
			Assert.AreEqual("qaa-x-kac", IetfLanguageTag.ToLanguageTag("xka2"));

			// following are already lang tags
			Assert.AreEqual("en-US", IetfLanguageTag.ToLanguageTag("en-US"));
			Assert.AreEqual("en-Latn-US-fonipa-x-etic", IetfLanguageTag.ToLanguageTag("en-Latn-US-fonipa-x-etic"));
		}

		/// <summary>
		/// Tests the ToIcuLocale() method which converts a language tag to an ICU locale.
		/// </summary>
		[Test]
		public void ToIcuLocale()
		{
			// language
			Assert.AreEqual("en", IetfLanguageTag.ToIcuLocale("en"));
			// language, script
			Assert.AreEqual("en_Latn", IetfLanguageTag.ToIcuLocale("en-Latn"));
			// language, region
			Assert.AreEqual("en_US", IetfLanguageTag.ToIcuLocale("en-US"));
			// language, script, region, ICU variant
			Assert.AreEqual("en_Latn_US_X_ETIC", IetfLanguageTag.ToIcuLocale("en-Latn-US-fonipa-x-etic"));
			// language, ICU variant
			Assert.AreEqual("en__X_EMIC", IetfLanguageTag.ToIcuLocale("en-fonipa-x-emic"));
			// language, region, ICU variant
			Assert.AreEqual("zh_CN_X_PY", IetfLanguageTag.ToIcuLocale("zh-CN-pinyin"));
			// private use language
			Assert.AreEqual("xkal", IetfLanguageTag.ToIcuLocale("qaa-x-kal"));
			// private use language, ICU variant
			Assert.AreEqual("xkal__X_ETIC", IetfLanguageTag.ToIcuLocale("qaa-fonipa-x-kal-etic"));
			// private use language, private use region
			Assert.AreEqual("xkal_XA", IetfLanguageTag.ToIcuLocale("qaa-QM-x-kal-XA"));
			// private use language, private use script
			Assert.AreEqual("xkal_Fake", IetfLanguageTag.ToIcuLocale("qaa-Qaaa-x-kal-Fake"));
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
		[ExpectedException(typeof(ArgumentException))]
		public void ToIcuLocale_InvalidLangTag()
		{
			IetfLanguageTag.ToIcuLocale("en_Latn_US_X_ETIC");
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
			Assert.That(IetfLanguageTag.TryGetSubtags("en", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTag.TryGetSubtags("en-Latn", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTag.TryGetSubtags("en-US", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "US"));
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTag.TryGetSubtags("en-Latn-US-fonipa-x-etic", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Latn"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "US"));
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] {"fonipa", "etic"}));

			Assert.That(IetfLanguageTag.TryGetSubtags("qaa-x-kal", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTag.TryGetSubtags("qaa-Qaaa-x-kal-Fake", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTag.TryGetSubtags("qaa-QM-x-kal-XA", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "kal"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "XA"));
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTag.TryGetSubtags("en-Qaaa-QM-x-Fake-QD", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "en"));
			Assert.That(scriptSubtag, Is.EqualTo((ScriptSubtag) "Fake"));
			Assert.That(regionSubtag, Is.EqualTo((RegionSubtag) "QD"));
			Assert.That(variantSubtags, Is.Empty);

			Assert.That(IetfLanguageTag.TryGetSubtags("en_Latn_US_X_ETIC", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.False);

			// Although dupl0 is in a position where it would normally be interpreted as a private language code, since it isn't a valid one,
			// we instead interpret it as simply a variant of qaa, the unknown language.
			Assert.That(IetfLanguageTag.TryGetSubtags("qaa-x-dupl0", out languageSubtag, out scriptSubtag, out regionSubtag, out variantSubtags), Is.True);
			Assert.That(languageSubtag, Is.EqualTo((LanguageSubtag) "qaa"));
			Assert.That(scriptSubtag, Is.Null);
			Assert.That(regionSubtag, Is.Null);
			Assert.That(variantSubtags, Is.EqualTo(new VariantSubtag[] {"dupl0"}));
		}
	}
}
