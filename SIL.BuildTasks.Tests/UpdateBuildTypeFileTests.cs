using System;
using System.Text;
using NUnit.Framework;

namespace SIL.BuildTasks.Tests
{
	[TestFixture]
	public class UpdateBuildTypeFileTests
	{
		[Test]
		public void GetVersionTypes_AlphaBetaRcProduction_GetsAllFourTypes()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var types = buildTypeFileMaker.GetVersionTypes(GetFileContentsForType("Alpha"));
			Assert.AreEqual(4, types.Count);
			Assert.IsTrue(types.Contains("Alpha"));
			Assert.IsTrue(types.Contains("Beta"));
			Assert.IsTrue(types.Contains("ReleaseCandidate"));
			Assert.IsTrue(types.Contains("Production"));
		}

		[Test]
		public void GetVersionTypes_Custom_GetsAllCustomTypes()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			var types = buildTypeFileMaker.GetVersionTypes(GetFileContents("Fred", "Wilma", "BamBam", "Fred", "Barney", "Pebbles"));
			Assert.AreEqual(5, types.Count);
			Assert.IsTrue(types.Contains("Wilma"));
			Assert.IsTrue(types.Contains("BamBam"));
			Assert.IsTrue(types.Contains("Fred"));
			Assert.IsTrue(types.Contains("Barney"));
			Assert.IsTrue(types.Contains("Pebbles"));
		}

		[Test]
		public void GetFileContents_UpdateAlphaToReleaseCandidate()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			string contents = buildTypeFileMaker.GetUpdatedFileContents(GetFileContentsForType("Alpha"), "ReleaseCandidate");
			Assert.AreEqual(GetFileContentsForType("ReleaseCandidate"), contents);
		}

		[Test]
		public void GetFileContents_UpdateReleaseCandidateToProduction()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			string contents = buildTypeFileMaker.GetUpdatedFileContents(GetFileContentsForType("ReleaseCandidate"), "Production");
			Assert.AreEqual(GetFileContentsForType("Production"), contents);
		}

		[Test]
		public void GetFileContents_UpdateBogusToBeta_NoReplacement()
		{
			var buildTypeFileMaker = new UpdateBuildTypeFile.UpdateBuildTypeFile();

			string contents = buildTypeFileMaker.GetUpdatedFileContents(GetFileContentsForType("Bogus"), "Beta");
			Assert.AreEqual(GetFileContentsForType("Bogus"), contents);
		}

		private string GetFileContentsForType(string type)
		{
			return GetFileContents(type, "Alpha", "Beta", "ReleaseCandidate", "Production");
		}

		private string GetFileContents(string existingType, params string[] types)
		{
			StringBuilder bldr = new StringBuilder();
			bldr.Append("namespace SIL.BuildTasks.Tests");
			bldr.Append(Environment.NewLine);
			bldr.Append("{");
			bldr.Append(Environment.NewLine);
			bldr.Append("\tpublic static class MyBuildType");
			bldr.Append(Environment.NewLine);
			bldr.Append("\t{");
			bldr.Append(Environment.NewLine);
			bldr.Append("\t\tpublic enum VersionType");
			bldr.Append(Environment.NewLine);
			bldr.Append("\t\t{");
			bldr.Append(Environment.NewLine);
			foreach (var type in types)
			{
				bldr.Append("\t\t\t");
				bldr.Append(type);
				bldr.Append(",");
				bldr.Append(Environment.NewLine);
			}
			bldr.Append("\t\t}");
			bldr.Append(Environment.NewLine);
			bldr.Append(Environment.NewLine);
			bldr.Append("\t\tpublic static VersionType BuildType { get { return VersionType.");
			bldr.Append(existingType);
			bldr.Append("; } }");
			bldr.Append("\t}");
			bldr.Append(Environment.NewLine);
			bldr.Append("}");

			return bldr.ToString();
		}
	}
}
