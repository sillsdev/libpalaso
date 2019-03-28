using NUnit.Framework;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems.Tests.Migration
{
	[TestFixture]
	public class IetfLanguageTagCleanerTests
	{
		[Test]
		public void CompleteTagConstructor_HasInvalidLanguageName_MovedToPrivateUse()
		{
			var cleaner = new IetfLanguageTagCleaner("234");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-x-234"));
		}

		[Test]
		public void CompleteTagConstructor_HasLanguageNameAndOtherName_OtherNameMovedToPrivateUse()
		{
			var cleaner = new IetfLanguageTagCleaner("abc-123");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("abc-x-123"));
		}

		[Test]
		public void CompleteTagConstructor_LanguageNameWithAudio_GetZxxxAdded()
		{
			var cleaner = new IetfLanguageTagCleaner("aaa-x-audio");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("aaa-Zxxx-x-audio"));
		}

		[Test]
		public void CompleteTagConstructor_InvalidLanguageNameWithScript_QaaAdded()
		{
			var cleaner = new IetfLanguageTagCleaner("wee-Latn");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-Latn-x-wee"));

			// Also when initially "Latn" is properly in the Script field.
			cleaner = new IetfLanguageTagCleaner("wee", "Latn", "", "", "");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-Latn-x-wee"));
		}

		[Test]
		public void CompleteTagConstructor_XDashBeforeValidLanguageNameInVariant_NoChange()
		{
			var cleaner = new IetfLanguageTagCleaner("", "", "", "x-de", "");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("x-de"));
		}

		[Test]
		public void ValidLanguageCodeMarkedPrivate_InsertsQaa()
		{
			var cleaner = new IetfLanguageTagCleaner("x-de", "", "", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "qaa", "", "", "", "qaa-x-de");
		}

		[Test]
		public void Language_XDashBeforeString_AddsQaa()
		{
			var cleaner = new IetfLanguageTagCleaner("x-blah");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-x-blah"));
		}

		[Test]
		public void CompleteTagConstructor_QaaWithXDashBeforeValidLanguageName_NoChange()
		{
			var cleaner = new IetfLanguageTagCleaner("qaa-x-th");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-x-th"));
			Assert.That(cleaner.Language, Is.EqualTo("qaa"));
			Assert.That(cleaner.PrivateUse, Is.EqualTo("th"));
		}

		[Test]
		public void CompleteTagConstructor_TagContainsPrivateUseWithAdditionalXDash_RedundantXDashRemoved()
		{
			var cleaner = new IetfLanguageTagCleaner("en-x-some-x-whatever");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("en-x-some-whatever"));
		}

		[Test]
		public void CompleteTagConstructor_TagContainsOnlyPrivateUseWithAdditionalXDash_RedundantXDashRemoved()
		{
			var cleaner = new IetfLanguageTagCleaner("x-some-x-whatever");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-x-some-whatever"));
		}

		[Test]
		public void CompleteTagConstructor_PrivateUseWithAudioAndDuplicateX_MakesAudioTag()
		{
			var cleaner = new IetfLanguageTagCleaner("x-en-Zxxx-x-audio");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-Zxxx-x-en-Zxxx-audio"));
		}

		[Test]
		public void CompleteTagConstructor_ValidRfctagWithPrivateUseElements_NoChange()
		{
			var cleaner = new IetfLanguageTagCleaner("qaa-Zxxx-x-Zxxx-AUDIO");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("qaa-Zxxx-x-Zxxx-AUDIO"));
			// Except, it should have moved things from Language, where the constructor puts them, to the appropriate places.
			Assert.That(cleaner.Language, Is.EqualTo("qaa"));
			Assert.That(cleaner.Script, Is.EqualTo("Zxxx"));
			Assert.That(cleaner.PrivateUse, Is.EqualTo("Zxxx-AUDIO"));
		}

		[Test]
		public void CompleteTagConstructor_ValidRfctagWithLegacyIso3Code_MigratesToRfc2LetterCode()
		{
			var cleaner = new IetfLanguageTagCleaner("eng");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("en"));
		}

		[Test]
		public void CompleteTagConstructor_ValidRfctagWithLegacyIso3CodeAndPrivateUse_MigratesToRfc2LetterCodeAndPrivateUse()
		{
			var cleaner = new IetfLanguageTagCleaner("eng-bogus");
			cleaner.Clean();
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo("en-x-bogus"));
		}

		[Test]
		public void AllPrivateComponents_InsertsStandardPrivateCodes()
		{
			var cleaner = new IetfLanguageTagCleaner("x-kal", "x-script", "x-RG", "fonipa-x-etic", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "qaa", "Qaaa", "QM", "fonipa", "qaa-Qaaa-QM-fonipa-x-kal-script-RG-etic");
		}

		[Test]
		public void PrivateScriptKnownLanguage_InsertsPrivateScriptCode()
		{
			var cleaner = new IetfLanguageTagCleaner("fr", "x-script", "", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "fr", "Qaaa", "", "", "fr-Qaaa-x-script");

		}

		[Test]
		public void PrivateScriptKnownLanguageAndRegion_InsertsPrivateScriptCode()
		{
			var cleaner = new IetfLanguageTagCleaner("fr", "x-script", "NO", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "fr", "Qaaa", "NO", "", "fr-Qaaa-NO-x-script");
		}

		[Test]
		public void PrivateRegionKnownLanguageAndScript_InsertsPrivateRegionCode()
		{
			var cleaner = new IetfLanguageTagCleaner("fr", "Latn", "x-ZY", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "fr", "Latn", "QM", "", "fr-Latn-QM-x-ZY");
		}

		[Test]
		public void PrivateRegionMultiPartVariant_InsertsPrivateRegionCode()
		{
			var cleaner = new IetfLanguageTagCleaner("fr", "", "x-ZY", "fonipa-x-etic", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "fr", "", "QM", "fonipa", "fr-QM-fonipa-x-ZY-etic");
		}

		[Test]
		public void MultiPartVariantWithoutX_InsertsX()
		{
			var cleaner = new IetfLanguageTagCleaner("fr", "", "", "fonipa-etic", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "fr", "", "", "fonipa", "fr-fonipa-x-etic");
		}

		[Test]
		public void ZhNoRegion_InsertsRegionCN()
		{
			var cleaner = new IetfLanguageTagCleaner("zh", "", "", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "zh", "", "CN", "", "zh-CN");
		}

		[Test]
		public void CmnNoRegion_BecomesZhCN()
		{
			var cleaner = new IetfLanguageTagCleaner("cmn", "", "", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "zh", "", "CN", "", "zh-CN");
		}

		[Test]
		public void Pes_BecomesFa()
		{
			var cleaner = new IetfLanguageTagCleaner("pes", "Latn", "", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "fa", "Latn", "", "", "fa-Latn");
		}

		[Test]
		public void Arb_BecomesAr()
		{
			var cleaner = new IetfLanguageTagCleaner("arb", "", "x-ZG", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "ar", "", "QM", "", "ar-QM-x-ZG");
		}
		[Test]
		public void ZhRegion_NoChange()
		{
			var cleaner = new IetfLanguageTagCleaner("zh", "", "NO", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "zh", "", "NO", "", "zh-NO");

			cleaner = new IetfLanguageTagCleaner("zh", "", "x-ZK", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "zh", "", "QM", "", "zh-QM-x-ZK");
		}

		/// <summary>
		/// JohnT: I have no idea why tpi gets moved to the end of the private-use section. This behavior was copied
		/// from the LdmlInFolderWritingSystemRepositoryMigratorTests test with a similar name, which indicated
		/// that such a re-ordering was expected, and I have apparently not broken it, so didn't worry.
		/// </summary>
		[Test]
		public void LanguageSubtagContainsMultipleValidLanguageSubtagsAsWellAsDataThatIsNotValidLanguageScriptRegionOrVariant_AllSubtagsButFirstValidLanguageSubtagAreMovedToPrivateUse()
		{
			var cleaner = new IetfLanguageTagCleaner("bogus-en-audio-tpi-bogus2-x-", "", "", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "en", "Zxxx", "", "", "en-Zxxx-x-bogus-audio-bogus2-tpi");
		}

		[Test]
		public void CmnRegion_BecomesZh()
		{
			var cleaner = new IetfLanguageTagCleaner("cmn", "", "NO", "", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "zh", "", "NO", "", "zh-NO");
		}

		[Test]
		public void EticWithoutFonipa_AddsFonipa()
		{
			var cleaner = new IetfLanguageTagCleaner("en", "", "", "x-etic", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "en", "", "", "fonipa", "en-fonipa-x-etic");
		}

		[Test]
		public void EmicWithoutFonipa_AddsFonipa()
		{
			var cleaner = new IetfLanguageTagCleaner("en", "", "", "x-emic", "");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "en", "", "", "fonipa", "en-fonipa-x-emic");
		}

		[Test]
		public void PrivateUseVariantLanguageCode_IsNotShortened()
		{
			var cleaner = new IetfLanguageTagCleaner("qaa", "", "", "", "x-kal");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "qaa", "", "", "", "qaa-x-kal");
		}

		[Test]
		public void LanguageCodeAfterX_IsNotShortened()
		{
			var cleaner = new IetfLanguageTagCleaner("qaa-x-kal");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "qaa", "", "", "", "qaa-x-kal");
		}

		[Test]
		public void NewTagWithPrivateLanguage_IsNotModified()
		{
			var cleaner = new IetfLanguageTagCleaner("qaa-Qaaa-QM-x-kal-Mysc-YY");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "qaa", "Qaaa", "QM", "", "qaa-Qaaa-QM-x-kal-Mysc-YY");
		}

		[Test]
		public void NewTag_IsNotModified()
		{
			var cleaner = new IetfLanguageTagCleaner("fr-Qaaa-QM-x-Mysc-YY");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "fr", "Qaaa", "QM", "", "fr-Qaaa-QM-x-Mysc-YY");
		}

		[Test]
		public void RegionCodesThatMatchLanguageCodesNotMovedToPrivateUse()
		{
			var cleaner = new IetfLanguageTagCleaner("rwr-IN");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "rwr", "", "IN", "", "rwr-IN");
		}

		[Test]
		public void CleanMarksCustomScriptMovedToPrivateUse()
		{
			var cleaner = new IetfLanguageTagCleaner("en-Zyxw");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "en", "Qaaa", "", "", "en-Qaaa-x-Zyxw");
		}

		[Test]
		public void ScriptEndingWithX_IsHandledCorrectly()
		{
			var cleaner = new IetfLanguageTagCleaner("zh-Phnx-CN-fonipa-x-emic");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "zh", "Phnx", "CN", "fonipa", "zh-Phnx-CN-fonipa-x-emic");
		}

		[Test]
		public void GetFullCode_CaseCleaning_Lang_Script_Region_Works()
		{
			var cleaner = new IetfLanguageTagCleaner("EN-latn-us-x-NotCHang");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "en", "Latn", "US", "", "en-Latn-US-x-NotCHang");
		}

		[Test]
		public void GetFullCode_CaseCleaning_AudioWs_PrivateUseIsChanged()
		{
			var cleaner = new IetfLanguageTagCleaner("EN-Zxxx-x-AudIO");
			cleaner.Clean();
			VerifyRfcCleaner(cleaner, "en", "Zxxx", "", "", "en-Zxxx-x-audio");
		}

		void VerifyRfcCleaner(IetfLanguageTagCleaner cleaner, string language, string script, string region, string variant, string completeTag)
		{
			Assert.That(cleaner.Language, Is.EqualTo(language));
			Assert.That(cleaner.Script, Is.EqualTo(script));
			Assert.That(cleaner.Region, Is.EqualTo(region));
			Assert.That(cleaner.Variant, Is.EqualTo(variant));
			Assert.That(cleaner.GetCompleteTag(), Is.EqualTo(completeTag));
		}
	}
}
