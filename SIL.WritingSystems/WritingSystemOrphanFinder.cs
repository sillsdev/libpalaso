using System.Collections.Generic;
using System.Linq;
using SIL.WritingSystems.Migration;

namespace SIL.WritingSystems
{
	/// <summary>
	/// The WritingSystemOrphanFinder class provides a uniform way for dealing with orphaned writing system ids.
	/// Orphaned writing system ids are writing system ids found in a file that are not contained
	/// in a given writing system repository.
	/// The algorithm used:
	/// 1. For each of these writing system ids passed in, check if it is contained in the repo.
	///    If so goto step 5.
	/// 2. Check with the repo whether the writing system id has been changed.
	///    If so, rename the writing system throughout the file and goto step 5.
	/// 3. Run the tag in question through the RfcTagCleaner and check if the new tag is in the repo.
	///    If so, rename the writing system throughout the file and goto step 5.
	/// 4. Create a new writing system with "cleaned" id and set it in the repo.
	/// 5. Complete
	/// </summary>
	public class WritingSystemOrphanFinder
	{
		///<summary>
		/// A delegate that implements the actual replacement of writing system tags in the file.
		/// The method is given the old writing system id 'oldId' and the new writing system id 'newId'.
		///</summary>
		///<param name="newId"></param>
		///<param name="oldId"></param>
		public delegate void IdReplacementStrategy(string newId, string oldId);

		///<summary>
		/// Constructor.
		///</summary>
		///<param name="idsInFile"></param>
		///<param name="replaceIdsInFile"></param>
		///<param name="writingSystemRepository"></param>
		public static void FindOrphans(
			IEnumerable<string> idsInFile,
			IdReplacementStrategy replaceIdsInFile,
			IWritingSystemRepository writingSystemRepository)
		{
			List<string> originalIds = idsInFile.ToList();
			List<string> updatedIds = originalIds.ToList();
			foreach (string wsId in originalIds)
			{
				// Check if it's in the repo
				if (writingSystemRepository.Contains(wsId))
				{
					continue;
				}
				string newId;
				if (writingSystemRepository.WritingSystemIdHasChanged(wsId))
				{
					newId = writingSystemRepository.WritingSystemIdHasChangedTo(wsId);
				}
				else
				{
					// It's an orphan
					// Clean it
					var rfcTagCleaner = new IetfLanguageTagCleaner(wsId);
					rfcTagCleaner.Clean();
					newId = rfcTagCleaner.GetCompleteTag();
				}

				WritingSystemDefinition conformantWritingSystem = writingSystemRepository.CreateNew(newId);
				// If it changed, then change
				if (newId != wsId)
				{
					conformantWritingSystem.MakeIetfLanguageTagUnique(updatedIds);
					replaceIdsInFile(wsId, conformantWritingSystem.IetfLanguageTag);
					updatedIds.Remove(wsId);
					updatedIds.Add(conformantWritingSystem.IetfLanguageTag);
				}

				// Check if it's in the repo
				if (writingSystemRepository.Contains(conformantWritingSystem.IetfLanguageTag))
					continue;

				// It's not in the repo so set it
				writingSystemRepository.Set(conformantWritingSystem);
			}
			writingSystemRepository.Save();
		}
	}
}