using NUnit.Framework;
using SIL.BuildTasks.UnitTestTasks;

namespace SIL.BuildTasks.Tests.UnitTestTasks
{
	[TestFixture]
	class NUnit3Tests
	{
		[TestCase("", "", "")]
		[TestCase(" ", "", "")]
		[TestCase(null, "", "")]
		[TestCase("CategoryToInclude", "", " --where \"(cat=CategoryToInclude)\"")]
		[TestCase("CategoryToInclude,OtherCatToInclude", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude)\"")]
		[TestCase("CategoryToInclude, OtherCatToInclude", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude)\"")]
		[TestCase("CategoryToInclude, OtherCatToInclude, ", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude)\"")]
		[TestCase("CategoryToInclude, OtherCatToInclude, ThirdCatToInclude", "", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude or cat=ThirdCatToInclude)\"")]
		[TestCase("", "CategoryToExclude", " --where \"(cat!=CategoryToExclude)\"")]
		[TestCase("", "CategoryToExclude, OtherCatToExclude", " --where \"(cat!=CategoryToExclude and cat!=OtherCatToExclude)\"")]
		[TestCase("CategoryToInclude,OtherCatToInclude", "CategoryToExclude,OtherCatToExclude", " --where \"(cat=CategoryToInclude or cat=OtherCatToInclude) and (cat!=CategoryToExclude and cat!=OtherCatToExclude)\"")]
		public void AddIncludeAndExcludeArguments_BuildsProperString(string include, string exclude, string result)
		{
			var nUnit3 = new NUnit3
			{
				IncludeCategory = include,
				ExcludeCategory = exclude
			};

			Assert.AreEqual(result, nUnit3.AddIncludeAndExcludeArguments());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void FailTaskIfAnyTestsFail_NotSetByUser_SetByTeamCityProperty(bool teamcity)
		{
			var nUnit3 = new NUnit3
			{
				TeamCity = teamcity
			};

			Assert.AreEqual(teamcity, nUnit3.FailTaskIfAnyTestsFail);
		}

		[TestCase(true)]
		[TestCase(false)]
		public void FailTaskIfAnyTestsFail_SetByUser_NotAffectedByTeamCityProperty(bool failTaskIfAnyTestsFail)
		{
			var nUnit3 = new NUnit3
			{
				FailTaskIfAnyTestsFail = failTaskIfAnyTestsFail
			};

			nUnit3.TeamCity = true;
			Assert.AreEqual(failTaskIfAnyTestsFail, nUnit3.FailTaskIfAnyTestsFail);
			nUnit3.TeamCity = false;
			Assert.AreEqual(failTaskIfAnyTestsFail, nUnit3.FailTaskIfAnyTestsFail);
		}
	}
}
