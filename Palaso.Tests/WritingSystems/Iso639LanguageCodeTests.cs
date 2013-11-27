using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Palaso.WritingSystems;

namespace Palaso.Tests.WritingSystems
{
	[TestFixture]
	public class Iso639LanguageCodeTests
	{
		[Test]
		public void GetNameForSorting_HasNonWordCharacters_RemovesNonWordCharacters()
		{
			Assert.AreEqual("Kxauein", Iso639LanguageCode.GetNameForSorting("=/Kx'au//'ein"));
		}
	}
}
