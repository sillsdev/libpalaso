using NUnit.Framework;
using System;
using System.IO;

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
		public void Verify_AllFilesDifferent ()
		{
			string stringone = "11111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111111";
			string stringtwo = "22222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222222";
			GetAndCheckSources getcheck = new GetAndCheckSources ();
			Assert.That (!getcheck.AreFilesDifferent(stringone, stringone));
			Assert.That (getcheck.AreFilesDifferent(stringone, stringtwo));
		}

		[Test ()]
		public void Verify_CheckSourcesAreDifferent ()
		{
		}

		[Test ()]
		public void Verify_TwoToThreeCodes ()
		{
		}

		[Test ()]
		public void GetOldSources_BadInputDir_throws ()
		{
			try
			{
				GetAndCheckSources getcheck = new GetAndCheckSources ();
				getcheck.GetOldSources ("gibberish");
			}
			catch (DirectoryNotFoundException dnfex)
			{
				Console.WriteLine (dnfex.Message);
			}
		}

		[Test ()]
		public void WriteNewFiles_BadOutputDir_throws ()
		{
			try
			{
				GetAndCheckSources getcheck = new GetAndCheckSources ();
				getcheck.WriteNewFiles ("gibberish");
			}
			catch (DirectoryNotFoundException dnfex)
			{
				Console.WriteLine (dnfex.Message);
			}
		}

		[Test ()]
		public void GetNewSources_Ok ()
		{
			GetAndCheckSources getcheck = new GetAndCheckSources ();
			Assert.That (getcheck.GetNewSources());
		}

		[Test ()]
		public void GetNewSources_NoNet ()
		{
			GetAndCheckSources getcheck = new GetAndCheckSources ();
			Assert.That (!getcheck.GetNewSources(true));
		}
	}
}

