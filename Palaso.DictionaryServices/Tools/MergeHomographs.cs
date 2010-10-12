using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Palaso.Code;
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
			File.Copy(inputLiftPath,outputLiftPath,true);
			RequireThat.File(outputLiftPath).Exists();

			using (var repo = new Palaso.DictionaryServices.LiftLexEntryRepository(outputLiftPath))
			{
				var writingSystemForMatching = new WritingSystemDefinition("en");

				progress.WriteMessage("Starting with {0} entries...", repo.Count);
				Merge(repo, writingSystemForMatching);
				progress.WriteMessage("Ended with {0} entries...", repo.Count);
			}
		}
//        public override void Run(string inputLiftPath, string outputLiftPath, IProgress progress)
//        {
//            using (var repo = new Palaso.DictionaryServices.LiftLexEntryRepository(inputLiftPath))
//            {
//                var writingSystemForMatching = new WritingSystemDefinition("en");
//
//
//                Merge(repo, writingSystemForMatching);
//                progress.WriteMessage("Starting with {0} entries...", repo.Count);
//
//                string headerContent = "";
//                using(var headerReader =  XmlReader.Create(inputLiftPath))
//                {
//                    headerReader.Read();
//                    headerReader.ReadStartElement("lift");
//                    headerReader.Read();
//                    if (headerReader.IsStartElement("header"))
//                    {
//                        headerContent =headerReader.ReadInnerXml();
//                        headerReader.ReadEndElement();
//                    }
//                }
//
//                using(var writer = new LiftWriter(outputLiftPath,LiftWriter.ByteOrderStyle.BOM))
//                {
//                    if(!string.IsNullOrEmpty(headerContent))
//                    {
//                        writer.WriteHeader(headerContent);
//                    }
//                    foreach (var id in repo.GetAllItems())
//                    {
//                        writer.Add(repo.GetItem(id));
//                    }
//
//                }
//            }
//        }

		private void Merge(LiftLexEntryRepository repo, WritingSystemDefinition writingSystemForMatching)
		{
		   var alreadyProcessed = new List<RepositoryId>();

			var ids = new List<RepositoryId>(repo.GetAllItems());
			for (int i = 0; i < ids.Count; i++)
			{
				if (alreadyProcessed.Contains(ids[i]))
					continue;
				alreadyProcessed.Add(ids[i]);
				var entry = repo.GetItem(ids[i]);
				var matches = repo.GetEntriesWithMatchingLexicalForm(entry.LexicalForm.GetExactAlternative(writingSystemForMatching.Id), writingSystemForMatching);
				foreach (RecordToken<LexEntry> token in matches)
				{
					if (alreadyProcessed.Contains(token.Id))
						continue;//we'll be here at least as each element matches itself

					alreadyProcessed.Add(token.Id);
					MergeEntries(entry,token.RealObject);
					repo.DeleteItem(token.RealObject);
					repo.SaveItem(entry);
				}
			}
		}

		private void MergeEntries(LexEntry entry1, LexEntry entry2)
		{
			if(entry2.ModificationTime > entry1.ModificationTime)
			{
				entry1.ModificationTime = entry2.ModificationTime;
			}
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