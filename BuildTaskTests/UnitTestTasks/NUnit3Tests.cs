using NUnit.Framework;
using Palaso.BuildTasks.UnitTestTasks;

namespace Palaso.BuildTask.Tests.UnitTestTasks
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
	}
}
