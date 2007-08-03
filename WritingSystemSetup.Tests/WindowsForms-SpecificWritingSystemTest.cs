using System;
using System.Collections.Generic;
using System.Diagnostics;
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
			IWritingSystemProvider provider =
				new Palaso.UI.WindowsForms.WritingSystems.WritingSystemFromWindowsLocaleProvider();
			IEnumerator<WritingSystemDefinition> enumerator = provider.ActiveOSLanguages.GetEnumerator();
			enumerator.MoveNext();
			Assert.IsNotNull(enumerator.Current);
		}

//        [Test]
//        public void GetGoodPropertiesOutOfCulture()
//        {
//            IWritingSystemProvider provider =
//                new Palaso.UI.WindowsForms.WritingSystems.WritingSystemFromWindowsLocaleProvider();
//            foreach (WritingSystemDefinition language in provider.ActiveOSLanguages)
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