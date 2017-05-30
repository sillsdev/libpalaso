using System;
using Microsoft.Build.Framework;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class ArchiveTests
	{
		private class EnvironmentForTest : IDisposable
		{
			public ITaskItem[] TwoItemsWithBasePath(string a, string b)
			{
				var result = new[]
				{
					new MockTaskItem(BasePath + a),
					new MockTaskItem(BasePath + b)
				};
				return result;
			}

			public string BasePath { get { return "/trim/path/"; }}

			public void Dispose()
			{
			}
		}

		[Test]
		public void ExecutableName_ForTar_Tar()
		{
			var task = new Archive.Archive();
			task.Command = "Tar";
			Assert.AreEqual("tar", task.ExecutableName());
		}

		[Test]
		public void ExecutableName_ForUnknown_EmptyString()
		{
			var task = new Archive.Archive();
			task.Command = "Unknown";
			Assert.AreEqual(String.Empty, task.ExecutableName());
		}

		[Test]
		public void Arguments_ForTar_CorrectAndIncludeFileName()
		{
			var task = new Archive.Archive();
			task.Command = "Tar";
			task.OutputFileName = "MyOutputFile.tar.gz";
			Assert.AreEqual("-cvzf MyOutputFile.tar.gz", task.Arguments());
		}

		[Test]
		public void Arguments_ForUnknown_EmptyString()
		{
			var task = new Archive.Archive();
			task.Command = "Unknown";
			task.OutputFileName = "MyOutputFile.tar.gz";
			Assert.AreEqual(String.Empty, task.Arguments());
		}

		[Test]
		public void TrimBaseFromFilePath_WithBase_ExcludesBasePath()
		{
			var task = new Archive.Archive();
			task.BasePath = "/trim/this/path/";
			var result = task.TrimBaseFromFilePath("/trim/this/path/myproject/here");
			Assert.AreEqual("myproject/here", result);
		}

		[Test]
		public void TrimBaseFromFilePath_WithBaseNoTrailingSlash_ExcludesBasePath()
		{
			var task = new Archive.Archive();
			task.BasePath = "/trim/this/path";
			var result = task.TrimBaseFromFilePath("/trim/this/path/myproject/here");
			Assert.AreEqual("myproject/here", result);
		}

		[Test]
		public void FlattenFilePaths_TwoItems_TrimsAndStringCorrect()
		{
			using (var e = new EnvironmentForTest())
			{
				var task = new Archive.Archive();
				task.BasePath = e.BasePath;
				task.InputFilePaths = e.TwoItemsWithBasePath("a.cs", "b.cs");
				var result = task.FlattenFilePaths(task.InputFilePaths, ' ', false);
				Assert.AreEqual("a.cs b.cs", result);
			}
		}

		[Test]
		public void FlattenFilePaths_TwoItemsOneWithSpace_TrimsAndQuotesStringCorrect()
		{
			using (var e = new EnvironmentForTest())
			{
				var task = new Archive.Archive();
				task.BasePath = e.BasePath;
				task.InputFilePaths = e.TwoItemsWithBasePath("a space.cs", "b.cs");
				var result = task.FlattenFilePaths(task.InputFilePaths, ' ', false);
				Assert.AreEqual("\"a space.cs\" b.cs", result);
			}
		}

		[Test]
		public void FlattenFilePaths_TwoItemsForceQuote_TrimsAndQuotesStringCorrect()
		{
			using (var e = new EnvironmentForTest())
			{
				var task = new Archive.Archive();
				task.BasePath = e.BasePath;
				task.InputFilePaths = e.TwoItemsWithBasePath("a.cs", "b.cs");
				var result = task.FlattenFilePaths(task.InputFilePaths, ' ', true);
				Assert.AreEqual("\"a.cs\" \"b.cs\"", result);
			}
		}
	}
}
