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
		[Test]
		public void ValidIso639LanguageCodes_HasEnglish_True()
		{
			var codes = StandardTags.ValidIso639LanguageCodes;
			Assert.IsTrue(codes.Any(code => code.Code == "en"));
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

	}
}
