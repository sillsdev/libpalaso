using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using NUnit.Framework;
using SIL.WritingSystems;
using SIL.WritingSystems.Tests;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class WritingSystemFromWindowsLocaleProviderTests
	{
		private class WritingSystemFromWindowsLocaleProviderAccessor: WritingSystemFromWindowsLocaleProvider
		{
			public WritingSystemFromWindowsLocaleProviderAccessor(
				IWritingSystemFactory writingSystemFactory) : base(writingSystemFactory)
			{
			}

			public static string CallGetRegion(string cultureName)
			{
				return GetRegion(cultureName);
			}
		}

		[Test]
		[Category("DesktopRequired")] // Fails on Jenkins because InputLanguage.InstalledInputLanguages returns an empty list.
		[Platform(Exclude = "Linux", Reason = "Linux only usually returns the Invariant, reimplement using IBus")]
		public void ActiveIncludesAtLeastOneLanguage()
		{
			IEnumerable<Tuple<string, string>> provider = new WritingSystemFromWindowsLocaleProvider(new TestWritingSystemFactory());
			Assert.IsNotNull(provider.First());
		}

		/// <summary>
		/// This is only really testing something if your computer happens to have multiple
		/// keyboards set up for a language.
		/// </summary>
		[Test]
		[Category("DesktopRequired")] // Fails on Jenkins because InputLanguage.InstalledInputLanguages returns an empty list.
		[Platform(Exclude = "Linux", Reason = "Linux only usually returns the Invariant, reimplement using IBus")]
		public void GetEnumerator_IfHaveMultipleSystemKeyboardsForSameLanguage_OnlyReturnsOneForEachLanguage()
		{
			IEnumerable<Tuple<string, string>> provider = new WritingSystemFromWindowsLocaleProvider(new TestWritingSystemFactory());
			Assert.IsNotNull(provider.First());
			foreach (var group in provider.GroupBy(d => d.Item1))
				Assert.AreEqual(1, group.Count());
		}

//        [Test]
//        public void GetGoodPropertiesOutOfCulture()
//        {
//            IEnumerable<WritingSystemDefinition> provider =
//                new SIL.Windows.Forms.WritingSystems.WritingSystemFromWindowsLocaleProvider();
//            foreach (WritingSystemDefinition language in provider.WritingSystems)
//            {
////                if (language.Language == "eng")
////                {
////                    Assert.AreEqual("Latn",language.Script);
////                    Assert.AreEqual("en-Latn", language.RFC5646);
////                }
//            }
//        }

		[TestCase("aa", ExpectedResult = "ET")] // returns default region
		[TestCase("chr", ExpectedResult = "US")] // previously ArgumentException
		[TestCase("chr-Cher", ExpectedResult = "US")] // previously ArgumentException
		[TestCase("chr-Cher-US", ExpectedResult = "US")]
		[TestCase("en", ExpectedResult = "US")] // previously ArgumentException
		[TestCase("en-US", ExpectedResult = "US")]
		[TestCase("en-001", ExpectedResult = "001")]
		[TestCase("jv-Latn", ExpectedResult = "ID")]
		[TestCase("jv-Latn-ID", ExpectedResult = "ID")]
		[TestCase("kok", ExpectedResult = "IN")] // previously ArgumentException
		[TestCase("kok-IN", ExpectedResult = "IN")]
		[TestCase("und", ExpectedResult = "")]
		[TestCase("und-001", ExpectedResult = "001")]
		public string GetRegion(string cultureName)
		{
			return WritingSystemFromWindowsLocaleProviderAccessor.CallGetRegion(cultureName);
		}
	}

}