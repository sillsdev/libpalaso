#if NO
This is here as a refugee from the purge of services from WeSay, but hasn't been
checked for use in Palaso.  We will revive it someday when there is call for it.


using NUnit.Framework;

namespace Palaso.DictionaryServices.Tests.Lift
{
	[TestFixture]
	public class HtmlArticleMakerTests
	{
		private ProjectDirectorySetupForTesting _projectDirectory;
		//   public WeSayWordsProject _project;

		/// <summary>
		/// time consuming to setup, so we reuse it.
		/// </summary>
		[OneTimeSetUp]
		public void SetupFixture()
		{
			_projectDirectory = new ProjectDirectorySetupForTesting("does not matter");
		}

		[OneTimeTearDown]
		public void FixtureTearDown()
		{
			_projectDirectory.Dispose();
		}

		[SetUp]
		public void Setup() { }

		[TearDown]
		public void TearDown() { }

		[Test]
		public void SmokeTest()
		{
			string contents =
					@"<entry id='one'><sense><gloss lang='en'><text>hello</text></gloss></sense></entry>";
			HtmlArticleMaker maker = new HtmlArticleMaker(
					_projectDirectory.PathToWritingSystemFile,
					_projectDirectory.PathToFactoryDefaultsPartsOfSpeech);
			string s = maker.GetHtmlFragment(contents);
			Assert.IsTrue(s.Contains("<html>"));
			Assert.IsTrue(s.Contains("hello"));
		}
	}
}
#endif