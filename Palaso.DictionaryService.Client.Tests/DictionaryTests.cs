using System.Collections.Generic;
using NUnit.Framework;

namespace Palaso.DictionaryService.Client.Tests
{
	[TestFixture]
	public class DictionaryTests
	{
		private string _dictionaryWritingSystemId = "qPretend";
		private string _primaryVernacularWritingSystemId = "qPretend";
		private IDictionary _dictionary;
		private ServiceFinder _finder;

		[SetUp]
		public void Setup()
		{
			_finder = new ServiceFinder();
			_finder.Init();
			_finder.LoadTestDictionary(_dictionaryWritingSystemId);
			_dictionary =  _finder.GetDictionaryService(_primaryVernacularWritingSystemId);
		}

		[TearDown]
		public void TearDown()
		{
			_dictionary.Dispose();
			_finder.Dispose();
		}

		[Test]
		public void GetDictionaryService_UnmatchedWritingSystem_ReturnNull()
		{
			Assert.IsNull(_finder.GetDictionaryService("x12"));
		}

		[Test]
		public void GetDictionaryService_ReturnsSameDictionaryInstanceEachTime()
		{
			Assert.AreEqual(_finder.GetDictionaryService(_dictionaryWritingSystemId), _finder.GetDictionaryService(_dictionaryWritingSystemId));
		}

		[Test]
		public void GetLexemeFormExact_SameAsWhatWeSet()
		{
			Entry e = _dictionary.CreateEntryLocally();
			e.AddLexemeForm(_primaryVernacularWritingSystemId, "testing");
			_dictionary.AddEntry(e);
			Assert.AreEqual("testing", e.GetLexemeFormWithExactWritingSystem(_primaryVernacularWritingSystemId));
		}


		[Test]
		public void PretendDictionaryHasMango()
		{
			IList<Entry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "mango", FindMethods.Exact);
			Assert.AreEqual(1, entries.Count);
		}


		[Test]
		public void EntryNotInDictionaryUntilAdded()
		{
			Entry e = _dictionary.CreateEntryLocally();
			e.AddLexemeForm(_primaryVernacularWritingSystemId, "testing");
			IList<Entry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "testing", FindMethods.Exact);
			Assert.AreEqual(0, entries.Count);
			_dictionary.AddEntry(e);
			entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "testing", FindMethods.Exact);
			Assert.AreEqual(1, entries.Count);
		}

		[Test]
		public void PretendDictionaryDoesNotHaveCucumber()
		{
			Entry e = _dictionary.CreateEntryLocally();
			IList<Entry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "cucumber", FindMethods.Exact);
			Assert.AreEqual(0, entries.Count);
		}

		[Test]
		public void DictionaryGivesHtmlForMango()
		{
			IList<Entry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "mango", FindMethods.Exact);
			Assert.IsTrue(entries[0].GetHtmlArticle(ArticleCompositionFlags.Simple).Contains("mango"));
		}
	}
}
