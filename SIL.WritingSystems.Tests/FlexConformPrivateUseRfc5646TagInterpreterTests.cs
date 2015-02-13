using NUnit.Framework;
using SIL.Data;

namespace SIL.WritingSystems.Tests
{
	[TestFixture]
	public class FlexConformPrivateUseRfc5646TagInterpreterTests
	{
		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_Language_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-en");
			AssertThatPropertiesAreSet("", "", "", "x-en", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_LanguageIsValidRfc5646TagStartingWithX_Throws()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			Assert.That(()=>interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("xh"), Throws.Exception.TypeOf<ValidationException>());
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_PrivateUseContainsMultipleXs_RemovesExtraXs()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("","","","x-x-x-audio");
			AssertThatPropertiesAreSet("", "", "", "x-audio", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_LanguageScript_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-en-Zxxx");
			AssertThatPropertiesAreSet("qaa", "Zxxx", "", "x-en", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_LanguageRegion_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-en-US");
			AssertThatPropertiesAreSet("qaa", "", "US", "x-en", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_LanguageVariant_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-en-fonipa");
			AssertThatPropertiesAreSet("qaa", "", "", "fonipa-x-en", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_NoLanguageScriptPrivateUse_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-Zxxx-x-audio");
			AssertThatPropertiesAreSet("qaa", "Zxxx", "", "x-audio", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_LanguagePrivateUse_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-en-x-private");
			AssertThatPropertiesAreSet("", "", "", "x-en-private", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_xDashZxxxDashXDashAudio_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-Zxxx-x-AUDIO");
			AssertThatPropertiesAreSet("qaa", "Zxxx", "", "x-AUDIO", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_PrivateUseContainsDuplicates_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-en-x-private-private");
			AssertThatPropertiesAreSet("", "", "", "x-en-private", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_LanguageSubtagTogetherWithPrivateUseContainsDuplicates_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-en-x-en");
			AssertThatPropertiesAreSet("", "", "", "x-en", interpreter);
		}

		[Test]
		public void ConvertToPalasoConformPrivateUseRfc5646Tag_xDashXDashZxxxDashAudio_IsConvertedCorrectly()
		{
			var interpreter = new FlexConformPrivateUseRfc5646TagInterpreter();
			interpreter.ConvertToPalasoConformPrivateUseRfc5646Tag("x-x-Zxxx-AUDIO");
			AssertThatPropertiesAreSet("qaa", "Zxxx", "", "x-AUDIO", interpreter);
		}

		private void AssertThatPropertiesAreSet(string language, string script, string region, string variant, FlexConformPrivateUseRfc5646TagInterpreter interpreter)
		{
			Assert.That(interpreter.Language, Is.EqualTo(language));
			Assert.That(interpreter.Script, Is.EqualTo(script));
			Assert.That(interpreter.Region, Is.EqualTo(region));
			Assert.That(interpreter.Variant, Is.EqualTo(variant));
		}
	}
}
