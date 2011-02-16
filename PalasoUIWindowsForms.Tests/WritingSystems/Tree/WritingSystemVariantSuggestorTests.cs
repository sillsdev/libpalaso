using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.UI.WindowsForms.WritingSystems.WSTree;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems.Tree
{
	[TestFixture]
	public class WritingSystemVariantSuggestorTests
	{
		[Test, Ignore("Only works if there is an ipa keyboard installed")]
		public void GetSuggestions_HasNormalLacksIpa_IpaSuggestedWhichCopiesAllRelevantFields()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, "region", "variant", "edo", true);
			etr.DefaultFontName = "font";
			etr.DefaultFontSize = 33;
			var list = new List<WritingSystemDefinition>(new[] {etr });
			var suggestor = new WritingSystemSuggestor();
			var suggestions = suggestor.GetSuggestions(etr, list);

			WritingSystemDefinition ipa = ((WritingSystemSuggestion)suggestions.First(defn => ((WritingSystemSuggestion)defn).TemplateDefinition.Script == "ipa")).TemplateDefinition;

			Assert.AreEqual("etr", ipa.ISO639);
			Assert.AreEqual("fonipa", ipa.Variant);
			Assert.AreEqual("Edolo", ipa.LanguageName);
			Assert.IsTrue(string.IsNullOrEmpty(ipa.NativeName));
			Assert.AreEqual("region", ipa.Region);
			//Assert.AreEqual("arial unicode ms", ipa.DefaultFontName); this depends on what fonts are installed on the test system
			Assert.AreEqual(33, ipa.DefaultFontSize);

			Assert.IsTrue(ipa.Keyboard.ToLower().Contains("ipa"));
		}

		[Test]
		public void GetSuggestions_HasNormalAndIPA_DoesNotIncludeItemToCreateIPA()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "edo", false);
			var etrIpa = new WritingSystemDefinition("etr", string.Empty, string.Empty,  "fonipa", "edo", false);
			var list = new List<WritingSystemDefinition>(new[] { etr, etrIpa });
			var suggestor = new WritingSystemSuggestor();
			var suggestions = suggestor.GetSuggestions(etr, list);

			Assert.IsFalse(suggestions.Any(defn => ((WritingSystemSuggestion)defn).TemplateDefinition.Variant == "fonipa"));
		}


		/// <summary>
		/// For English, it's very unlikely that they'll want to add IPA, in a app like wesay
		/// </summary>
		[Test]
		public void GetSuggestions_MajorWorlLanguage_SuggestsOnlyIfSuppressSuggesstionsForMajorWorldLanguagesIsFalse()
		{
			var english = new WritingSystemDefinition("en", string.Empty, string.Empty, string.Empty, "eng", false);
			var list = new List<WritingSystemDefinition>(new[] { english });
			var suggestor = new WritingSystemSuggestor();
			suggestor.SuppressSuggestionsForMajorWorldLanguages =false;
			var suggestions = suggestor.GetSuggestions(english, list);
			Assert.IsTrue(suggestions.Any(defn => ((WritingSystemSuggestion)defn).TemplateDefinition.Variant == "fonipa"));

			suggestor.SuppressSuggestionsForMajorWorldLanguages =true;
			suggestions = suggestor.GetSuggestions(english, list);
			Assert.IsFalse(suggestions.Any(defn => ((WritingSystemSuggestion)defn).TemplateDefinition.Variant == "fonipa"));
		}
	}
}
