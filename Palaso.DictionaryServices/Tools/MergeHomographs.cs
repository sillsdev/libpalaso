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
using Palaso.Lift;
using Palaso.Lift.Options;
using Palaso.Progress.LogBox;
using Palaso.WritingSystems;

namespace Palaso.DictionaryServices.Tools
{
	public class MergeHomographs : Tool
	{
		private IProgress _progress;
		public override void Run(string inputLiftPath, string outputLiftPath, IProgress progress)
		{
			_progress = progress;
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

					entry.IsDirty = true;
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

			var senses = entry2.Senses.ToArray();
			foreach (var sense in senses)
			{
				MergeOrAddSense(entry1, sense);
			}

//            MergeMultiText(entry1, entry2, LexEntry.WellKnownProperties.Citation);
//            MergeMultiText(entry1, entry2, LexEntry.WellKnownProperties.LiteralMeaning);
//            MergeMultiText(entry1, entry2, PalasoDataObject.WellKnownProperties.Note);

			foreach (var property in entry2.Properties)
			{
				//absorb it only if we don't have a matching one
				if(entry1.Properties.Any(k=>k.Key == property.Key))
				{
					_progress.WriteWarning("{0}: Clashing values of {1}, merging anyways",entry1.GetSimpleFormForLogging(), property.Key);
				}
				else
				{
					entry1.Properties.Add(property);
				}
			}
		}

		private void MergeOrAddSense(LexEntry targetEntry, LexSense incomingSense)
		{
			if(targetEntry.Senses.Count ==0)
			{
				targetEntry.Senses.Add(incomingSense);//no problem!
			}
			else if (SenseHasNoNewInformation(targetEntry, incomingSense))
			{
				//just drop it
			}
			else if(TryMergeSenseWithSomeExistingSense(targetEntry, incomingSense))
			{
				//it was merged in
			}
			else
			{
				//it needs to be added
				targetEntry.Senses.Add(incomingSense);
			}
		}

		private bool TryMergeSenseWithSomeExistingSense(LexEntry targetEntry, LexSense incomingSense)
		{
			if (targetEntry.Senses.Count > 1)
				return false; //yes, we're pretty lazy!
			var targetSense = targetEntry.Senses[0];

			var targetPOS = targetSense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);
			var incomingPOS = incomingSense.GetOrCreateProperty<OptionRef>(LexSense.WellKnownProperties.PartOfSpeech);

			if(!targetPOS.IsEmpty)
			{
				if(!incomingPOS.IsEmpty)
				{
					if (targetPOS.Value != incomingPOS.Value)
						return false; //clashing POS
				}
			}

			if (!targetSense.Gloss.Empty)
			{
				if (incomingSense.Gloss.Empty)
					return false;

				if(!targetSense.Gloss.CanBeUnifiedWith(incomingSense.Gloss))
					return false;
			}

			if (!targetSense.Definition.Empty)
			{
				if (incomingSense.Definition.Empty)
					return false;

				if (!targetSense.Definition.CanBeUnifiedWith(incomingSense.Definition))
					return false;
			}

			if(targetPOS.IsEmpty)
			{
				targetPOS.Value = incomingPOS.Value;
			}
			targetSense.Gloss.MergeIn(incomingSense.Gloss);
			targetSense.Definition.MergeIn(incomingSense.Definition);
			return true;

		}

//        private bool TryMergeSenseWithSomeExistingSense(LexEntry targetEntry, LexSense incomingSense)
//        {
//
//           foreach (var sense in targetEntry.Senses)
//           {
//               if (sense.Gloss.Empty && !sense.Definition.Empty && incomingSense.Gloss.Empty &&
//                   !incomingSense.Definition.Empty)
//               {
//                   if (sense.Definition.TryMergeIn(incomingSense.Definition))
//                       return true; //we were able to merge definitions
//               }
//           }
//            return false;
//        }

		private bool SenseHasNoNewInformation(LexEntry targetEntry, LexSense incomingSense)
		{
			//can we find an existing sense which
//            foreach (var sense in targetEntry.Senses)
//            {
//                if(sense.Gloss.Empty && !incomingSense.Gloss.Empty)
//                    return false;
//
//                if(sense.Definition.Empty && !incomingSense.Definition.Empty)
//                    return false;
//
				// incoming has some other ws for gloss
//                if(incomingSense.Gloss.Forms.Any(f=>!sense.Gloss.ContainsAlternative(f.WritingSystemId)))
//                    return false;
//
				// incoming has some other ws for def
//                if(incomingSense.Definition.Forms.Any(f=>!sense.Definition.ContainsAlternative(f.WritingSystemId)))
//                    return false;
//
//
//            }
//            return true;
			return false;
		}

/*        private void MergeMultiText(PalasoDataObject first, PalasoDataObject second, string propertyName)
		{
			if (first.GetOrCreateProperty<MultiText>(propertyName).Empty && !second.GetOrCreateProperty<MultiText>(propertyName).Empty)
				ChangeMultiText(first, second, propertyName);
		}

		private void ChangeMultiText(PalasoDataObject target, PalasoDataObject source , string propertyName)
		{
			KeyValuePair<string, object> prop = target.Properties.Find(p => p.Key == propertyName);
			if (prop.Key != null)
			{
				target.Properties.Remove(prop);
				MultiText newText = source.GetOrCreateProperty<MultiText>(propertyName);
				var newGuy = new KeyValuePair<string, object>(propertyName, newText);
				//source.Properties.Remove(prop);//detach else it gets deleted
				target.Properties.Add(newGuy); ;
			}
		}
		*/
		public override string ToString()
		{
			return "Merge Homographs (Brutal)";
		}
	}
}