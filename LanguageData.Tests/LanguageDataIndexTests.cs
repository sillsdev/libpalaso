using System.IO;
using NUnit.Framework;
using SIL.WritingSystems;

namespace LanguageData.Tests
{
	class LanguageDataIndexTests
	{
		private LanguageDataIndex _langIndex;
		private Options _options;

		[TestFixtureSetUp]
		public void TestFixtureSetup()
		{
			Sldr.Initialize(true);
			_options =
				new Options
				{
					InputDir = Path.Combine("..", "..", "SIL.WritingSystems", "Resources"),
					OutputFile = "testLanguageDataIndex.txt"
				};
			GetAndCheckSources getcheck = new GetAndCheckSources();
			getcheck.GetOldSources(_options.InputDir);
			_langIndex = new LanguageDataIndex(getcheck.GetFileStrings(_options.GetFresh));
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Sldr.Cleanup();
		}

		[Test]
		public void WriteIndex()
		{
			_langIndex.WriteIndex(_options.OutputFile);

			Assert.True(File.ReadAllText(_options.OutputFile).Length > 0);
		}

		[Test]
		public void WriteJson()
		{
			string basename = Path.GetFileNameWithoutExtension(_options.OutputFile);
			string jsonFile = basename + ".json";
			_langIndex.WriteJson(jsonFile);

			Assert.True(File.ReadAllText(jsonFile).Length > 0);
		}
	}
}
