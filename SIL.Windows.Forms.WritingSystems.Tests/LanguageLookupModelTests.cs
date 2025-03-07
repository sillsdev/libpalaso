using System.Linq;
using L10NSharp;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	[OfflineSldr]
	public class LanguageLookupModelTests
	{
		[SetUp]
		public void SetUp()
		{
			LocalizationManager.StrictInitializationMode = false;
		}

		[Test]
		public void IncludeRegionCodes_SetToFalse_DialectsNotReturned()
		{
			var model = new LanguageLookupModel();
			model.LoadLanguages();
			model.IncludeRegionalDialects = false;
			model.SearchText = "english";
			Assert.That(model.MatchingLanguages.Select(li => li.LanguageTag), Has.None.EqualTo("en-US"));
		}

		[Test]
		public void IncludeRegionCodes_SetToFalseSearchForChinese_ReturnsTaiwanAndMainlandChina()
		{
			var model = new LanguageLookupModel();
			model.LoadLanguages();
			model.IncludeRegionalDialects = false;
			model.SearchText = "chinese";
			string[] codes = model.MatchingLanguages.Select(li => li.LanguageTag).ToArray();
			Assert.That(codes, Contains.Item(WellKnownSubtags.ChineseSimplifiedTag));
			Assert.That(codes, Contains.Item("zh-TW"));
		}

		[Test]
		public void IncludeRegionCodes_SetToTrue_DialectsReturned()
		{
			var model = new LanguageLookupModel();
			model.LoadLanguages();
			model.IncludeRegionalDialects = true;
			model.SearchText = "english";
			string[] codes = model.MatchingLanguages.Select(li => li.LanguageTag).ToArray();
			Assert.That(codes, Contains.Item("en-GB"));
		}
	}
}
