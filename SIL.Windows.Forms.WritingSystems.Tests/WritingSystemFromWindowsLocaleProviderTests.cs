using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SIL.WritingSystems;
using SIL.WritingSystems.Tests;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class WritingSystemFromWindowsLocaleProviderTests
	{

		[Test]
		[Category("DesktopRequired")] // Fails on Jenkins because InputLanguage.InstalledInputLanguages returns an empty list.
#if MONO
		[Ignore("Linux only usually returns the Invariant, reimplement using IBus")]
#endif
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
#if MONO
		[Ignore("Linux only usually returns the Invariant, reimplement using IBus")]
#endif
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
	}

}