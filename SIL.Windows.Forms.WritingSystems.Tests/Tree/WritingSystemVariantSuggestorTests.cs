using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.Windows.Forms.WritingSystems.WSTree;
using SIL.WritingSystems;
using SIL.WritingSystems.Tests;

namespace SIL.Windows.Forms.WritingSystems.Tests.Tree
{
	[TestFixture]
	[OfflineSldr]
	public class WritingSystemVariantSuggestorTests
	{
		[Test, Ignore("Only works if there is an ipa keyboard installed")]
		public void GetSuggestions_HasNormalLacksIpa_IpaSuggestedWhichCopiesAllRelevantFields()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, "region", "variant", "edo", true) {DefaultFont = new FontDefinition("font"), DefaultFontSize = 33};
			var list = new List<WritingSystemDefinition>(new[] {etr });
			var suggestor = new WritingSystemSuggestor(new TestWritingSystemFactory());
			var suggestions = suggestor.GetSuggestions(etr, list);

			WritingSystemDefinition ipa = suggestions.First(defn => defn is IpaSuggestion).ShowDialogIfNeededAndGetDefinition();

			Assert.That(ipa.Language, Is.EqualTo(new LanguageSubtag("etr", "Edolo")));
			Assert.That(ipa.Variants, Is.EqualTo(new VariantSubtag[] {"fonipa"}));
			Assert.That(ipa.Region, Is.EqualTo((RegionSubtag) "region"));
			//Assert.AreEqual("arial unicode ms", ipa.DefaultFontName); this depends on what fonts are installed on the test system
			Assert.That(ipa.DefaultFontSize, Is.EqualTo(33));

			Assert.That(ipa.Keyboard.ToLower().Contains("ipa"), Is.True);
		}

		[Test] // ok
		public void GetSuggestions_HasNormalAndIPA_DoesNotIncludeItemToCreateIPA()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "edo", false);
			var etrIpa = new WritingSystemDefinition("etr", string.Empty, string.Empty,  "fonipa", "edo", false);
			var list = new List<WritingSystemDefinition>(new[] { etr, etrIpa });
			var suggestor = new WritingSystemSuggestor(new TestWritingSystemFactory());
			IEnumerable<IWritingSystemDefinitionSuggestion> suggestions = suggestor.GetSuggestions(etr, list);

			Assert.That(suggestions.Any(defn => defn is IpaSuggestion), Is.False);
		}

		[Test]
		public void OtherKnownWritingSystems_TokPisinDoesNotAlreadyExist_HasTokPisin()
		{
			var suggestor = new WritingSystemSuggestor(new TestWritingSystemFactory());
			Assert.That(suggestor.GetOtherLanguageSuggestions(Enumerable.Empty<WritingSystemDefinition>()).Any(ws => ws.Label == "Tok Pisin"), Is.True);
		}

		[Test]
		public void OtherKnownWritingSystems_TokPisinAlreadyExists_DoesNotHaveTokPisin()
		{
			var suggestor = new WritingSystemSuggestor(new TestWritingSystemFactory());

			var existingWritingSystems = new List<WritingSystemDefinition>{new WritingSystemDefinition("tpi")};
			Assert.That(suggestor.GetOtherLanguageSuggestions(existingWritingSystems).Any(ws => ws.Label == "Tok Pisin"), Is.False);
		}


		/// <summary>
		/// For English, it's very unlikely that they'll want to add IPA, in an app like WeSay
		/// </summary>
		[Test, Category("KnownMonoIssue")]
		public void GetSuggestions_MajorWorldLanguage_SuggestsOnlyIfSuppressSuggestionsForMajorWorldLanguagesIsFalse()
		{
			var english = new WritingSystemDefinition("en", string.Empty, string.Empty, string.Empty, "eng", false);
			var list = new List<WritingSystemDefinition>(new[] { english });
			var suggestor = new WritingSystemSuggestor(new TestWritingSystemFactory());
			suggestor.SuppressSuggestionsForMajorWorldLanguages = false;
			var suggestions = suggestor.GetSuggestions(english, list);
			Assert.That(suggestions.Any(defn => defn is IpaSuggestion), Is.True);

			suggestor.SuppressSuggestionsForMajorWorldLanguages = true;
			suggestions = suggestor.GetSuggestions(english, list);
			Assert.That(suggestions.Any(defn => defn is IpaSuggestion), Is.False);
		}
	}
}
