using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests.WritingSystems.Tree
{
	[TestFixture]
	public class WritingSystemVariantSuggestorTests
	{
		[Test]
		public void GetSuggestions_HasNormalLacksIpa_IpaSuggestedWhichCopiesAllRelevantFields()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, "region", "variant", "Edolo", "edo", true);
			etr.DefaultFontName = "font";
			etr.DefaultFontSize = 33;
			var list = new List<WritingSystemDefinition>(new[] {etr });
			var suggestor = new WritingSystemVariantSuggestor();
			var suggestions = suggestor.GetSuggestions(etr, list);

			var ipa = suggestions.First(defn => defn.Script == "ipa");

			Assert.AreEqual("etr", ipa.ISO);
			Assert.AreEqual("ipa", ipa.Script);
			Assert.AreEqual("Edolo", ipa.LanguageName);
			Assert.IsTrue(string.IsNullOrEmpty(ipa.NativeName));
			Assert.AreEqual("region", ipa.Region);
			//Assert.AreEqual("arial unicode ms", ipa.DefaultFontName); this depends on what fonts are installed on the test system
			Assert.AreEqual(33, ipa.DefaultFontSize);

			Assert.AreEqual("ipa", ipa.Keyboard.ToLower());
		}

		[Test]
		public void GetSuggestions_HasNormalAndIPA_DoesNotIncludeItemToCreateIPA()
		{
			var etr = new WritingSystemDefinition("etr", string.Empty, string.Empty, string.Empty, "Edolo", "edo", false);
			var etrIpa = new WritingSystemDefinition("etr", "ipa", string.Empty, string.Empty, "Edolo", "edo", false);
			var list = new List<WritingSystemDefinition>(new[] { etr, etrIpa });
			var suggestor = new WritingSystemVariantSuggestor();
			var suggestions = suggestor.GetSuggestions(etr, list);

			Assert.IsFalse(suggestions.Any(defn => defn.Script == "ipa"));
		}
	}
}
