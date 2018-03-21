using System.Linq;
using NUnit.Framework;
using SIL.Archiving.Generic;

namespace SIL.Archiving.Tests
{
	[TestFixture]
	[Category("Archiving")]
	class SimpleObjectTests
	{
		[Test]
		public void LanguageStringComparer_CorrectlyComparesLanguageStrings()
		{
			const string firstValue = "Value 1";
			const string frenchValue = "This is the french value.";

			LanguageString ls1 = new LanguageString { Iso3LanguageId = "eng", Value = firstValue };
			LanguageString ls2 = new LanguageString { Iso3LanguageId = "eng", Value = "This is the second value." };
			LanguageString ls3 = new LanguageString { Iso3LanguageId = "fra", Value = frenchValue };

			LanguageStringCollection hs = new LanguageStringCollection();

			var first = hs.Add(ls1);  // this one should be added
			var second = hs.Add(ls2); // this one should not be added, it is a duplicate
			var third = hs.Add(ls3);  // this one should be added

			Assert.IsTrue(first);
			Assert.IsFalse(second);
			Assert.IsTrue(third);
			Assert.AreEqual(2, hs.Count);
			Assert.AreEqual(firstValue, hs.First(ls => ls.Iso3LanguageId == "eng").Value);
			Assert.AreEqual(frenchValue, hs.First(ls => ls.Iso3LanguageId == "fra").Value);
		}

		[Test]
		public void ToLatinOnly_MultiplePeriod_OnePeriod()
		{
			var result = "Langauge.wav.meta".ToLatinOnly("_", "+", ".");
			Assert.AreEqual("Langauge_wav.meta", result);
		}
	}
}
