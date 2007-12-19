using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using NUnit.Framework;

namespace Palaso.DictionaryService.Client.Tests
{
	/// <summary>
	/// These are not really testing much. Rather, they are working
	/// examples of how to use the DictionaryServices.
	/// </summary>
	[TestFixture]
	public class TypicalUsage
	{

		[Test]
		public void LookupAWordAndDisplayArticle()
		{
			using (ServiceFinder finder = new ServiceFinder())
			{
				finder.Init();
				finder.LoadTestDictionary("qTest");

				using (IDictionary dict = finder.GetDictionaryService("qTest"))
				{
					FindAndPrintWord(dict, "mango");
				}
			}
		}


		/// <summary>
		/// In languages with affixation, the word you're looking for may not exist
		/// in the dictionary in the exact form.  After the user finds the correct
		/// existing entry, though, the client application can push this affixed
		/// form into the dictionary as an "inflectional variant".
		/// </summary>
		[Test]
		public void LookupAWordAndAddAVariant()
		{
			using (ServiceFinder finder = new ServiceFinder())
			{
				finder.Init();
				finder.LoadTestDictionary("qTest");

				using (IDictionary dict = finder.GetDictionaryService("qTest"))
				{
					IList<IEntry> matches = dict.FindEntries("qTest", "oranges", FindMethods.DefaultApproximate);
					//pretend the user finds 'orange' in the resulting list and says "that's the one"...
					//your app would then show them the definition for that or whatever, and meanwhile
					//record that this inflectional form goes with that entry:
					int indexOfTheEntryUserChose = 0;
					matches[indexOfTheEntryUserChose].AddInflectionalVariant("qTest", "oranges");
				}
			}
		}

		[Test]
		public void AddWordToDictionary()
		{
			using (ServiceFinder finder = new ServiceFinder())
			{
				finder.Init();
				finder.LoadTestDictionary("qTest");

				using (IDictionary dict = finder.GetDictionaryService("qTest"))
				{
					IList<IEntry> matches = dict.FindEntries("qTest", "kumquat", FindMethods.Exact);
					if (matches.Count == 0 && dict.CanAddEntries)
					{
						IEntry kumquat = dict.CreateEntryLocally();
						kumquat.AddLexemeForm("qTest", "kumquat");
						kumquat.AddPrimaryDefinition("en", "small, edible, orangelike fruit");
						kumquat.AddPrimaryExampleSentence("qTest", "I'll take a kumquat shake.");
						dict.AddEntry(kumquat);
					}
				}
			}
		}

		private void FindAndPrintWord(IDictionary dict, string fruitName)
		{
			IList<IEntry> matches = dict.FindEntries("qTest", fruitName, FindMethods.Exact);
			StringBuilder htmlBuilder = new StringBuilder();
			htmlBuilder.Append("<html><body>");
			if (matches.Count == 0)
			{
				htmlBuilder.Append("Not Found");
			}
			foreach (IEntry entry in matches)
			{
				htmlBuilder.AppendFormat("<p>{0}</p>", entry.GetHtmlArticle(
					ArticleCompositionFlags.Definition |
					ArticleCompositionFlags.RelatedByDomain |
					ArticleCompositionFlags.Synonyms));
			}
			htmlBuilder.Append("</body></html>");
			Debug.WriteLine(htmlBuilder.ToString());
		}
	}
}
