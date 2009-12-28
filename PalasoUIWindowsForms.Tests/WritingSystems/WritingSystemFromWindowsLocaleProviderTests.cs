using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using NUnit.Framework;
using Palaso.UI.WindowsForms.WritingSystems;
using Palaso.WritingSystems;

namespace PalasoUIWindowsForms.Tests
{
	[TestFixture]
	public class WritingSystemFromWindowsLocaleProviderTests
	{
		[SetUp]
		public void Setup()
		{

		}

		[TearDown]
		public void TearDown()
		{

		}
		[Test]
		public void ActiveIncludesAtLeastOneLanguage()
		{
			IEnumerable<WritingSystemDefinition> provider =
				new Palaso.UI.WindowsForms.WritingSystems.WritingSystemFromWindowsLocaleProvider();
			Assert.IsNotNull(provider.First());
		}

		/// <summary>
		/// This is only really testing something if your computer happens to have multiple
		/// keyboards set up for a language.
		/// </summary>
		[Test]
		public void GetEnumerator_IfHaveMultipleSystemKeyboardsForSameLanguage_OnlyReturnsOneForEachLanguage()
		{
			IEnumerable<WritingSystemDefinition> provider =
				new Palaso.UI.WindowsForms.WritingSystems.WritingSystemFromWindowsLocaleProvider();
			Assert.IsNotNull(provider.First());
			foreach (var group in provider.GroupBy(d=>d.RFC4646))
			{
				Assert.AreEqual(1, group.Count());
			}
		}

//        [Test]
//        public void GetGoodPropertiesOutOfCulture()
//        {
//            IEnumerable<WritingSystemDefinition> provider =
//                new Palaso.UI.WindowsForms.WritingSystems.WritingSystemFromWindowsLocaleProvider();
//            foreach (WritingSystemDefinition language in provider.WritingSystems)
//            {
////                if (language.ISO == "eng")
////                {
////                    Assert.AreEqual("Latn",language.Script);
////                    Assert.AreEqual("en-Latn", language.RFC4646);
////                }
//            }
//        }
	}

}