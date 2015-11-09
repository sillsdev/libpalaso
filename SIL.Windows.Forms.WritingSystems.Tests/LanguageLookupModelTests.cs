using System;
using System.Linq;
using NUnit.Framework;
using SIL.TestUtilities;
using SIL.WritingSystems;

namespace SIL.Windows.Forms.WritingSystems.Tests
{
	[TestFixture]
	public class LanguageLookupModelTests
	{
		private class TestEnvironment : IDisposable
		{
			private readonly TemporaryFolder _sldrCacheFolder;
			private readonly LanguageLookupModel _model;

			public TestEnvironment()
			{
				Sldr.OfflineMode = true;
				_sldrCacheFolder = new TemporaryFolder("SldrCache");
				_model = new LanguageLookupModel();
				_model.LoadLanguages();
			}

			public LanguageLookupModel Model
			{
				get { return _model; }
			}

			public void Dispose()
			{
				_sldrCacheFolder.Dispose();
				Sldr.OfflineMode = false;
			}
		}

		[Test]
		public void ShowDialects_SetToFalse_DialectsNotReturned()
		{
			using (var env = new TestEnvironment())
			{
				env.Model.ShowDialects = false;
				env.Model.SearchText = "english";
				Assert.That(env.Model.MatchingLanguages.Select(li => li.LanguageTag), Has.None.EqualTo("en-US"));
			}
		}

		[Test]
		public void ShowDialects_SetToFalseSearchForChinese_ReturnsTaiwanAndMainlandChina()
		{
			using (var env = new TestEnvironment())
			{
				env.Model.ShowDialects = false;
				env.Model.SearchText = "chinese";
				string[] codes = env.Model.MatchingLanguages.Select(li => li.LanguageTag).ToArray();
				Assert.That(codes, Contains.Item("zh-CN"));
				Assert.That(codes, Contains.Item("zh-TW"));
			}
		}

		[Test]
		public void ShowDialects_SetToTrue_DialectsReturned()
		{
			using (var env = new TestEnvironment())
			{
				env.Model.ShowDialects = true;
				env.Model.SearchText = "english";
				Assert.That(env.Model.MatchingLanguages.Select(li => li.LanguageTag), Contains.Item("en-US"));
			}
		}
	}
}
