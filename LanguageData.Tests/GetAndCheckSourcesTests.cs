using NUnit.Framework;
using System;

// possible tests
// verify AreFilesDifferent for 2 identical and 2 different strings
// verify CheckSourcesAreDifferent in 2 cases, same and different
// verify GenerateTwoToThreeCodes generates the same thing consistently on all platforms
// what happens to GetNewSources if no net?
// what happens to GetOldSources if input dir is bad (not exists or files not readable)?
// what about WriteNewFiles if output dir is bad (not exists or system folder)?

namespace LanguageData.Tests
{
	[TestFixture ()]
	public class GetAndCheckSourcesTests
	{
		[Test ()]
		public void TestCase ()
		{
		}
	}
}

