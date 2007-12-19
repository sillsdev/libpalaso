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

		[Test]
		public void AddWordToDictionary()
		{
			using (ServiceFinder finder = new ServiceFinder())
			{
				finder.Init();
				finder.LoadTestDictionary("qTest");

				using (IDictionary dict = finder.GetDictionaryService("qTest"))
				{
					IList<Entry> matches = dict.FindEntries("qTest", "kumquat", FindMethods.Exact);
					if (matches.Count == 0 && dict.CanAddEntries)
					{
						Entry kumquat = dict.CreateEntryLocally();
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
			IList<Entry> matches = dict.FindEntries("qTest", fruitName, FindMethods.Exact);
			StringBuilder htmlBuilder = new StringBuilder();
			htmlBuilder.Append("<html><body>");
			if (matches.Count == 0)
			{
				htmlBuilder.Append("Not Found");
			}
			foreach (Entry entry in matches)
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
