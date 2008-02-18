using System.Collections.Generic;
using NUnit.Framework;
using Palaso.Services.Dictionary;

namespace Palaso.Services.Dictionary.Tests
{
	[TestFixture]
	public class DictionaryAccessorTests
	{
	   // private DictionaryAccessor _accessor;


		/*       private string _dictionaryWritingSystemId = "qPretend";
			   private string _primaryVernacularWritingSystemId = "qPretend";
			   private ServiceFinder _finder;

			   [SetUp]
			   public void Setup()
			   {
				   _finder = new ServiceFinder();
				   _finder.Init();
				   _finder.LoadTestDictionary(_dictionaryWritingSystemId, false);
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
				   IEntry e = _dictionary.CreateEntryLocally();
				   e.AddLexemeForm(_primaryVernacularWritingSystemId, "testing");
				   _dictionary.AddEntry(e);
				   Assert.AreEqual("testing", e.GetLexemeFormWithExactWritingSystem(_primaryVernacularWritingSystemId));
			   }


			   [Test]
			   public void PretendDictionaryHasMango()
			   {
				   IList<IEntry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "mango", FindMethods.Exact);
				   Assert.AreEqual(1, entries.Count);
			   }


			   [Test]
			   public void EntryNotInDictionaryUntilAdded()
			   {
				   IEntry e = _dictionary.CreateEntryLocally();
				   e.AddLexemeForm(_primaryVernacularWritingSystemId, "testing");
				   IList<IEntry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "testing", FindMethods.Exact);
				   Assert.AreEqual(0, entries.Count);
				   _dictionary.AddEntry(e);
				   entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "testing", FindMethods.Exact);
				   Assert.AreEqual(1, entries.Count);
			   }

			   [Test]
			   public void PretendDictionaryDoesNotHaveCucumber()
			   {
				   IEntry e = _dictionary.CreateEntryLocally();
				   IList<IEntry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "cucumber", FindMethods.Exact);
				   Assert.AreEqual(0, entries.Count);
			   }

			   [Test]
			   public void DictionaryGivesHtmlForMango()
			   {
				   IList<IEntry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "mango", FindMethods.Exact);
				   Assert.IsTrue(entries[0].GetHtmlArticle(ArticleCompositionFlags.Simple).Contains("mango"));
			   }

			   [Test]
			   public void AfterAddingVariantCanFindViaExactMatch()
			   {
				   IList<IEntry> entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "oranges", FindMethods.Exact);
				   Assert.AreEqual(0, entries.Count, "Should not have been an exact match for oranges");
				   entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "orange", FindMethods.Exact);
				   Assert.AreEqual(1, entries.Count, "Should have found an approximate match for orange");
				   entries[0].AddInflectionalVariant(_primaryVernacularWritingSystemId, "oranges");
				   entries = _dictionary.FindEntries(_primaryVernacularWritingSystemId, "oranges", FindMethods.Exact);
				   Assert.AreEqual(1, entries.Count, "Should now find an exact match for oranges, in the list of variants");

			   }

		 * */
	}
}
