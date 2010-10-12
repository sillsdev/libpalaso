using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Palaso.Data;
using Palaso.DictionaryServices.Lift;
using Palaso.DictionaryServices.Model;
using Palaso.Progress.LogBox;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Tools
{
	public class MergeHomographs : Tool
	{
		public override void Run(string inputLiftPath, string outputLiftPath, IProgress progress)
		{
			using (var repo = new Palaso.DictionaryServices.LiftLexEntryRepository(inputLiftPath))
			{
				var writingSystemForMatching = new WritingSystemDefinition("en");


				Merge(repo, writingSystemForMatching);
				progress.WriteMessage("Starting with {0} entries...", repo.Count);

				string headerContent = "";
				using(var headerReader =  XmlReader.Create(inputLiftPath))
				{
					headerReader.Read();
					headerReader.ReadStartElement("lift");
					headerReader.Read();
					if (headerReader.IsStartElement("header"))
					{
						headerContent =headerReader.ReadInnerXml();
						headerReader.ReadEndElement();
					}
				}

				using(var writer = new LiftWriter(outputLiftPath,LiftWriter.ByteOrderStyle.BOM))
				{
					if(!string.IsNullOrEmpty(headerContent))
					{
						writer.WriteHeader(headerContent);
					}
					foreach (var id in repo.GetAllItems())
					{
						writer.Add(repo.GetItem(id));
					}

				}
			}
		}

		private void Merge(LiftLexEntryRepository repo, WritingSystemDefinition writingSystemForMatching)
		{
		   var alreadyProcessed = new List<RepositoryId>();

			var ids = new List<RepositoryId>(repo.GetAllItems());
			for (int i = 0; i < ids.Count; i++)
			{
				if (alreadyProcessed.Contains(ids[i]))
					continue;
				var entry = repo.GetItem(ids[i]);
				var matches = repo.GetEntriesWithMatchingLexicalForm(entry.LexicalForm.GetExactAlternative(writingSystemForMatching.Id), writingSystemForMatching);
				foreach (RecordToken<LexEntry> token in matches)
				{
					alreadyProcessed.Add(token.Id);
					MergeEntries(entry,token.RealObject);
					repo.DeleteItem(token.RealObject);
				}
			}
		}

		private void MergeEntries(LexEntry entry1, LexEntry entry2)
		{
			var toAdd = entry2.Senses.ToArray();
			foreach (var sense in toAdd)
			{
				entry1.Senses.Add(sense);
			}

		}

		public override string ToString()
		{
			return "Merge Homographs (Brutal)";
		}
	}
}